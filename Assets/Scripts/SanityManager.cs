using UnityEngine;

// ตัวกลางเชื่อม ChoiceOption กับตัวละครที่กำลังแอคทีฟอยู่
public class SanityManager : MonoBehaviour
{
    public static SanityManager Instance;

    [Header("ตัวละครที่กำลังแอคทีฟอยู่ตอนนี้")]
    [SerializeField] private EachCharacter currentCharacter;

    private void Awake()
    {
        Instance = this;
    }

    // เรียกตอนเปลี่ยนฉาก/สลับตัวละครที่กำลังคุยอยู่
    public void SetCurrentCharacter(EachCharacter character)
    {
        currentCharacter = character;
    }

    // เรียกจาก ChoiceOption.cs ตอนผู้เล่นเลือก choice
    public void ChangeSanity(int amount)
    {
        if (currentCharacter == null)
        {
            Debug.LogWarning("SanityManager: ยังไม่มี current character ถูกตั้งค่า");
            return;
        }

        currentCharacter.ChangeSanity(amount);
        Debug.Log(currentCharacter.name + " sanity ตอนนี้: " + currentCharacter.Sanity);
    }

    // เผื่อ choice บางอันอยากให้กระทบทุกตัวละคร (เช่น event สยองขวัญรวม)
    public void ChangeSanityForAll(EachCharacter[] characters, int amount)
    {
        foreach (var character in characters)
        {
            character.ChangeSanity(amount);
        }
    }

    // เผื่อ choice อยากให้ผลลัพธ์เป็น % ของ sanity ปัจจุบัน แทนค่าคงที่
    public void ChangeSanityByPercent(float percent)
    {
        if (currentCharacter == null) return;

        int amount = Mathf.RoundToInt(currentCharacter.Sanity * (percent / 100f));
        currentCharacter.ChangeSanity(amount);
    }

    public int GetCurrentSanity()
    {
        if (currentCharacter == null) return 0;
        return currentCharacter.Sanity;
    }
}