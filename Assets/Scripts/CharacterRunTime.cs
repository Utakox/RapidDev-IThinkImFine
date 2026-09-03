using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterRuntime : MonoBehaviour
{
    public CharacterData data;

    [Header("(ไม่ใส่ก็ได้) TMP โชว์ค่า Sanity ตอนเทส")]
    public TextMeshProUGUI sanityText;

    [Header("Effect (GameObject ในซีน) ตอนเข้า Mental State ของตัวละครนี้")]
    [Tooltip("เว้นว่าง = ใช้ Default Mental State Effects ของ DialogueManager\n(ต้องไว้ตรงนี้ ไม่ใช่ใน CharacterData เพราะ ScriptableObject ลาก GameObject ในซีนใส่ไม่ได้)")]
    public GameObject[] mentalStateEffectOverride;

    public int Sanity { get; private set; }

    // ไล่ index ว่าตอนนี้ choice ปกติ (goodChoices/badChoices) ไปถึงคู่ที่เท่าไหร่แล้ว
    // ใช้แทนการสุ่ม: รอบที่ N จะจับคู่ goodChoices[N] กับ badChoices[N] เสมอ
    public int NormalChoiceIndex { get; private set; }

    public void AdvanceNormalChoiceIndex() => NormalChoiceIndex++;

    private Image faceImage;
    private bool initialized;

    private readonly HashSet<ChoiceOptionData> usedChoices = new HashSet<ChoiceOptionData>();
    private readonly HashSet<SanityDialogueTrigger> triggeredSanityDialogues = new HashSet<SanityDialogueTrigger>();

    private void Awake() => Init();

    // เรียก Init จาก Awake และเผื่อถูกเปิดใช้งานทีหลัง (ลำดับ Awake ระหว่างสคริปต์ไม่การันตี)
    private void Init()
    {
        if (initialized) return;

        if (data == null)
        {
            Debug.LogError($"[CharacterRuntime] {name} ยังไม่ได้ลาก CharacterData ใส่");
            return;
        }

        faceImage = GetComponent<Image>();
        Sanity = Mathf.Clamp(data.startingSanity, 0, 100);
        UpdateFace();
        UpdateSanityText();
        initialized = true;
    }

    public void ChangeSanity(int amount)
    {
        Sanity = Mathf.Clamp(Sanity + amount, 0, 100);
        UpdateFace();
        UpdateSanityText();
    }

    private void UpdateFace()
    {
        if (faceImage == null || data == null) return;

        if (Sanity > 75)      faceImage.sprite = data.faceHigh;    // 76-100
        else if (Sanity > 50) faceImage.sprite = data.faceMid;     // 51-75
        else if (Sanity > 25) faceImage.sprite = data.faceLow;     // 26-50
        else                  faceImage.sprite = data.faceBroken;  // 0-25
    }

    private void UpdateSanityText()
    {
        if (sanityText == null) return;
        sanityText.text = $"Sanity: {Sanity}";
    }

    public bool HasUsedChoice(ChoiceOptionData choice) => choice != null && usedChoices.Contains(choice);
    public void MarkChoiceUsed(ChoiceOptionData choice) { if (choice != null) usedChoices.Add(choice); }
    public bool HasTriggeredSanityDialogue(SanityDialogueTrigger t) => t != null && triggeredSanityDialogues.Contains(t);
    public void MarkSanityDialogueTriggered(SanityDialogueTrigger t) { if (t != null) triggeredSanityDialogues.Add(t); }
}