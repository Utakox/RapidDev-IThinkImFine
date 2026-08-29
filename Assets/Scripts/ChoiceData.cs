using UnityEngine;

[CreateAssetMenu(fileName = "NewChoice", menuName = "VisualNovel/Choice")]
public class ChoiceData : ScriptableObject
{
    [Header("ข้อความที่โชว์ให้ผู้เล่นเลือก")]
    [TextArea] public string choiceText;

    [Header("ผลกระทบต่อ Sanity")]
    public int sanityChange; // ค่าที่จะบวก/ลบ กำหนดทีหลังได้ในแต่ละ choice

    [Header("ผลกระทบอื่นๆ ในอนาคต (ถ้ามี)")]
    // ตรงนี้เผื่อไว้ ถ้าอนาคตมี stat อื่นเพิ่ม เช่น affection, trust
    // public int affectionChange;
    // public int trustChange;

    [Header("ไปที่ node/scene ไหนต่อ")]
    public string nextNodeID; // เผื่อระบบ dialogue tree
}