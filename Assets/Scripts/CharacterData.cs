using UnityEngine;

// 1 asset = 1 ตัวละคร ครบทุกอย่างในไฟล์เดียว
[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Character")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public int startingSanity = 50;

    [Header("หน้าตัวละครแต่ละช่วง Sanity")]
    public Sprite faceHigh;   // > 75
    public Sprite faceMid;    // 51 - 75
    public Sprite faceLow;    // 26 - 50
    public Sprite faceBroken; // <= 25

    [Header("บทพูดตอนเริ่ม (Startup) เรียงตามลำดับ")]
    public DialogueLine[] introDialogue;

    [Header("--- Choice ปกติ (จะสุ่มโชว์ต่อเนื่องหลายรอบ จนกว่าจะหมดทั้ง 2 คลังนี้ สุ่มรวมกันแบบไม่ถ่วงน้ำหนัก) ---")]
    public ChoiceOptionData[] goodChoices;
    public ChoiceOptionData[] badChoices;

    [Header("--- พอ Choice ปกติหมด เช็ค Sanity ตอนนั้นเทียบกับค่านี้ ---")]
    [Tooltip("Sanity ต่ำกว่าค่านี้ = ถือว่าวิกฤต ใช้ Crisis Choices ด้านล่าง / เท่ากับหรือมากกว่า = ใช้ Good Ending Choices\nตั้งแยกได้ต่อตัวละคร เพราะแต่ละคนเกณฑ์ไม่เท่ากัน")]
    public int sanityThreshold = 50;

    [Header("Special Choice ฝั่งวิกฤต (โผล่เมื่อ Choice ปกติหมด และ Sanity < threshold) เช่น เรียก รปภ, ฉีดยาสมอง")]
    public ChoiceOptionData[] crisisChoices;

    [Header("Special Choice ฝั่งจบดี (โผล่เมื่อ Choice ปกติหมด และ Sanity >= threshold) เช่น เรียกคนไข้ถัดไป, หยุด session")]
    public ChoiceOptionData[] goodEndingChoices;
}