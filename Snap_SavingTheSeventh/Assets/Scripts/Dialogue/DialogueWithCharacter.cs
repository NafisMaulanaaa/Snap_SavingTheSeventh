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
    
    [Header("Next Dialogue")]
    public GameObject nextDialogBox;

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
            EndDialogue();
        }
    }
    
    void EndDialogue()
    {
        gameObject.SetActive(false);
        
        if (nextDialogBox != null)
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