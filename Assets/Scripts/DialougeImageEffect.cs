using UnityEngine;
using UnityEngine.UI;

// ติดกับ Object กลางที่คุมกลุ่ม Image effect ของ dialogue ทั้งหมด
// เตรียม Image slot ไว้ล่วงหน้าใน scene กี่อันก็ได้ (เช่นลูกของ dialogue panel) แล้วลากใส่ imageSlots ตามลำดับ
// ตอนโชว์ จะเอา sprite จาก DialogueLine.effectImages ใส่ slot ตามลำดับ ที่เหลือ (ไม่ได้ใช้) จะถูกซ่อน
public class DialogueImageEffect : MonoBehaviour
{
    public static DialogueImageEffect Instance;

    [Header("ลาก Image slot ที่เตรียมไว้ใน scene ใส่ตามลำดับ (จำนวน slot = จำนวนรูปสูงสุดที่โชว์พร้อมกันได้ต่อบรรทัด)")]
    public Image[] imageSlots;

    private void Awake()
    {
        Instance = this;
        HideAll();
    }

    // เรียกจาก DialogueManager ทุกครั้งที่ขึ้นบรรทัดใหม่ ส่ง sprite array ของบรรทัดนั้นมา (null/ว่าง = ไม่โชว์อะไรเลย)
    public void ShowImages(Sprite[] sprites)
    {
        HideAll();

        if (sprites == null) return;

        for (int i = 0; i < sprites.Length && i < imageSlots.Length; i++)
        {
            if (sprites[i] == null) continue;

            imageSlots[i].sprite = sprites[i];
            imageSlots[i].enabled = true;
        }
    }

    // เรียกจาก DialogueManager ตอนเคลียร์จอ (เปลี่ยนบรรทัด/เข้าสู่หน้า choice/จบตา)
    public void HideAll()
    {
        if (imageSlots == null) return;

        foreach (var slot in imageSlots)
        {
            if (slot == null) continue;
            slot.enabled = false;
        }
    }
}