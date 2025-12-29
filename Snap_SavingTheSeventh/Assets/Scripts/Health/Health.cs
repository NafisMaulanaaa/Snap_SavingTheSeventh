using UnityEngine;
using UnityEngine.SceneManagement;  

public class Health : MonoBehaviour
{
    [SerializeField] private float startingHealth;
    [SerializeField] private float destroyDelay = 2f;
    public float currentHealth { get; private set; }
    private Animator anim;
    private bool dead;

    private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(float _damage)
    {
        if (dead) return; // Jika sudah mati, tidak bisa kena damage lagi

        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0)
        {
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
        dead = true;

        if (anim != null) anim.SetTrigger("4_Death");

        // CEK: Jika ini adalah Player (King), jangan di-Destroy, tapi Respawn
        King playerScript = GetComponent<King>();
        if (playerScript != null)
        {
            Invoke("GoToGameOverScene", 1f);
        }
        else
        {
            // Jika ini musuh (Skeleton), jalankan logika mati seperti biasa
            // if (GetComponent<Skeleton>() != null) GetComponent<Skeleton>().enabled = false;
            // if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;
            // if (GetComponent<Rigidbody2D>() != null) GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

            Destroy(gameObject, destroyDelay);
        }
    }

    void GoToGameOverScene()
{
    SceneManager.LoadScene("GameOver");
}

    // Coroutine baru untuk menangani Respawn Player
    private System.Collections.IEnumerator RespawnPlayer(King player)
    {
        // Tunggu sebentar agar animasi mati terlihat
        yield return new WaitForSeconds(destroyDelay);

        // Reset status kesehatan
        dead = false;
        currentHealth = startingHealth;

        // Reset Animasi (kembali ke Idle)
        if (anim != null) anim.Play("Idle"); // Pastikan nama state Idle-mu sesuai

        // Panggil fungsi Respawn yang ada di script King
        player.Respawn();
    }

    public void AddHealth(float _value)
    {
        currentHealth = Mathf.Clamp(currentHealth + _value, 0, startingHealth);
    }
}