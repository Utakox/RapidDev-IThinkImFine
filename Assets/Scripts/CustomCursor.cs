using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomCursor : MonoBehaviour
{
    public static CustomCursor Instance;

    [Header("ตัว Image cursor ที่จะตามเมาส์ (ไม่ใส่ = ใช้ RectTransform ของ object นี้เอง)")]
    public RectTransform cursorRect;

    [Header("เลขนับถอยหลัง โชว์ตอนกำลังชี้ของที่ interact ได้ (ลาก Text ลูกของ cursor มาใส่)")]
    public TextMeshProUGUI countdownText;

    private Canvas canvas;
    private RectTransform canvasRect;
    private Image cursorImage;

    private void Awake()
    {
        Instance = this;
        Cursor.visible = false; // ซ่อนเมาส์ของระบบ ใช้ Image นี้แทน

        // เดิม cursorRect เป็น field ที่ต้องลากเองใน Inspector ถ้าลืมลาก = Update() จะ return เงียบๆ ทุกเฟรม
        // ดูเหมือนเมาส์ "ค้าง" ทั้งที่จริงๆ แค่ไม่มี reference ให้ขยับ กันไว้ด้วยการ fallback ไปใช้ตัวเอง
        if (cursorRect == null)
            cursorRect = GetComponent<RectTransform>();

        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas != null ? canvas.transform as RectTransform : null;
        cursorImage = GetComponent<Image>();

        if (cursorRect == null)
            Debug.LogError("[CustomCursor] หา RectTransform ไม่เจอเลย (ทั้งลากเองและ GetComponent) เมาส์จะไม่ขยับแน่นอน");
        if (canvas == null)
            Debug.LogError("[CustomCursor] หา Canvas ใน parent ไม่เจอ — object นี้ต้องอยู่ใต้ Canvas ใน hierarchy เมาส์จะไม่ขยับแน่นอน");

        transform.SetAsLastSibling();

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (canvas == null || canvasRect == null || cursorRect == null) return;

        Vector2 localPoint;
        bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            canvas.worldCamera, // Screen Space - Overlay ต้องเป็น null อยู่แล้วโดยอัตโนมัติ / Camera หรือ World Space ต้องมี Render Camera ตั้งไว้ใน Canvas ด้วย
            out localPoint
        );

        if (!ok) return;

        // แปลงจากพิกัดของ "Canvas" ไปเป็นพิกัดของ "พ่อจริงๆ ของ cursorRect" อีกที
        // เดิมโค้ดตั้ง anchoredPosition (ซึ่งอิงพ่อของตัวเอง) ด้วยค่าที่คำนวณเทียบกับ Canvas ตรงๆ
        // ถ้า cursorRect ไม่ได้เป็นลูกตรงของ Canvas (เช่นอยู่ใต้ panel ลูกอีกที) พิกัดจะเพี้ยน เมาส์ดูเหมือนขยับน้อยมากหรือไม่ขยับเลย
        if (cursorRect.parent == canvasRect)
        {
            cursorRect.anchoredPosition = localPoint;
        }
        else
        {
            Vector3 worldPoint = canvasRect.TransformPoint(localPoint);
            cursorRect.position = worldPoint;
        }
    }

    public void ShowCountdown()
    {
        if (countdownText != null) countdownText.gameObject.SetActive(true);
        if (cursorImage != null) cursorImage.enabled = false;
    }

    public void UpdateCountdown(float secondsLeft)
    {
        if (countdownText != null)
            countdownText.text = secondsLeft.ToString("F1");
    }

    public void HideCountdown()
    {
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (cursorImage != null) cursorImage.enabled = true;
    }
}