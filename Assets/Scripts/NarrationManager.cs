using System.Collections;
using UnityEngine;
using TMPro;

public class NarrationManager : MonoBehaviour
{
    public static NarrationManager Instance;

    [System.Serializable]
    public struct UIConfig
    {
        public CanvasGroup textGroup;
        public TextMeshProUGUI narrationText;
    }

    [System.Serializable]
    public struct AudioConfig
    {
        public AudioSource typingSource;
        public AudioSource ambienceSource;
    }

    [System.Serializable]
    public struct FadeConfig
    {
        public float textFadeInDuration;
        public float textFadeOutDuration;
        public float audioFadeOutDuration;
    }

    [Header("=== Inspector Groups ===")]
    [SerializeField] private UIConfig ui;
    [SerializeField] private AudioConfig audioConfig;
    [SerializeField] private FadeConfig fade;

    private Coroutine routine;
    private bool isPlaying;

    public bool IsPlaying => isPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (ui.textGroup != null)
        {
            ui.textGroup.alpha = 0f;
            ui.textGroup.blocksRaycasts = false;
        }
        if (ui.narrationText != null)
        {
            ui.narrationText.text = string.Empty;
            ui.narrationText.maxVisibleCharacters = 0;
        }
        if (audioConfig.typingSource != null)
        {
            audioConfig.typingSource.playOnAwake = false;
            audioConfig.typingSource.loop = true;
            audioConfig.typingSource.Stop();
        }
    }

    public void PlaySequence(NarrationSequence seq, System.Action onComplete, bool alreadyBlack = false, bool fadeOutAtEnd = true)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(SequenceRoutine(seq, onComplete, alreadyBlack, fadeOutAtEnd));
    }

    private IEnumerator SequenceRoutine(NarrationSequence seq, System.Action onComplete, bool alreadyBlack, bool fadeOutAtEnd)
    {
        isPlaying = true;

        if (!alreadyBlack)
            yield return TransitionManager.Instance.FadeToBlackRoutine();
        else
            TransitionManager.Instance.SetBlackInstant(true);

        if (seq == null || !seq.HasContent)
        {
            if (fadeOutAtEnd)
                yield return TransitionManager.Instance.FadeFromBlackRoutine();
            isPlaying = false;
            routine = null;
            onComplete?.Invoke();
            yield break;
        }

        if (seq.ambienceOneShot != null && audioConfig.ambienceSource != null)
            audioConfig.ambienceSource.PlayOneShot(seq.ambienceOneShot);

        if (ui.narrationText != null)
        {
            ui.narrationText.text = string.Empty;
            ui.narrationText.maxVisibleCharacters = 0;
            ui.narrationText.ForceMeshUpdate(true, true);
        }

        yield return FadeCanvas(ui.textGroup, 0f, 1f, fade.textFadeInDuration);

        string accumulated = string.Empty;

        if (!seq.stopSoundBetweenLines)
            StartTypingSound(seq);

        for (int i = 0; i < seq.lines.Length; i++)
        {
            NarrationLine line = seq.lines[i];
            if (line == null) continue;

            if (line.clearBefore) accumulated = string.Empty;

            int startVisible = CountVisibleChars(accumulated);
            accumulated = string.IsNullOrEmpty(accumulated)
                ? line.text
                : accumulated + "\n" + line.text;

            if (seq.stopSoundBetweenLines) StartTypingSound(seq);

            bool isLastLine = (i == seq.lines.Length - 1);

            yield return Typewriter.TypeLine(
                ui.narrationText, accumulated, line.typeSpeed,
                audioConfig.typingSource, seq.typingLoopClip, seq.typingVolume, fade.audioFadeOutDuration,
                startVisible, checkSkip: null, unscaled: true,
                stopSoundAtEnd: seq.stopSoundBetweenLines || isLastLine);

            if (!isLastLine)
                yield return Wait(line.delayAfter);
        }

        yield return Wait(seq.holdAfterFinish);

        yield return FadeCanvas(ui.textGroup, ui.textGroup != null ? ui.textGroup.alpha : 1f, 0f, fade.textFadeOutDuration);
        
        if (ui.narrationText != null)
        {
            ui.narrationText.text = string.Empty;
            ui.narrationText.maxVisibleCharacters = 0;
        }

        if (fadeOutAtEnd)
            yield return TransitionManager.Instance.FadeFromBlackRoutine();

        isPlaying = false;
        routine = null;
        onComplete?.Invoke();
    }

    private void StartTypingSound(NarrationSequence seq)
    {
        if (audioConfig.typingSource == null || seq.typingLoopClip == null) return;

        audioConfig.typingSource.clip = seq.typingLoopClip;
        audioConfig.typingSource.volume = seq.typingVolume;
        audioConfig.typingSource.loop = true;
        if (!audioConfig.typingSource.isPlaying) audioConfig.typingSource.Play();
    }

    private int CountVisibleChars(string s)
    {
        if (string.IsNullOrEmpty(s) || ui.narrationText == null) return 0;

        ui.narrationText.text = s;
        ui.narrationText.ForceMeshUpdate(true, true);
        return ui.narrationText.textInfo.characterCount;
    }

    private IEnumerator Wait(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeCanvas(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null) yield break;

        if (duration <= 0f) { group.alpha = to; yield break; }

        float t = 0f;
        group.alpha = from;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
    }
}