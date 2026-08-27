using UnityEngine;
using TMPro;

public class ChoiceManager : MonoBehaviour
{
    public static ChoiceManager Instance;

    public GameObject leftChoice;
    public GameObject rightChoice;

    [SerializeField] public string[] goodChoices = new string[]
    {

    };

    [SerializeField] public string[] mediumChoices = new string[]
    {

    };

    [SerializeField] public string[] badChoices = new string[]
    {

    };

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

        NextChoices();
    }

    public void HideBothChoices()
    {
        leftChoice.SetActive(false);
        rightChoice.SetActive(false);
    }

    public void NextChoices()
    {
        leftChoice.SetActive(true);
        rightChoice.SetActive(true);

        SetupOneChoice(leftText, leftOption);
        SetupOneChoice(rightText, rightOption);
    }

    // สุ่ม tier ให้ 1 ฝั่ง แล้วเซ็ตข้อความ + ค่า sanity ให้เลย
    private void SetupOneChoice(TextMeshProUGUI text, ChoiceOption option)
    {
        int tier = PickTier(); // 0 = good, 1 = medium, 2 = bad

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

    // สุ่มว่าจะได้ tier ไหน โดยถ่วงน้ำหนักตาม sanity ปัจจุบัน
    private int PickTier()
    {
        int currentSanity = SanityManager.Instance.GetCurrentSanity();

        // sanity สูง -> goodWeight เยอะ, sanity ต่ำ -> badWeight เยอะ
        int goodWeight = currentSanity;
        int badWeight = 100 - currentSanity;
        int mediumWeight = 50; // คงที่ไว้ก่อน ปรับได้ตามใจ

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
}