using UnityEngine;

public class King : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Respawn")]
    public Vector2 respawnPoint;
    public float fallLimitY = -10f;

    [Header("Map Boundary")]
    public float minX;
    public float maxX;

    private Rigidbody2D rb;
    public Animator anim;
    private bool isFacingRight = true;
    private bool isGrounded;

    // Cek apakah Animator punya parameter "Grounded"
    private bool hasGroundedParam = false;
    private float moveInput;

    void Awake()
    {
        // ambil komponen di Awake
        rb = GetComponent<Rigidbody2D>();
        // anim = GetComponent<Animator>();
    }

    void Start()
    {
        respawnPoint = transform.position;

        // VALIDASI DEPENDENCY — jika ada yang null, beri pesan jelas dan matikan skrip
        if (rb == null)
        {
            Debug.LogError("[King] Rigidbody2D tidak ditemukan di GameObject ini. Tambahkan Rigidbody2D dan pastikan 'Body Type' = Dynamic.");
            enabled = false;
            return;
        }

        if (groundCheck == null)
        {
            Debug.LogError("[King] groundCheck belum di-assign di Inspector. Buat child empty object sebagai groundCheck dan drag ke field ini.");
            enabled = false;
            return;
        }

        // if (anim == null)
        // {
        //     Debug.LogWarning("[King] Animator tidak ditemukan — animasi akan dinonaktifkan. Jika ingin animasi, tambahkan Animator dan AnimatorController.");
        //     // tidak `return` — kita masih bisa jalan tanpa anim
        // }

        // Cek apakah Animator punya parameter "Grounded" jika anim ada
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
            Debug.LogError("GAWAT: Animator masih kosong! Kamu belum drag UnitRoot ke slot Anim di Script King.");
        }
        else
        {
            Debug.Log("AMAN: Animator sudah terhubung ke: " + anim.gameObject.name);
            
            // Cek apakah parameter 1_Move ada
            bool paramFound = false;
            foreach(var param in anim.parameters)
            {
                if(param.name == "1_Move") paramFound = true;
            }

            if(paramFound) Debug.Log("AMAN: Parameter 1_Move DITEMUKAN.");
            else Debug.LogError("GAWAT: Parameter 1_Move TIDAK DITEMUKAN di Animator ini!");
        }
    }

    void Update()
    {
        // kalau skrip dinonaktifkan di Start() karena error, Update gak akan dipanggil
        Move();
        CheckGround();
        Jump();
        CheckFall();
        ClampPosition();
        UpdateAnimations();
    }

    void Move()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (moveInput > 0 && !isFacingRight) Flip();
        else if (moveInput < 0 && isFacingRight) Flip();
    }

    void CheckGround()
    {
        // safety: pastikan groundCheck tidak null (seharusnya sudah divalidasi di Start)
        if (groundCheck == null) return;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void Jump()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }
    }

    void CheckFall()
    {
        if (transform.position.y < fallLimitY)
        {
            transform.position = respawnPoint;
            rb.linearVelocity = Vector2.zero;
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        transform.position = pos;
    }

    void UpdateAnimations()
    {
        if (anim == null) return;

        // anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));

        bool isWalking = moveInput != 0;
        anim.SetBool("1_Move", isWalking);

        bool isJumping = rb.linearVelocity.y > 0.1f && !isGrounded;
        anim.SetBool("7_Jump", isJumping);

        if (hasGroundedParam)
            anim.SetBool("Grounded", isGrounded);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}