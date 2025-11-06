using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Enemy : MonoBehaviour
{
    [SerializeField] float moveSpeed = 10.0f;

    Transform player;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;                                   // CC와 안정적으로 충돌
        rb.interpolation = RigidbodyInterpolation.Interpolate;   // 부드러운 이동
        // 콜라이더(박스/캡슐)는 isTrigger = false 로 두세요.
    }

    void Start()
    {
        var p = GameObject.Find("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (!player) return;

        // 수평 방향으로만 추적
        Vector3 pos = rb.position;
        Vector3 target = player.position;
        target.y = pos.y;

        Vector3 dir = (target - pos).normalized;
        Vector3 next = pos + dir * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(next); // 물리 쿼리 갱신과 함께 이동 (Transform.position 직접 세팅 금지)
    }
}
