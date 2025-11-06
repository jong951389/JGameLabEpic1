
using UnityEngine;
using UnityEngine.InputSystem;

public class CapsulePick : MonoBehaviour
{
    [Header("Pick")]
    [SerializeField] InputActionReference mouseLeftAction;

    private Camera mainCamera;
    private GameObject pickedObject;
    private Vector3 offset;
    private float initialDistance;
    private Vector3 originalPosition;

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
            Vector3 worldPoint = mouseRay.GetPoint(initialDistance);
            Vector3 newPosition = worldPoint + offset;
            pickedObject.transform.position = new Vector3(newPosition.x, newPosition.y, newPosition.z);
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
                Ray mouseRay = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                Vector3 worldPoint = mouseRay.GetPoint(initialDistance);
                offset = pickedObject.transform.position - worldPoint;
            }
        }
    }

    private void OnRelease(InputAction.CallbackContext context)
    {
        if (pickedObject != null)
        {
            pickedObject.transform.position = originalPosition;
            pickedObject = null;
        }
    }
}

