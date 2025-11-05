using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class Fire : MonoBehaviour
{
    [Header("Fire Gun")]
    [SerializeField] InputActionReference fire;

    private void OnEnable()
    {
        fire.action.Enable();
        fire.action.performed += OnFire;
    }

    private void OnDisable()
    {
        fire.action.Disable();
        fire.action.performed -= OnFire;
    }

   
    private void OnFire(InputAction.CallbackContext context)
    {
        // 화면 정중앙에서 레이를 생성합니다.
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        // 레이캐스트를 발사하고, 부딪힌 오브젝트 정보를 hit 변수에 저장합니다.
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 부딪힌 오브젝트의 이름을 콘솔에 출력합니다.
            Debug.Log("Raycast hit: " + hit.collider.name);
        }

        GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }
}