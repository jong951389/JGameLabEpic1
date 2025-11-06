using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CapsulePick : MonoBehaviour
{
    public static CapsulePick Instance;

    [Header("Pick")]
    [SerializeField] InputActionReference mouseLeftAction;
    [SerializeField] private Gun gun;
    [SerializeField] List<Transform> slots = new List<Transform>();

    public GameObject pickedObject;
    private Camera mainCamera;
    private float initialDistance;
    private Vector3 originalPosition;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        mouseLeftAction.action.Enable();
        mouseLeftAction.action.performed += OnPick;
        mouseLeftAction.action.canceled += OnRelease;
    }

    private void OnDisable()
    {
        mouseLeftAction.action.Disable();
        mouseLeftAction.action.performed -= OnPick;
        mouseLeftAction.action.canceled -= OnRelease;
    }

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (pickedObject != null && mouseLeftAction.action.IsPressed())
        {
            Ray mouseRay = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Vector3 newPosition = mouseRay.GetPoint(initialDistance);
            pickedObject.transform.position = newPosition;
        }
    }

    private void OnPick(InputAction.CallbackContext context)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        // "모든 레이어에서 6번만 제외"하는 마스크
        int layerMask = ~(1 << 6);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            if (hit.collider.gameObject.layer == 3)
            {
                pickedObject = hit.collider.gameObject;
                originalPosition = pickedObject.transform.position;
                initialDistance = Vector3.Distance(mainCamera.transform.position, pickedObject.transform.position);
            }
        }
    }

    public void OnRelease(InputAction.CallbackContext context)
    {
        if (pickedObject != null)
        {
            Transform emptySlot = null;

            for(int i = 0;i < slots.Count ; i++)
            {
                if(!slots[i].GetComponent<CapsuleSlot>().isFilled) emptySlot = slots[i];
            }

            pickedObject.transform.position = emptySlot.position;
            pickedObject = null;
        }
    }
}

