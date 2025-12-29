using UnityEngine;

public class CoinCollectible : MonoBehaviour
{
    [Header("Floating Animation")]
    [SerializeField] private float amplitude = 0.5f; 
    [SerializeField] private float frequency = 1f;   

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
            // PERUBAHAN DI SINI: Kita cari object "Portal"
            Portal portalScript = FindFirstObjectByType<Portal>();

            if (portalScript != null)
            {
                // Lapor ke portal
                portalScript.AddCoin();
                
                // Hancurkan koin
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("GAWAT: Tidak ada object Portal di scene ini!");
            }
        }
    }
}