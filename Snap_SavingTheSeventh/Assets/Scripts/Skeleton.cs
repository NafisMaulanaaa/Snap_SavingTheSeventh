using UnityEngine;

public class Skeleton : MonoBehaviour
{
    [Header("Target (Player)")]
    public Transform player;
    public float detectionRange = 5f;
    public float stopChaseRange = 7f;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3f;

    [Header("Patrol Points")]
    public Transform leftPoint;
    public Transform rightPoint;

    // PRIVATE
    private Rigidbody2D rb;
    private Animator anim;
    private bool facingRight = true;
    private bool isChasing = false;

    private float antiStuckTimer = 0f;
    private Vector2 lastPos;
    public float stuckCheckInterval = 1f;
    public float stuckMoveThreshold = 0.05f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // !!! FIX UTAMA (Animator SPUM selalu ada di child)
        anim = GetComponentInChildren<Animator>();

        lastPos = transform.position;
    }

    private void Update()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        // --- CHASE LOGIC ---
        if (!isChasing && dist < detectionRange)
            isChasing = true;

        if (isChasing && dist > stopChaseRange)
            isChasing = false;

        // RUN STATES
        if (isChasing)
            Chase();
        else
            Patrol();

        AntiStuckCheck();
    }

    // ==========================
    //         PATROL
    // ==========================
    void Patrol()
    {
        float speed = patrolSpeed;

        if (facingRight)
        {
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);

            if (transform.position.x >= rightPoint.position.x)
                Flip();
        }
        else
        {
            rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);

            if (transform.position.x <= leftPoint.position.x)
                Flip();
        }

        anim.SetBool("1_Move", true);
    }

    // ==========================
    //          CHASE
    // ==========================
    void Chase()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);

        if (dir > 0 && !facingRight) Flip();
        if (dir < 0 && facingRight) Flip();

        anim.SetBool("1_Move", true);
    }

    // ==========================
    //         FLIP SAFE
    // ==========================
    void Flip()
    {
        facingRight = !facingRight;

        // Flip aman — tidak geser posisi
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (facingRight ? 1 : -1);
        transform.localScale = s;
    }

    // ==========================
    //       ANTI—STUCK
    // ==========================
    void AntiStuckCheck()
    {
        antiStuckTimer += Time.deltaTime;

        if (antiStuckTimer >= stuckCheckInterval)
        {
            float moved = Vector2.Distance(lastPos, transform.position);

            if (moved < stuckMoveThreshold)
            {
                // Gerakin sedikit biar lepas
                float push = facingRight ? 2f : -2f;
                rb.AddForce(new Vector2(push, 0), ForceMode2D.Impulse);
            }

            lastPos = transform.position;
            antiStuckTimer = 0;
        }
    }
}
