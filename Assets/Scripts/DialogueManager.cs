using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [System.Serializable]
    public struct UIConfig
    {
        public TextMeshProUGUI speakerNameText;
        public TextMeshProUGUI dialogueText;
        public Image[] dialogueImages;
        public TextShakeEffect dialogueShake;
        public TextGlitchEffect dialogueGlitch;

        [Header("=== Patient Sanity UI ===")]
        public Slider patientSanitySlider;
        public TextMeshProUGUI patientSanityText;
    }

    [System.Serializable]
    public struct TypingConfig
    {
        public float typeSpeed;
        public float delayBeforeNext;
        public AudioSource typingSource;
        public AudioClip defaultTypingLoop;
        [Range(0f, 1f)] public float typingVolume;
        public float typingFadeOut;
    }

    [System.Serializable]
    public struct MusicConfig
    {
        public AudioSource normalMusicSource;
        public AudioClip[] normalMusicList;
        [Range(0f, 1f)] public float normalVolume;
        public AudioSource mentalStateMusicSource;
        public AudioClip defaultMentalStateClip;
        [Range(0f, 1f)] public float mentalStateVolume;
        public float musicCrossfadeTime;
    }

    [System.Serializable]
    public struct MentalStateConfig
    {
        [Tooltip("ตัวละครไหนไม่ตั้ง Mental State Effect Override บน CharacterRuntime จะใช้ลิสต์นี้")]
        public GameObject[] defaultMentalStateEffects;
    }

    [Header("=== Group Configurations ===")]
    [SerializeField] private UIConfig ui;
    [SerializeField] private TypingConfig typing;
    [SerializeField] private MusicConfig music;
    [SerializeField] private MentalStateConfig mentalState;

    [Header("Sanity หมอต่ำ (Glitch) — โอกาสที่การรักษาจะถูกตัดจบกลางคันในแต่ละรอบ")]
    [Range(0f, 1f)] [SerializeField] private float glitchForceEndChancePerRound = 0.08f;

    private CharacterRuntime currentCharacter;
    private DialogueLine[] currentLines;
    private System.Action onLinesFinished;
    private Coroutine typingCoroutine;

    private bool specialModeDecided;
    private bool isCrisisMode;
    private bool currentRoundIsNormal;
    private bool currentRoundIsSpecialEnding;

    private bool isInMentalState;
    public bool IsInMentalState => isInMentalState;
    private Coroutine normalMusicFadeCoroutine;
    private Coroutine mentalStateFadeCoroutine;
    private GameObject[] activeMentalStateEffects;

    private bool lastGoodOnLeft;
    private int sameSideStreak;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (typing.typingSource != null)
        {
            typing.typingSource.playOnAwake = false;
            typing.typingSource.loop = true;
            typing.typingSource.Stop();
        }

        if (ui.dialogueShake == null && ui.dialogueText != null)
            ui.dialogueShake = ui.dialogueText.GetComponent<TextShakeEffect>();

        if (ui.dialogueGlitch == null && ui.dialogueText != null)
            ui.dialogueGlitch = ui.dialogueText.GetComponent<TextGlitchEffect>();

        SetImagesActive(false);
        StartNormalMusicImmediate();
    }

    private void StartNormalMusicImmediate()
    {
        if (music.normalMusicSource == null || music.normalMusicList == null || music.normalMusicList.Length == 0) return;

        AudioClip clip = music.normalMusicList[Random.Range(0, music.normalMusicList.Length)];
        music.normalMusicSource.clip = clip;
        music.normalMusicSource.loop = true;
        music.normalMusicSource.volume = music.normalVolume;
        music.normalMusicSource.Play();
    }

    public void StartCharacter(CharacterRuntime character)
    {
        currentCharacter = character;
        specialModeDecided = false;
        isCrisisMode = false;

        if (ui.speakerNameText != null && character != null && character.data != null)
            ui.speakerNameText.text = character.data.characterName;

        isInMentalState = false;
        UpdateMentalStateMusic();
        UpdatePatientSanityUI();

        PlayLines(character.data.introDialogue, PlayNextChoiceRound);
    }

    public void UpdatePatientSanityUI()
    {
        if (currentCharacter == null) return;

        if (ui.patientSanitySlider != null)
        {
            ui.patientSanitySlider.minValue = 0;
            ui.patientSanitySlider.maxValue = 100;
            ui.patientSanitySlider.value = currentCharacter.Sanity;
        }

        if (ui.patientSanityText != null)
        {
            ui.patientSanityText.text = $"Sanity: {currentCharacter.Sanity}";
        }
    }

    private void SetImagesActive(bool on)
    {
        if (ui.dialogueImages == null) return;
        foreach (var img in ui.dialogueImages)
        {
            if (img == null) continue;
            img.gameObject.SetActive(on);
        }
    }

    private void PlayLines(DialogueLine[] lines, System.Action onFinished)
    {
        currentLines = lines;
        onLinesFinished = onFinished;

        if (ChoiceManager.Instance != null)
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

            // เอฟเฟกต์สั่น: หากอยู่ในสภาวะ Meltdown จะเปิดสั่นเสมอ
            if (ui.dialogueShake != null)
            {
                if (isInMentalState)
                    ui.dialogueShake.SetShaking(true);
                else
                    ui.dialogueShake.SetShaking(line.shakeText, line.shakeIntensity);
            }

            // เอฟเฟกต์ Glitch: ถ้าคนไข้ Meltdown อยู่ จะบังคับปิด Glitch เอาแค่สั่นอย่างเดียว
            if (ui.dialogueGlitch != null)
            {
                ui.dialogueGlitch.SetBaseText(line.text);
                bool doctorGlitching = DoctorSanityManager.Instance != null && DoctorSanityManager.Instance.IsGlitching;
                ui.dialogueGlitch.SetGlitching(doctorGlitching && !isInMentalState);
            }

            float speed = line.typeSpeedOverride > 0f ? line.typeSpeedOverride : typing.typeSpeed;
            AudioClip loop = line.typingLoopOverride != null ? line.typingLoopOverride : typing.defaultTypingLoop;

            yield return Typewriter.TypeLine(ui.dialogueText, line.text, speed, typing.typingSource, loop, typing.typingVolume, typing.typingFadeOut);

            float delay = line.delayAfterOverride >= 0f ? line.delayAfterOverride : typing.delayBeforeNext;
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

        if (typing.typingSource != null) typing.typingSource.Stop();

        if (ui.dialogueShake != null) ui.dialogueShake.SetShaking(false);
        if (ui.dialogueGlitch != null) ui.dialogueGlitch.SetGlitching(false);

        SetImagesActive(false);
        if (ui.dialogueText != null)
        {
            ui.dialogueText.text = string.Empty;
            ui.dialogueText.maxVisibleCharacters = int.MaxValue;
            ui.dialogueText.ForceMeshUpdate(true, true);
        }
    }

    private void PlayNextChoiceRound()
    {
        if (TryPlaySanityDialogue(PlayNextChoiceRound)) return;
        if (TryForceGlitchCutToEnding()) return;

        ClearDialogue();
        CharacterData data = currentCharacter.data;

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

    private bool TryForceGlitchCutToEnding()
    {
        if (DoctorSanityManager.Instance == null || !DoctorSanityManager.Instance.IsGlitching) return false;
        if (Random.value > glitchForceEndChancePerRound) return false;

        if (!specialModeDecided)
        {
            isCrisisMode = currentCharacter.Sanity < currentCharacter.data.sanityThreshold;
            specialModeDecided = true;
        }

        Debug.Log(currentCharacter.data.characterName + " : Sanity หมอต่ำเกินไป ตัดจบการรักษากลางคัน");
        EndTurn();
        return true;
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

        if (ChoiceManager.Instance != null)
            ChoiceManager.Instance.ShowChoices(left, right);
    }

    private void ShowPairedChoice(ChoiceOptionData good, ChoiceOptionData bad)
    {
        ChoiceOptionData left, right;

        if (good != null && bad != null)
        {
            bool goodOnLeft;

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

        if (ChoiceManager.Instance != null)
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

    public void OnChoicePicked(ChoiceOptionData picked)
    {
        currentCharacter.MarkChoiceUsed(picked);
        currentCharacter.ChangeSanity(picked.sanityChange);
        UpdatePatientSanityUI();
        UpdateMentalStateMusic();

        // คำนวณ Sanity หมอ: (ลดอัตโนมัติรายข้อ) + (ผลกระทบจาก Choice นั้นๆ)
        if (DoctorSanityManager.Instance != null)
        {
            int totalDoctorChange = -DoctorSanityManager.Instance.BaseSanityLossPerChoice + picked.doctorSanityChange;
            DoctorSanityManager.Instance.ChangeSanity(totalDoctorChange);
        }

        if (currentRoundIsNormal)
        {
            currentCharacter.AdvanceNormalChoiceIndex();

            if (picked.afterDialogue != null && picked.afterDialogue.Length > 0)
                PlayLines(picked.afterDialogue, PlayNextChoiceRound);
            else
                PlayNextChoiceRound();
            return;
        }

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

        if (isInMentalState)
        {
            isInMentalState = false;
            ExitMentalState();
        }

        Debug.Log(currentCharacter.data.characterName + " จบตาแล้ว");
        if (CharacterManager.Instance != null)
            CharacterManager.Instance.NextCharacter(isCrisisMode);
    }

    private void UpdateMentalStateMusic()
    {
        if (currentCharacter == null || currentCharacter.data == null) return;

        bool shouldPlay = currentCharacter.Sanity < currentCharacter.data.mentalStateThreshold;
        if (shouldPlay == isInMentalState) return;

        isInMentalState = shouldPlay;

        if (shouldPlay) EnterMentalState();
        else ExitMentalState();
    }

    private void EnterMentalState()
    {
        FadeAudioSource(music.normalMusicSource, ref normalMusicFadeCoroutine, 0f, music.musicCrossfadeTime, stopWhenSilent: false);

        AudioClip clip = GetActiveMentalStateClip();
        if (music.mentalStateMusicSource != null && clip != null)
        {
            music.mentalStateMusicSource.clip = clip;
            music.mentalStateMusicSource.loop = true;
            music.mentalStateMusicSource.volume = 0f;
            music.mentalStateMusicSource.Play();
            FadeAudioSource(music.mentalStateMusicSource, ref mentalStateFadeCoroutine, music.mentalStateVolume, music.musicCrossfadeTime, stopWhenSilent: false);
        }

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

    private AudioClip GetActiveMentalStateClip()
    {
        if (currentCharacter != null && currentCharacter.data != null && currentCharacter.data.mentalStateClipOverride != null)
            return currentCharacter.data.mentalStateClipOverride;

        return music.defaultMentalStateClip;
    }

    private GameObject[] GetActiveMentalStateEffects()
    {
        if (currentCharacter != null && currentCharacter.mentalStateEffectOverride != null && currentCharacter.mentalStateEffectOverride.Length > 0)
            return currentCharacter.mentalStateEffectOverride;

        return mentalState.defaultMentalStateEffects;
    }

    private void ExitMentalState()
    {
        FadeAudioSource(music.mentalStateMusicSource, ref mentalStateFadeCoroutine, 0f, music.musicCrossfadeTime, stopWhenSilent: true);

        if (music.normalMusicSource != null)
        {
            if (!music.normalMusicSource.isPlaying) music.normalMusicSource.Play();
            FadeAudioSource(music.normalMusicSource, ref normalMusicFadeCoroutine, music.normalVolume, music.musicCrossfadeTime, stopWhenSilent: false);
        }

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