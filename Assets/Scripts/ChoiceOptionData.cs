using UnityEngine;

// 1 ตัวเลือก (ซ้าย หรือ ขวา) ของตัวละคร fix ไว้ตายตัวใน CharacterData
[System.Serializable]
public class ChoiceOptionData
{
    [TextArea] public string choiceText;
    public int sanityChange;

    [Header("บทพูดพิเศษหลังเลือกอันนี้ (ไม่ใส่ก็ได้ ปล่อยว่างได้)")]
    public DialogueLine[] afterDialogue;
}