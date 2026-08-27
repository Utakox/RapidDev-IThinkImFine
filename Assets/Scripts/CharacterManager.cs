using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    public EachCharacter[] characters; // ลากตัวละครทุกตัวใส่เรียงตามลำดับที่อยากให้ออก
    private int currentIndex = 0;

    private void Awake()
    {
        Instance = this;

        // เซ็ตตัวละครทั้งหมดแบบเรียงลำดับก่อน (ยังไม่สุ่ม)
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].gameObject.SetActive(i == 0); // โชว์แค่ตัวแรกก่อน ตัวอื่นซ่อนไว้
        }
    }

    public EachCharacter GetCurrentCharacter()
    {
        return characters[currentIndex];
    }

    public void NextCharacter()
    {
        characters[currentIndex].gameObject.SetActive(false);

        currentIndex++;
        if (currentIndex >= characters.Length)
        {
            Debug.Log("ตัวละครหมดแล้ว");
            return;
        }

        characters[currentIndex].gameObject.SetActive(true);
    }
}