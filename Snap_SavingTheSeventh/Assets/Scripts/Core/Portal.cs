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

        if (notificationUI != null)
        {
            notificationUI.ShowNotification("Portal telah terbuka!");
        }

        // PLAY PORTAL UNLOCK SOUND
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
                StartCoroutine(FadeInPortal());
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
            
            Invoke(nameof(LoadNextScene), delayBeforeLoad);
        }
    }

    private IEnumerator FadeInPortal()
    {
        if (portalSprite == null) yield break;

        Color originalColor = portalSprite.color;
        float targetAlpha = 1f;

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