using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomCursor : MonoBehaviour
{
    public static CustomCursor Instance;

    [Header("--- รูปเมาส์ขนนก (Hardware Cursor) ---")]
    [Tooltip("ลากไฟล์รูปขนนกมาใส่ (ต้องตั้ง Texture Type เป็น Cursor ใน Inspector ก่อน ถ้าจะใช้โหมด Hardware Cursor)")]
    public Texture2D cursorTexture;

    [Tooltip("จุดคลิกบนรูปเมาส์ (ถ้าปลายขนนกอยู่มุมซ้ายบนให้ใส่ X:0, Y:0) ใช้เฉพาะโหมด Hardware Cursor")]
    public Vector2 hotSpot = Vector2.zero;

    [Header("--- Software Cursor (แนะนำสำหรับ Build ลง Itch.io/เว็บ) ---")]
    [Tooltip("เปิดไว้เพื่อให้เกมวาดรูปเมาส์เอง (ผ่าน UI Image) แทนการฝากเบราว์เซอร์วาดให้ " +
             "วิธีนี้จะได้ขนาด/ตำแหน่งตรงกันทุกเบราว์เซอร์ เพราะ Hardware Cursor บนเว็บถูกเบราว์เซอร์จำกัดขนาดภาพไม่เกิน 128x128 " +
             "และมักเพี้ยนเรื่อง DPI/การ scale ของหน้าเว็บ")]
    [SerializeField] private bool useSoftwareCursor = true;

    [Tooltip("ลาก RectTransform ของ UI Image ที่จะใช้แสดงรูปเมาส์แบบ Software " +
             "ตั้ง Pivot ของ RectTransform นี้ให้ตรงกับจุดคลิกบนภาพ (เช่นปลายขนนกอยู่มุมซ้ายบน ให้ตั้ง Pivot เป็น X:0, Y:1)")]
    [SerializeField] private RectTransform softwareCursorImage;

    [Tooltip("ระยะห่างเพิ่มเติมของรูปเมาส์ Software จากตำแหน่งเมาส์จริง ปกติปล่อย 0,0 แล้วปรับที่ Pivot ของรูปแทน")]
    [SerializeField] private Vector2 cursorOffset = Vector2.zero;

    [Header("--- ตัวเลขนับถอยหลัง ---")]
    [Tooltip("ลาก TextMeshProUGUI ตัวเลขมาใส่")]
    public TextMeshProUGUI countdownText;

    [Tooltip("ระยะห่างของตัวเลขจากปลายเมาส์")]
    public Vector2 textOffset = new Vector2(25f, -25f);

    private Canvas parentCanvas;
    private RectTransform textRect;

    private void Awake()
    {
        Instance = this;

        if (useSoftwareCursor)
        {
            // ซ่อนเมาส์จริงของระบบ/เบราว์เซอร์ แล้วใช้ Image ในเกมวาดแทน
            // เพื่อให้ตำแหน่ง/ขนาดตรงกันทุกแพลตฟอร์ม ไม่ขึ้นกับข้อจำกัดของ Hardware Cursor บนเว็บ
            Cursor.visible = false;

            if (softwareCursorImage != null)
            {
                parentCanvas = softwareCursorImage.GetComponentInParent<Canvas>();

                // ปิด Raycast Target ของ Image เมาส์ ไม่ให้ไปบล็อกการคลิกของ UI อื่น
                Image cursorImg = softwareCursorImage.GetComponent<Image>();
                if (cursorImg != null) cursorImg.raycastTarget = false;

                softwareCursorImage.gameObject.SetActive(true);
            }
        }
        else
        {
            // 1. ตั้งค่ารูปเมาส์ให้เป็นระบบ Hardware Cursor ของ Unity
            SetHardwareCursor();
        }

        // 2. ตั้งค่า Text ตัวเลข
        if (countdownText != null)
        {
            textRect = countdownText.GetComponent<RectTransform>();

            if (parentCanvas == null)
                parentCanvas = countdownText.GetComponentInParent<Canvas>();

            // สำคัญมาก: ปิด Raycast Target เพื่อไม่ให้ข้อความไปบล็อกการคลิกเมาส์
            countdownText.raycastTarget = false; 

            countdownText.gameObject.SetActive(false);
        }
    }

    private void SetHardwareCursor()
    {
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
        }
        Cursor.visible = true; // เปิดให้เห็นเมาส์ระบบ
    }

    private void Update()
    {
        if (parentCanvas == null) return;

        bool needsCursorMove = useSoftwareCursor && softwareCursorImage != null;
        bool needsTextMove = countdownText != null && countdownText.gameObject.activeSelf;

        if (!needsCursorMove && !needsTextMove) return;

        Camera cam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition,
            cam,
            out Vector2 localPoint))
        {
            if (needsCursorMove)
                softwareCursorImage.anchoredPosition = localPoint + cursorOffset;

            if (needsTextMove)
                textRect.anchoredPosition = localPoint + textOffset;
        }
    }

    public void ShowCountdown()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);
    }

    public void UpdateCountdown(float secondsLeft)
    {
        if (countdownText != null)
            countdownText.text = secondsLeft.ToString("F1");
    }

    public void UpdateCooldownText(float secondsLeft)
    {
        if (countdownText != null)
            countdownText.text = $"Cooldown: {secondsLeft:F1}";
    }

    public void HideCountdown()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // คืนค่าเมาส์ปกติเมื่อปิดหรือเปลี่ยนฉาก
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.visible = true;
        if (Instance == this)
            Instance = null;
    }
}