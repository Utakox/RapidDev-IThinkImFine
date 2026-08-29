using UnityEngine;

// ตัวกลางเชื่อม ChoiceOption กับตัวละครที่กำลังแอคทีฟอยู่
public class SanityManager : MonoBehaviour
{
    public static SanityManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void ChangeSanity(int amount)//now this function is called from ChoiceOption.cs when a choice is made, and it will change the sanity of the current character accordingly
    {
        EachCharacter current = CharacterManager.Instance.GetCurrentCharacter();
        current.ChangeSanity(amount);

        Debug.Log(current.name + " sanity ตอนนี้: " + current.sanity);
    }

    public int GetCurrentSanity()
    {
        return CharacterManager.Instance.GetCurrentCharacter().sanity;
    }
}