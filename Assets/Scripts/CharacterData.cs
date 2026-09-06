using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Character")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    [Range(0, 100)] public int startingSanity = 50;

    [Header("=== จอดำเล่าเรื่องก่อนเริ่มตัวละครนี้ ===")]
    public NarrationSequence introNarration;

    [Header("=== รูปแฟ้มประวัติ (ใช้เฉพาะระบบ Patient History / Hold Inspect เท่านั้น) ===")]
    [Tooltip("รูปที่โชว์ตอนกด Hold Inspect ดูแฟ้มประวัติคนไข้คนนี้")]
    public Sprite inspectSprite;

    [Header("=== รูปตัวละคร (คนละอันกับ inspectSprite ด้านบน ใช้ที่อื่นได้ตามต้องการ) ===")]
    public Sprite portraitSprite;

    [Header("หน้าตัวละครแต่ละช่วง Sanity")]
    public Sprite faceHigh;   // > 75
    public Sprite faceMid;    // 51 - 75
    public Sprite faceLow;    // 26 - 50
    public Sprite faceBroken; // <= 25

    [Header("บทพูดตอนเริ่ม (Startup)")]
    public DialogueLine[] introDialogue;

    [Header("--- Dialogue พิเศษตาม Sanity (ครั้งเดียวต่อคน) ---")]
    public SanityDialogueTrigger[] sanityDialogueTriggers;

    [Header("--- Choice ปกติ (จับคู่ good[i] กับ bad[i] ตามลำดับ) ---")]
    public ChoiceOptionData[] goodChoices;
    public ChoiceOptionData[] badChoices;

    [Header("--- เกณฑ์ตัดสินตอน Choice ปกติหมด ---")]
    [Tooltip("Sanity < ค่านี้ = ใช้ Crisis Choices / >= = ใช้ Good Ending Choices")]
    public int sanityThreshold = 50;

    public ChoiceOptionData[] crisisChoices;
    public ChoiceOptionData[] goodEndingChoices;

    [Header("=== Mental State (Meltdown) ===")]
    [Tooltip("Sanity ต่ำกว่าค่านี้ = เข้าสถานะ Mental State (เปลี่ยนเพลง + เปิดเอฟเฟกต์ + dialogue สั่นตลอด)")]
    [Range(0, 100)] public int mentalStateThreshold = 25;

    [Tooltip("เสียง Mental State เฉพาะตัวละครนี้ — ไม่ใส่ = ใช้ default ของ DialogueManager")]
    public AudioClip mentalStateClipOverride;

    [Header("=== จอดำสรุปตอนจบตา ===")]
    [Tooltip("เล่นเมื่อรอบ Ending นี้ไปจบที่ pool goodEndingChoices (Sanity ไม่ต่ำกว่าเกณฑ์ตอนตัดสิน)")]
    public NarrationSequence goodEndingNarration;

    [Tooltip("เล่นเมื่อรอบ Ending นี้ไปจบที่ pool crisisChoices (Sanity ต่ำกว่าเกณฑ์ตอนตัดสิน)")]
    public NarrationSequence crisisEndingNarration;
}