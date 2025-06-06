using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 180;
    private int currentHealth;

    public bool isDead = false;

    public float regenDelay = 5f;
    private float regenTimer = 0f;
    private bool canRegen = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (canRegen && !isDead)
        {
            regenTimer -= Time.deltaTime;
            if (regenTimer <= 0f)
            {
                RegenerateHealth();
                canRegen = false;  
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log("Player took damage. Current HP: " + currentHealth);

        SoundManager.Instance.PlayGlobalOneShot(SoundManager.Instance.playerDamageClip);
        UIManager.Instance.ShowDust(); 

        if (currentHealth <= 0)
        {
            isDead = true;
            Debug.Log("Player is dead.");
            SoundManager.Instance.PlayJumpScareSFX(SoundManager.Instance.playerDeath);
            UIManager.Instance.ShowGameOverScreen(); 
            CameraShake.Instance.Shake(0.5f, 0.5f);
            ReloadScene();
        }
        else
        {
            regenTimer = regenDelay;
            canRegen = true;
        }
    }

    private void RegenerateHealth()
    {
        currentHealth = maxHealth;
        Debug.Log("Player health regenerated to max: " + currentHealth);
        UIManager.Instance.FadeOutDust();
    }

    public void ReloadScene()
    {
        Invoke(nameof(ReloadCurrentScene), 5f);
    }

    private void ReloadCurrentScene()
    {
        UIManager.Instance.HideGameOverScreen();
        SceneManager.LoadScene("Shop");
    }
}
