using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private HUDManager hudManager;
    private PlayerScript player;
    private float currentHealth = 100f;
    private int score = 0;

    public float maxHealth = 100f;

    void Start()
    {
        player = GetComponent<PlayerScript>();
    }

    void Update()
    {
        if (hudManager.IsPlayState() != player.IsPlayerPlayState())
        {
            player.TogglePlayState(hudManager.IsPlayState());
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);
        hudManager.UpdateHealth(currentHealth, maxHealth);
        if (currentHealth <= 0) {
            hudManager.GameOver();
            Destroy(gameObject);
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        hudManager.UpdateScore(score);
    }

    public void shot()
    {
        hudManager.soundShot();
    }

    public void dash()
    {
        hudManager.soundDash();
    }

    public void projectilHit()
    {
        
    }
}
