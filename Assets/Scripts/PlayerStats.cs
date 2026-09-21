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
            int lives = hudManager.less1Live();
            player.MoveToLastCheckpoint();
            currentHealth = 100;
            if (lives < 0) Destroy(gameObject);
        }
        hudManager.UpdateHealth(currentHealth, maxHealth);
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
        hudManager.soundProjectilHit();
    }
}
