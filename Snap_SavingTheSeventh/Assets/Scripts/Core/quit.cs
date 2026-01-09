using UnityEngine;
using System.Collections;

public class quit : MonoBehaviour
{
    public void KeluarGame()
    {
        StartCoroutine(QuitRoutine());
    }

    private IEnumerator QuitRoutine()
    {
        Debug.Log("Memulai Fade Out sebelum Quit...");
        
        // Panggil FadeManager milikmu
        if (FadeManager.Instance != null)
        {
            yield return StartCoroutine(FadeManager.Instance.FadeOut());
        }

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}