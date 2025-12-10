using UnityEngine;

public class Skeleton_1 : MonoBehaviour
{
    [Header("Target (Player)")]
    public Transform target; 
    public float detectionRange = 5f;
    public float attackRange = 1.2f;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float patrolDistance = 3f;
    private float leftPoint;
    private float rightPoint;
    private bool movingRight = true;

    [Header("Attack")]
    public float attackCooldown = 1f;
    private float attackTimer = 0f;

    private Rigidbody2D rb;
    private Animator anim;

    [Header("Visual (UnitRoot)")]
    public Transform visual;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();

        // Patroli dimulai dari posisi pertama skeleton
        leftPoint = transform.position.x - patrolDistance;
        rightPoint = transform.position.x + patrolDistance;
    }

    void Update()
    {
        attackTimer -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange)
        {
            ChasePlayer();
        }
        else if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        anim.SetBool("1_Move", true);

        if (movingRight)
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            visual.localScale = new Vector3(1, 1, 1);

            if (transform.position.x >= rightPoint)
                movingRight = false;
        }
        else
        {
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
            visual.localScale = new Vector3(-1, 1, 1);

            if (transform.position.x <= leftPoint)
                movingRight = true;
        }
    }

    void ChasePlayer()
    {
        anim.SetBool("1_Move", true);

        if (target.position.x > transform.position.x)
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            visual.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
            visual.localScale = new Vector3(-1, 1, 1);
        }
    }

    void AttackPlayer()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (attackTimer <= 0)
        {
            anim.SetBool("2_Attack", true);
            attackTimer = attackCooldown;
        }
        else
        {
            anim.SetBool("2_Attack", false);
        }
    }
}
