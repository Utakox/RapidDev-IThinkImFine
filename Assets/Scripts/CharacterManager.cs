using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public abstract class CharacterBase  : MonoBehaviour
{
    public static CharacterBase Instance;
    [SerializeField] private EachCharacter[] characters; // ลากตัวละครทุกตัวใส่เรียงตามลำดับที่อยากให้ออก
    [SerializeField] private int startingIndex = 0; // ตัวละครเริ่มต้น (0 = ตัวแรก, 1 = ตัวที่สอง, ...)
    [SerializeField] private int currentIndex = 0; // ตัวละครปัจจุบัน (0 = ตัวแรก, 1 = ตัวที่สอง, ...)
    [SerializeField] private int maxCharacters;
    
    [Header("Sanity Settings")]
    [SerializeField] protected int sanity = 50;
    [SerializeField] protected int maxSanity = 100;
    [SerializeField] protected int minSanity = 0;
    
    [Header("UI Reference")]
    public TextMeshProUGUI sanityText;
    
    [Header("Sanity Calculation")]
    [Tooltip("ตัวคูณผลกระทบ sanity เช่น 1 = ปกติ, 0.5 = ทนทานขึ้น 2 เท่า, 1.5 = อ่อนไหวง่าย")]
    [SerializeField] protected float sanityResistance = 1f;
        
    [Header("หน้าตัวละครแต่ละช่วง Sanity")]
    public Sprite faceHigh;    // > 75
    public Sprite faceMid;     // 51 - 75
    public Sprite faceLow;     // 26 - 50
    public Sprite faceBroken;  // <= 25

    protected Image faceImage;
    public int Sanity => sanity;// อ่านค่าได้จากข้างนอก แต่แก้ตรงๆ ไม่ได้

    protected virtual void Awake()
    {
        Instance = this;

        // เซ็ตตัวละครทั้งหมดแบบเรียงลำดับก่อน (ยังไม่สุ่ม)
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].gameObject.SetActive(i == 0); // โชว์แค่ตัวแรกก่อน ตัวอื่นซ่อนไว้
        }
        faceImage = GetComponent<Image>();
        UpdateFace();
    }
    protected virtual void UpdateFace()
    {
        if (faceImage == null) return;

        if (sanity > 75)
            faceImage.sprite = faceHigh;
        else if (sanity > 50)
            faceImage.sprite = faceMid;
        else if (sanity > 25)
            faceImage.sprite = faceLow;
        else
            faceImage.sprite = faceBroken;
    }

    public virtual EachCharacter GetCurrentCharacter()
    {
        return characters[currentIndex];// คืนค่าตัวละครปัจจุบันที่กำลังแอคทีฟอยู่
    }

    public void NextCharacter()// ฟังก์ชันนี้จะถูกเรียกจาก ChoiceManager
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
     protected virtual void UpdateUISanity()
    {
        if (sanityText != null)
            sanityText.text = "Sanity: " + sanity.ToString();
    }


    // แก้ ChangeSanity ให้คำนวณผ่าน resistance ก่อน
    public virtual void ChangeSanity(int amount)
    {
        int calculatedAmount = CalculateSanityChange(amount);
        sanity += calculatedAmount;
        sanity = Mathf.Clamp(sanity, minSanity, maxSanity);
        UpdateFace();
        UpdateUISanity();
    }

    // แยกฟังก์ชันคำนวณออกมาต่างหาก เผื่อ override เฉพาะตัวละคร
    protected virtual int CalculateSanityChange(int rawAmount)
    {
        // ถ้าติดลบ (เสีย sanity) ให้คูณ resistance
        // ถ้าบวก (ฟื้นฟู) ใช้ค่าปกติ ไม่ต้องคูณ resistance
        if (rawAmount < 0)
            return Mathf.RoundToInt(rawAmount * sanityResistance);
        
        return rawAmount;
    }
}