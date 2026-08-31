using System.Collections;
using UnityEngine;
using TMPro;

// ติดกับ GameObject เปล่าใน Canvas
// ต้องมี: CanvasGroup ครอบ TMP ของ narration (ลากใส่ textGroup) + AudioSource 1 ตัว
// จอดำใช้ร่วมกับ TransitionManager ไม่ต้องสร้าง panel ดำซ้อนอีกอัน
public class NarrationManager : MonoBehaviour
{
    public static NarrationManager Instance;

    [Header("UI References")]
    [SerializeField] private CanvasGroup textGroup;          // CanvasGroup ที่ครอบ text ของ narration
    [SerializeField] private TextMeshProUGUI narrationText;  // ต้องใช้ Font Asset ที่มีสระไทยครบ

    [Header("Audio")]
    [SerializeField] private AudioSource typingSource;       // เสียงตอนพิมพ์ (จะถูกตั้ง loop อัตโนมัติ)
    [SerializeField] private AudioSource ambienceSource;     // (ไม่ใส่ก็ได้) เสียงบรรยากาศ one-shot

    [Header("--- ค่า Default (บรรทัดไหนไม่ override ก็ใช้ค่านี้) ---")]
    [SerializeField] private float defaultTypeSpeed = 0.05f;        // วินาที/ตัวอักษร
    [SerializeField] private float defaultDelayAfterLine = 0.8f;    // หน่วงระหว่างบรรทัด
    [SerializeField] private float textFadeInDuration = 0.4f;       // text ค่อยๆ ปรากฏ
    [SerializeField] private float holdAfterFinish = 2.5f;          // ⭐ ค้างจอหลังข้อความขึ้นครบ
    [SerializeField] private float textFadeOutDuration = 0.8f;      // text ค่อยๆ จาง
    [SerializeField] private float audioFadeOutDuration = 0.3f;     // กันเสียงตัดห้วน

    [Header("--- Skip (กดข้าม) ---")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private bool skipOnMouseClick = true;

    private Coroutine routine;
    private bool isPlaying;
    private bool skipRequested;

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
            typingSource.loop = true;   // สำคัญ: ลูปไว้ แล้วให้สคริปต์เป็นคนสั่งหยุดตอนข้อความจบ
            typingSource.Stop();
        }
    }

    private void Update()
    {
        if (!isPlaying || !allowSkip) return;

        if (Input.GetKeyDown(skipKey) || (skipOnMouseClick && Input.GetMouseButtonDown(0)))
            skipRequested = true;
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
        skipRequested = false;

        // ---------- 1) เข้าจอดำ ----------
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

        // ---------- 2) เตรียม text ----------
        narrationText.text = string.Empty;
        narrationText.maxVisibleCharacters = 0;
        narrationText.ForceMeshUpdate(true, true);
        yield return FadeCanvas(textGroup, 0f, 1f, textFadeInDuration);

        // ---------- 3) พิมพ์ทีละบรรทัด ----------
        string accumulated = string.Empty;

        if (!seq.stopSoundBetweenLines)
            StartTypingSound(seq);   // เสียงวิ่งยาวคลุมทั้งชุด

        for (int i = 0; i < seq.lines.Length; i++)
        {
            NarrationLine line = seq.lines[i];
            if (line == null) continue;

            if (line.clearBefore) accumulated = string.Empty;

            int startVisible = CountVisibleChars(accumulated);
            accumulated = string.IsNullOrEmpty(accumulated)
                ? line.text
                : accumulated + "\n" + line.text;

            // ใส่ข้อความเต็มก้อนลงไปก่อน แล้วค่อยเปิดให้เห็นทีละตัวด้วย maxVisibleCharacters
            // วิธีนี้จำเป็นสำหรับภาษาไทย เพราะสระ/วรรณยุกต์จะได้ประกอบกับพยัญชนะถูกต้อง
            // (ถ้าใช้ text += c แบบเดิม ตัวอักษรจะกระโดดและสระลอย) และรองรับ rich text ด้วย
            narrationText.text = accumulated;
            narrationText.ForceMeshUpdate(true, true);

            int total = narrationText.textInfo.characterCount;
            narrationText.maxVisibleCharacters = startVisible;

            if (seq.stopSoundBetweenLines) StartTypingSound(seq);

            float speed = line.typeSpeedOverride > 0f ? line.typeSpeedOverride : defaultTypeSpeed;

            for (int c = startVisible + 1; c <= total; c++)
            {
                if (skipRequested)
                {
                    skipRequested = false;
                    narrationText.maxVisibleCharacters = total;
                    break;
                }

                narrationText.maxVisibleCharacters = c;
                yield return WaitUnscaled(speed);
            }

            narrationText.maxVisibleCharacters = total;

            bool isLastLine = (i == seq.lines.Length - 1);

            // ⭐ เสียงหยุด "ตอนข้อความจบ" ไม่ใช่ตอนคลิปจบ
            if (seq.stopSoundBetweenLines || isLastLine)
                yield return StopTypingSound();

            if (!isLastLine)
            {
                float delay = line.delayAfterOverride >= 0f ? line.delayAfterOverride : defaultDelayAfterLine;
                yield return WaitWithSkip(delay);
            }
        }

        // ---------- 4) ค้างจอไว้ตามเวลาที่ตั้ง ----------
        float hold = seq.holdAfterFinishOverride >= 0f ? seq.holdAfterFinishOverride : holdAfterFinish;
        yield return WaitWithSkip(hold);

        // ---------- 5) text จางหาย แล้วจอดำค่อยเฟดออก ----------
        yield return FadeCanvas(textGroup, textGroup.alpha, 0f, textFadeOutDuration);
        narrationText.text = string.Empty;
        narrationText.maxVisibleCharacters = 0;

        yield return TransitionManager.Instance.FadeFromBlackRoutine();

        isPlaying = false;
        routine = null;
        onComplete?.Invoke();
    }

    // ========== Audio ==========

    private void StartTypingSound(NarrationSequence seq)
    {
        if (typingSource == null || seq.typingLoopClip == null) return;

        typingSource.clip = seq.typingLoopClip;
        typingSource.volume = seq.typingVolume;
        typingSource.loop = true;
        if (!typingSource.isPlaying) typingSource.Play();
    }

    private IEnumerator StopTypingSound()
    {
        if (typingSource == null || !typingSource.isPlaying) yield break;

        float startVolume = typingSource.volume;
        float t = 0f;
        while (t < audioFadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            typingSource.volume = Mathf.Lerp(startVolume, 0f, t / audioFadeOutDuration);
            yield return null;
        }

        typingSource.Stop();
        typingSource.volume = startVolume;
    }

    // ========== Helpers ==========

    private int CountVisibleChars(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;

        narrationText.text = s;
        narrationText.ForceMeshUpdate(true, true);
        return narrationText.textInfo.characterCount;
    }

    private IEnumerator WaitWithSkip(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            if (skipRequested) { skipRequested = false; yield break; }
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private IEnumerator WaitUnscaled(float seconds)
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