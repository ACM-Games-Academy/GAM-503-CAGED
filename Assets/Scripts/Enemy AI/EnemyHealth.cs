using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyHealth : MonoBehaviour
{
    public int health = 3;
    public float knockbackForce = 1f;
    public Color hitColor = Color.red;
    public float colorChangeDuration = 0.2f;
    public float damageCooldown = 0.5f; // Prevent constant damage per frame

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine colorChangeCoroutine;

    private float lastDamageTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        lastDamageTime = -damageCooldown; // Allow immediate damage on start
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerAttack"))
        {
            if (Time.time - lastDamageTime >= damageCooldown)
            {
                Vector2 knockbackDir = (transform.position - collision.transform.position).normalized;
                TakeDamage(1, knockbackDir);
                lastDamageTime = Time.time;
            }
        }
    }

    public void TakeDamage(int amount, Vector2 knockbackDirection)
    {
        health -= amount;

        // Apply knockback
        rb.velocity = Vector2.zero;
        var mothAI = GetComponent<MothAI>();
        if (mothAI != null)
        {
            mothAI.ApplyKnockback(knockbackDirection * knockbackForce);
        }
        else
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }

        // Visual feedback
        if (colorChangeCoroutine != null)
        {
            StopCoroutine(colorChangeCoroutine);
        }
        colorChangeCoroutine = StartCoroutine(ChangeColorRoutine());

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator ChangeColorRoutine()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(colorChangeDuration);
        spriteRenderer.color = originalColor;
    }
}
