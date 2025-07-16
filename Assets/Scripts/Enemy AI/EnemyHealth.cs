using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyHealth : MonoBehaviour
{
    public int health = 3;
    public float knockbackForce = 1f;
    public Color hitColor = Color.red;       // Color to change to on hit
    public float colorChangeDuration = 0.2f; // Duration to keep the hit color

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine colorChangeCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerAttack"))
        {
            Vector2 knockbackDir = (transform.position - collision.transform.position).normalized;
            rb.velocity = Vector2.zero;
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

            TakeDamage(1);
        }
    }

    public void TakeDamage(int amount)
    {
        health -= amount;

        // Change color to indicate hit
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
