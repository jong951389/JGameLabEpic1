using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float moveSpeed = 10.0f;
    GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position =  Vector3.MoveTowards(transform.position, player.transform.position, moveSpeed*Time.deltaTime);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == player)
        {
            CharacterController playerController = other.transform.GetComponent<CharacterController>();
            if (playerController != null)
            {
                Vector3 pushDirection = (other.transform.position - transform.position).normalized;
                float pushPower = 2.0f; // Adjust this value as needed
                playerController.Move(pushDirection * pushPower);
            }
        }
    }

}
