using UnityEngine;

[System.Serializable]
public class SanityDialogueTrigger
{
    public enum TriggerDirection { AtOrBelow, AtOrAbove }

    [Header("เกณฑ์ Sanity ที่จะ trigger")]
    public int sanityThreshold = 50;

    [Tooltip("AtOrBelow = trigger เมื่อ Sanity <= ค่านี้\nAtOrAbove = trigger เมื่อ Sanity >= ค่านี้\n\n⚠ ตั้ง AtOrBelow ใกล้ 100 จะ trigger ทันทีตั้งแต่ intro จบ")]
    public TriggerDirection direction = TriggerDirection.AtOrBelow;

    [Header("บทพูดพิเศษ (เล่นครั้งเดียวต่อคนไข้)")]
    public DialogueLine[] dialogue;

    public bool IsMet(int currentSanity)
    {
        return direction == TriggerDirection.AtOrBelow
            ? currentSanity <= sanityThreshold
            : currentSanity >= sanityThreshold;
    }
}

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Character")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    [Range(0, 100)] public int startingSanity = 50;

    [Header("=== จอดำเล่าเรื่องก่อนเริ่มตัวละครนี้ ===")]
    public NarrationSequence introNarration;

    [Header("หน้าตัวละครแต่ละช่วง Sanity")]
    public Sprite faceHigh;   // > 75
    public Sprite faceMid;    // 51 - 75
    public Sprite faceLow;    // 26 - 50
    public Sprite faceBroken; // <= 25

    [Header("บทพูดตอนเริ่ม (Startup)")]
    public DialogueLine[] introDialogue;

    [Header("--- Dialogue พิเศษตาม Sanity (ครั้งเดียวต่อคน) ---")]
    public SanityDialogueTrigger[] sanityDialogueTriggers;

    [Header("--- Choice ปกติ (สุ่มรวม 2 คลัง ไม่ถ่วงน้ำหนัก) ---")]
    public ChoiceOptionData[] goodChoices;
    public ChoiceOptionData[] badChoices;

    [Header("--- เกณฑ์ตัดสินตอน Choice ปกติหมด ---")]
    [Tooltip("Sanity < ค่านี้ = ใช้ Crisis Choices / >= = ใช้ Good Ending Choices")]
    public int sanityThreshold = 50;

    public ChoiceOptionData[] crisisChoices;
    public ChoiceOptionData[] goodEndingChoices;

    [Header("--- Mental State (เพลง/สถานะ Sanity ต่ำ) ---")]
    [Tooltip("Sanity ของตัวละครนี้ < ค่านี้ = เข้าสถานะ Mental State (เพลง/effect เปลี่ยน)\nแยกจากเกณฑ์ Crisis Choices ด้านบน ตั้งไม่เท่ากันได้ต่อตัวละคร")]
    public int mentalStateThreshold = 50;

    [Tooltip("เสียง Mental State เฉพาะตัวละครนี้ (ไม่บังคับใส่)\nถ้าไม่ใส่ clip (ปล่อยว่าง) จะใช้เสียง default จาก DialogueManager แทน\nปรับ volume ของตัวละครนี้แยกจากตัวอื่นได้ ใส่ทีหลังได้ ไม่กระทบตัวละครอื่น")]
    public MentalStateSound mentalStateSoundOverride;

    [Tooltip("Effect (GameObject) เฉพาะตัวละครนี้ ตอนเข้า Mental State จะ SetActive(true) ให้ทุกอันในลิสต์ / ออกแล้ว SetActive(false) ทั้งหมด\nถ้าไม่ใส่ (ปล่อยว่าง) จะใช้ effect default จาก DialogueManager แทน")]
    public GameObject[] mentalStateEffectOverride;
}