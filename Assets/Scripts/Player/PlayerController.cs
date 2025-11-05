using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Controll")]
    [SerializeField] float moveSpeed = 10.0f;
    [SerializeField] float jumpForce = 3.0f;
    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference jump;
    CharacterController cc;

    [Header("Camera Controll")]
    [SerializeField] CinemachineCamera playerCam;

    // playerVelocity: 이제 y(중력/점프) + xz(수평 모멘텀) 모두 사용
    private Vector3 playerVelocity;
    private bool isGrounded;
    private float gravity = -9.81f;

    // 공중에서 방향 전환을 얼마나 허용할지 (낮을수록 무겁고 둔함)
    [SerializeField] float airControl = 2.0f;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        move.action.Enable();
        jump.action.Enable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        move.action.Disable();
        jump.action.Disable();
    }

    void Update()
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

        // 한 번의 Move로 이동 (수평+수직)
        cc.Move(playerVelocity * Time.deltaTime);

        // 바닥 밀착 안정화
        if (cc.isGrounded && playerVelocity.y < 0f)
            playerVelocity.y = -2f;
    }

    private void LateUpdate()
    {
        if (playerCam == null) return;

        transform.rotation = Quaternion.Euler(
            playerCam.transform.rotation.x,
            playerCam.transform.rotation.y,
            transform.rotation.z
        );
    }
}
