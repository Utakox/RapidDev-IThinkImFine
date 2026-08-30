using UnityEngine;

// 1 เกณฑ์ Sanity ที่จะเล่นบทพูดพิเศษแยกต่างหาก (เล่นแค่ครั้งเดียวต่อคนไข้ 1 คน)
// ต่างจาก DialogueLine.sanityOverrides ตรงที่อันนี้ "trigger เองอัตโนมัติ" ทันทีที่ Sanity
// เข้าเกณฑ์เป็นครั้งแรก ไม่ต้องรอให้บทพูดเดิมบทไหนถูกพูดอยู่พอดี
[System.Serializable]
public class SanityDialogueTrigger
{
    public enum TriggerDirection
    {
        AtOrBelow, // Sanity <= ค่านี้ (ใช้บ่อยสุด เช่น ตกลงไปถึงจุดวิกฤต)
        AtOrAbove  // Sanity >= ค่านี้ (เช่น ฟื้นขึ้นมาถึงจุดที่ดีพอ)
    }

    [Header("เกณฑ์ Sanity ที่จะ trigger (เลขเดียว)")]
    public int sanityThreshold = 50;

    [Tooltip("AtOrBelow = trigger ทันทีที่ Sanity ลดลงมาถึงค่านี้หรือต่ำกว่า (ปกติใช้แบบนี้)\nAtOrAbove = trigger ทันทีที่ Sanity เพิ่มขึ้นมาถึงค่านี้หรือสูงกว่า\n\n⚠ ระวัง: ถ้าตั้ง AtOrBelow ไว้ที่ค่าสูงๆ ใกล้ 100 มันจะ trigger ทันทีตั้งแต่ intro จบเลย เพราะ Sanity เริ่มต้นก็ <= ค่านั้นอยู่แล้ว")]
    public TriggerDirection direction = TriggerDirection.AtOrBelow;

    [Header("บทพูดพิเศษ (เล่นแค่ครั้งเดียวต่อคนไข้ 1 คน)")]
    public DialogueLine[] dialogue;

    // เช็คว่า sanity ปัจจุบันเข้าเกณฑ์นี้ไหม ตามทิศทางที่เลือก
    public bool IsMet(int currentSanity)
    {
        return direction == TriggerDirection.AtOrBelow
            ? currentSanity <= sanityThreshold
            : currentSanity >= sanityThreshold;
    }
}

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

    [Header("--- Dialogue พิเศษตาม Sanity (เล่นครั้งเดียวต่อคน ตอนที่ Sanity เข้าเกณฑ์เป็นครั้งแรก) ---")]
    [Tooltip("เช็คทุกครั้งก่อนโชว์ choice รอบใหม่ (หลัง intro จบ, หลังตอบทุก choice) ถ้า Sanity ตอนนั้นเข้าเกณฑ์ไหนที่ยังไม่เคยเล่น จะเล่นบทนั้นก่อน\nรองรับหลายอัน เช่น อันนึงตั้ง 60 (AtOrBelow) อีกอันตั้ง 25 (AtOrBelow) แต่ละอันเล่นแค่ครั้งเดียว")]
    public SanityDialogueTrigger[] sanityDialogueTriggers;

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