using UnityEngine;

public class HealthCollectible : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float healthValue;

    [Tooltip("Peluang item ini memberikan damage (0 = Tidak Pernah, 1 = Selalu, 0.5 = 50:50)")]
    [Range(0f, 1f)] 
    [SerializeField] private float badLuckChance = 0.5f; // Default 50% untung, 50% buntung

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Health playerHealth = collision.GetComponent<Health>();

            // Pastikan komponen Health ditemukan
            if (playerHealth != null)
            {
                // Mengambil angka acak antara 0.0 sampai 1.0
                float randomRoll = Random.value;

                // Jika angka acak LEBIH KECIL dari badLuckChance, maka kena damage
                if (randomRoll < badLuckChance)
                {
                    // EFEK BURUK (Zonk)
                    // Saya asumsikan di script Health kamu ada method TakeDamage()
                    // (berdasarkan kode Skeleton yang kamu kirim sebelumnya)
                    playerHealth.TakeDamage(healthValue);
                    Debug.Log("Zonk! Item ini malah ngurangin darah.");
                }
                else
                {
                    // EFEK BAIK (Lucky)
                    playerHealth.AddHealth(healthValue);
                    Debug.Log("Beruntung! Darah bertambah.");
                }

                gameObject.SetActive(false);
            }
        }
    }
}