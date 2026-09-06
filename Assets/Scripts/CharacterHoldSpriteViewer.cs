using UnityEngine;
using UnityEngine.UI;

// ติดสคริปต์นี้ไว้ที่ปุ่ม/ไอคอน UI บนโต๊ะ
// ชี้ค้างที่ไอคอนเดิมครั้งที่ 1 = เปิดรูป | ชี้ค้างที่ไอคอนเดิมครั้งที่ 2 = ปิดรูป
public class CharacterHoldSpriteViewer : HoldInteractable
{
    [Header("=== UI Image กลางหน้าจอสำหรับแสดงภาพประวัติ ===")]
    [Tooltip("ลาก UI Image กลางหน้าจอที่จะใช้โชว์ภาพประวัติมาใส่ตรงนี้")]
    [SerializeField] private Image inspectImage;

    protected override void OnEnable()
    {
        base.OnEnable();
        HideUI();
    }

    // เมื่อชี้ค้างที่ไอคอนบนโต๊ะจนนับเวลาครบ
    protected override void Confirm()
    {
        if (inspectImage != null && inspectImage.gameObject.activeSelf)
        {
            // ถ้าเปิดรูปอยู่ -> ให้ปิด
            HideUI();
        }
        else
        {
            // ถ้าปิดรูปอยู่ -> ให้เปิด
            ShowCurrentSprite();
        }

        // รีเซ็ตระบบ Hold เพื่อให้ไอคอนเดิมรับการชี้ค้างในรอบถัดไปได้ทันที
        ResetInteractable();
    }

    public void ShowCurrentSprite()
    {
        if (CharacterManager.Instance == null || inspectImage == null) return;

        Sprite currentSprite = CharacterManager.Instance.GetCurrentInspectSprite();

        if (currentSprite != null)
        {
            inspectImage.sprite = currentSprite;
            inspectImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[CharacterHoldSpriteViewer] ตัวละครปัจจุบันไม่มีรูป inspectSprite ใน CharacterData");
        }
    }

    // สั่งปิดภาพ UI
    public void HideUI()
    {
        if (inspectImage != null)
        {
            inspectImage.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        HideUI();
    }
}