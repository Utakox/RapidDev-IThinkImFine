using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))] // ต้องมี Image เพื่อให้ raycast โดน (base.Awake จะ set alphaHitTestMinimumThreshold ให้)
public class SanityHoldInteractable : HoldInteractable
{
    [Header("ปรับ Sanity หมอเมื่อ hold ครบเวลา (ค่าบวก = เพิ่ม, ค่าลบ = ลด)")]
    [SerializeField] private int sanityDelta = -10; // int ให้ตรงกับ DoctorSanityManager.ChangeSanity(int)

    [Header("Dialogue พิเศษ (ถ้ามี) — เล่นผ่าน DialogueManager หลัง Confirm (ไม่ใส่ก็ได้)")]
    [Tooltip("ถ้ากำลังพิมพ์บทพูดหลักอยู่ หรือกำลังรอผู้เล่นเลือก choice ค้างอยู่ dialogue นี้จะไม่เล่น (กันชนกัน) แล้วข้ามไปทำ Finish ทันที")]
    [SerializeField] private DialogueLine[] specialDialogue;

    [Header("คูลดาวน์หลัง Confirm แต่ละครั้ง (ปรับเลขได้ใน Inspector, กดซ้ำได้เรื่อยๆ หลังหมดคูลดาวน์)")]
    [SerializeField] private float cooldownDuration = 5f;

    protected override void Confirm()
    {
        if (DoctorSanityManager.Instance != null)
            DoctorSanityManager.Instance.ChangeSanity(sanityDelta);
        else
            Debug.LogWarning("[SanityHoldInteractable] ไม่พบ DoctorSanityManager.Instance ในซีน");

        bool playedDialogue = false;

        if (specialDialogue != null && specialDialogue.Length > 0)
        {
            if (DialogueManager.Instance != null)
                playedDialogue = DialogueManager.Instance.TryPlayInterjectDialogue(specialDialogue, FinishInteract);
            else
                Debug.LogWarning("[SanityHoldInteractable] ไม่พบ DialogueManager.Instance ในซีน — ข้าม dialogue พิเศษ");
        }

        // ถ้าไม่มี dialogue ให้เล่น หรือเล่นไม่สำเร็จ (ชนกับบทพูดหลัก/choice ค้างอยู่) ให้ finish ทันทีแทนที่จะรอ callback ที่จะไม่ถูกเรียก
        if (!playedDialogue)
            FinishInteract();
    }

    private void FinishInteract()
    {
        // เดิม: disableAfterConfirm ? gameObject.SetActive(false) : ResetInteractable()
        // ตอนนี้ไม่มี "ใช้ครั้งเดียวจบ" อีกแล้ว เข้าคูลดาวน์เสมอ กดซ้ำได้ตลอดหลังหมดเวลา
        StartCooldown(cooldownDuration);
    }
}