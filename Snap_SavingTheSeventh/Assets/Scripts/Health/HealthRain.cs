using UnityEngine;

public class HealthRain : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float healthValue = 10f;

    [Tooltip("Peluang item ini memberikan damage (0 = Tidak Pernah, 1 = Selalu, 0.5 = 50:50)")]
    [Range(0f, 1f)]
    [SerializeField] private float badLuckChance = 0.5f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
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

                if (collectSound != null)
                {
                    AudioSource.PlayClipAtPoint(collectSound, transform.position);
                }

                Destroy(gameObject);
            }
        }
    }
}