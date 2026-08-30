using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ติดกับ Image ของตัวละครแต่ละตัวใน scene แล้วลาก CharacterData asset ใส่ช่อง Data
// ตัวนี้ถือ sanity ของตัวเองโดยตรง ไม่ต้องผ่านตัวกลางอีกแล้ว
//
// หมายเหตุ: อย่าติด EachCharacter.cs ไว้บน GameObject เดียวกันตัวนี้ เพราะเป็นสคริปต์คู่ขนาน
// ที่ไม่ได้ถูกเกมใช้งานจริง (CharacterManager อ้างอิงเฉพาะ CharacterRuntime) ถ้าติดพร้อมกันจะทำให้
// ค่า sanity ที่เห็นใน Inspector ของ EachCharacter ไม่ตรงกับค่าที่เกมใช้เล่นจริง
public class CharacterRuntime : MonoBehaviour
{
    public CharacterData data;

    [Header("(ไม่ใส่ก็ได้) ลาก TextMeshPro มาไว้โชว์ค่า Sanity ปัจจุบันของตัวละครนี้ เผื่อเช็คตอนเทส")]
    public TextMeshProUGUI sanityText;

    public int Sanity { get; private set; }

    private Image faceImage;

    // เก็บว่า choice อันไหนของตัวละครนี้ถูกเลือกไปแล้วบ้าง (เก็บที่ runtime ไม่ได้แก้ asset ตรงๆ
    // เพราะ asset ใช้ร่วมกันได้หลายที่ ถ้าลบออกจาก asset ถาวรจะพังตอนรันใหม่/เล่นซ้ำ)
    private readonly HashSet<ChoiceOptionData> usedChoices = new HashSet<ChoiceOptionData>();

    private void Awake()
    {
        faceImage = GetComponent<Image>();
        Sanity = data.startingSanity;
        UpdateFace();
        UpdateSanityText();
    }

    public void ChangeSanity(int amount)
    {
        Sanity = Mathf.Clamp(Sanity + amount, 0, 100);
        UpdateFace();
        UpdateSanityText();
    }

    private void UpdateFace()
    {
        if (Sanity > 75)
            faceImage.sprite = data.faceHigh;
        else if (Sanity > 50)
            faceImage.sprite = data.faceMid;
        else if (Sanity > 25)
            faceImage.sprite = data.faceLow;
        else
            faceImage.sprite = data.faceBroken;
    }

    private void UpdateSanityText()
    {
        if (sanityText == null) return;
        sanityText.text = $"{data.characterName}: {Sanity}";
    }

    // เรียกตอนสุ่ม choice เพื่อเช็คว่าอันนี้เคยถูกตัวละครคนนี้เลือกไปแล้วหรือยัง
    public bool HasUsedChoice(ChoiceOptionData choice)
    {
        return usedChoices.Contains(choice);
    }

    // เรียกตอนผู้เล่นเลือก choice นี้ไปแล้ว ตัดออกจากคลังของตัวละครนี้ถาวร (จนกว่าจะรันเกมใหม่)
    public void MarkChoiceUsed(ChoiceOptionData choice)
    {
        usedChoices.Add(choice);
    }
}