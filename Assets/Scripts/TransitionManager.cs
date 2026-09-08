using System.Collections;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (fadeGroup == null)
        {
            Debug.LogError("[TransitionManager] ยังไม่ได้ลาก fadeGroup ใส่ Inspector");
            return;
        }

        fadeGroup.alpha = 1f;
        fadeGroup.blocksRaycasts = true;
        transform.SetAsLastSibling();
    }

    private void Start()
    {
        transform.SetAsLastSibling();
    }

    public void FadeToBlack(System.Action onBlack)
    {
        StartCoroutine(FadeToBlackThen(onBlack));
    }

    private IEnumerator FadeToBlackThen(System.Action onBlack)
    {
        yield return FadeToBlackRoutine();
        onBlack?.Invoke();
    }

    public void FadeFromBlack(System.Action onClear)
    {
        StartCoroutine(FadeFromBlackThen(onClear));
    }

    private IEnumerator FadeFromBlackThen(System.Action onClear)
    {
        yield return FadeFromBlackRoutine();
        onClear?.Invoke();
    }

    public IEnumerator FadeToBlackRoutine()
    {
        // ดับเสียง Glitch Loop ตั้งแต่เริ่ม Transition (ใช้ได้ทุกจุดตั้งแต่ Intro ยัน Ending เพราะเรียกผ่านจุดนี้จุดเดียว)
        if (DoctorSanityManager.Instance != null)
            DoctorSanityManager.Instance.SetGlitchLoopSuppressed(true);

        if (fadeGroup != null) fadeGroup.blocksRaycasts = true;
        yield return Fade(fadeGroup != null ? fadeGroup.alpha : 0f, 1f);
    }

    public IEnumerator FadeFromBlackRoutine()
    {
        yield return Fade(fadeGroup != null ? fadeGroup.alpha : 1f, 0f);
        if (fadeGroup != null) fadeGroup.blocksRaycasts = false;

        // จอสว่างขึ้นแล้ว คืนเสียง Glitch Loop (ถ้ายังอยู่ในเกณฑ์ Glitch ก็จะเล่นต่อเอง)
        if (DoctorSanityManager.Instance != null)
            DoctorSanityManager.Instance.SetGlitchLoopSuppressed(false);
    }

    public void SetBlackInstant(bool black)
    {
        if (DoctorSanityManager.Instance != null)
            DoctorSanityManager.Instance.SetGlitchLoopSuppressed(black);

        if (fadeGroup == null) return;
        fadeGroup.alpha = black ? 1f : 0f;
        fadeGroup.blocksRaycasts = black;
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeGroup == null) yield break;
        if (Mathf.Approximately(from, to)) { fadeGroup.alpha = to; yield break; }

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = to;
    }
}