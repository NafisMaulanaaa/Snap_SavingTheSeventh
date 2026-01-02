using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health bossHealth;           // Reference ke script Health milik Morgath
    [SerializeField] private Morgath morgathScript;       // Reference ke script Morgath untuk ambil max health
    [SerializeField] private Image healthBarFill;         // Image di dalam Mask (HealthBarFrame)
    [SerializeField] private Text healthText;             // Text "HP: 100/100"

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.red;
    [SerializeField] private Color lowHealthColor = Color.yellow;      // Warna pas HP < 30%
    [SerializeField] private Color criticalColor = new Color(1f, 0.5f, 0f); // Orange pas HP < 10%

    [Header("Animation")]
    [SerializeField] private float smoothSpeed = 5f;      // Kecepatan animasi HP turun
    private float targetFillAmount;

    [Header("Show/Hide")]
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private GameObject healthBarGroup;   // Parent MorgathHealthBar untuk show/hide

    private float maxHealth;                              // Simpan max HP saat Start
    private RectTransform fillRectTransform;              // Untuk manipulasi width
    private float maxWidth;                               // Lebar maksimal HP bar

    private void Start()
    {
        if (bossHealth == null)
        {
            Debug.LogError("[BossHealthBar] Boss Health belum di-assign di Inspector!");
            return;
        }

        if (healthBarFill == null)
        {
            Debug.LogError("[BossHealthBar] Health Bar Fill belum di-assign di Inspector!");
            return;
        }

        // Ambil max health dari Morgath script atau langsung dari current health
        if (morgathScript != null)
        {
            maxHealth = morgathScript.GetMaxHealth();
        }
        else
        {
            maxHealth = bossHealth.currentHealth;
        }

        Debug.Log($"[BossHealthBar] Max Health: {maxHealth}");

        // Ambil RectTransform dari Fill image untuk manipulasi width
        fillRectTransform = healthBarFill.GetComponent<RectTransform>();
        if (fillRectTransform != null)
        {
            maxWidth = fillRectTransform.sizeDelta.x;
        }

        // Hide HP bar di awal jika penuh
        if (hideWhenFull && healthBarGroup != null)
        {
            healthBarGroup.SetActive(false);
        }

        // Set initial value
        UpdateHealthBar();
    }

    private void Update()
    {
        if (bossHealth == null || healthBarFill == null) return;

        // Update HP bar setiap frame dengan smooth animation
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        // Hitung persentase HP (0.0 - 1.0)
        float healthPercent = bossHealth.currentHealth / maxHealth;
        targetFillAmount = Mathf.Clamp01(healthPercent);

        // Smooth transition menggunakan fillAmount
        healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);

        // Update warna berdasarkan HP
        UpdateHealthColor(healthPercent);

        // Update text HP (optional)
        if (healthText != null)
        {
            healthText.text = $"HP: {Mathf.Ceil(bossHealth.currentHealth)} / {Mathf.Ceil(maxHealth)}";
        }

        // Show/Hide logic
        if (hideWhenFull && healthBarGroup != null)
        {
            // Tampilkan HP bar saat boss kena damage pertama kali
            if (healthPercent < 1f && !healthBarGroup.activeSelf)
            {
                healthBarGroup.SetActive(true);
                Debug.Log("[BossHealthBar] HP Bar muncul! Boss kena damage.");
            }

            // Hide saat boss mati
            if (bossHealth.currentHealth <= 0)
            {
                healthBarGroup.SetActive(false);
            }
        }
    }

    private void UpdateHealthColor(float healthPercent)
    {
        // Ubah warna HP bar berdasarkan persentase
        if (healthPercent > 0.3f)
        {
            healthBarFill.color = normalColor; // Merah normal
        }
        else if (healthPercent > 0.1f)
        {
            healthBarFill.color = lowHealthColor; // Kuning (low HP)
        }
        else
        {
            healthBarFill.color = criticalColor; // Orange (critical)
        }
    }

    // Method untuk di-call dari luar (optional)
    public void ShowHealthBar()
    {
        if (healthBarGroup != null)
        {
            healthBarGroup.SetActive(true);
        }
    }

    public void HideHealthBar()
    {
        if (healthBarGroup != null)
        {
            healthBarGroup.SetActive(false);
        }
    }

    // Method untuk manual set max health jika perlu
    public void SetMaxHealth(float max)
    {
        maxHealth = max;
    }

    // Method untuk force update (misal pas transform phase)
    public void ForceUpdate()
    {
        if (bossHealth != null && healthBarFill != null)
        {
            float healthPercent = bossHealth.currentHealth / maxHealth;
            healthBarFill.fillAmount = healthPercent;
        }
    }
}