using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IPlayerHealth
{
    public int maxHealth = 5;
    private int currentHealth;
    public float iFrameDuration = 1f;

    private bool isInvincible;
    private bool isRespawning = false;

    public bool IsRespawning => isRespawning;

    public Transform respawnPoint;
    public float fadeDuration = 1f;
    public CanvasGroup fadeCanvas;

    private SpriteRenderer spriteRend;
    private Collider2D col;

    [Header("Health UI")]
    public GameObject healthBarParent; // Assign the HealthBar GameObject in inspector

    private Image[] healthImages;

    private void Start()
    {
        currentHealth = maxHealth;
        col = GetComponent<Collider2D>();
        spriteRend = GetComponent<SpriteRenderer>();
        SetInvincible(false);

        // Cache health images from children of healthBarParent
        healthImages = new Image[healthBarParent.transform.childCount];
        for (int i = 0; i < healthBarParent.transform.childCount; i++)
        {
            healthImages[i] = healthBarParent.transform.GetChild(i).GetComponent<Image>();
        }

        UpdateHealthUI();

        // Make sure fade starts transparent
        if (fadeCanvas)
        {
            fadeCanvas.alpha = 0f;
            fadeCanvas.interactable = false;
            fadeCanvas.blocksRaycasts = false;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        StartCoroutine(InvincibilityFrames());
    }

    public void HealFull()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        spriteRend.color = Color.white;
        SetInvincible(false);
    }

    private void UpdateHealthUI()
    {
        /* Disable images starting from right to left based on currentHealth
           The images are ordered left to right in hierarchy (0 is leftmost, 4 is rightmost)
           We want to disable starting from index 4 backward as health drops */

        int healthToShow = currentHealth;

        for (int i = 0; i < healthImages.Length; i++)
        {
            int imageIndex = healthImages.Length - 1 - i; // reverse index: 4,3,2,1,0
            if (i < (maxHealth - healthToShow))
            {
                // Disable health image (lost HP)
                healthImages[imageIndex].enabled = false;
            }
            else
            {
                // Enable health image (remaining HP)
                healthImages[imageIndex].enabled = true;
            }
        }
    }

    public void SpikeDeathRespawn()
    {
        if (!isRespawning)
            StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        // Fade to black
        if (fadeCanvas)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeCanvas.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
        }

        TakeDamage(1); // Lose 1 HP

        // Respawn
        transform.position = respawnPoint.position;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        // Small delay to make the respawn feel like a "pause"
        yield return new WaitForSeconds(0.2f);

        // Fade back in
        if (fadeCanvas)
        {
            float t = fadeDuration;
            while (t > 0f)
            {
                t -= Time.deltaTime;
                fadeCanvas.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
        }

        isRespawning = false;
    }

    public bool IsDead() => currentHealth <= 0;

    private IEnumerator InvincibilityFrames()
    {
        SetInvincible(true);
        spriteRend.color = new Color(1, 0, 0, 0.5f);
        yield return new WaitForSeconds(iFrameDuration);
        SetInvincible(false);
        spriteRend.color = Color.white;
    }

    private void SetInvincible(bool value)
    {
        isInvincible = value;
        Physics2D.IgnoreLayerCollision(6, 7, value);
    }
}
