using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ChoiceManager : MonoBehaviour
{
    public static ChoiceManager Instance;

    [System.Serializable]
    public struct ChoiceSlot
    {
        public GameObject container;
        [HideInInspector] public TextMeshProUGUI text;
        [HideInInspector] public ChoiceOption option;
        [HideInInspector] public TextShakeEffect shake;
        [HideInInspector] public TextGlitchEffect glitch;
    }

    [Header("=== UI Choice Slots ===")]
    [SerializeField] private ChoiceSlot leftSlot;
    [SerializeField] private ChoiceSlot rightSlot;

    [Header("=== UI กรอบ/ฉากหลังของ Choice ทั้งชุด (ไม่ใส่ก็ได้) ===")]
    [Tooltip("เช่น panel พื้นหลัง, กรอบตกแต่ง, หัวข้อ 'เลือกคำตอบ' ฯลฯ - จะเปิด/ปิดพร้อมกับตอน choice โผล่/หายไปเสมอ")]
    [SerializeField] private GameObject choiceUIRoot;

    private ChoiceOptionData leftData, rightData;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        SetupSlot(ref leftSlot);
        SetupSlot(ref rightSlot);

        HideBothChoices();
    }

    private void SetupSlot(ref ChoiceSlot slot)
    {
        if (slot.container == null) return;

        slot.text = slot.container.GetComponentInChildren<TextMeshProUGUI>(true);
        slot.option = slot.container.GetComponent<ChoiceOption>();

        if (slot.text != null)
        {
            slot.shake = slot.text.GetComponent<TextShakeEffect>();
            slot.glitch = slot.text.GetComponent<TextGlitchEffect>();
        }
    }

    public void HideBothChoices()
    {
        ResetSlotEffects(leftSlot);
        ResetSlotEffects(rightSlot);

        if (leftSlot.container != null) leftSlot.container.SetActive(false);
        if (rightSlot.container != null) rightSlot.container.SetActive(false);

        if (choiceUIRoot != null) choiceUIRoot.SetActive(false);
    }

    private void ResetSlotEffects(ChoiceSlot slot)
    {
        if (slot.shake != null) slot.shake.SetShaking(false);
        if (slot.glitch != null) slot.glitch.SetGlitching(false);
    }

    public void ShowChoices(ChoiceOptionData left, ChoiceOptionData right)
    {
        if (left == null && right == null) return;

        if (left == null) { left = right; right = null; }

        leftData = left;
        rightData = right;

        // เปิด UI กรอบรวมก่อน แล้วค่อยเปิด choice แต่ละฝั่ง
        if (choiceUIRoot != null) choiceUIRoot.SetActive(true);

        bool isPatientMeltdown = DialogueManager.Instance != null && DialogueManager.Instance.IsInMentalState;
        bool isDoctorGlitching = DoctorSanityManager.Instance != null && DoctorSanityManager.Instance.IsGlitching;

        ApplySlotChoice(leftSlot, left, isPatientMeltdown, isDoctorGlitching);
        ApplySlotChoice(rightSlot, right, isPatientMeltdown, isDoctorGlitching);
    }

    private void ApplySlotChoice(ChoiceSlot slot, ChoiceOptionData data, bool isPatientMeltdown, bool isDoctorGlitching)
    {
        if (slot.container == null) return;

        if (data == null)
        {
            slot.container.SetActive(false);
            ResetSlotEffects(slot);
            return;
        }

        slot.container.SetActive(true);

        if (slot.text != null)
            slot.text.text = data.choiceText;

        if (slot.option != null)
            slot.option.ResetChoice();

        ResetSlotEffects(slot);

        if (isDoctorGlitching)
        {
            if (slot.glitch != null)
            {
                slot.glitch.SetBaseText(data.choiceText);
                slot.glitch.SetGlitching(true);
            }
            return;
        }

        if (isPatientMeltdown)
        {
            if (slot.shake != null) slot.shake.SetShaking(true);
            return;
        }
    }

    public void OnChoiceConfirmed(bool isLeftSide)
    {
        ChoiceOptionData picked = isLeftSide ? leftData : rightData;
        if (picked == null) return;

        HideBothChoices();
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnChoicePicked(picked);
    }
}