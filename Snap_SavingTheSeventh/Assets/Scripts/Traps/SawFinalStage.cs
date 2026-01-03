using UnityEngine;

public class SawFinalStage : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 5f; // Hancur otomatis setelah 5 detik (biar gak menuhin memori)
    private int direction = -1; // -1 Kiri, 1 Kanan

    private void Start()
    {
        // Otomatis hancur kalau kelamaan (misal udah keluar map)
        Destroy(gameObject, lifeTime);
    }

    public void Setup(int dir)
    {
        direction = dir;
        
        // Membalik gambar gergaji sesuai arah (Opsional)
        // Kalau gergajinya muter, mungkin gak perlu ini.
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (dir == 1 ? 1 : -1); 
        transform.localScale = scale;
    }

    private void Update()
    {
        // Logic Jalan: Gerak ke sumbu X sesuai arah & kecepatan
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Ambil script Health player & kasih damage
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10); // Masukkan damage yang sesuai
            }
            
            // Hancurkan gergaji setelah kena player (Opsional, atau biarkan tembus)
            // Destroy(gameObject); 
        }
        else if (collision.gameObject.CompareTag("Wall")) // Pastikan Tembok punya tag "Wall"
        {
            Destroy(gameObject); // Hancur kalau kena tembok pinggir map
        }
    }
}