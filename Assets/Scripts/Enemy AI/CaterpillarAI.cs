using UnityEngine;

public class CaterpillarAI : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float rayDistance = 0.2f;
    public LayerMask groundLayer;
    public Transform surfaceCheck;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Vector2 surfaceNormal;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Start crawling to the right on floor
        moveDirection = Vector2.right;
        surfaceNormal = Vector2.up;
    }

    void FixedUpdate()
    {
        Vector2 origin = surfaceCheck.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, -surfaceNormal, rayDistance, groundLayer);

        if (hit.collider != null)
        {
            // Update surface normal from hit
            surfaceNormal = hit.normal;

            // Calculate new tangent direction
            moveDirection = new Vector2(-surfaceNormal.y, surfaceNormal.x);

            // Rotate to match surface
            float angle = Mathf.Atan2(surfaceNormal.y, surfaceNormal.x) * Mathf.Rad2Deg - 90f;
            rb.MoveRotation(angle);

            // Move along surface
            rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // If no surface below, try to rotate around corner
            TryCornerRotation();
        }
    }

    void TryCornerRotation()
    {
        // Try checking both directions to find a new surface to crawl
        Vector2[] directions = new Vector2[]
        {
            Quaternion.Euler(0, 0, 90) * -surfaceNormal,
            Quaternion.Euler(0, 0, -90) * -surfaceNormal
        };

        foreach (Vector2 dir in directions)
        {
            RaycastHit2D cornerHit = Physics2D.Raycast(surfaceCheck.position, dir, rayDistance, groundLayer);
            if (cornerHit.collider != null)
            {
                surfaceNormal = cornerHit.normal;
                moveDirection = new Vector2(-surfaceNormal.y, surfaceNormal.x);
                return;
            }
        }

        // No surface found — optional: idle, fall, or reverse
    }

    void OnDrawGizmosSelected()
    {
        if (surfaceCheck != null)
            Gizmos.color = Color.red;
        Gizmos.DrawLine(surfaceCheck.position, surfaceCheck.position - (Vector3)surfaceNormal * rayDistance);
    }
}
