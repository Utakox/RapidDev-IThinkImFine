using UnityEngine;

// 1 บรรทัดบทพูด แค่ข้อความเฉยๆ ไม่มี choice ซ้อนอยู่ข้างใน
// (จุดสำคัญ: คลาสนี้ต้อง "ไม่" อ้างกลับไปหา ChoiceOptionData เด็ดขาด ไม่งั้นจะวนเป็นวงกลมอีก)
[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 5)]
    public string text;

    [Header("ความเร็วพิมพ์เฉพาะบรรทัดนี้ (0 = ใช้ค่า default ของ DialogueManager)")]
    public float typeSpeedOverride = 0f;

    [Header("หน่วงเวลาหลังพิมพ์จบเฉพาะบรรทัดนี้ ก่อนไปต่อ (ใส่ -1 = ใช้ค่า default ของ DialogueManager)")]
    public float delayAfterOverride = -1f;
}