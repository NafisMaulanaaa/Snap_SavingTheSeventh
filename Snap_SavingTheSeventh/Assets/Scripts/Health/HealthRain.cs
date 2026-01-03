using UnityEngine;

public class HealthRain : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float healthValue = 10f; // Kasih nilai default biar aman

    [Tooltip("Peluang item ini memberikan damage (0 = Tidak Pernah, 1 = Selalu, 0.5 = 50:50)")]
    [Range(0f, 1f)]
    [SerializeField] private float badLuckChance = 0.5f;

    // Bagian Start & Update boleh dihapus kalau tidak dipakai floating-nya
    // private Vector3 startPosition;
    // private void Start() { startPosition = transform.position; }

    // PERBAIKAN DI SINI:
    // Gunakan 'Collision2D', bukan 'Collider2D'
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Harus pakai .gameObject dulu baru bisa cek Tag
        if (collision.gameObject.CompareTag("Player"))
        {
            // Pakai .gameObject dulu baru GetComponent
            Health playerHealth = collision.gameObject.GetComponent<Health>();

            if (playerHealth != null)
            {
                float randomRoll = Random.value;

                if (randomRoll < badLuckChance)
                {
                    playerHealth.TakeDamage(healthValue);
                    Debug.Log("Zonk! Item ini malah ngurangin darah.");
                }
                else
                {
                    playerHealth.AddHealth(healthValue);
                    Debug.Log("Beruntung! Darah bertambah.");
                }

                // Matikan/Hancurkan item setelah diambil
                // Gunakan Destroy() kalau mau hilang permanen, atau SetActive(false) kalau mau di-pool
                Destroy(gameObject); 
            }
        }
    }
}