using UnityEngine;
using UnityEngine.SceneManagement;

public class King : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Attack Settings")] // BAGIAN BARU
    public Transform attackPoint;      // Objek kosong di depan King sebagai pusat serangan
    public float attackRange = 0.5f;   // Jangkauan serangan
    public LayerMask enemyLayer;      // Layer khusus untuk Musuh (Skeleton/Enemy)
    public float attackDamage = 0.5f;       // Besar damage
    public float attackDuration = 0.4f; 
    private bool isAttacking = false;

    [Header("Respawn")]
    public Vector2 respawnPoint;
    public float fallLimitY = -10f;

    [Header("Map Boundary")]
    public float minX;
    public float maxX;

    [Header("Vertical Boundary")]
    public float maxY;

    private Rigidbody2D rb;
    public Animator anim;
    private bool isFacingRight = true;
    private bool isGrounded;

    private bool hasGroundedParam = false;
    private float moveInput;

    // [Header("Attack")]
    // public float attackDuration = 0.4f; 
    // private bool isAttacking = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        PlayerPrefs.SetInt("LastSceneIndex", SceneManager.GetActiveScene().buildIndex);
        respawnPoint = transform.position;

        if (rb == null)
        {
            Debug.LogError("[King] Rigidbody2D tidak ditemukan.");
            enabled = false;
            return;
        }

        if (groundCheck == null)
        {
            Debug.LogError("[King] groundCheck belum di-assign.");
            enabled = false;
            return;
        }

        if (anim != null)
        {
            foreach (var p in anim.parameters)
            {
                if (p.name == "Grounded" && p.type == AnimatorControllerParameterType.Bool)
                {
                    hasGroundedParam = true;
                    break;
                }
            }
        }

        if (anim == null)
        {
            Debug.LogError("GAWAT: Animator masih kosong!");
        }
        else
        {
            Debug.Log("AMAN: Animator terhubung ke: " + anim.gameObject.name);

            bool paramFound = false;
            foreach (var param in anim.parameters)
            {
                if (param.name == "1_Move") paramFound = true;
            }

            if (paramFound) Debug.Log("AMAN: Parameter 1_Move ditemukan.");
            else Debug.LogError("GAWAT: Parameter 1_Move tidak ditemukan!");
        }
    }

    void Update()
    {
        Move();
        CheckGround();
        Jump();
        Attack(); // Fungsi ini diperbarui di bawah
        CheckFall();
        ClampPosition();
        UpdateAnimations();
    }

    // void Move()
    // {
    //     if (isAttacking) return;

    //     moveInput = Input.GetAxisRaw("Horizontal");
    //     rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

    //     if (moveInput > 0 && !isFacingRight) Flip();
    //     else if (moveInput < 0 && isFacingRight) Flip();
    // }

    // void CheckGround()
    // {
    //     if (groundCheck == null) return;
    //     isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    // }

    // void Jump()
    // {
    //     if (!isGrounded || isAttacking) return;

    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    //         isGrounded = false;
    //     }
    // }

    void Attack()
    {
        if (isAttacking) return;

        if (Input.GetMouseButtonDown(0))
        {
            isAttacking = true;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (anim != null) anim.SetBool("2_Attack", true);

            // LOGIKA DAMAGE:
            // 1. Deteksi semua musuh di dalam jangkauan serangan
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

            // 2. Berikan damage ke setiap musuh yang kena
            foreach (Collider2D enemy in hitEnemies)
            {
                // Mencari script Health di musuh
                Health enemyHealth = enemy.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(attackDamage);
                    Debug.Log("King memukul " + enemy.name);
                }
            }

            StartCoroutine(ResetAttack());
        }
    }

    System.Collections.IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(attackDuration);
        if (anim != null) anim.SetBool("2_Attack", false);
        isAttacking = false;
    }

    // void CheckFall()
    // {
    //     if (transform.position.y < fallLimitY)
    //     {
    //         transform.position = respawnPoint;
    //         rb.linearVelocity = Vector2.zero;
    //     }
    // }

    // void Flip()
    // {
    //     isFacingRight = !isFacingRight;
    //     Vector3 s = transform.localScale;
    //     s.x *= -1;
    //     transform.localScale = s;
    // }

    // void ClampPosition()
    // {
    //     Vector3 pos = transform.position;

    //     pos.x = Mathf.Clamp(pos.x, minX, maxX);

    //     if (pos.y > maxY)
    //         pos.y = maxY;

    //     transform.position = pos;
    // }

    // void UpdateAnimations()
    // {
    //     if (anim == null) return;

    //     bool isWalking = moveInput != 0 && !isAttacking;
    //     anim.SetBool("1_Move", isWalking);

    //     bool isJumping = rb.linearVelocity.y > 0.1f && !isGrounded && !isAttacking;
    //     anim.SetBool("7_Jump", isJumping);

    //     if (hasGroundedParam)
    //         anim.SetBool("Grounded", isGrounded);
    // }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
    public void Respawn()
    {
        transform.position = respawnPoint;
        rb.linearVelocity = Vector2.zero;
        isAttacking = false; // Pastikan tidak terjebak dalam status menyerang
        
        // Aktifkan kembali input jika sebelumnya dimatikan
        this.enabled = true; 
        
        Debug.Log("King telah Respawn ke titik awal.");
    }

    // Update fungsi CheckFall agar menggunakan fungsi Respawn yang sama
void CheckFall()
    {
        if (transform.position.y < fallLimitY)
        {
            // Ambil komponen Health yang ada di objek ini
            Health playerHealth = GetComponent<Health>();

            if (playerHealth != null)
            {
                // Kurangi darah (misal: jatuh langsung mati atau kurangi 1-2 HP)
                // Jika ingin langsung mati, masukkan nilai besar (misal: 10 atau playerHealth.currentHealth)
                playerHealth.TakeDamage(99f); 
            }

            // Jika darah masih ada, baru respawn manual. 
            // Jika darah habis (0), script Health.cs yang akan menangani respawn via Die()
            if (playerHealth != null && playerHealth.currentHealth > 0)
            {
                Respawn();
            }
        }
    }

    void Move() { /* kode lamamu */ if (isAttacking) return; moveInput = Input.GetAxisRaw("Horizontal"); rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y); if (moveInput > 0 && !isFacingRight) Flip(); else if (moveInput < 0 && isFacingRight) Flip(); }
    void CheckGround() { if (groundCheck == null) return; isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer); }
    void Jump() { if (!isGrounded || isAttacking) return; if (Input.GetKeyDown(KeyCode.Space)) { rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); isGrounded = false; } }
    void Flip() { isFacingRight = !isFacingRight; Vector3 s = transform.localScale; s.x *= -1; transform.localScale = s; }
    void ClampPosition() { Vector3 pos = transform.position; pos.x = Mathf.Clamp(pos.x, minX, maxX); if (pos.y > maxY) pos.y = maxY; transform.position = pos; }
    void UpdateAnimations() { if (anim == null) return; bool isWalking = moveInput != 0 && !isAttacking; anim.SetBool("1_Move", isWalking); bool isJumping = rb.linearVelocity.y > 0.1f && !isGrounded && !isAttacking; anim.SetBool("7_Jump", isJumping); if (hasGroundedParam) anim.SetBool("Grounded", isGrounded); }

}

