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

    [Header("Attack Settings")] 
    public Transform attackPoint;      
    public float attackRange = 0.5f;   
    public LayerMask enemyLayer;      
    public float attackDamage = 0.5f;      
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
    private bool isCutscene = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        PlayerPrefs.SetInt("LastSceneIndex", SceneManager.GetActiveScene().buildIndex);
        respawnPoint = transform.position;

        // =========================================================
        // LOGIKA BARU: TAMBAH NYAWA KHUSUS DI STAGE BOSS
        // =========================================================
        string sceneName = SceneManager.GetActiveScene().name;
        
        // Pastikan nama scenenya SAMA PERSIS dengan di Unity (huruf besar/kecil berpengaruh)
        if (sceneName == "Stage_vs_boss") 
        {
            Health myHealth = GetComponent<Health>();
            if (myHealth != null)
            {
                // Set nyawa jadi 5
                myHealth.currentHealth = 5f; 
                Debug.Log("⚔️ Memasuki Boss Stage: Nyawa King di-set menjadi 5 (Fill 0.5)");
                
                // CATATAN PENTING:
                // Pastikan script 'Health.cs' kamu punya MaxHealth = 10 agar fill amount jadi 0.5 (5/10).
                // Jika script Health kamu punya fungsi manual update UI, panggil disini.
                // Contoh: myHealth.UpdateHealthUI(); 
            }
            else
            {
                Debug.LogError("[King] Script Health tidak ditemukan di Player!");
            }
        }
        // =========================================================

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
    }

    void Update()
    {
        Move();
        CheckGround();
        Jump();

        if (!isCutscene) 
        {
            Attack();
        }

        CheckFall();
        ClampPosition();
        UpdateAnimations();
    }

    void Attack()
    {
        if (isAttacking) return;

        if (Input.GetMouseButtonDown(0))
        {
            isAttacking = true;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (anim != null) anim.SetBool("2_Attack", true);

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

            foreach (Collider2D enemy in hitEnemies)
            {
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

    public void Respawn()
    {
        transform.position = respawnPoint;
        rb.linearVelocity = Vector2.zero;
        isAttacking = false; 
        
        this.enabled = true; 
        
        Debug.Log("King telah Respawn ke titik awal.");
    }

    void CheckFall()
    {
        if (transform.position.y < fallLimitY)
        {
            Health playerHealth = GetComponent<Health>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(99f); 
            }

            if (playerHealth != null && playerHealth.currentHealth > 0)
            {
                Respawn();
            }
        }
    }

    void Move() 
    { 
        if (isAttacking) return; 
        if (!isCutscene)
        {
            moveInput = Input.GetAxisRaw("Horizontal");
        }
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y); 
        if (moveInput > 0 && !isFacingRight) Flip(); 
        else if (moveInput < 0 && isFacingRight) Flip(); 
    }

    void CheckGround() { if (groundCheck == null) return; isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer); }
    void Jump() { if (!isGrounded || isAttacking) return; if (Input.GetKeyDown(KeyCode.Space)) { rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); isGrounded = false; } }
    void Flip() { isFacingRight = !isFacingRight; Vector3 s = transform.localScale; s.x *= -1; transform.localScale = s; }
    void ClampPosition() { Vector3 pos = transform.position; pos.x = Mathf.Clamp(pos.x, minX, maxX); if (pos.y > maxY) pos.y = maxY; transform.position = pos; }
    void UpdateAnimations() { if (anim == null) return; bool isWalking = moveInput != 0 && !isAttacking; anim.SetBool("1_Move", isWalking); bool isJumping = rb.linearVelocity.y > 0.1f && !isGrounded && !isAttacking; anim.SetBool("7_Jump", isJumping); if (hasGroundedParam) anim.SetBool("Grounded", isGrounded); }

    public void StartAutoWalk(float direction)
    {
        isCutscene = true;      
        moveInput = direction;  
        Debug.Log("Player mulai jalan sendiri...");
    }
}