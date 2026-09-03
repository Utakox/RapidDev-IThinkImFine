using UnityEngine;

// 1 ตัวเลือก (ซ้าย หรือ ขวา) ของตัวละคร fix ไว้ตายตัวใน CharacterData
[System.Serializable]
public class ChoiceOptionData
{
    [TextArea] public string choiceText;
    public int sanityChange;

    [Tooltip("ผลต่อ Sanity ของ 'หมอ' (รวมทั้งเกม แยกจาก sanity คนไข้ด้านบน) ปล่อย 0 ถ้าไม่อยากให้ choice นี้กระทบหมอเลย")]
    public int doctorSanityChange;

    [Header("บทพูดพิเศษหลังเลือกอันนี้ (ไม่ใส่ก็ได้ ปล่อยว่างได้)")]
    public DialogueLine[] afterDialogue;
}