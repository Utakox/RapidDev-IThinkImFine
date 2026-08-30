using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// ติดกับ Choice ซ้าย และ Choice ขวา (คนละอัน) ตั้ง Is Left Side ใน Inspector ให้ตรงฝั่งด้วย
//
// เช็ค hover ด้วย EventSystem.RaycastAll (เหมือนที่ระบบ UI ใช้จริงตอนยิง OnPointerEnter) แทนการเช็ค
// แค่กรอบ RectTransform เฉยๆ เพราะถ้า RectTransform ของปุ่มนี้ถูก stretch กว้างกว่าที่เห็นด้วยตา
// (เช่น ครอบครึ่งจอ) การเช็คกรอบเฉยๆ จะทำให้เมาส์ที่อยู่ "ห่างจากปุ่มที่เห็น" แต่ยังอยู่ในกรอบใหญ่
// นั้น ถูกนับว่า "ทับ" อยู่ดี แล้วเริ่มนับเวลาค้างเองโดยไม่ต้องตั้งใจ
public class ChoiceOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool isLeftSide = true;
    public float holdDuration = 2f;

    private bool isPointerOver;
    private bool isConfirmed;
    private float hoverTimer;

    private static readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    private void OnEnable()
    {
        // เช็คทันทีตอนปุ่มโผล่ขึ้นมาว่าเมาส์ทับ "พื้นที่ที่เห็นจริง" อยู่แล้วหรือเปล่า
        isPointerOver = IsPointerActuallyOverThis();
        hoverTimer = 0f;
    }

    public void OnPointerEnter(PointerEventData eventData) => isPointerOver = true;

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        hoverTimer = 0f;
    }

    private void Update()
    {
        if (isConfirmed) return;

        // สำรองไว้เผื่อ enter/exit event หลุดเฟรม (เช่นปุ่มเพิ่งโผล่ทับเมาส์พอดี)
        if (!isPointerOver)
            isPointerOver = IsPointerActuallyOverThis();

        if (isPointerOver)
        {
            hoverTimer += Time.deltaTime;
            if (hoverTimer >= holdDuration)
                Confirm();
        }
        else
        {
            hoverTimer = 0f;
        }
    }

    // ยิง raycast ผ่านระบบ UI จริง เช็คว่าอันดับแรกๆ ที่โดนคือตัวเอง (หรือลูกของตัวเอง เช่น Text/Icon
    // ข้างใน) ไหม แม่นกว่าการเช็คกรอบ RectTransform เฉยๆ เพราะสนใจ Graphic ที่มองเห็น/raycast target จริง
    private bool IsPointerActuallyOverThis()
    {
        if (EventSystem.current == null) return false;

        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
                return true;
        }

        return false;
    }

    private void Confirm()
    {
        isConfirmed = true;
        ChoiceManager.Instance.OnChoiceConfirmed(isLeftSide);
    }

    // เรียกจาก ChoiceManager.ShowChoices() ทุกครั้งที่โชว์ choice ใหม่
    public void ResetChoice()
    {
        isConfirmed = false;
        hoverTimer = 0f;
        isPointerOver = IsPointerActuallyOverThis();
    }
}