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
        if (cursorImage != null)
            cursorImage.enabled = true;
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
            if (cursorRect.parent == canvasRect)
            {
                cursorRect.anchoredPosition = localPoint;
            }
            else
            {
                Vector3 worldPoint = canvasRect.TransformPoint(localPoint);
                cursorRect.position = worldPoint;
            }

            // *** เอาบรรทัด "cursorImage.enabled = true;" ที่เคยอยู่ตรงนี้ออกแล้ว ***
            // ของเดิมบังคับเปิด cursorImage ทุกเฟรมที่คำนวณตำแหน่งสำเร็จ (เกือบทุกเฟรมที่เมาส์อยู่บนจอ)
            // เลยไปเขียนทับค่าที่ ShowCountdown() เพิ่งสั่งปิดไปเมื่อเฟรมก่อนหน้า -> คอร์เซอร์เลยไม่ยอมหาย
            // ต่อไปนี้ปล่อยให้ ShowCountdown()/HideCountdown() เป็นจุดเดียวที่คุมการเปิด-ปิด cursorImage
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

    // เหมือน UpdateCountdown แต่เปลี่ยนข้อความเป็น "Cooldown: X" ใช้ตอน HoldInteractable อยู่ในสถานะคูลดาวน์
    public void UpdateCooldownText(float secondsLeft)
    {
        if (countdownText != null)
            countdownText.text = $"Cooldown: {secondsLeft:F1}";
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