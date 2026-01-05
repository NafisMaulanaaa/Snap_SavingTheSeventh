using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Portal : MonoBehaviour
{
    public static Portal Instance { get; private set; }

    [Header("UI Reference")]
    public CoinUI coinUI;
    [SerializeField] private NotificationUI notificationUI;

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
    
    [Header("Audio")]
    [SerializeField] private AudioClip portalUnlockSound;

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
        if (portalSprite != null)
        {
            Color c = portalSprite.color;
            c.a = 0f;
            portalSprite.color = c;
        }

        if (coinUI != null)
        {
            coinUI.UpdateScoreText(currentCoins, coinsNeeded);
        }
    }

    public void AddCoin()
    {
        currentCoins++;
        
        if (coinUI != null)
        {
            coinUI.UpdateScoreText(currentCoins, coinsNeeded);
        }

        if (currentCoins >= coinsNeeded && !isUnlocked)
        {
            UnlockPortal();
        }
    }

    private void UnlockPortal()
    {
        isUnlocked = true;
        Debug.Log("✨ PORTAL TERBUKA!");

        if (notificationUI != null)
        {
            notificationUI.ShowNotification("Portal telah terbuka!");
        }

        if (portalUnlockSound != null)
        {
            AudioSource.PlayClipAtPoint(portalUnlockSound, transform.position);
        }

        if (unlockEffect != null)
        {
            unlockEffect.Play();
        }

        if (portalSprite != null)
        {
            if (useFadeEffect)
            {
                StartCoroutine(FadeInPortalVisual());
            }
            else
            {
                Color c = portalSprite.color;
                c.a = 1f;
                portalSprite.color = c;
            }
        }
        
        if (autoLoadScene)
        {
            King playerScript = FindFirstObjectByType<King>(); 
            if (playerScript != null)
            {
                playerScript.StartAutoWalk(1f); 
            }
            
            // Beri jeda sedikit agar efek portal muncul dulu baru layar menghitam
            Invoke(nameof(LoadNextScene), delayBeforeLoad);
        }
    }

    // Coroutine untuk memunculkan sprite portal (Alpha 0 ke 1)
    private IEnumerator FadeInPortalVisual()
    {
        if (portalSprite == null) yield break;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            portalSprite.color = new Color(portalSprite.color.r, portalSprite.color.g, portalSprite.color.b, alpha);
            yield return null;
        }
    }

    // Fungsi utama untuk pindah scene dengan FadeManager
    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(namaSceneTujuan))
        {
            Debug.Log($"Memanggil FadeManager untuk ke: {namaSceneTujuan}");
            
            // Cek apakah FadeManager ada di scene
            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.LoadSceneWithFade(namaSceneTujuan);
            }
            else
            {
                // Jika FadeManager tidak ditemukan, pindah scene biasa agar tidak error
                SceneManager.LoadScene(namaSceneTujuan);
                Debug.LogWarning("FadeManager tidak ditemukan! Pindah scene tanpa efek.");
            }
        }
        else
        {
            Debug.LogError("Nama scene tujuan belum diisi di Inspector Portal!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Jika tidak autoLoad, pemain harus menyentuh portal yang sudah terbuka
        if (collision.CompareTag("Player") && isUnlocked && !autoLoadScene)
        {
            LoadNextScene();
        }
    }
}