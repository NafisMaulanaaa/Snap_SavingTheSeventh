using UnityEngine;
using System.Collections;

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
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackRange = 1.5f;
    private float cooldownTimer = 0;

    [Header("Boss Phase Settings")]
    [SerializeField] private float transformHealthPercent = 0.05f; // 5% HP untuk transform
    [SerializeField] private RuntimeAnimatorController phase2AnimatorController; // Animator untuk Monster form
    [SerializeField] private GameObject transformEffect; // Particle effect saat transform (optional)
    private bool isPhase2 = false;
    private bool hasTransformed = false;
    private float maxHealth; // Simpan max health di awal

    [Header("Skill Settings - Phase 1")]
    [SerializeField] private float skillCooldown = 8f; // Cooldown untuk skill
    [SerializeField] private GameObject sawPrefab;
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private GameObject poisonPrefab;
    [SerializeField] private Transform[] sawSpawnPoints;  // Titik spawn Saw (kiri & kanan)
    [SerializeField] private Transform[] spikeSpawnPoints; // Titik spawn Spike dari tanah
    [SerializeField] private Transform poisonSpawnArea;   // Area spawn Poison dari atas
    [SerializeField] private int poisonDropCount = 5;     // Jumlah poison yang jatuh Phase 1
    private float skillTimer = 0;

    [Header("Skill Settings - Phase 2")]
    [SerializeField] private int phase2PoisonCount = 10;  // Lebih banyak poison di Phase 2
    private int sawSpawnMultiplier = 1;  // Phase 1 = 1, Phase 2 = 2
    private int spikeSpawnMultiplier = 1;

    [Header("Components")]
    private Animator anim;
    private Rigidbody2D rb;
    private Transform player;
    private Health playerHealth;
    private Health bossHealth;

    private bool movingLeft;
    private float leftEdge;
    private float rightEdge;

    private enum BossState { Patrol, Chase, MeleeAttack, CastingSkill }
    private BossState currentState = BossState.Patrol;
    private bool isCasting = false;

    private void Awake()
    {
        leftEdge = transform.position.x - movementDistance;
        rightEdge = transform.position.x + movementDistance;
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bossHealth = GetComponent<Health>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = player.GetComponent<Health>();
        }

        if (bossHealth == null)
        {
            Debug.LogError("[Morgath] Health component tidak ditemukan!");
        }
        else
        {
            // Simpan max health di awal game
            maxHealth = bossHealth.currentHealth;
            Debug.Log($"[Morgath] Max Health: {maxHealth}");
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Cek apakah harus transform ke Phase 2
        CheckPhaseTransition();

        // Tambah timer
        cooldownTimer += Time.deltaTime;
        skillTimer += Time.deltaTime;

        // State machine logic
        if (!isCasting)
        {
            UpdateBossState();
        }
    }

    private void CheckPhaseTransition()
    {
        if (hasTransformed || bossHealth == null) return;

        // Hitung persentase HP
        float healthPercent = bossHealth.currentHealth / maxHealth;

        if (healthPercent <= transformHealthPercent && !isPhase2)
        {
            StartCoroutine(TransformToPhase2());
        }
    }

    private IEnumerator TransformToPhase2()
    {
        hasTransformed = true;
        isCasting = true;

        // Hentikan semua gerakan
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool("1_Move", false);

        Debug.Log("[MORGATH] TRANSFORMING TO MONSTER FORM!");

        // Spawn effect (optional)
        if (transformEffect != null)
        {
            Instantiate(transformEffect, transform.position, Quaternion.identity);
        }

        // Delay untuk animasi transform
        yield return new WaitForSeconds(2f);

        // Ganti Animator Controller
        if (phase2AnimatorController != null && anim != null)
        {
            anim.runtimeAnimatorController = phase2AnimatorController;
            Debug.Log("[Morgath] Animator diganti ke Monster form!");
        }

        // Aktifkan Phase 2
        isPhase2 = true;
        sawSpawnMultiplier = 2;
        spikeSpawnMultiplier = 2;

        // Buff stats (optional)
        chaseSpeed *= 1.3f;
        attackCooldown *= 0.8f;
        skillCooldown *= 0.7f;

        isCasting = false;
        Debug.Log("[MORGATH] PHASE 2 ACTIVATED!");
    }

    private void UpdateBossState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool isPlayerInBounds = player.position.x >= leftEdge && player.position.x <= rightEdge;

        // Prioritas skill casting
        if (skillTimer >= skillCooldown && isPlayerInBounds)
        {
            StartCoroutine(CastRandomSkill());
            return;
        }

        // Jika dekat dengan player
        if (distanceToPlayer < chaseDistance && isPlayerInBounds)
        {
            if (distanceToPlayer <= attackRange)
            {
                currentState = BossState.MeleeAttack;
                AttackLogic();
            }
            else
            {
                currentState = BossState.Chase;
                Chase();
            }
        }
        else
        {
            currentState = BossState.Patrol;
            Move();
        }
    }

    private IEnumerator CastRandomSkill()
    {
        isCasting = true;
        skillTimer = 0;

        // Hentikan gerakan
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool("1_Move", false);

        // Random skill (0 = Saw, 1 = Spike, 2 = Poison)
        int randomSkill = Random.Range(0, 3);

        Debug.Log($"[Morgath] Casting Skill {randomSkill}");

        // Delay casting animation (bisa tambahin trigger animasi "Cast" di sini)
        yield return new WaitForSeconds(0.8f);

        switch (randomSkill)
        {
            case 0:
                SpawnSaws();
                break;
            case 1:
                SpawnSpikes();
                break;
            case 2:
                SpawnPoisonRain();
                break;
        }

        yield return new WaitForSeconds(0.5f);
        isCasting = false;
    }

    private void SpawnSaws()
    {
        if (sawPrefab == null || sawSpawnPoints == null || sawSpawnPoints.Length == 0)
        {
            Debug.LogWarning("[Morgath] Saw Prefab atau Spawn Points belum di-set!");
            return;
        }

        int spawnCount = sawSpawnMultiplier; // Phase 1 = 1, Phase 2 = 2

        for (int i = 0; i < spawnCount; i++)
        {
            Transform spawnPoint = sawSpawnPoints[Random.Range(0, sawSpawnPoints.Length)];
            Instantiate(sawPrefab, spawnPoint.position, Quaternion.identity);
        }

        Debug.Log($"[Morgath] Spawned {spawnCount} Saw(s)!");
    }

    private void SpawnSpikes()
    {
        if (spikePrefab == null || spikeSpawnPoints == null || spikeSpawnPoints.Length == 0)
        {
            Debug.LogWarning("[Morgath] Spike Prefab atau Spawn Points belum di-set!");
            return;
        }

        int spawnCount = spikeSpawnMultiplier;

        for (int i = 0; i < spawnCount; i++)
        {
            Transform spawnPoint = spikeSpawnPoints[Random.Range(0, spikeSpawnPoints.Length)];
            Instantiate(spikePrefab, spawnPoint.position, Quaternion.identity);
        }

        Debug.Log($"[Morgath] Spawned {spawnCount} Spike(s)!");
    }

    private void SpawnPoisonRain()
    {
        if (poisonPrefab == null || poisonSpawnArea == null)
        {
            Debug.LogWarning("[Morgath] Poison Prefab atau Spawn Area belum di-set!");
            return;
        }

        int dropCount = isPhase2 ? phase2PoisonCount : poisonDropCount;

        for (int i = 0; i < dropCount; i++)
        {
            // Random posisi X di dalam area spawn
            float randomX = Random.Range(leftEdge, rightEdge);
            Vector3 spawnPos = new Vector3(randomX, poisonSpawnArea.position.y, 0);

            Instantiate(poisonPrefab, spawnPos, Quaternion.identity);
        }

        Debug.Log($"[Morgath] Spawned {dropCount} Poison Drops!");
    }

    private void AttackLogic()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (anim != null) anim.SetBool("1_Move", false);

        if (cooldownTimer >= attackCooldown)
        {
            cooldownTimer = 0;
            if (anim != null) anim.SetTrigger("2_Attack");

            if (playerHealth != null)
                playerHealth.TakeDamage(damage);
        }
    }

    private void Move()
    {
        if (movingLeft)
        {
            if (transform.position.x > leftEdge)
            {
                rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
                transform.localScale = new Vector3(2f, 2f, 2f);
            }
            else
            {
                movingLeft = false;
            }
        }
        else
        {
            if (transform.position.x < rightEdge)
            {
                rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
                transform.localScale = new Vector3(-2f, 2f, 2f);
            }
            else
            {
                movingLeft = true;
            }
        }

        if (anim != null) anim.SetBool("1_Move", true);
    }

    private void Chase()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);

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
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(leftEdge, transform.position.y, 0), new Vector3(rightEdge, transform.position.y, 0));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);

        if (poisonSpawnArea != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(poisonSpawnArea.position, 1f);
        }
    }

    // Public method untuk BossHealthBar bisa akses max health
    public float GetMaxHealth()
    {
        return maxHealth;
    }
}