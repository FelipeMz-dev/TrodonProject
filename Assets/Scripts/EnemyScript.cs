using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public float xStart;
    public float xEnd;
    public float speed;
    public float health = 5;

    private float _speed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _speed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        float currentX = transform.position.x;
        if (currentX < xStart) speed = _speed;
        if (currentX > xEnd) speed = -_speed;
        
        transform.position += new Vector3(speed * Time.deltaTime, 0, 0);
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
