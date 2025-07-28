using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour, IPlayerAttack
{
    [Header("Attack Settings")]
    public float attackDuration = 0.2f;
    public float attackCooldown = 0.3f;

    [Header("Attack Hitboxes")]
    public GameObject neutralAttackZone;
    public GameObject upAttackZone;
    public GameObject downAttackZone;

    private Collider2D neutralCollider;
    private Collider2D upCollider;
    private Collider2D downCollider;

    private SpriteRenderer neutralVisual;
    private SpriteRenderer upVisual;
    private SpriteRenderer downVisual;

    private bool isAttacking = false;
    private bool canAttack = true;
    private bool attackEnabled = true;

    private Vector2 inputDirection;

    private void Start()
    {
        neutralCollider = neutralAttackZone.GetComponent<Collider2D>();
        upCollider = upAttackZone.GetComponent<Collider2D>();
        downCollider = downAttackZone.GetComponent<Collider2D>();

        neutralVisual = neutralAttackZone.GetComponent<SpriteRenderer>();
        upVisual = upAttackZone.GetComponent<SpriteRenderer>();
        downVisual = downAttackZone.GetComponent<SpriteRenderer>();

        neutralAttackZone.SetActive(false);
        upAttackZone.SetActive(false);
        downAttackZone.SetActive(false);
    }

    private void Update()
    {
        if (!attackEnabled) return;
        HandleInput();
    }

    private void HandleInput()
    {
        inputDirection = PlayerInputManager.Instance.GetMovement();

        if (PlayerInputManager.Instance.GetAttack() && canAttack)
        {
            StartCoroutine(DoAttack());
        }
    }


    private IEnumerator DoAttack()
    {
        isAttacking = true;
        canAttack = false;

        GameObject selectedZone = neutralAttackZone;

        if (inputDirection.y > 0.5f)
            selectedZone = upAttackZone;
        else if (inputDirection.y < -0.5f)
            selectedZone = downAttackZone;

        selectedZone.SetActive(true);

        yield return new WaitForSeconds(attackDuration);
        selectedZone.SetActive(false);

        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void EnableAttack(bool enabled)
    {
        attackEnabled = enabled;
    }
}
