using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel; // Seret PausePanel ke sini di Inspector
    public GameObject pauseButtonUI;
    public static bool isPaused = false;

    void Update()
    {
        // Tekan tombol 'Esc' untuk Pause/Resume
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
        pauseButtonUI.SetActive(false); // Sembunyikan tombol kecil
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        pauseButtonUI.SetActive(true); // Munculkan kembali tombol kecil
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Restart()
    {
        Time.timeScale = 1f; // Pastikan waktu jalan lagi sebelum reload
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Home"); // Ganti dengan nama scene menu kamu
    }
}