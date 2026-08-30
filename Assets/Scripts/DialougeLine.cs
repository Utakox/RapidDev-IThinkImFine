using UnityEngine;

// 1 บรรทัดบทพูด แค่ข้อความเฉยๆ ไม่มี choice ซ้อนอยู่ข้างใน
// (จุดสำคัญ: คลาสนี้ต้อง "ไม่" อ้างกลับไปหา ChoiceOptionData เด็ดขาด ไม่งั้นจะวนเป็นวงกลมอีก)
[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 5)]
    public string text;
}