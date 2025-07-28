using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    public PlayerMovement Movement { get; private set; }
    public PlayerJump Jump { get; private set; }
    public PlayerWall Wall { get; private set; }
    public PlayerDash Dash { get; private set; }
    public PlayerKnockback Knockback { get; private set; }

    private void Awake()
    {
        Movement = GetComponent<PlayerMovement>();
        Jump = GetComponent<PlayerJump>();
        Wall = GetComponent<PlayerWall>();
        Dash = GetComponent<PlayerDash>();
        Knockback = GetComponent<PlayerKnockback>();
    }

    private void Update()
    {
        Movement?.HandleInput();
        Jump?.HandleUpdate();
        Wall?.HandleUpdate();
        Dash?.HandleUpdate();
    }

    private void FixedUpdate()
    {
        if (Dash.IsDashing) return;

        Movement?.HandleMovement();
        Wall?.HandleWallSlideMovement();
        Jump?.HandleFixedUpdate();
        Movement?.ApplyGravity();
    }
}
