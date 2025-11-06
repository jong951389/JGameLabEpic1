using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField] float detectDistance = 10f;
    GameObject player;
    private void Awake()
    {
        player = GameObject.Find("Player");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.childCount <= 0)
        {
            Destroy(gameObject);
        }

        if (Vector3.Distance(transform.position, player.transform.position) < detectDistance)
        {
            GetComponent<Enemy>().enabled = true;
        }
    }
}
