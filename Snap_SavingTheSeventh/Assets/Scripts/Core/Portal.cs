using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private string namaSceneTujuan;
    
    [Header("Mission Settings")]
    public int coinsNeeded = 3;  // Target koin
    public int currentCoins = 0; // Koin yang sudah didapat (bisa dilihat di Inspector)

    // Fungsi ini dipanggil oleh coinCollectible
    public void AddCoin()
    {
        currentCoins++;
        Debug.Log("Portal mencatat: " + currentCoins + "/" + coinsNeeded + " Koin.");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (currentCoins >= coinsNeeded)
            {
                Debug.Log("Misi Selesai! Pindah Stage...");
                SceneManager.LoadScene(namaSceneTujuan);
            }
            else
            {
                int sisa = coinsNeeded - currentCoins;
                Debug.Log("Portal Masih Terkunci! Butuh " + sisa + " koin lagi.");
            }
        }
    }
}