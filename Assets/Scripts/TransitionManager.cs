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
        if (fadeGroup != null) fadeGroup.blocksRaycasts = true;
        yield return Fade(fadeGroup != null ? fadeGroup.alpha : 0f, 1f);
    }

    public IEnumerator FadeFromBlackRoutine()
    {
        yield return Fade(fadeGroup != null ? fadeGroup.alpha : 1f, 0f);
        if (fadeGroup != null) fadeGroup.blocksRaycasts = false;
    }

    public void SetBlackInstant(bool black)
    {
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