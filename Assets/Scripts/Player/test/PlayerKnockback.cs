using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerKnockback : MonoBehaviour
{
    public float force = 5;
    public ForceMode2D forceMode = ForceMode2D.Impulse;

    private Rigidbody2D rb;

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            ContactPoint2D contactPoint = collision.GetContact(0);
            Vector2 dir = (Vector2)transform.position - contactPoint.point;
            dir.Normalize();

            rb.velocity = Vector2.zero;
            rb.inertia = 0;
            rb.AddForce(dir * force, forceMode);
        }
    }
}