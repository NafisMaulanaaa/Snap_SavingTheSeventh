using UnityEngine;
using TMPro; // Wajib ada untuk pakai TextMeshPro

public class CoinUI : MonoBehaviour
{
    private TextMeshProUGUI textMesh;

    private void Awake()
    {
        // Ambil komponen teks yang nempel di object ini
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    // Fungsi ini akan dipanggil oleh Portal
    public void UpdateScoreText(int current, int target)
    {
        // Mengubah tulisan jadi format "2/3" atau "1/5"
        textMesh.text = current + "/" + target;
    }
}