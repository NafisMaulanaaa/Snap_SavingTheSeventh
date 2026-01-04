using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dialogue_1 : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed;
    
    [Header("Next Action")]
    public GameObject nextDialogBox; // Untuk dialog berikutnya
    public bool loadSceneAfterDialogue = false; // Centang ini kalau mau pindah scene
    public string sceneToLoad; // Nama scene yang mau di-load
    public float delayBeforeTransition = 0.5f; // Delay sebelum transisi

    private int index;

    void Start()
    {
        textComponent.text = string.Empty;
        StartDialogue();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }

    void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            StartCoroutine(EndDialogue());
        }
    }
    
    IEnumerator EndDialogue()
    {
        yield return new WaitForSeconds(delayBeforeTransition);
        
        gameObject.SetActive(false);
        
        // Kalau mau pindah scene
        if (loadSceneAfterDialogue && !string.IsNullOrEmpty(sceneToLoad))
        {
            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.LoadSceneWithFade(sceneToLoad);
            }
            else
            {
                Debug.LogWarning("FadeManager tidak ditemukan! Loading scene tanpa fade.");
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
            }
        }
        // Kalau mau lanjut ke dialog berikutnya
        else if (nextDialogBox != null)
        {
            nextDialogBox.SetActive(true);
        }
    }
}