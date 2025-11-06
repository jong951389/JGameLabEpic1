using UnityEngine;

public class CapsuleSlot : MonoBehaviour
{
    [Header("Slot")]
    public bool isFilled;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            isFilled = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            isFilled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            isFilled = false;
        }
    }
}