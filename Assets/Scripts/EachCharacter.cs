using UnityEngine;
using UnityEngine.UI;
using TMPro; // ต้องมี using นี้ด้วยนะ ถ้าใช้ TextMeshPro

// ติดสคริปต์นี้กับ Image ของตัวละครแต่ละตัว
public class EachCharacter : MonoBehaviour
{
    public int sanity = 50; // ค่าเริ่มต้น เปิดมาแล้วมี 50 เลย หรือจะไปแก้ในแต่ละตัวใน Inspector ก็ได้
    public TextMeshProUGUI sanityText; // ลาก Text ของตัวละครแต่ละตัวใส่ตรงนี้

    [Header("หน้าตัวละครแต่ละช่วง Sanity (ลากรูปใส่)")]
    public Sprite faceHigh;   // sanity มากกว่า 75
    public Sprite faceMid;    // 51 - 75
    public Sprite faceLow;    // 26 - 50
    public Sprite faceBroken; // 25 หรือน้อยกว่า

    private Image faceImage;

    private void Awake()
    {
        faceImage = GetComponent<Image>();
        UpdateFace();
    }

    public void ChangeSanity(int amount)
    {
        sanity += amount;
        sanity = Mathf.Clamp(sanity, 0, 100);
        UpdateFace();
    }

    private void UpdateFace()
    {
        if (sanity > 75)
            faceImage.sprite = faceHigh;
        else if (sanity > 50)
            faceImage.sprite = faceMid;
        else if (sanity > 25)
            faceImage.sprite = faceLow;
        else
            faceImage.sprite = faceBroken;

        UpdateUISanity(); // เรียกฟังก์ชันนี้เพื่ออัปเดต UI ของ sanity
    }

    public void UpdateUISanity()
    {

        if (sanityText != null)
        {
            sanityText.text = "Sanity: " + sanity.ToString();
        }
    }
}