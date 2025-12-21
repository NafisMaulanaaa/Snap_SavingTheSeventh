using UnityEngine;

public class HealthCollectible : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float healthValue;

    [Tooltip("Peluang item ini memberikan damage (0 = Tidak Pernah, 1 = Selalu, 0.5 = 50:50)")]
    [Range(0f, 1f)] 
    [SerializeField] private float badLuckChance = 0.5f;

    [Header("Floating Animation")]
    [SerializeField] private float amplitude = 0.5f; // Seberapa tinggi naiknya
    [SerializeField] private float frequency = 1f;   // Seberapa cepat gerakannya

    private Vector3 startPosition;

    private void Start()
    {
        // Simpan posisi awal item saat game dimulai
        startPosition = transform.position;
    }

    private void Update()
    {
        // Membuat efek melayang naik turun
        // Rumus: Posisi Awal + (Sinus dari waktu * kecepatan) * tinggi
        float newY = startPosition.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Health playerHealth = collision.GetComponent<Health>();

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

                gameObject.SetActive(false);
            }
        }
    }
}