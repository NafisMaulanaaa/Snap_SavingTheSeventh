using UnityEngine;

public class Morgath : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float movementDistance;
    [SerializeField] private float speed;
    [SerializeField] private float damage;

    [Header("Chase Settings")]
    [SerializeField] private float chaseDistance = 5f;
    [SerializeField] private float chaseSpeed = 4f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 1.5f; // Jeda antar serangan
    [SerializeField] private float attackRange = 1.5f;   // Jarak minimal untuk mulai memukul
    private float cooldownTimer = 0;

    [Header("Components")]
    private Animator anim;
    private Rigidbody2D rb;
    private Transform player;
    private Health playerHealth;

    private bool movingLeft;
    private float leftEdge;
    private float rightEdge;

    private void Awake()
    {
        leftEdge = transform.position.x - movementDistance;
        rightEdge = transform.position.x + movementDistance;
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = player.GetComponent<Health>();
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Tambah timer setiap frame
        cooldownTimer += Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool isPlayerInBounds = player.position.x >= leftEdge && player.position.x <= rightEdge;

        if (distanceToPlayer < chaseDistance && isPlayerInBounds)
        {
            // Jika sangat dekat dengan player, BERHENTI dan SERANG
            if (distanceToPlayer <= attackRange)
            {
                AttackLogic();
            }
            else
            {
                Chase();
            }
        }
        else
        {
            Move();
        }
    }

    private void AttackLogic()
    {
        // Hentikan gerak saat menyerang
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (anim != null) anim.SetBool("1_Move", false);

        // Cek apakah cooldown sudah selesai
        if (cooldownTimer >= attackCooldown)
        {
            cooldownTimer = 0;
            if (anim != null) anim.SetTrigger("2_Attack");
            
            // Memberikan damage (Bisa juga dipanggil via Animation Event: DamagePlayer)
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);
        }
    }

    private void Move() // PATROL
    {
  // Logika Patroli menggunakan Velocity (bukan transform.position)
        if (movingLeft)
        {
            if (transform.position.x > leftEdge)
            {
                // Gerak ke Kiri
                rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
                transform.localScale = new Vector3(2f, 2f, 2f);
            }
            else
            {
                movingLeft = false; // Putar balik ke Kanan
            }
        }
        else
        {
            if (transform.position.x < rightEdge)
            {
                // Gerak ke Kanan
                rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
                transform.localScale = new Vector3(-2f, 2f, 2f);
            }
            else
            {
                movingLeft = true; // Putar balik ke Kiri
            }
        }

        if (anim != null) anim.SetBool("1_Move", true);
    }

    private void Chase() // MENGEJAR
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);

        // Batasi pengejaran agar tidak melewati batas patroli
        if ((dir < 0 && transform.position.x <= leftEdge) || (dir > 0 && transform.position.x >= rightEdge))
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (anim != null) anim.SetBool("1_Move", false);
            return;
        }

        rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);
        transform.localScale = new Vector3(dir < 0 ? 2f : -2f, 2f, 2f);
        if (anim != null) anim.SetBool("1_Move", true);
    }

    private void OnDrawGizmos()
    {
        // Gizmos Area Patroli
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(leftEdge, transform.position.y, 0), new Vector3(rightEdge, transform.position.y, 0));
        
        // Gizmos Jarak Serang (Attack Range)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}