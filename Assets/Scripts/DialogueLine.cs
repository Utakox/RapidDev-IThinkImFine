using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueLine", menuName = "VisualNovel/DialogueLine")]
public class DialogueLine : ScriptableObject
{
    [Header("ตัวละครที่พูด")]
    public string speakerName;

    [Header("เนื้อหา")]
    [TextArea(3, 10)] public string text;

    [Header("ความเร็วพิมพ์เฉพาะบรรทัดนี้ (0 = ใช้ค่า default)")]
    public float customTypeSpeed = 0f;

    [Header("หน่วงเวลาก่อนไปบรรทัดถัดไป (วินาที)")]
    public float delayBeforeNext = 1f;

    [Header("บรรทัดนี้พิมพ์จบแล้ว ให้โชว์ choice สุ่มไหม")]
    public bool hasChoices = false;

    [Header("ไปบรรทัดไหนต่อ (ใช้ทั้งกรณีมี/ไม่มี choice)")]
    public DialogueLine nextLine;
}