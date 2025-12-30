using UnityEngine;

public class CoinCollectible : MonoBehaviour
{
    [Header("Floating Animation")]
    [SerializeField] private float amplitude = 0.5f; 
    [SerializeField] private float frequency = 1f;

    [Header("Effects (Optional)")]
    [SerializeField] private ParticleSystem collectEffect;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // PAKAI SINGLETON (Lebih simple!)
            if (Portal.Instance != null)
            {
                Portal.Instance.AddCoin();

                if (collectEffect != null)
                {
                    Instantiate(collectEffect, transform.position, Quaternion.identity);
                }

                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("Portal.Instance NULL! Pastikan ada object Portal di scene.");
            }
        }
    }
}