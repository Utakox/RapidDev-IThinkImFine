using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Base class สำหรับ "ของที่ต้องเอาเมาส์ไปแตะค้างถึงจะทำงาน" ใช้ได้กับ choice หรือของอื่นในอนาคต
// วิธีใช้: สร้างคลาสใหม่ที่ : HoldInteractable แล้ว override Confirm() ว่าค้างครบเวลาแล้วให้ทำอะไร
//
// เช็ค hover ด้วย EventSystem.RaycastAll ใหม่ "ทุกเฟรม" แบบไม่เชื่อค่าเดิมที่ cache ไว้เลย
// (เวอร์ชันก่อนหน้าเช็คซ้ำแค่ตอน isPointerOver = false เท่านั้น พอเป็น true แล้วไม่เช็คซ้ำอีก
//  ถ้า OnPointerExit ของ Unity หลุดไปแม้แค่ครั้งเดียว ค่าจะค้าง true ตลอดไป นี่คือบั๊กที่เจอ)
public abstract class HoldInteractable : MonoBehaviour
{
    public float holdDuration = 2f;

    private bool isConfirmed;
    private float hoverTimer;

    private static readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    protected virtual void Awake()
    {
        // ให้ raycast นับเฉพาะจุดที่ "ไม่โปร่งใส" ของรูปจริงๆ ไม่ใช่กรอบ RectTransform เต็มๆ
        // *** ต้องเปิด Read/Write Enabled ใน Import Settings ของรูปนั้นด้วย ไม่งั้นจะ error ตอนรัน ***
        Image image = GetComponent<Image>();
        if (image != null)
            image.alphaHitTestMinimumThreshold = 0.1f;
    }

    protected virtual void OnEnable()
    {
        hoverTimer = 0f;
        isConfirmed = false;
    }

    private void Update()
    {
        if (isConfirmed) return;

        // เช็คสดใหม่ทุกเฟรม ไม่พึ่ง OnPointerEnter/Exit เลย กันปัญหาค้างสถานะผิด
        bool isPointerOverNow = IsPointerActuallyOverThis();

        if (isPointerOverNow)
        {
            if (hoverTimer == 0f)
                CustomCursor.Instance.ShowCountdown();

            hoverTimer += Time.deltaTime;

            float secondsLeft = Mathf.Max(0f, holdDuration - hoverTimer);
            CustomCursor.Instance.UpdateCountdown(secondsLeft);

            if (hoverTimer >= holdDuration)
                DoConfirm();
        }
        else if (hoverTimer > 0f)
        {
            hoverTimer = 0f;
            CustomCursor.Instance.HideCountdown();
        }
    }

    private bool IsPointerActuallyOverThis()
    {
        if (EventSystem.current == null) return false;

        var pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };

        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
                return true;
        }

        return false;
    }

    private void DoConfirm()
    {
        isConfirmed = true;
        CustomCursor.Instance.HideCountdown();
        Confirm();
    }

    // ลูกคลาส implement ตรงนี้ว่า "ค้างครบเวลาแล้วให้ทำอะไร"
    protected abstract void Confirm();

    // เรียกจากภายนอกตอนอยากรีเซ็ตสถานะ (เช่น ตอนโชว์ choice รอบใหม่)
    public void ResetInteractable()
    {
        isConfirmed = false;
        hoverTimer = 0f;
        CustomCursor.Instance.HideCountdown();
    }
}