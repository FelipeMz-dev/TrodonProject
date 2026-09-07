using UnityEngine;

public class CoinScript : MonoBehaviour
{

    public float rotationSpeed = 3f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, 0, rotationSpeed));
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats player = other.GetComponent<PlayerStats>();
            player.AddScore(1);
            Destroy(gameObject);
        }
    }
}
