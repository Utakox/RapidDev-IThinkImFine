using UnityEngine;
using TMPro;

public class ChoiceManager : MonoBehaviour
{
    public static ChoiceManager Instance;

    public GameObject leftChoice;
    public GameObject rightChoice;

    [SerializeField] public string[] goodChoices;
    [SerializeField] public string[] mediumChoices;
    [SerializeField] public string[] badChoices;

    [Header("แต่ละ tier ส่งผลต่อ sanity เท่าไหร่")]
    public int goodSanityValue = 10;
    public int mediumSanityValue = 0;
    public int badSanityValue = -10;

    private TextMeshProUGUI leftText;
    private TextMeshProUGUI rightText;
    private ChoiceOption leftOption;
    private ChoiceOption rightOption;

    private void Awake()
    {
        Instance = this;

        leftText = leftChoice.GetComponentInChildren<TextMeshProUGUI>();
        rightText = rightChoice.GetComponentInChildren<TextMeshProUGUI>();
        leftOption = leftChoice.GetComponent<ChoiceOption>();
        rightOption = rightChoice.GetComponent<ChoiceOption>();

        HideBothChoices(); // เริ่มเกมมาต้องซ่อนไว้ก่อน รอ DialogueManager สั่งโชว์
    }

    public void HideBothChoices()
    {
        leftChoice.SetActive(false);
        rightChoice.SetActive(false);
    }

    // เปลี่ยนชื่อจาก NextChoices -> ShowChoices ให้สื่อว่า "โชว์ครั้งนี้" ไม่ใช่ "วนไปเรื่อยๆ"
    public void ShowChoices()
    {
        leftChoice.SetActive(true);
        rightChoice.SetActive(true);

        SetupOneChoice(leftText, leftOption);
        SetupOneChoice(rightText, rightOption);
    }

    private void SetupOneChoice(TextMeshProUGUI text, ChoiceOption option)
    {
        int tier = PickTier();

        if (tier == 0)
        {
            text.text = GetRandomText(goodChoices);
            option.SetChoice(goodSanityValue);
        }
        else if (tier == 1)
        {
            text.text = GetRandomText(mediumChoices);
            option.SetChoice(mediumSanityValue);
        }
        else
        {
            text.text = GetRandomText(badChoices);
            option.SetChoice(badSanityValue);
        }
    }

    private int PickTier()
    {
        int currentSanity = SanityManager.Instance.GetCurrentSanity();

        int goodWeight = currentSanity;
        int badWeight = 100 - currentSanity;
        int mediumWeight = 50;

        int totalWeight = goodWeight + mediumWeight + badWeight;
        int roll = Random.Range(0, totalWeight);

        if (roll < goodWeight)
            return 0;
        else if (roll < goodWeight + mediumWeight)
            return 1;
        else
            return 2;
    }

    private string GetRandomText(string[] arr)
    {
        if (arr == null || arr.Length == 0)
            return "";

        return arr[Random.Range(0, arr.Length)];
    }

    // เรียกจาก ChoiceOption.Confirm() หลังผู้เล่นชี้ค้างครบเวลา
    public void OnChoiceConfirmed()
    {
        HideBothChoices();
        DialogueManager.Instance.ContinueAfterChoice(); // กลับไปพิมพ์บรรทัดถัดไปต่อ ไม่วนสุ่ม choice ใหม่ทันที
    }
}