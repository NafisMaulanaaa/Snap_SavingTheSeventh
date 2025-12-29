using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public void RestartLevel()
    {
        // Mengambil index scene terakhir yang disimpan sebelum mati
        int lastLevelIndex = PlayerPrefs.GetInt("LastSceneIndex", 0);
        SceneManager.LoadScene(lastLevelIndex);
    }

    public void BackToMainMenu()
    {
        // Jika kamu punya scene menu, ganti namanya di sini
        SceneManager.LoadScene("Home");
    }
}