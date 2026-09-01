using System.Collections;
using UnityEngine;
using TMPro;

// ติดกับ GameObject เปล่าใน Canvas
// ต้องมี: CanvasGroup ครอบ TMP ของ narration (ลากใส่ textGroup) + AudioSource 1 ตัว
// จอดำใช้ร่วมกับ TransitionManager ไม่ต้องสร้าง panel ดำซ้อนอีกอัน
//
// ค่าต่างๆ (ความเร็วพิมพ์, delay, hold) ตั้งที่ CharacterData ของแต่ละตัวละครทั้งหมดแล้ว
// ไฟล์นี้เหลือแค่ค่าที่เป็น "ความรู้สึกของจอเปลี่ยนฉาก" ล้วนๆ ไม่ผูกกับเนื้อหาตัวละคร
public class NarrationManager : MonoBehaviour
{
    public static NarrationManager Instance;

    [Header("UI References")]
    [SerializeField] private CanvasGroup textGroup;          // CanvasGroup ที่ครอบ text ของ narration
    [SerializeField] private TextMeshProUGUI narrationText;  // ต้องใช้ Font Asset ที่มีสระไทยครบ

    [Header("Audio")]
    [SerializeField] private AudioSource typingSource;       // เสียงตอนพิมพ์ (จะถูกตั้ง loop อัตโนมัติ)
    [SerializeField] private AudioSource ambienceSource;     // (ไม่ใส่ก็ได้) เสียงบรรยากาศ one-shot

    [Header("--- Fade ของจอ (ค่ากลาง ไม่ผูกกับตัวละคร) ---")]
    [SerializeField] private float textFadeInDuration = 0.4f;
    [SerializeField] private float textFadeOutDuration = 0.8f;
    [SerializeField] private float audioFadeOutDuration = 0.3f;

    private Coroutine routine;
    private bool isPlaying;

    public bool IsPlaying => isPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (textGroup != null)
        {
            textGroup.alpha = 0f;
            textGroup.blocksRaycasts = false;
        }
        if (narrationText != null)
        {
            narrationText.text = string.Empty;
            narrationText.maxVisibleCharacters = 0;
        }
        if (typingSource != null)
        {
            typingSource.playOnAwake = false;
            typingSource.loop = true;
            typingSource.Stop();
        }
    }

    /// <summary>
    /// เล่นจอดำ + narration
    /// alreadyBlack = true เมื่อจอดำอยู่แล้ว (เช่นถูกเรียกจากตอนสลับตัวละคร) จะไม่เฟดดำซ้ำ
    /// จบแล้วจะเฟดจอดำออกให้เอง แล้วค่อยเรียก onComplete ตอนจอใสสนิท
    /// </summary>
    public void PlaySequence(NarrationSequence seq, System.Action onComplete, bool alreadyBlack = false)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(SequenceRoutine(seq, onComplete, alreadyBlack));
    }

    private IEnumerator SequenceRoutine(NarrationSequence seq, System.Action onComplete, bool alreadyBlack)
    {
        isPlaying = true;

        if (!alreadyBlack)
            yield return TransitionManager.Instance.FadeToBlackRoutine();
        else
            TransitionManager.Instance.SetBlackInstant(true);

        // ไม่มีเนื้อหา = เฟดกลับออกไปเฉยๆ (กันเคสตัวละครที่ไม่ต้องการ intro)
        if (seq == null || !seq.HasContent)
        {
            yield return TransitionManager.Instance.FadeFromBlackRoutine();
            isPlaying = false;
            routine = null;
            onComplete?.Invoke();
            yield break;
        }

        if (seq.ambienceOneShot != null && ambienceSource != null)
            ambienceSource.PlayOneShot(seq.ambienceOneShot);

        narrationText.text = string.Empty;
        narrationText.maxVisibleCharacters = 0;
        narrationText.ForceMeshUpdate(true, true);
        yield return FadeCanvas(textGroup, 0f, 1f, textFadeInDuration);

        string accumulated = string.Empty;

        if (!seq.stopSoundBetweenLines)
            StartTypingSound(seq); // เสียงวิ่งยาวคลุมทั้งชุด

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
                narrationText, accumulated, line.typeSpeed,
                typingSource, seq.typingLoopClip, seq.typingVolume, audioFadeOutDuration,
                startVisible, checkSkip: null, unscaled: true,
                stopSoundAtEnd: seq.stopSoundBetweenLines || isLastLine);

            if (!isLastLine)
                yield return Wait(line.delayAfter);
        }

        yield return Wait(seq.holdAfterFinish);

        yield return FadeCanvas(textGroup, textGroup.alpha, 0f, textFadeOutDuration);
        narrationText.text = string.Empty;
        narrationText.maxVisibleCharacters = 0;

        yield return TransitionManager.Instance.FadeFromBlackRoutine();

        isPlaying = false;
        routine = null;
        onComplete?.Invoke();
    }

    private void StartTypingSound(NarrationSequence seq)
    {
        if (typingSource == null || seq.typingLoopClip == null) return;

        typingSource.clip = seq.typingLoopClip;
        typingSource.volume = seq.typingVolume;
        typingSource.loop = true;
        if (!typingSource.isPlaying) typingSource.Play();
    }

    private int CountVisibleChars(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;

        narrationText.text = s;
        narrationText.ForceMeshUpdate(true, true);
        return narrationText.textInfo.characterCount;
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