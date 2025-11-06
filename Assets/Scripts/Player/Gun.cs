using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

enum GunState
{
    normal = 0,
    red,
    green,
    blue
}

public class Gun : MonoBehaviour
{
    [Header("Fire ")]
    [SerializeField] InputActionReference mouseLeftAction;
    [SerializeField] InputActionReference pick;
    [SerializeField] float maxFireDistance = 100f;

    [Header("GunState")]
    [SerializeField]GunState gunstate = GunState.normal;

    [Header("Capsule Prefabs")]
    [SerializeField] GameObject redCapsulePrefab;
    [SerializeField] GameObject greenCapsulePrefab;
    [SerializeField] GameObject blueCapsulePrefab;
    [SerializeField] GameObject normalCapsulePrefab;
    [SerializeField] Transform capsuleSlot;
    [SerializeField] Material normalMat;
    [SerializeField] Material redMat;
    [SerializeField] Material greenMat;
    [SerializeField] Material blueMat;


    private void OnEnable()
    {
        mouseLeftAction.action.Enable();
        pick.action.Enable();
        mouseLeftAction.action.performed += Fire;
    }

    private void OnDisable()
    {
        mouseLeftAction.action.Disable();
        pick.action.Disable();
        mouseLeftAction.action.performed -= Fire;
    }

    private void Fire(InputAction.CallbackContext context)
    {
        if (pick.action.IsPressed())
        {
            return;
        }
        else
        {
            // 화면 정중앙에서 레이를 생성합니다.
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            // Layer 7을 무시하기 위한 마스크 설정
            int layerToIgnore = 1 << 7;
            int layerMask = ~layerToIgnore; // 7번 레이어만 제외한 모든 레이어 탐지

            // 레이캐스트를 발사하고, 부딪힌 오브젝트 정보를 hit 변수에 저장합니다.
            Debug.DrawRay(ray.origin, ray.direction * maxFireDistance, Color.red, 1f);
            if (Physics.Raycast(ray, out RaycastHit hit, maxFireDistance, layerMask))
            {
                if(gunstate == GunState.red && hit.collider.name.Contains("Red")) Destroy(hit.collider.gameObject);
                else if(gunstate == GunState.green && hit.collider.name.Contains("Green")) Destroy(hit.collider.gameObject);
                else if(gunstate == GunState.blue && hit.collider.name.Contains("Blue")) Destroy(hit.collider.gameObject);
                else if(gunstate == GunState.normal && hit.collider.name.Contains("Normal")) Destroy(hit.collider.gameObject);
            }

            GetComponent<CinemachineImpulseSource>().GenerateImpulse();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer ==  3)
        {
            if (other.gameObject.name.Contains("Red"))
            {
                Destroy(other.gameObject);
                CapsulePick.Instance.pickedObject = Instantiate(GetCapsulePrefab(),capsuleSlot);
                gunstate = GunState.red;
                GetComponent<MeshRenderer>().material = redMat;
                CapsulePick.Instance.OnRelease(new InputAction.CallbackContext());
            }
            else if (other.gameObject.name.Contains("Green"))
            {
                Destroy(other.gameObject);
                CapsulePick.Instance.pickedObject = Instantiate(GetCapsulePrefab(), capsuleSlot);
                gunstate = GunState.green;
                GetComponent<MeshRenderer>().material = greenMat;
                CapsulePick.Instance.OnRelease(new InputAction.CallbackContext());
            }
            else if (other.gameObject.name.Contains("Blue"))
            {
                Destroy(other.gameObject);
                CapsulePick.Instance.pickedObject = Instantiate(GetCapsulePrefab(), capsuleSlot);
                gunstate = GunState.blue;
                GetComponent<MeshRenderer>().material = blueMat;
                CapsulePick.Instance.OnRelease(new InputAction.CallbackContext());
            }
            else if (other.gameObject.name.Contains("Normal"))
            {
                Destroy(other.gameObject);
                CapsulePick.Instance.pickedObject = Instantiate(GetCapsulePrefab(), capsuleSlot);
                gunstate = GunState.normal;
                GetComponent<MeshRenderer>().material = normalMat;
                CapsulePick.Instance.OnRelease(new InputAction.CallbackContext());
            }
        }
    }

    public GameObject GetCapsulePrefab()
    {
        switch (gunstate)
        {
            case GunState.red:
                return redCapsulePrefab;
            case GunState.green:
                return greenCapsulePrefab;
            case GunState.blue:
                return blueCapsulePrefab;
            case GunState.normal:
            default:
                return normalCapsulePrefab;
        }
    }
}