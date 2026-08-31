using UnityEngine;
using TMPro;

public class ChoiceManager : MonoBehaviour
{
    public static ChoiceManager Instance;

    [SerializeField] private GameObject leftChoice;
    [SerializeField] private GameObject rightChoice;

    private TextMeshProUGUI leftText, rightText;
    private ChoiceOption leftOption, rightOption;
    private ChoiceOptionData leftData, rightData;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // true = รวม inactive ด้วย กันเคสปุ่มถูกปิดไว้ใน scene แล้วหา component ไม่เจอ
        leftText = leftChoice.GetComponentInChildren<TextMeshProUGUI>(true);
        rightText = rightChoice.GetComponentInChildren<TextMeshProUGUI>(true);
        leftOption = leftChoice.GetComponent<ChoiceOption>();
        rightOption = rightChoice.GetComponent<ChoiceOption>();

        HideBothChoices();
    }

    public void HideBothChoices()
    {
        leftChoice.SetActive(false);
        rightChoice.SetActive(false);
    }

    public void ShowChoices(ChoiceOptionData left, ChoiceOptionData right)
    {
        if (left == null && right == null)
        {
            Debug.LogError("[ChoiceManager] ไม่มี choice ให้โชว์เลย เช็ค pool ใน CharacterData");
            return;
        }

        // เหลืออันเดียว ให้ยกไปไว้ฝั่งซ้ายเสมอ แล้วซ่อนฝั่งขวา
        if (left == null) { left = right; right = null; }

        leftData = left;
        rightData = right;

        leftChoice.SetActive(true);
        leftText.text = left.choiceText;
        leftOption.ResetChoice();

        rightChoice.SetActive(right != null);
        if (right != null)
        {
            rightText.text = right.choiceText;
            rightOption.ResetChoice();
        }
    }

    public void OnChoiceConfirmed(bool isLeftSide)
    {
        ChoiceOptionData picked = isLeftSide ? leftData : rightData;
        if (picked == null) return;

        HideBothChoices();
        DialogueManager.Instance.OnChoicePicked(picked);
    }
}