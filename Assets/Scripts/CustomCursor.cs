using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ติดกับ Image ที่เป็นรูป cursor ที่คุณวาดเอง (ต้องเป็นลูกของ Canvas)
// สคริปต์นี้จะซ่อนเมาส์ของระบบ แล้วให้ Image นี้ตามตำแหน่งเมาส์แทน
public class CustomCursor : MonoBehaviour
{
    public static CustomCursor Instance;

    [Header("ตัว Image cursor ที่จะตามเมาส์ (คือ object ที่ติดสคริปต์นี้อยู่)")]
    public RectTransform cursorRect;

    [Header("เลขนับถอยหลัง โชว์ตอนกำลังชี้ของที่ interact ได้ (ลาก Text ลูกของ cursor มาใส่)")]
    public TextMeshProUGUI countdownText;

    private Canvas canvas;
    private Image cursorImage; // ตัวรูป cursor เอง ไว้ปิด/เปิดตอนแตะ Interactable

    private void Awake()
    {
        Instance = this;

        Cursor.visible = false; // ซ่อนเมาส์ของระบบ ใช้ Image นี้แทน

        canvas = GetComponentInParent<Canvas>();
        cursorImage = GetComponent<Image>();
        transform.SetAsLastSibling(); // เอา cursor ไว้บนสุดเสมอ ไม่ให้ UI อื่นบังทับ

        countdownText.gameObject.SetActive(false);
    }

    private void Update()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out localPoint
        );

        cursorRect.anchoredPosition = localPoint;
    }

    // เรียกจาก HoldInteractable ตอนเริ่ม hover ของที่ interact ได้
    public void ShowCountdown()
    {
        countdownText.gameObject.SetActive(true);
        cursorImage.enabled = false; // ซ่อนตัว cursor ไว้ตอนกำลังนับถอยหลัง
    }

    // อัปเดตทุกเฟรมระหว่างค้างอยู่ เช่น 1.9, 1.8, 1.7 ...
    public void UpdateCountdown(float secondsLeft)
    {
        countdownText.text = secondsLeft.ToString("F1");
    }

    public void HideCountdown()
    {
        countdownText.gameObject.SetActive(false);
        cursorImage.enabled = true; // เอาเมาส์ออกหรือ confirm แล้ว โชว์ cursor กลับมา
    }
}