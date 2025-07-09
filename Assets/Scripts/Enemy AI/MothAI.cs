using UnityEngine;

public class MothAI : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 3f;
    public float stoppingDistance = 2f;

    [Header("Dash Attack")]
    public float dashSpeed = 8f;
    public float dashCooldown = 2f;
    public float dashDistance = 1.5f;
    public float dashDuration = 0.3f;

    [Header("References")]
    public Transform player;

    private Rigidbody2D rb;
    private float dashTimer;
    private bool isDashing;
    private float dashTimeElapsed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (isDashing)
        {
            dashTimeElapsed += Time.fixedDeltaTime;

            // Recalculate direction every frame during dash
            Vector2 dynamicDirection = (player.position - transform.position).normalized;
            rb.velocity = dynamicDirection * dashSpeed;

            if (dashTimeElapsed >= dashDuration)
            {
                isDashing = false;
                dashTimer = dashCooldown;
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            dashTimer -= Time.fixedDeltaTime;

            if (distance <= dashDistance && dashTimer <= 0f)
            {
                StartDash();
            }
            else if (distance > stoppingDistance)
            {
                Vector2 moveDirection = (player.position - transform.position).normalized;
                rb.velocity = moveDirection * speed;
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimeElapsed = 0f;
    }
}
