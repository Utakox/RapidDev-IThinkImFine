using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class HoldInteractable : MonoBehaviour
{
    public float holdDuration = 2f;

    [Header("Sanity หมอต่ำ = เมาส์ฝืด (นับเวลาแตะค้างช้าลงกว่าปกติ)")]
    [Range(0.05f, 1f)] [SerializeField] private float glitchHoverSpeedMultiplier = 0.4f;

    private bool isConfirmed;
    private float hoverTimer;

    // คูลดาวน์หลัง Confirm (subclass เรียก StartCooldown เอง) ระหว่างนี้ hold-to-confirm จะไม่ทำงาน
    private bool isOnCooldown;
    private float cooldownTimer;

    private static readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    protected virtual void Awake()
    {
        Image image = GetComponent<Image>();
        if (image != null)
            image.alphaHitTestMinimumThreshold = 0.1f;
    }

    protected virtual void OnEnable()
    {
        hoverTimer = 0f;
        isConfirmed = false;
        // ตั้งใจไม่รีเซ็ต isOnCooldown ตรงนี้ เผื่อ object ถูกปิด/เปิดกลางคันระหว่างคูลดาวน์อยู่
        // อยากให้นับต่อจากเดิม ไม่ใช่หายไปเฉยๆ ถ้าอยากรีเซ็ตคูลดาวน์ด้วยให้เรียก ResetInteractable() เอง
    }

    private void Update()
    {
        if (isOnCooldown)
        {
            UpdateCooldown();
            return;
        }

        if (isConfirmed) return;

        bool isPointerOverNow = IsPointerActuallyOverThis();

        if (isPointerOverNow)
        {
            if (hoverTimer == 0f && CustomCursor.Instance != null)
                CustomCursor.Instance.ShowCountdown();

            bool isGlitching = DoctorSanityManager.Instance != null && DoctorSanityManager.Instance.IsGlitching;
            float speedMultiplier = isGlitching ? glitchHoverSpeedMultiplier : 1f;

            hoverTimer += Time.deltaTime * speedMultiplier;

            float secondsLeft = Mathf.Max(0f, holdDuration - hoverTimer);
            if (CustomCursor.Instance != null)
                CustomCursor.Instance.UpdateCountdown(secondsLeft);

            if (hoverTimer >= holdDuration)
                DoConfirm();
        }
        else if (hoverTimer > 0f)
        {
            hoverTimer = 0f;
            if (CustomCursor.Instance != null)
                CustomCursor.Instance.HideCountdown();
        }
    }

    // ระหว่างคูลดาวน์: นับถอยหลังไปเรื่อยๆ ไม่ว่าเมาส์จะอยู่ตรงไหน แต่โชว์ "Cooldown: X" เฉพาะตอนเอาเมาส์ไปชี้ที่ object นี้เท่านั้น
    private void UpdateCooldown()
    {
        cooldownTimer -= Time.deltaTime;

        bool isPointerOverNow = IsPointerActuallyOverThis();

        if (cooldownTimer <= 0f)
        {
            isOnCooldown = false;
            cooldownTimer = 0f;

            if (CustomCursor.Instance != null)
                CustomCursor.Instance.HideCountdown(); // เคลียร์ข้อความ cooldown ทิ้ง ให้เฟรมถัดไปเริ่ม hover ปกติได้สะอาด

            return;
        }

        if (isPointerOverNow)
        {
            if (CustomCursor.Instance != null)
            {
                CustomCursor.Instance.ShowCountdown();
                CustomCursor.Instance.UpdateCooldownText(cooldownTimer);
            }
        }
        else if (CustomCursor.Instance != null)
        {
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
        if (CustomCursor.Instance != null)
            CustomCursor.Instance.HideCountdown();
        Confirm();
    }

    protected abstract void Confirm();

    // เรียกจาก subclass ใน Confirm()/หลังจบ Confirm() แทนการ disable ตัวเอง — เข้าสถานะคูลดาวน์แทน
    // ระหว่างคูลดาวน์ hold-to-confirm จะไม่ทำงาน จนกว่าจะครบเวลา แล้วกลับมา hold ซ้ำได้ตามปกติ
    protected void StartCooldown(float duration)
    {
        isOnCooldown = true;
        cooldownTimer = Mathf.Max(0f, duration);
        isConfirmed = false;
        hoverTimer = 0f;
    }

    public bool IsOnCooldown => isOnCooldown;

    // เรียกจากภายนอกตอนอยากรีเซ็ตสถานะทั้งหมดทันที (ทั้ง hold และ cooldown)
    public void ResetInteractable()
    {
        isConfirmed = false;
        hoverTimer = 0f;
        isOnCooldown = false;
        cooldownTimer = 0f;
        if (CustomCursor.Instance != null)
            CustomCursor.Instance.HideCountdown();
    }
}