using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Portal : MonoBehaviour
{
    public static Portal Instance { get; private set; }

    [Header("UI Reference")]
    public CoinUI coinUI;
    [SerializeField] private NotificationUI notificationUI; // TAMBAH INI

    [Header("Scene Settings")]
    [SerializeField] private string namaSceneTujuan;
    
    [Header("Mission Settings")]
    public int coinsNeeded = 3;
    public int currentCoins = 0;

    [Header("Portal Visual")]
    [SerializeField] private SpriteRenderer portalSprite;
    [SerializeField] private bool useFadeEffect = true;
    [SerializeField] private float fadeInDuration = 0.5f;

    [Header("Effects (Optional)")]
    [SerializeField] private ParticleSystem unlockEffect;
    [SerializeField] private bool autoLoadScene = true;
    [SerializeField] private float delayBeforeLoad = 1.5f;

    private bool isUnlocked = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Ada lebih dari 1 Portal di scene!");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Sembunyikan sprite (transparan)
        if (portalSprite != null)
        {
            Color c = portalSprite.color;
            c.a = 0f;
            portalSprite.color = c;
        }
        else
        {
            Debug.LogWarning("Portal Sprite belum di-assign!");
        }

        if (coinUI != null)
        {
            coinUI.UpdateScoreText(currentCoins, coinsNeeded);
        }

        Debug.Log($"Portal butuh {coinsNeeded} koin.");
    }

    public void AddCoin()
    {
        currentCoins++;
        
        if (coinUI != null)
        {
            coinUI.UpdateScoreText(currentCoins, coinsNeeded);
        }

        Debug.Log($"Koin: {currentCoins}/{coinsNeeded}");

        if (currentCoins >= coinsNeeded && !isUnlocked)
        {
            UnlockPortal();
        }
    }

    private void UnlockPortal()
    {
        isUnlocked = true;
        Debug.Log("✨ PORTAL TERBUKA!");

        // TAMPILKAN NOTIFIKASI!
        if (notificationUI != null)
        {
            notificationUI.ShowNotification("Portal telah terbuka!");
        }

        if (unlockEffect != null)
        {
            unlockEffect.Play();
        }

        if (portalSprite != null)
        {
            if (useFadeEffect)
            {
                StartCoroutine(FadeInPortal());
            }
            else
            {
                // Langsung muncul
                Color c = portalSprite.color;
                c.a = 1f;
                portalSprite.color = c;
                
            }
        }
        
        if (autoLoadScene)
        {
            // Cari script King di scene
            // (Ganti 'King' dengan nama script playermu kalau beda)
            King playerScript = FindFirstObjectByType<King>(); 
            
            if (playerScript != null)
            {
                // Suruh dia jalan ke KANAN (1f). Kalau mau ke kiri ganti jadi (-1f)
                playerScript.StartAutoWalk(1f); 
            }
            
            Invoke(nameof(LoadNextScene), delayBeforeLoad);
        }
    }

    private IEnumerator FadeInPortal()
    {
        if (portalSprite == null) yield break;

        Color originalColor = portalSprite.color;
        float targetAlpha = 1f; // Selalu fade ke alpha 1

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, targetAlpha, elapsed / fadeInDuration);
            Color newColor = originalColor;
            newColor.a = alpha;
            portalSprite.color = newColor;
            yield return null;
        }

        // Pastikan full alpha
        Color finalColor = originalColor;
        finalColor.a = targetAlpha;
        portalSprite.color = finalColor;

        if (autoLoadScene)
        {
            yield return new WaitForSeconds(delayBeforeLoad);
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(namaSceneTujuan))
        {
            Debug.Log($"Loading: {namaSceneTujuan}");
            SceneManager.LoadScene(namaSceneTujuan);
        }
        else
        {
            Debug.LogError("Nama scene kosong!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isUnlocked && !autoLoadScene)
        {
            LoadNextScene();
        }
    }
}