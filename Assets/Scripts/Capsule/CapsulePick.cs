
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class CapsulePick : MonoBehaviour
{
    public static CapsulePick Instance;

    [Header("Pick")]
    [SerializeField] InputActionReference mouseLeftAction;
    [SerializeField] private Gun gun;

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
        if (Physics.Raycast(ray, out RaycastHit hit))
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
            pickedObject.transform.position = originalPosition;
            pickedObject = null;
        }
    }
}

