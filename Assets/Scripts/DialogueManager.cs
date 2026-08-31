using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Image ที่โชว์ตอนกำลังพูด (ไม่พูด = ปิดหมด)")]
    [SerializeField] private Image[] dialogueImages;

    [Header("Typing Settings (default เมื่อบรรทัดนั้นไม่ override)")]
    [SerializeField] private float typeSpeed = 0.03f;
    [SerializeField] private float delayBeforeNext = 1f;

    [Header("เสียงตอนพิมพ์บทพูด (หยุดพร้อมข้อความจบ)")]
    [SerializeField] private AudioSource typingSource;
    [SerializeField] private AudioClip defaultTypingLoop;
    [Range(0f, 1f)][SerializeField] private float typingVolume = 0.8f;
    [SerializeField] private float typingFadeOut = 0.15f;

    private CharacterRuntime currentCharacter;
    private DialogueLine[] currentLines;
    private System.Action onLinesFinished;
    private Coroutine typingCoroutine;

    private bool specialModeDecided;
    private bool isCrisisMode;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (typingSource != null)
        {
            typingSource.playOnAwake = false;
            typingSource.loop = true;
            typingSource.Stop();
        }

        SetImagesActive(false);
    }

    public void StartCharacter(CharacterRuntime character)
    {
        currentCharacter = character;
        specialModeDecided = false;
        speakerNameText.text = character.data.characterName;

        PlayLines(character.data.introDialogue, PlayNextChoiceRound);
    }

    // ========== Image ==========

    private void SetImagesActive(bool on)
    {
        if (dialogueImages == null) return;
        foreach (var img in dialogueImages)
        {
            if (img == null) continue;
            img.gameObject.SetActive(on);
        }
    }

    // ========== เล่นบทพูด ==========

    private void PlayLines(DialogueLine[] lines, System.Action onFinished)
    {
        currentLines = lines;
        onLinesFinished = onFinished;

        ChoiceManager.Instance.HideBothChoices();

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(PlayLinesRoutine());
    }

    private IEnumerator PlayLinesRoutine()
    {
        int count = currentLines != null ? currentLines.Length : 0;
        SetImagesActive(count > 0);

        for (int i = 0; i < count; i++)
        {
            DialogueLine line = currentLines[i];
            if (line == null) continue;

            if (TextShakeEffect.Instance != null)
                TextShakeEffect.Instance.SetShaking(line.shakeText, line.shakeIntensity);

            // ใส่ข้อความเต็มก่อน แล้วเปิดทีละตัว -> สระ/วรรณยุกต์ไทยไม่แตก + rich text ใช้ได้
            dialogueText.text = line.text;
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.ForceMeshUpdate(true, true);

            int total = dialogueText.textInfo.characterCount;
            float speed = line.typeSpeedOverride > 0f ? line.typeSpeedOverride : typeSpeed;

            StartTypingSound(line);

            for (int c = 1; c <= total; c++)
            {
                dialogueText.maxVisibleCharacters = c;
                yield return new WaitForSeconds(speed);
            }
            dialogueText.maxVisibleCharacters = total;

            yield return StopTypingSound();   // เสียงจบพร้อมข้อความบรรทัดนี้

            float delay = line.delayAfterOverride >= 0f ? line.delayAfterOverride : delayBeforeNext;
            yield return new WaitForSeconds(delay);
        }

        SetImagesActive(false);
        typingCoroutine = null;
        onLinesFinished?.Invoke();
    }

    private void StartTypingSound(DialogueLine line)
    {
        if (typingSource == null) return;

        AudioClip clip = line.typingLoopOverride != null ? line.typingLoopOverride : defaultTypingLoop;
        if (clip == null) return;

        typingSource.clip = clip;
        typingSource.volume = typingVolume;
        typingSource.loop = true;
        typingSource.Play();
    }

    private IEnumerator StopTypingSound()
    {
        if (typingSource == null || !typingSource.isPlaying) yield break;

        float start = typingSource.volume;
        float t = 0f;
        while (t < typingFadeOut)
        {
            t += Time.deltaTime;
            typingSource.volume = Mathf.Lerp(start, 0f, t / typingFadeOut);
            yield return null;
        }
        typingSource.Stop();
        typingSource.volume = start;
    }

    private void ClearDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (typingSource != null) typingSource.Stop();

        if (TextShakeEffect.Instance != null)
            TextShakeEffect.Instance.SetShaking(false);

        SetImagesActive(false);
        dialogueText.text = string.Empty;
        dialogueText.maxVisibleCharacters = int.MaxValue;
        dialogueText.ForceMeshUpdate(true, true);
    }

    // ========== flow หลัก ==========

    private void PlayNextChoiceRound()
    {
        if (TryPlaySanityDialogue(PlayNextChoiceRound)) return;

        ClearDialogue();
        CharacterData data = currentCharacter.data;

        if (HasRemaining(data.goodChoices) || HasRemaining(data.badChoices))
        {
            ShowChoicePair(exclude => PickNormalChoice(data, exclude));
            return;
        }

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

        EndTurn();
    }

    private bool TryPlaySanityDialogue(System.Action onAfter)
    {
        var triggers = currentCharacter.data.sanityDialogueTriggers;
        if (triggers == null) return false;

        foreach (var trigger in triggers)
        {
            if (trigger == null) continue;
            if (currentCharacter.HasTriggeredSanityDialogue(trigger)) continue;
            if (!trigger.IsMet(currentCharacter.Sanity)) continue;

            currentCharacter.MarkSanityDialogueTriggered(trigger);

            if (trigger.dialogue == null || trigger.dialogue.Length == 0) continue;

            PlayLines(trigger.dialogue, onAfter);
            return true;
        }
        return false;
    }

    // ========== สุ่ม choice ==========

    private bool HasRemaining(ChoiceOptionData[] pool)
    {
        if (pool == null) return false;
        foreach (var entry in pool)
        {
            if (entry == null) continue;
            if (!currentCharacter.HasUsedChoice(entry)) return true;
        }
        return false;
    }

    private void ShowChoicePair(System.Func<HashSet<ChoiceOptionData>, ChoiceOptionData> picker)
    {
        var exclude = new HashSet<ChoiceOptionData>();

        ChoiceOptionData left = picker(exclude);
        if (left != null) exclude.Add(left);

        ChoiceOptionData right = picker(exclude);

        // เหลืออันเดียว = โชว์ฝั่งเดียว ดีกว่าโชว์ซ้ำ 2 ฝั่งแบบเดิม
        ChoiceManager.Instance.ShowChoices(left, right);
    }

    private ChoiceOptionData PickNormalChoice(CharacterData data, HashSet<ChoiceOptionData> exclude)
    {
        var candidates = new List<ChoiceOptionData>();
        AddCandidates(data.goodChoices, exclude, candidates);
        AddCandidates(data.badChoices, exclude, candidates);
        return candidates.Count == 0 ? null : candidates[Random.Range(0, candidates.Count)];
    }

    private ChoiceOptionData PickFromPool(ChoiceOptionData[] pool, HashSet<ChoiceOptionData> exclude)
    {
        var candidates = new List<ChoiceOptionData>();
        AddCandidates(pool, exclude, candidates);
        return candidates.Count == 0 ? null : candidates[Random.Range(0, candidates.Count)];
    }

    private void AddCandidates(ChoiceOptionData[] pool, HashSet<ChoiceOptionData> exclude, List<ChoiceOptionData> candidates)
    {
        if (pool == null) return;
        foreach (var entry in pool)
        {
            if (entry == null) continue;
            if (exclude.Contains(entry)) continue;
            if (currentCharacter.HasUsedChoice(entry)) continue;
            candidates.Add(entry);
        }
    }

    // ========== callback ==========

    public void OnChoicePicked(ChoiceOptionData picked)
    {
        currentCharacter.MarkChoiceUsed(picked);
        currentCharacter.ChangeSanity(picked.sanityChange);

        if (picked.afterDialogue != null && picked.afterDialogue.Length > 0)
            PlayLines(picked.afterDialogue, PlayNextChoiceRound);
        else
            PlayNextChoiceRound();
    }

    private void EndTurn()
    {
        ClearDialogue();
        Debug.Log(currentCharacter.data.characterName + " จบตาแล้ว");
        CharacterManager.Instance.NextCharacter();
    }
}