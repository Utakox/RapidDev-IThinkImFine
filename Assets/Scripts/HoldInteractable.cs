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
    }

    private void Update()
    {
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

    public void ResetInteractable()
    {
        isConfirmed = false;
        hoverTimer = 0f;
        if (CustomCursor.Instance != null)
            CustomCursor.Instance.HideCountdown();
    }
}