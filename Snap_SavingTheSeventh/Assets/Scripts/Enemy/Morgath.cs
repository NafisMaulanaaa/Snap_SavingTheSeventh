using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Morgath : MonoBehaviour
{
    [Header("Transition Settings (Wajib Diisi)")]
    [SerializeField] private CanvasGroup whiteFlashUI; 
    [SerializeField] private GameObject humanModel;    
    [SerializeField] private GameObject monsterModel;  
    [SerializeField] private BossHealthBar bossHealthBarScript; 
    [SerializeField] private float fadeDuration = 1.0f; 

    [Header("Phase 2 Hitbox Settings (CAPSULE)")]
    [SerializeField] private Vector2 monsterColliderSize;
    [SerializeField] private Vector2 monsterColliderOffset;

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

    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] humanVoiceClips;      // Array suara human (bisa banyak)
    [SerializeField] private AudioClip[] monsterVoiceClips;    // Array suara monster (bisa banyak)
    [SerializeField] private AudioClip transformSound;          // Sound pas transform
    [SerializeField] private float voiceCooldownMin = 7f;      // Minimal jeda suara
    [SerializeField] private float voiceCooldownMax = 7f;      // Maksimal jeda suara
    private AudioSource audioSource;
    private float voiceTimer = 0f;
    private float nextVoiceTime = 0f;

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

    [Header("Death Settings")]
    [SerializeField] private string nextSceneName; 
    [SerializeField] private float deathDelay = 2.0f;
    private bool isDead = false;

    // Tambahkan variabel ini di bagian private atas
    private bool isInvulnerable = false; 

    // Tambahkan fungsi publik untuk mengecek status kebal
    public bool IsInvulnerable() { return isInvulnerable; }

    private void Awake()
    {
        leftEdge = transform.position.x - movementDistance;
        rightEdge = transform.position.x + movementDistance;
        
        currentScale = humanScale;
        transform.localScale = new Vector3(currentScale, currentScale, currentScale);

        if (humanModel != null)
            anim = humanModel.GetComponentInChildren<Animator>();
        
        rb = GetComponent<Rigidbody2D>();
        bossHealth = GetComponent<Health>();

        if (humanModel != null)
            spumSprites = humanModel.GetComponentsInChildren<SpriteRenderer>();
        
        // Setup Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        
        // Set jeda suara pertama
        nextVoiceTime = Random.Range(voiceCooldownMin, voiceCooldownMax);
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
            bossHealth.UpgradeMaxHealth(humanMaxHealth);
            maxHealth = humanMaxHealth;
            
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
        if (player == null || isDead) return;

        // Cek jika HP Boss habis
        if (bossHealth != null && bossHealth.currentHealth <= 0)
        {
            StartCoroutine(HandleBossDeath());
            return; 
        }

        CheckPhaseTransition();

        cooldownTimer += Time.deltaTime;
        skillTimer += Time.deltaTime;
        voiceTimer += Time.deltaTime;
        
        // Play voice setiap 3-4 detik
        if (voiceTimer >= 7f) 
        {
            PlayRandomVoice();
            voiceTimer = 0f;
        }

        if (!isCasting)
        {
            UpdateBossState();
        }
    }

    private IEnumerator HandleBossDeath()
    {
        isDead = true; // Kunci agar tidak terpanggil berulang kali
        isInvulnerable = true;
        
        Debug.Log("Morgath Mati. Memulai proses delay...");

        // 1. Hentikan semua gerakan fisik
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Berhenti seketika
            rb.bodyType = RigidbodyType2D.Kinematic; // Agar tidak terpengaruh gravitasi/dorongan
        }

        // 2. Samakan Trigger Animasi dengan Health.cs
        // Kamu pakai "4_Death" di script Health, jadi pakai itu juga di sini
        if (anim != null) anim.SetTrigger("4_Death");

        // 3. Matikan Collider (Opsional tapi disarankan)
        // Agar Player bisa lewat/tidak menabrak mayat Boss selama delay
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 4. Tunggu selama delay (Misal 2 detik)
        yield return new WaitForSeconds(deathDelay);

        if (monsterModel != null) monsterModel.SetActive(false);

        // 5. Pindah ke scene selanjutnya
        if (!string.IsNullOrEmpty(nextSceneName))
    {
        if (FadeManager.Instance != null)
            {
                FadeManager.Instance.LoadSceneWithFade(nextSceneName);
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }

        Destroy(gameObject, 1f);
    }

    private void PlayRandomVoice()
    {
        AudioClip[] voiceClips = isPhase2 ? monsterVoiceClips : humanVoiceClips;
        
        if (voiceClips != null && voiceClips.Length > 0 && audioSource != null)
        {
            // Pilih random voice dari array
            int randomIndex = Random.Range(0, voiceClips.Length);
            AudioClip selectedVoice = voiceClips[randomIndex];
            
            if (selectedVoice != null)
            {
                audioSource.PlayOneShot(selectedVoice);
            }
        }
    }

    private void ShuffleDeck()
    {
        skillDeck.Clear();
        skillDeck.Add(0); skillDeck.Add(0); skillDeck.Add(0);
        skillDeck.Add(1);

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
        isInvulnerable = true;
        
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool("1_Move", false);
        
        Debug.Log("[MORGATH] CAHAYA ILAHI (TRANSISI)!");
        
        // PLAY TRANSFORM SOUND
        if (transformSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(transformSound);
        }

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

        currentScale = monsterScale;
        float facingDirection = Mathf.Sign(transform.localScale.x);
        transform.localScale = new Vector3(facingDirection * currentScale, currentScale, currentScale);

        // 3. UPDATE REFERENCE & COLLIDER
        if (monsterModel != null)
        {
            anim = monsterModel.GetComponentInChildren<Animator>(); 
            spumSprites = monsterModel.GetComponentsInChildren<SpriteRenderer>(); 
            
            CapsuleCollider2D capCol = GetComponent<CapsuleCollider2D>();
            
            if (capCol != null)
            {
                Vector2 oldSize = capCol.size;
                capCol.size = monsterColliderSize;    
                capCol.offset = monsterColliderOffset; 
                Debug.Log($"[Morgath] Capsule Collider Updated! Old: {oldSize} -> New: {capCol.size}");
            }
            else
            {
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
        }

        if (bossHealth != null)
        {
            bossHealth.UpgradeMaxHealth(monsterMaxHealth);
            bossHealth.RefreshAnimator(); 
        }

        if (transformEffect != null)
            Instantiate(transformEffect, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);

        bossHealth.UpgradeMaxHealth(monsterMaxHealth);
        maxHealth = monsterMaxHealth;

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

        isInvulnerable = false;
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