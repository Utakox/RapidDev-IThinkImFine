using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;

    [Header("Typing Settings")]
    [SerializeField] private float typeSpeed = 0.03f;
    [SerializeField] private float delayBeforeNext = 1f;

    private CharacterRuntime currentCharacter;
    private DialogueLine[] currentLines;
    private int lineIndex;
    private System.Action onLinesFinished; // ทำอะไรต่อหลังพูดชุดนี้จบ

    private Coroutine typingCoroutine;

    // ตัดสินใจแค่ครั้งเดียวว่าตัวละครนี้เข้าโหมด crisis หรือ good-ending ตอนไหน แล้วล็อกไว้
    // (กัน crisis/goodEnding สลับกันเองระหว่างทาง ถ้า sanity แกว่งข้าม threshold ระหว่างเล่น special pool)
    private bool specialModeDecided;
    private bool isCrisisMode;

    private void Awake()
    {
        Instance = this;
    }

    // เรียกตอนเริ่มตัวละครใหม่ (เริ่มเกม หรือ CharacterManager สลับตัว)
    public void StartCharacter(CharacterRuntime character)
    {
        currentCharacter = character;
        specialModeDecided = false; // รีเซ็ตทุกครั้งที่ขึ้นตัวละครใหม่
        speakerNameText.text = character.data.characterName;

        PlayLines(character.data.introDialogue, PlayNextChoiceRound);
    }

    // เล่นชุดบทพูด แล้วเรียก callback ที่กำหนดตอนพูดจบ
    private void PlayLines(DialogueLine[] lines, System.Action onFinished)
    {
        currentLines = lines;
        lineIndex = 0;
        onLinesFinished = onFinished;

        ChoiceManager.Instance.HideBothChoices();
        PlayCurrentLine();
    }

    private void PlayCurrentLine()
    {
        if (currentLines == null || lineIndex >= currentLines.Length)
        {
            onLinesFinished?.Invoke();
            return;
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(currentLines[lineIndex].text));
    }

    private IEnumerator TypeText(string text)
    {
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        yield return new WaitForSeconds(delayBeforeNext);

        lineIndex++;
        PlayCurrentLine();
    }

    // ========== ตรงนี้คือหัวใจของ flow ==========
    // เรียกทุกครั้งที่ต้องโชว์ choice รอบใหม่ให้ตัวละครปัจจุบัน (หลัง intro จบ และหลังตอบ choice ทุกครั้ง)
    // เช็คเองว่าตอนนี้ควรสุ่มจาก pool ไหน (ปกติ / วิกฤต / จบดี) หรือหมดจริงแล้วควรจบตา
    private void PlayNextChoiceRound()
    {
        dialogueText.text = "";

        CharacterData data = currentCharacter.data;

        // 1) ยังมี choice ปกติเหลืออยู่ (ฝั่งดีหรือฝั่งแย่ ฝั่งใดฝั่งหนึ่งก็พอ) → สุ่มรวมกันแบบเปล่าๆ ต่อ
        if (HasRemaining(data.goodChoices) || HasRemaining(data.badChoices))
        {
            ShowChoicePair(exclude => PickNormalChoice(data, exclude));
            return;
        }

        // 2) choice ปกติหมดแล้ว เลือกว่าจะใช้ special pool ฝั่งไหน
        // ตัดสินใจแค่ "ครั้งแรก" ที่มาถึงจุดนี้เท่านั้น แล้วล็อกไว้ตลอดตาของตัวละครนี้
        if (!specialModeDecided)
        {
            isCrisisMode = currentCharacter.Sanity < data.sanityThreshold;
            specialModeDecided = true;
        }

        ChoiceOptionData[] specialPool = isCrisisMode ? data.crisisChoices : data.goodEndingChoices;

        if (HasRemaining(specialPool))
        {
            ShowChoicePair(exclude => PickFromPool(specialPool, exclude));
            return;
        }

        // 3) ไม่เหลือ choice ให้เลือกอีกแล้วจริงๆ (ทั้ง pool ปกติและ special) จบตาตัวละครนี้
        EndTurn();
    }

    // เช็คว่า pool นี้ยังมี choice ที่ตัวละครปัจจุบันยังไม่เคยเลือกเหลืออยู่ไหม
    private bool HasRemaining(ChoiceOptionData[] pool)
    {
        if (pool == null) return false;

        foreach (var entry in pool)
        {
            if (!currentCharacter.HasUsedChoice(entry))
                return true;
        }

        return false;
    }

    // สุ่ม choice มา 2 อัน (ซ้าย/ขวา) ด้วยฟังก์ชันสุ่มที่ส่งเข้ามา
    // ถ้าสุ่มฝั่งขวาไม่ได้ (pool เหลือตัวเดียว) จะโชว์ตัวเดียวกันซ้ำทั้ง 2 ฝั่งแทนที่จะปล่อยว่าง
    private void ShowChoicePair(System.Func<HashSet<ChoiceOptionData>, ChoiceOptionData> picker)
    {
        var exclude = new HashSet<ChoiceOptionData>();

        ChoiceOptionData left = picker(exclude);
        if (left != null) exclude.Add(left);

        ChoiceOptionData right = picker(exclude);
        if (right == null) right = left;

        ChoiceManager.Instance.ShowChoices(left, right);
    }

    // สุ่ม 1 choice จาก choice ปกติ โดยรวม goodChoices กับ badChoices เข้าด้วยกันแล้วสุ่มเปล่าๆ
    // (ไม่ถ่วงน้ำหนักตาม sanity แล้วตามที่ขอให้ตัดฟีเจอร์นี้ออก)
    private ChoiceOptionData PickNormalChoice(CharacterData data, HashSet<ChoiceOptionData> exclude)
    {
        var candidates = new List<ChoiceOptionData>();
        AddCandidates(data.goodChoices, exclude, candidates);
        AddCandidates(data.badChoices, exclude, candidates);

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    private void AddCandidates(ChoiceOptionData[] pool, HashSet<ChoiceOptionData> exclude, List<ChoiceOptionData> candidates)
    {
        if (pool == null) return;

        foreach (var entry in pool)
        {
            if (exclude.Contains(entry)) continue;
            if (currentCharacter.HasUsedChoice(entry)) continue;
            candidates.Add(entry);
        }
    }

    // สุ่ม 1 choice จาก pool ที่ระบุ เฉพาะตัวที่ตัวละครนี้ยังไม่เคยเลือก และไม่ซ้ำกับ exclude ของตานี้
    private ChoiceOptionData PickFromPool(ChoiceOptionData[] pool, HashSet<ChoiceOptionData> exclude)
    {
        if (pool == null || pool.Length == 0)
            return null;

        var candidates = new List<ChoiceOptionData>();
        foreach (var entry in pool)
        {
            if (exclude.Contains(entry))
                continue;

            if (currentCharacter.HasUsedChoice(entry))
                continue;

            candidates.Add(entry);
        }

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    // เรียกจาก ChoiceManager หลังผู้เล่นชี้ค้างเลือกฝั่งไหนแล้ว
    public void OnChoicePicked(ChoiceOptionData picked)
    {
        currentCharacter.MarkChoiceUsed(picked); // ตัดออกจากคลังของตัวละครนี้ถาวร
        currentCharacter.ChangeSanity(picked.sanityChange);

        // เล่นบทพิเศษถ้ามี แล้ววนกลับไปเช็ค/โชว์ choice รอบถัดไปต่อ (ไม่ใช่จบตาทันที)
        if (picked.afterDialogue != null && picked.afterDialogue.Length > 0)
            PlayLines(picked.afterDialogue, PlayNextChoiceRound);
        else
            PlayNextChoiceRound();
    }

    // เรียกจาก PlayNextChoiceRound เมื่อไม่เหลือ choice ให้เลือกอีกแล้วจริงๆ (ทั้ง pool ปกติและ special)
    private void EndTurn()
    {
        Debug.Log(currentCharacter.data.characterName + " จบตาแล้ว");
        CharacterManager.Instance.NextCharacter();
    }
}