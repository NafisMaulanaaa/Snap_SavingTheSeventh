using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [SerializeField] private float startingHealth;
    [SerializeField] private float destroyDelay = 2f;

    public float currentHealth { get; set; } 
    
    private Animator anim;
    private bool dead;

    private void Awake()
    {
        currentHealth = startingHealth;
        // Ini mengambil Animator Human saat awal game
        anim = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(float _damage)
    {
        if (dead) return;

        Morgath boss = GetComponent<Morgath>();
        if (boss != null && boss.IsInvulnerable()) 
        {
            Debug.Log("Boss sedang kebal!");
            return; // Keluar dari fungsi, damage tidak masuk
        }

        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0)
        {
            // Jika referensi anim belum di-update, dia akan panggil animasi Human
            if (anim != null) anim.SetTrigger("3_Damaged");
            
            // PLAY HIT SOUND
            King king = GetComponent<King>();
            if (king != null)
            {
                king.PlayHitSound();
            }
        }
        else
        {
            Die();
        }
    }

    private void Die()
    {
        if (dead) return;

        Morgath bossScript = GetComponent<Morgath>();
        
        // --- LOGIC KHUSUS BOSS (MORGATH) ---
        // Cegah boss mati di Phase 1 (saat transisi ke Phase 2)
        if (bossScript != null && !bossScript.IsPhase2Activated())
        {
            return; 
        }

        dead = true;

        // Gunakan parameter animasi yang konsisten. 
        // Jika di Animator kamu namanya "4_Death", pakai itu.
        if (anim != null) anim.SetTrigger("4_Death");
        
        // Suara kematian (jika ada script King/Audio)
        King king = GetComponent<King>();
        if (king != null) king.PlayDeathSound();

        // LOGIC PERPINDAHAN SCENE
        if (king != null && gameObject.CompareTag("Player")) 
        {
            // Jika yang mati adalah Player
            Invoke("GoToGameOverScene", 1f);
        }
        else if (bossScript != null)
        {
            // JIKA INI BOSS: 
            // Kita DIAMKAN saja di sini. 
            // Biarkan Coroutine 'HandleBossDeath' di script Morgath yang bekerja
            // untuk menghitung delay 1.5 detik lalu pindah scene.
            Debug.Log("[Health] Morgath tewas. Menunggu script Morgath untuk pindah scene...");
        }
        else
        {
            // JIKA INI MUSUH BIASA (BUKAN BOSS / BUKAN PLAYER)
            Destroy(gameObject, destroyDelay);
        }
    }

    void GoToGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }

    public void AddHealth(float _value)
    {
        currentHealth = Mathf.Clamp(currentHealth + _value, 0, startingHealth);
    }

    public void UpgradeMaxHealth(float newMaxHealth)
    {
        startingHealth = newMaxHealth; 
        currentHealth = newMaxHealth;  
    }

    // ================================================================
    // TAMBAHAN PENTING: Panggil fungsi ini dari script Morgath!
    // ================================================================
    public void RefreshAnimator()
    {
        // Cari ulang Animator di anak-anak object (sekarang Monster yang aktif)
        anim = GetComponentInChildren<Animator>();
        Debug.Log("[Health] Animator reference updated to: " + (anim != null ? anim.name : "null"));
    }
}