using UnityEngine;
using UnityEngine.UI;

// ติดกับ Object กลางที่คุมกลุ่ม Image effect ของ dialogue ทั้งหมด
public class DialogueImageEffect : MonoBehaviour
{
    public static DialogueImageEffect Instance;

    [Header("ลาก Image slot ที่เตรียมไว้ใน scene ใส่ตามลำดับ")]
    public Image[] imageSlots;

    private void Awake()
    {
        Instance = this;
        HideAll();
    }

    public void ShowImages(Sprite[] sprites)
    {
        HideAll();

        if (sprites == null || imageSlots == null) return;

        for (int i = 0; i < sprites.Length && i < imageSlots.Length; i++)
        {
            if (sprites[i] == null || imageSlots[i] == null) continue;

            imageSlots[i].sprite = sprites[i];
            imageSlots[i].enabled = true;
        }
    }

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