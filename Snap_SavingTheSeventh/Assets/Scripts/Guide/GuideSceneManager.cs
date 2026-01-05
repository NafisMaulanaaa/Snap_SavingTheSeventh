using UnityEngine;
using UnityEngine.SceneManagement;

public class GuideSceneManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName;

    private bool isTransitioning = false;

    void Update()
    {
        // Jika pemain klik kiri mouse / tap layar
        if (!isTransitioning && Input.GetMouseButtonDown(0))
        {
            StartNextScene();
        }
    }

    private void StartNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            isTransitioning = true;

            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.LoadSceneWithFade(nextSceneName);
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}