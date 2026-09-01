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

    [Header("เพลงพื้นหลังปกติ (คลอไปเรื่อยๆ ตอน sanity ยังไม่ต่ำกว่าเกณฑ์)")]
    [SerializeField] private AudioSource normalMusicSource;
    [SerializeField] private AudioClip[] normalMusicList;
    [Range(0f, 1f)][SerializeField] private float normalVolume = 0.6f;

    [Header("เสียง Mental State — ค่า Default (ใช้กับตัวละครที่ไม่ได้ตั้งเสียงของตัวเองใน CharacterData)")]
    [SerializeField] private AudioSource mentalStateMusicSource;
    [SerializeField] private MentalStateSound defaultMentalStateSound;

    [Header("Effect (GameObject) ตอนอยู่ในสถานะ Mental State — ค่า Default")]
    [Tooltip("จะ SetActive(true) ให้ทุกอันในลิสต์ตอนเข้า Mental State และ SetActive(false) ทั้งหมดตอนออก/จบตัวละคร")]
    [SerializeField] private GameObject[] defaultMentalStateEffects;

    [Header("Crossfade ระหว่างเพลงปกติ <-> เพลง Mental State")]
    [SerializeField] private float musicCrossfadeTime = 1f;

    private CharacterRuntime currentCharacter;
    private DialogueLine[] currentLines;
    private System.Action onLinesFinished;
    private Coroutine typingCoroutine;

    private bool specialModeDecided;
    private bool isCrisisMode;

    // true = รอบนี้กำลังโชว์คู่จาก choice ปกติ (good/bad ตาม index) ไม่ใช่ special pool
    // ใช้ตอน OnChoicePicked เพื่อรู้ว่าควรขยับ index ของ choice ปกติต่อไหม
    private bool currentRoundIsNormal;

    // true = รอบนี้กำลังโชว์คู่จาก special pool (crisisChoices/goodEndingChoices)
    // เลือกแค่อันเดียวจากพูลนี้ก็จบตาทันที ไม่ต้องเลือกให้ครบทั้งพูล
    private bool currentRoundIsSpecialEnding;

    // ===== Background Music (ปกติ / Mental State) =====
    private bool isInMentalState;
    private Coroutine normalMusicFadeCoroutine;
    private Coroutine mentalStateFadeCoroutine;
    private GameObject[] activeMentalStateEffects;

    // ===== กันไม่ให้ good/bad อยู่ฝั่งเดิมติดกันเกิน 2 รอบ (ลดโอกาสเดาทางได้) =====
    private bool lastGoodOnLeft;
    private int sameSideStreak;

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

        // เพลงปกติเริ่มเล่นคลอตั้งแต่ต้นเกม แล้วจะถูก crossfade ทับตอนเข้า/ออก mental state
        StartNormalMusicImmediate();
    }

    private void StartNormalMusicImmediate()
    {
        if (normalMusicSource == null) return;
        if (normalMusicList == null || normalMusicList.Length == 0) return;

        AudioClip clip = normalMusicList[Random.Range(0, normalMusicList.Length)];
        normalMusicSource.clip = clip;
        normalMusicSource.loop = true;
        normalMusicSource.volume = normalVolume;
        normalMusicSource.Play();
    }

    public void StartCharacter(CharacterRuntime character)
    {
        currentCharacter = character;
        specialModeDecided = false;
        speakerNameText.text = character.data.characterName;

        // ตัวละครใหม่ = เริ่มนับ mental state ใหม่ เผื่อ sanity เริ่มต้นต่ำกว่าเกณฑ์อยู่แล้ว
        isInMentalState = false;
        UpdateMentalStateMusic();

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

            float speed = line.typeSpeedOverride > 0f ? line.typeSpeedOverride : typeSpeed;
            AudioClip loop = line.typingLoopOverride != null ? line.typingLoopOverride : defaultTypingLoop;

            yield return Typewriter.TypeLine(dialogueText, line.text, speed, typingSource, loop, typingVolume, typingFadeOut);

            float delay = line.delayAfterOverride >= 0f ? line.delayAfterOverride : delayBeforeNext;
            yield return new WaitForSeconds(delay);
        }

        SetImagesActive(false);
        typingCoroutine = null;
        onLinesFinished?.Invoke();
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

        // 1) ยังมี choice ปกติเหลืออยู่ → จับคู่ good[idx] กับ bad[idx] ตามลำดับ ไม่สุ่มว่าจะได้คู่ไหน
        //    (สุ่มแค่ว่าใครอยู่ซ้าย/ขวา)
        int idx = currentCharacter.NormalChoiceIndex;
        ChoiceOptionData good = (data.goodChoices != null && idx < data.goodChoices.Length) ? data.goodChoices[idx] : null;
        ChoiceOptionData bad = (data.badChoices != null && idx < data.badChoices.Length) ? data.badChoices[idx] : null;

        if (good != null || bad != null)
        {
            currentRoundIsNormal = true;
            currentRoundIsSpecialEnding = false;
            ShowPairedChoice(good, bad);
            return;
        }

        currentRoundIsNormal = false;

        if (!specialModeDecided)
        {
            isCrisisMode = currentCharacter.Sanity < data.sanityThreshold;
            specialModeDecided = true;
        }

        ChoiceOptionData[] specialPool = isCrisisMode ? data.crisisChoices : data.goodEndingChoices;

        if (HasRemaining(specialPool))
        {
            currentRoundIsSpecialEnding = true;
            ShowChoicePair(exclude => PickFromPool(specialPool, exclude));
            return;
        }

        currentRoundIsSpecialEnding = false;
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

    // โชว์คู่ good/bad ของรอบนี้ สุ่มแค่ว่าใครอยู่ซ้ายใครอยู่ขวา (ไม่สุ่มว่าจะได้ choice ไหน)
    // ถ้า goodChoices/badChoices ยาวไม่เท่ากันแล้วรอบนี้เหลือฝั่งเดียว จะโชว์อันเดียวไปก่อน
    private void ShowPairedChoice(ChoiceOptionData good, ChoiceOptionData bad)
    {
        ChoiceOptionData left, right;

        if (good != null && bad != null)
        {
            bool goodOnLeft;

            // ถ้าฝั่งเดิมติดกันมา 2 รอบแล้ว บังคับสลับฝั่ง กันคนเดาทางจากตำแหน่งได้
            if (sameSideStreak >= 2)
                goodOnLeft = !lastGoodOnLeft;
            else
                goodOnLeft = Random.value < 0.5f;

            sameSideStreak = (goodOnLeft == lastGoodOnLeft) ? sameSideStreak + 1 : 1;
            lastGoodOnLeft = goodOnLeft;

            if (goodOnLeft) { left = good; right = bad; }
            else { left = bad; right = good; }
        }
        else
        {
            left = good != null ? good : bad;
            right = null;
        }

        ChoiceManager.Instance.ShowChoices(left, right);
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
        UpdateMentalStateMusic();

        // เป็นรอบ choice ปกติ (ไม่ใช่ special pool) → ขยับไปคู่ index ถัดไปเสมอ
        // ไม่ว่าจะเลือกฝั่ง good หรือ bad ก็ถือว่าคู่นี้จบแล้ว ไปคู่ถัดไป
        if (currentRoundIsNormal)
        {
            currentCharacter.AdvanceNormalChoiceIndex();

            if (picked.afterDialogue != null && picked.afterDialogue.Length > 0)
                PlayLines(picked.afterDialogue, PlayNextChoiceRound);
            else
                PlayNextChoiceRound();
            return;
        }

        // รอบ Ending (crisisChoices/goodEndingChoices) → เลือกอันเดียวจบ ไม่ต้องเลือกให้ครบพูล
        if (currentRoundIsSpecialEnding)
        {
            if (picked.afterDialogue != null && picked.afterDialogue.Length > 0)
                PlayLines(picked.afterDialogue, EndTurn);
            else
                EndTurn();
            return;
        }

        if (picked.afterDialogue != null && picked.afterDialogue.Length > 0)
            PlayLines(picked.afterDialogue, PlayNextChoiceRound);
        else
            PlayNextChoiceRound();
    }

    private void EndTurn()
    {
        ClearDialogue();

        // จบตัวละครนี้แล้ว เพลง mental state ต้องหายไปไม่ว่าจะยังอยู่ในสถานะนั้นหรือไม่
        // (crossfade กลับไปเป็นเพลงปกติ)
        if (isInMentalState)
        {
            isInMentalState = false;
            ExitMentalState();
        }

        Debug.Log(currentCharacter.data.characterName + " จบตาแล้ว");
        CharacterManager.Instance.NextCharacter();
    }

    // ========== Background Music: ปกติ <-> Mental State (crossfade) ==========

    private void UpdateMentalStateMusic()
    {
        if (currentCharacter == null) return;

        bool shouldPlay = currentCharacter.Sanity < currentCharacter.data.mentalStateThreshold;
        if (shouldPlay == isInMentalState) return;

        isInMentalState = shouldPlay;

        if (shouldPlay) EnterMentalState();
        else ExitMentalState();
    }

    private void EnterMentalState()
    {
        // เพลงปกติค่อยๆเบาลง
        FadeAudioSource(normalMusicSource, ref normalMusicFadeCoroutine, 0f, musicCrossfadeTime, stopWhenSilent: false);

        // พร้อมกับเสียง mental state ค่อยๆดังขึ้นมาแทน (ถ้าตัวละครมีเสียงของตัวเองใช้อันนั้นก่อน ไม่งั้นใช้ default)
        // volume เป้าหมายก็มาจากตัวเสียงนั้นๆเอง ไม่ได้ fix ค่าเดียวรวมทุกตัวละครแล้ว
        MentalStateSound sound = GetActiveMentalStateSound();
        if (mentalStateMusicSource != null && sound.clip != null)
        {
            mentalStateMusicSource.clip = sound.clip;
            mentalStateMusicSource.loop = true;
            mentalStateMusicSource.volume = 0f;
            mentalStateMusicSource.Play();
            FadeAudioSource(mentalStateMusicSource, ref mentalStateFadeCoroutine, sound.volume, musicCrossfadeTime, stopWhenSilent: false);
        }

        // เปิด effect ของ mental state ทุกอันในลิสต์ (ตัวละครนี้มีของตัวเองใช้อันนั้นก่อน ไม่งั้นใช้ default)
        GameObject[] effects = GetActiveMentalStateEffects();
        if (effects != null && effects.Length > 0)
        {
            foreach (var effect in effects)
            {
                if (effect == null) continue;
                effect.SetActive(true);
            }
            activeMentalStateEffects = effects;
        }
    }

    // ตัวละครมี mentalStateSoundOverride ของตัวเอง (ตั้ง clip ไว้ใน CharacterData) → ใช้อันนั้น (พร้อม volume ของมันเอง)
    // ไม่งั้น fallback ไปใช้ defaultMentalStateSound ที่เป็น default ของ DialogueManager
    private MentalStateSound GetActiveMentalStateSound()
    {
        if (currentCharacter != null && currentCharacter.data.mentalStateSoundOverride.clip != null)
            return currentCharacter.data.mentalStateSoundOverride;

        return defaultMentalStateSound;
    }

    // ตัวละครมี mentalStateEffectOverride ของตัวเอง (ใส่ไว้อย่างน้อย 1 อัน) → ใช้ลิสต์นั้น
    // ไม่งั้น fallback ไปใช้ defaultMentalStateEffects ที่เป็น default ของ DialogueManager
    private GameObject[] GetActiveMentalStateEffects()
    {
        if (currentCharacter != null
            && currentCharacter.data.mentalStateEffectOverride != null
            && currentCharacter.data.mentalStateEffectOverride.Length > 0)
        {
            return currentCharacter.data.mentalStateEffectOverride;
        }

        return defaultMentalStateEffects;
    }

    private void ExitMentalState()
    {
        // เพลง mental state ค่อยๆเบาลงแล้วหยุด
        FadeAudioSource(mentalStateMusicSource, ref mentalStateFadeCoroutine, 0f, musicCrossfadeTime, stopWhenSilent: true);

        // พร้อมกับเพลงปกติค่อยๆดังกลับมา (เผื่อโดนหยุดไปก่อนหน้า เช่น ตอนจบตา)
        if (normalMusicSource != null)
        {
            if (!normalMusicSource.isPlaying) normalMusicSource.Play();
            FadeAudioSource(normalMusicSource, ref normalMusicFadeCoroutine, normalVolume, musicCrossfadeTime, stopWhenSilent: false);
        }

        // ปิด effect ของ mental state ทุกอันที่เปิดค้างไว้
        if (activeMentalStateEffects != null)
        {
            foreach (var effect in activeMentalStateEffects)
            {
                if (effect == null) continue;
                effect.SetActive(false);
            }
            activeMentalStateEffects = null;
        }
    }

    private void FadeAudioSource(AudioSource source, ref Coroutine handle, float targetVolume, float duration, bool stopWhenSilent)
    {
        if (source == null) return;
        if (handle != null) StopCoroutine(handle);
        handle = StartCoroutine(FadeAudioSourceRoutine(source, targetVolume, duration, stopWhenSilent));
    }

    private IEnumerator FadeAudioSourceRoutine(AudioSource source, float targetVolume, float duration, bool stopWhenSilent)
    {
        float startVolume = source.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, duration <= 0f ? 1f : t / duration);
            yield return null;
        }

        source.volume = targetVolume;
        if (stopWhenSilent && targetVolume <= 0f) source.Stop();
    }
}