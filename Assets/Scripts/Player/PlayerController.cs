using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Player Controll")]
    [SerializeField] float moveSpeed = 10.0f;
    [SerializeField] float jumpForce = 3.0f;
    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference jump;
    [SerializeField] InputActionReference pick;
    CharacterController cc;

    [Header("Camera Controll")]
    [SerializeField] CinemachineCamera playerCam;

    [Header("Pick Controll")]
    [SerializeField] float PickTimeScale = 0.2f;

    // playerVelocity: 이제 y(중력/점프) + xz(수평 모멘텀) 모두 사용
    private Vector3 playerVelocity;
    private Vector3 externalMove; // 외부에서 가해지는 움직임(넉백 등)
    private bool isGrounded;
    private float gravity = -9.81f;

    // 공중에서 방향 전환을 얼마나 허용할지 (낮을수록 무겁고 둔함)
    [SerializeField] float airControl = 2.0f;

    [Header("Knockback")]
    [SerializeField] float pushPower = 10.0f;
    [SerializeField] float pushDuration = 0.3f;
    [SerializeField] AnimationCurve pushCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    Coroutine pushRoutine;

    // 🔹 겹침 폴링용 (너무 자주 연속 트리거 방지)
    [SerializeField] LayerMask enemyMask = ~0; // 필요하면 "Enemy" 레이어만 지정해서 성능/안정성↑
    [SerializeField] float touchCooldown = 0.15f;
    float _pushCooldownUntil;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        move.action.Enable();
        jump.action.Enable();
        pick.action.Enable();
    }

    private void OnDisable()
    {
        move.action.Disable();
        jump.action.Disable();
        pick.action.Disable();
    }

    void Update()
    {
        PlayerControll();

        if (!pick.action.IsPressed())
        {
            LockCursor();
            Time.timeScale = 1f;
        }
        else
        {
            UnlockCursor();
            Time.timeScale = PickTimeScale;
        }
    }

    private void LateUpdate()
    {
        if (!pick.action.IsPressed())
        {
            PlayerRot();
            playerCam.GetComponent<CinemachineInputAxisController>().enabled = true;
            Time.timeScale = 1f;
        }
        else
        {
            playerCam.GetComponent<CinemachineInputAxisController>().enabled = false;
            Time.timeScale = 0.2f;
        }
    }

    #region 플레이어 조작 메서드
    void PlayerRot()
    {
        if (playerCam == null) return;

        transform.rotation = Quaternion.Euler(
            playerCam.transform.rotation.x,
            playerCam.transform.rotation.y,
            transform.rotation.z
        );
    }

    void PlayerControll()
    {
        bool wasGrounded = isGrounded;
        isGrounded = cc.isGrounded;

        // 착지 프레임에 남은 낙하속도 정리(바닥에 붙이기)
        if (isGrounded && !wasGrounded && playerVelocity.y < 0f)
            playerVelocity.y = -2f;

        // 입력
        Vector2 moveInput = move.action.ReadValue<Vector2>();

        // 기준 벡터(카메라 기준으로 수평화)
        Vector3 forward = playerCam ? playerCam.transform.forward : transform.forward;
        Vector3 right = playerCam ? playerCam.transform.right : transform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        // 목표 수평 방향
        Vector3 desiredDir = (right * moveInput.x + forward * moveInput.y).normalized;

        // 현재 수평 속도
        Vector3 horizontalVel = new Vector3(playerVelocity.x, 0f, playerVelocity.z);

        if (isGrounded)
        {
            // 지상: 즉시 목표 속도로 스냅(접지력 느낌)
            horizontalVel = desiredDir * moveSpeed;
        }
        else
        {
            // 공중: 기존 속도 → 목표 속도로 천천히 보간(관성 유지)
            // airControl이 낮을수록 공중에서 방향 전환이 둔함
            Vector3 target = desiredDir * moveSpeed;
            horizontalVel = Vector3.Lerp(horizontalVel, target, airControl * Time.deltaTime);
        }

        // 점프
        if (jump.action.triggered && isGrounded)
            playerVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

        // 중력
        playerVelocity.y += gravity * Time.deltaTime;

        // 최종 속도 반영
        playerVelocity.x = horizontalVel.x;
        playerVelocity.z = horizontalVel.z;

        // 한 번의 Move로 이동 (수평+수직+외부 힘)
        cc.Move((playerVelocity + externalMove) * Time.deltaTime);

        // 🔸 정지 중에도 '닿았으면' 넉백: CC 캡슐과 겹침 폴링
        if (Time.time >= _pushCooldownUntil)
        {
            Bounds b = cc.bounds;
            float radius = cc.radius * 1.02f; // 살짝 여유
            Vector3 top = new Vector3(b.center.x, b.max.y - radius, b.center.z);
            Vector3 bottom = new Vector3(b.center.x, b.min.y + radius, b.center.z);

            // 레이어 필터가 지정되어 있으면 그 레이어만, 아니면 전체(~0)
            int mask = enemyMask.value;
            if (mask == 0) mask = ~0;

            var cols = Physics.OverlapCapsule(bottom, top, radius, mask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < cols.Length; i++)
            {
                var c = cols[i];
                if (!c) continue;

                // 태그로도 한 번 더 필터링 (원하지 않으면 제거 가능)
                if (!c.CompareTag("Enemy")) continue;

                // 수평 넉백 방향
                Vector3 dir = (transform.position - c.transform.position);
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.0001f) continue;
                dir.Normalize();

                if (pushRoutine != null) StopCoroutine(pushRoutine);
                pushRoutine = StartCoroutine(PushOverTime(dir));

                _pushCooldownUntil = Time.time + touchCooldown; // 연속 트리거 방지
                break;
            }
        }

        // 바닥 밀착 안정화
        if (cc.isGrounded && playerVelocity.y < 0f)
            playerVelocity.y = -2f;
    }

    public void SetExternalMove(Vector3 move)
    {
        externalMove = move;
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    #endregion

    #region 넉백
    // 이동 중 부딪칠 때도 계속 작동(추가 트리거)
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!hit.collider || !hit.collider.CompareTag("Enemy")) return;

        Vector3 dir = (transform.position - hit.collider.transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        if (pushRoutine != null) StopCoroutine(pushRoutine);
        pushRoutine = StartCoroutine(PushOverTime(dir));
    }

    IEnumerator PushOverTime(Vector3 direction)
    {
        float t = 0f;
        while (t < pushDuration)
        {
            float k = pushCurve.Evaluate(t / pushDuration);
            SetExternalMove(direction * pushPower * k);
            t += Time.deltaTime;
            yield return null;
        }
        SetExternalMove(Vector3.zero);
        pushRoutine = null;
    }
    #endregion
}
