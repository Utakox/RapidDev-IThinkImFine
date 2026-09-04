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
    if (canvas == null || cursorRect == null) return;

    // เช็ค RenderMode ของ Canvas เพื่อเลือก Camera ที่ถูกต้อง
    Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

    // แปลงพิกัดหน้าจอตรงเข้า RectTransform ของ Parent ได้ทันที
    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
        cursorRect.parent as RectTransform,
        Input.mousePosition,
        cam,
        out Vector2 localPoint))
    {
        cursorRect.anchoredPosition = localPoint;
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
    private void OnDestroy()
    {
        Cursor.visible = true;
        if (Instance == this)
            Instance = null;
    }
}