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

        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0)
        {
            // Jika referensi anim belum di-update, dia akan panggil animasi Human
            if (anim != null) anim.SetTrigger("3_Damaged");
        }
        else
        {
            Die();
        }
    }

    private void Die()
    {
        if (dead) return;

        // --- LOGIC KHUSUS BOSS (MORGATH) ---
        Morgath bossScript = GetComponent<Morgath>();
        
        if (bossScript != null)
        {
            if (!bossScript.IsPhase2Activated())
            {
                return; 
            }
        }
        // -----------------------------------

        dead = true;

        if (anim != null) anim.SetTrigger("4_Death");

        King playerScript = GetComponent<King>();
        if (playerScript != null)
        {
            Invoke("GoToGameOverScene", 1f);
        }
        else
        {
            if (bossScript != null) bossScript.enabled = false;
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