using System.Collections;
using UnityEngine;

// ติดกับ GameObject ที่มี CanvasGroup ครอบเต็มจอ (Image สีดำ) วางบนสุดของ Canvas
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

        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;
        transform.SetAsLastSibling(); // การันตีว่าอยู่บนสุดเสมอ
    }

    private void Start()
{
    // TransitionManager.Awake() เรียก SetAsLastSibling()
    // Start ทำงานทีหลัง Awake เสมอ -> narration ทับจอดำได้ชัวร์
    transform.SetAsLastSibling();
}

    // ===== ของเดิม: ดำเข้า -> onBlack -> ดำออก -> onComplete =====
    public void PlayTransition(System.Action onBlack, System.Action onComplete = null)
    {
        StartCoroutine(TransitionRoutine(onBlack, onComplete));
    }

    private IEnumerator TransitionRoutine(System.Action onBlack, System.Action onComplete)
    {
        yield return FadeToBlackRoutine();
        onBlack?.Invoke();
        yield return FadeFromBlackRoutine();
        onComplete?.Invoke();
    }

    // ===== ใหม่: ดำเข้าแล้ว "ค้างไว้" ให้ NarrationManager ทำงานต่อ =====
    public void FadeToBlack(System.Action onBlack)
    {
        StartCoroutine(FadeToBlackThen(onBlack));
    }

    private IEnumerator FadeToBlackThen(System.Action onBlack)
    {
        yield return FadeToBlackRoutine();
        onBlack?.Invoke();
    }

    public IEnumerator FadeToBlackRoutine()
    {
        fadeGroup.blocksRaycasts = true;              // กันคนกดอะไรระหว่างจอดำ
        yield return Fade(fadeGroup.alpha, 1f);
    }

    public IEnumerator FadeFromBlackRoutine()
    {
        yield return Fade(fadeGroup.alpha, 0f);
        fadeGroup.blocksRaycasts = false;
    }

    public void SetBlackInstant(bool black)
    {
        fadeGroup.alpha = black ? 1f : 0f;
        fadeGroup.blocksRaycasts = black;
    }

    private IEnumerator Fade(float from, float to)
    {
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