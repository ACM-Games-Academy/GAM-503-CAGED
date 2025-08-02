using UnityEngine;

public abstract class EnemyAIBase : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    protected Rigidbody2D rb;

    [Header("Detection")]
    public float detectionRange = 5f;
    public LayerMask groundLayer;
    public LayerMask playerLayer;

    protected bool isFacingRight = true;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected void FlipTowardsPlayer()
    {
        if (player != null)
        {
            if ((player.position.x > transform.position.x && !isFacingRight) ||
                (player.position.x < transform.position.x && isFacingRight))
            {
                Flip();
            }
        }
    }

    protected void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    protected bool PlayerInRange()
    {
        return Vector2.Distance(transform.position, player.position) < detectionRange;
    }

    protected abstract void HandleAI();

    protected virtual void FixedUpdate()
    {
        if (player != null)
            HandleAI();
    }
}