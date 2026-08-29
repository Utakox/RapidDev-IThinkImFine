using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;

    [Header("Typing Settings")]
    [Tooltip("วินาทีต่อ 1 ตัวอักษร ยิ่งน้อยยิ่งไว")]
    [SerializeField] private float defaultTypeSpeed = 0.03f;

    private bool isTyping = false;
    private DialogueLine currentLine;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    // เรียกจากภายนอกเพื่อเริ่ม dialogue
    public void StartDialogue(DialogueLine line)
    {
        DisplayLine(line);
    }

    // ต้องเป็น public เพราะ ChoiceManager เรียกกลับมาหลัง choice ถูกยืนยัน
    public void DisplayLine(DialogueLine line)
    {
        if (line == null)
        {
            EndDialogue();
            return;
        }

        currentLine = line;

        ChoiceManager.Instance.HideBothChoices(); // ซ่อน choice ไว้ก่อนเสมอตอนเริ่มบรรทัดใหม่
        speakerNameText.text = line.speakerName;

        float speed = line.customTypeSpeed > 0f ? line.customTypeSpeed : defaultTypeSpeed;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(line.text, speed));
    }

    // Coroutine พิมพ์ทีละตัวอักษร — ผู้เล่น "ข้ามไม่ได้" เพราะไม่มีการเช็ค input ระหว่างพิมพ์เลย
    private IEnumerator TypeText(string fullText, float speed)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(speed);
        }

        isTyping = false;

        yield return new WaitForSeconds(currentLine.delayBeforeNext);

        ProceedAfterTyping();
    }

    private void ProceedAfterTyping()
    {
        if (currentLine.hasChoices)
        {
            ChoiceManager.Instance.ShowChoices();
        }
        else if (currentLine.nextLine != null)
        {
            DisplayLine(currentLine.nextLine);
        }
        else
        {
            EndDialogue();
        }
    }

    // เรียกจาก ChoiceManager.OnChoiceConfirmed() หลังผู้เล่นชี้ค้างครบเวลา
    public void ContinueAfterChoice()
    {
        DisplayLine(currentLine.nextLine);
    }

    private void EndDialogue()
    {
        Debug.Log("Dialogue จบแล้ว");
    }
}