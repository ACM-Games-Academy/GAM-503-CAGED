using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float acceleration = 10f;
    public float gravityScale = 4f;
    public float fallMultiplier = 2f;

    [HideInInspector] public float horizontalInput;
    public int FacingDirection { get; private set; } = 1;

    private Rigidbody2D rb;

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    public void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput != 0)
        {
            FacingDirection = (int)Mathf.Sign(horizontalInput);
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * FacingDirection;
            transform.localScale = scale;
        }
    }

    public void HandleMovement()
    {
        float targetSpeed = horizontalInput * moveSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        rb.velocity = new Vector2(rb.velocity.x + speedDiff * acceleration * Time.fixedDeltaTime, rb.velocity.y);
    }

    public void ApplyGravity()
    {
        rb.gravityScale = (rb.velocity.y < 0) ? gravityScale * fallMultiplier : gravityScale;
    }
}