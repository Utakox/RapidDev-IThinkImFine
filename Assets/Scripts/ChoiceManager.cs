using UnityEngine;
using TMPro;

public class ChoiceManager : MonoBehaviour
{
    public static ChoiceManager Instance;

    public GameObject leftChoice;
    public GameObject rightChoice;

    private TextMeshProUGUI leftText;
    private TextMeshProUGUI rightText;
    private ChoiceOption leftOption;
    private ChoiceOption rightOption;

    private ChoiceOptionData leftData;
    private ChoiceOptionData rightData;

    private void Awake()
    {
        Instance = this;

        leftText = leftChoice.GetComponentInChildren<TextMeshProUGUI>();
        rightText = rightChoice.GetComponentInChildren<TextMeshProUGUI>();
        leftOption = leftChoice.GetComponent<ChoiceOption>();
        rightOption = rightChoice.GetComponent<ChoiceOption>();

        HideBothChoices();
    }

    public void HideBothChoices()
    {
        leftChoice.SetActive(false);
        rightChoice.SetActive(false);
    }

    // เรียกจาก DialogueManager พร้อมส่ง choice ที่สุ่มมาแล้วของตัวละครคนปัจจุบัน (ซ้าย/ขวา)
    public void ShowChoices(ChoiceOptionData left, ChoiceOptionData right)
    {
        if (left == null || right == null)
        {
            // เกิดได้ถ้า pool ใน CharacterData (goodChoices/badChoices) ว่างเกินไปจนสุ่มไม่ครบ 2 อัน
            Debug.LogError("ShowChoices ได้ choice ไม่ครบ 2 อัน เช็ค goodChoices/badChoices ใน CharacterData");
            return;
        }

        leftData = left;
        rightData = right;

        leftChoice.SetActive(true);
        rightChoice.SetActive(true);

        leftText.text = left.choiceText;
        rightText.text = right.choiceText;

        leftOption.ResetChoice();
        rightOption.ResetChoice();
    }

    // เรียกจาก ChoiceOption ตอนผู้เล่นชี้ค้างครบเวลา ส่งมาว่าเป็นฝั่งซ้ายหรือขวา
    public void OnChoiceConfirmed(bool isLeftSide)
    {
        HideBothChoices();
        DialogueManager.Instance.OnChoicePicked(isLeftSide ? leftData : rightData);
    }
}