using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Morgath : MonoBehaviour
{
    [Header("Transition Settings (Wajib Diisi)")]
    [SerializeField] private CanvasGroup whiteFlashUI; 
    [SerializeField] private GameObject humanModel;    
    [SerializeField] private GameObject monsterModel;  
    [SerializeField] private BossHealthBar bossHealthBarScript; 
    [SerializeField] private float fadeDuration = 1.0f; 

    [Header("Phase 2 Hitbox Settings (CAPSULE)")]
    [SerializeField] private Vector2 monsterColliderSize;   // Ukuran Capsule saat jadi Monster
    [SerializeField] private Vector2 monsterColliderOffset; // Posisi Capsule saat jadi Monster

    [Header("Scale Settings")]
    [SerializeField] private float humanScale = 2f;    
    [SerializeField] private float monsterScale = 3.5f; 
    private float currentScale; 

    [Header("Boss Stats (HP)")]
    [SerializeField] private float humanMaxHealth = 25f;   
    [SerializeField] private float monsterMaxHealth = 30f; 

    [Header("Movement Settings")]
    [SerializeField] private float movementDistance;
    [SerializeField] private float speed;
    [SerializeField] private float damage;

    [Header("Chase Settings")]
    [SerializeField] private float chaseDistance = 5f;
    [SerializeField] private float chaseSpeed = 4f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 3f; 
    [SerializeField] private float attackRange = 1.5f;
    private float cooldownTimer = 0;

    [Header("Boss Phase Settings")]
    [SerializeField] private float transformHealthPercent = 0.3f; 
    [SerializeField] private GameObject transformEffect; 
    private bool isPhase2 = false;
    private bool hasTransformed = false;
    private float maxHealth;

    [Header("Skill Settings - Phase 1")]
    [SerializeField] private float skillCooldown = 8f;
    [SerializeField] private GameObject sawPrefab;
    [SerializeField] private GameObject poisonPrefab;

    [Header("Spawn area/points")]
    [SerializeField] private float sawSpawnYOffset = 0.5f;
    [SerializeField] private Transform arenaLeftPoint;
    [SerializeField] private Transform arenaRightPoint;
    [SerializeField] private Transform poisonSpawnArea;
    [SerializeField] private int poisonDropCount = 5;
    private float skillTimer = 0;

    [Header("Skill Settings - Phase 2")]
    [SerializeField] private int phase2PoisonCount = 10;
    private int sawSpawnMultiplier = 1;

    [Header("RNG System (Deck)")]
    private List<int> skillDeck = new List<int>(); 

    [Header("Components")]
    private Animator anim;
    private Rigidbody2D rb;
    private Transform player;
    private Health playerHealth;
    private Health bossHealth;
    
    private SpriteRenderer[] spumSprites; 

    private bool movingLeft;
    private float leftEdge;
    private float rightEdge;

    private enum BossState { Patrol, Chase, MeleeAttack, CastingSkill }
    [SerializeField] private BossState currentState = BossState.Patrol;
    private bool isCasting = false;

    private void Awake()
    {
        leftEdge = transform.position.x - movementDistance;
        rightEdge = transform.position.x + movementDistance;
        
        // Set Scale Awal (Human)
        currentScale = humanScale;
        transform.localScale = new Vector3(currentScale, currentScale, currentScale);

        // Cari Animator di Human
        if (humanModel != null)
            anim = humanModel.GetComponentInChildren<Animator>();
        
        rb = GetComponent<Rigidbody2D>();
        bossHealth = GetComponent<Health>();

        if (humanModel != null)
            spumSprites = humanModel.GetComponentsInChildren<SpriteRenderer>();
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
            // SETUP AWAL: Set HP sesuai Human (25)
            bossHealth.UpgradeMaxHealth(humanMaxHealth);
            maxHealth = humanMaxHealth;
            
            // UI Update Awal
            if (bossHealthBarScript != null)
            {
                bossHealthBarScript.SetMaxHealth(humanMaxHealth);
                bossHealthBarScript.ForceUpdate();
            }

            if (monsterModel != null) monsterModel.SetActive(false);
            if (humanModel != null) humanModel.SetActive(true);
        }

        ShuffleDeck();
    }

    private void Update()
    {
        if (player == null) return;

        CheckPhaseTransition();

        cooldownTimer += Time.deltaTime;
        skillTimer += Time.deltaTime;

        if (!isCasting)
        {
            UpdateBossState();
        }
    }

    private void ShuffleDeck()
    {
        skillDeck.Clear();
        skillDeck.Add(0); skillDeck.Add(0); skillDeck.Add(0); // 3x Saw
        skillDeck.Add(1); // 1x Poison

        for (int i = 0; i < skillDeck.Count; i++)
        {
            int temp = skillDeck[i];
            int randomIndex = Random.Range(i, skillDeck.Count);
            skillDeck[i] = skillDeck[randomIndex];
            skillDeck[randomIndex] = temp;
        }
    }

    private void CheckPhaseTransition()
    {
        if (hasTransformed || bossHealth == null) return;
        float healthPercent = bossHealth.currentHealth / maxHealth;

        // Berubah saat HP <= 30% atau HP 0 (Safety net)
        if ((healthPercent <= transformHealthPercent || bossHealth.currentHealth <= 0) && !isPhase2)
        {
            if (bossHealth.currentHealth <= 0) bossHealth.currentHealth = 1; 
            StartCoroutine(TransformToPhase2());
        }
    }

    private IEnumerator TransformToPhase2()
    {
        hasTransformed = true;
        isCasting = true;
        
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool("1_Move", false);
        
        Debug.Log("[MORGATH] CAHAYA ILAHI (TRANSISI)!");

        // 1. FADE IN
        if (whiteFlashUI != null)
        {
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                whiteFlashUI.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }
            whiteFlashUI.alpha = 1f; 
        }

        // 2. TUKAR MODEL
        if (humanModel != null) humanModel.SetActive(false); 
        if (monsterModel != null) monsterModel.SetActive(true);

        // --- UPDATE SCALE JADI MONSTER ---
        currentScale = monsterScale;
        float facingDirection = Mathf.Sign(transform.localScale.x);
        transform.localScale = new Vector3(facingDirection * currentScale, currentScale, currentScale);

        // 3. UPDATE REFERENCE & COLLIDER (MODIFIED FOR CAPSULE)
        if (monsterModel != null)
        {
            anim = monsterModel.GetComponentInChildren<Animator>(); 
            spumSprites = monsterModel.GetComponentsInChildren<SpriteRenderer>(); 
            
            // ============================================
            // PERBAIKAN UTAMA: PRIORITASKAN CAPSULE COLLIDER
            // ============================================
            CapsuleCollider2D capCol = GetComponent<CapsuleCollider2D>();
            
            if (capCol != null)
            {
                // Simpan size lama untuk debug
                Vector2 oldSize = capCol.size;
                
                // Set ukuran baru dari Inspector
                capCol.size = monsterColliderSize;    
                capCol.offset = monsterColliderOffset; 

                Debug.Log($"[Morgath] Capsule Collider Updated! Old: {oldSize} -> New: {capCol.size}");
            }
            else
            {
                // Fallback ke Box Collider jika Capsule tidak ditemukan (Hanya jaga-jaga)
                BoxCollider2D col = GetComponent<BoxCollider2D>();
                if (col != null)
                {
                    col.size = monsterColliderSize;    
                    col.offset = monsterColliderOffset; 
                    Debug.Log("[Morgath] Warning: CapsuleCollider2D tidak ditemukan, menggunakan BoxCollider2D.");
                }
                else
                {
                    Debug.LogError("[Morgath] FATAL: Tidak ada Collider 2D yang ditemukan di Boss!");
                }
            }
            // ============================================
        }

        if (bossHealth != null)
        {
            // Update Max Health
            bossHealth.UpgradeMaxHealth(monsterMaxHealth);
            
            // PENTING: Refresh Animator agar Health tau kalau model sudah ganti
            bossHealth.RefreshAnimator(); 
        }

        if (transformEffect != null)
            Instantiate(transformEffect, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);

        // --- UPDATE HP MONSTER (25 -> 30) ---
        bossHealth.UpgradeMaxHealth(monsterMaxHealth); // Set Max HP jadi 30 & isi full
        maxHealth = monsterMaxHealth;

        // Update UI
        if (bossHealthBarScript != null)
        {
            bossHealthBarScript.SetMaxHealth(monsterMaxHealth);
            bossHealthBarScript.ForceUpdate();
        }
        
        Debug.Log($"[Morgath] HP Upgraded to {monsterMaxHealth}");

        isPhase2 = true;
        sawSpawnMultiplier = 2;
        chaseSpeed *= 1.3f;
        attackCooldown *= 0.8f;
        skillCooldown *= 0.7f;

        // 4. FADE OUT
        if (whiteFlashUI != null)
        {
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                whiteFlashUI.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                yield return null;
            }
            whiteFlashUI.alpha = 0f;
        }

        isCasting = false;
        Debug.Log("[MORGATH] PHASE 2 START!");
    }

    private void UpdateBossState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (skillTimer >= skillCooldown)
        {
            StartCoroutine(CastRandomSkill());
            return;
        }

        if (distanceToPlayer < chaseDistance)
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

        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool("1_Move", false);

        if (skillDeck.Count == 0) ShuffleDeck();

        int selectedSkill = skillDeck[0];
        skillDeck.RemoveAt(0);

        if (selectedSkill == 0)
        {
            StartCoroutine(PlayWarningFlash());
        }

        yield return new WaitForSeconds(0.8f);

        switch (selectedSkill)
        {
            case 0: SpawnSaws(); break;
            case 1: SpawnPoisonRain(); break;
        }

        yield return new WaitForSeconds(0.5f);
        isCasting = false;
    }

    private IEnumerator PlayWarningFlash()
    {
        for (int i = 0; i < 3; i++)
        {
            SetSpumColor(Color.red);
            yield return new WaitForSeconds(0.1f);
            SetSpumColor(Color.white);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void SetSpumColor(Color color)
    {
        if (spumSprites != null)
        {
            foreach (SpriteRenderer sr in spumSprites)
            {
                if (sr != null) sr.color = color;
            }
        }
    }

    private void SpawnSaws()
    {
        if (sawPrefab == null) return;
        int facingDir = transform.localScale.x > 0 ? -1 : 1;
        float spawnX = transform.position.x + (facingDir * 1.5f);
        float spawnY = transform.position.y + sawSpawnYOffset;
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);

        GameObject saw = Instantiate(sawPrefab, spawnPos, Quaternion.identity);
        SawFinalStage sawScript = saw.GetComponent<SawFinalStage>();
        if (sawScript != null) sawScript.Setup(facingDir);
    }

    private void SpawnPoisonRain()
    {
        if (poisonPrefab == null) return;
        int dropCount = isPhase2 ? phase2PoisonCount : poisonDropCount;

        for (int i = 0; i < dropCount; i++)
        {
            float randomX = Random.Range(arenaLeftPoint.position.x, arenaRightPoint.position.x);
            Vector3 spawnPos = new Vector3(randomX, poisonSpawnArea.position.y, 0);
            Instantiate(poisonPrefab, spawnPos, Quaternion.identity);
        }
    }

    private void AttackLogic()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (anim != null) anim.SetBool("1_Move", false);

        if (cooldownTimer >= attackCooldown)
        {
            cooldownTimer = 0;
            if (anim != null) anim.SetTrigger("2_Attack");
            if (playerHealth != null) playerHealth.TakeDamage(damage);
        }
    }

    private void Move()
    {
        if (movingLeft)
        {
            if (transform.position.x > leftEdge)
            {
                rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
                transform.localScale = new Vector3(currentScale, currentScale, currentScale);
            }
            else movingLeft = false;
        }
        else
        {
            if (transform.position.x < rightEdge)
            {
                rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
                transform.localScale = new Vector3(-currentScale, currentScale, currentScale);
            }
            else movingLeft = true;
        }
        if (anim != null) anim.SetBool("1_Move", true);
    }

    private void Chase()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);
        
        transform.localScale = new Vector3(dir < 0 ? currentScale : -currentScale, currentScale, currentScale);
        
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
        if (poisonSpawnArea != null) {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(poisonSpawnArea.position, 1f);
        }
    }

    public float GetMaxHealth() { return maxHealth; }
    public bool IsPhase2Activated() { return isPhase2; }
}