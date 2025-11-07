using UnityEngine;

public class Boss : MonoBehaviour
{
    GameObject player;
    [SerializeField] float firstDetectDistance =25f;

    private void Awake()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.childCount <= 0)
        {
            Destroy(gameObject);
        }

        if (Vector3.Distance(transform.position, player.transform.position) < firstDetectDistance) GetComponent<Enemy>().enabled = true;
    }
}
