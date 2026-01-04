using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueWithCharacter : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI textComponent;
    public TextMeshProUGUI characterNameText; // Text untuk nama
    public Image characterImage; // Image untuk avatar/sprite
    
    [Header("Dialogue Data")]
    public DialogueLine[] dialogueLines;
    public float textSpeed;
    
    [Header("Next Action")]
    public GameObject nextDialogBox; // Untuk dialog berikutnya
    public bool loadSceneAfterDialogue = false; // Centang ini kalau mau pindah scene
    public string sceneToLoad; // Nama scene yang mau di-load
    public float delayBeforeTransition = 0.5f; // Delay sebelum transisi
    
    [Header("Audio Control")]
    public bool stopCutsceneAudio = false; // Centang ini kalau mau stop cutscene audio

    private int index;

    void OnEnable()
    {
        // Reset saat dialog aktif
        textComponent.text = string.Empty;
        StartDialogue();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (textComponent.text == dialogueLines[index].line)
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = dialogueLines[index].line;
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
        DialogueLine currentLine = dialogueLines[index];
        
        // Update nama karakter
        if (characterNameText != null)
        {
            characterNameText.text = currentLine.characterName;
        }
        
        // Update sprite karakter
        if (characterImage != null && currentLine.characterSprite != null)
        {
            characterImage.sprite = currentLine.characterSprite;
            characterImage.enabled = true;
        }
        else if (characterImage != null)
        {
            characterImage.enabled = false; // Sembunyikan kalau tidak ada sprite
        }
        
        textComponent.text = string.Empty;
        
        foreach (char c in currentLine.line.ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < dialogueLines.Length - 1)
        {
            index++;
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
        
        // Stop cutscene audio kalau dicentang
        if (stopCutsceneAudio && CutsceneAudioManager.Instance != null)
        {
            CutsceneAudioManager.Instance.StopAndDestroy();
        }
        
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

// Class untuk setiap line dialog
[System.Serializable]
public class DialogueLine
{
    public string characterName;
    public Sprite characterSprite;
    [TextArea(2, 5)]
    public string line;
}