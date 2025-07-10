using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("IFrames")]
    public float iFrameDuration = 1f;
    private bool isInvincible = false;

    private SpriteRenderer spriteRend;
    private Collider2D col;
    

    private void Start()
    {
        currentHealth = maxHealth;
        col = GetComponent<Collider2D>();
        spriteRend = GetComponent<SpriteRenderer>();
        isInvincible = false;
        Debug.Log(isInvincible);
        Physics2D.IgnoreLayerCollision(6, 7, isInvincible);
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible) return;
        currentHealth -= amount;

        Debug.Log("Player took damage. Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Debug.Log("Player Died");
        }

        StartCoroutine(InvincibilityFrames());
        isInvincible = false;
    }

    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        // Player collision with Enemy disabled
        Physics2D.IgnoreLayerCollision(6, 7, isInvincible);
        spriteRend.color = new Color(1, 0, 0, 0.5f);

        yield return new WaitForSeconds(iFrameDuration);

        // Player collision with Enemy enabled
        isInvincible = false;
        Physics2D.IgnoreLayerCollision(6, 7, isInvincible);
        spriteRend.color = Color.white;
    }
}
