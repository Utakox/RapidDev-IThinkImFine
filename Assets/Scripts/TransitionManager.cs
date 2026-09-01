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

        // เริ่มเกมด้วยจอดำสนิททันที ไม่ต้องเฟดเข้า
        // (จะเฟด "ออก" จากดำก็ต่อเมื่อ narration เล่นเสียง/ข้อความจบแล้วเท่านั้น เรียกผ่าน FadeFromBlack)
        fadeGroup.alpha = 1f;
        fadeGroup.blocksRaycasts = true;
        transform.SetAsLastSibling(); // การันตีว่าอยู่บนสุดเสมอ
    }

    private void Start()
{
    // TransitionManager.Awake() เรียก SetAsLastSibling()
    // Start ทำงานทีหลัง Awake เสมอ -> narration ทับจอดำได้ชัวร์
    transform.SetAsLastSibling();
}

    // ===== ดำเข้าแล้ว "ค้างไว้" ให้ NarrationManager ทำงานต่อ =====
    public void FadeToBlack(System.Action onBlack)
    {
        StartCoroutine(FadeToBlackThen(onBlack));
    }

    private IEnumerator FadeToBlackThen(System.Action onBlack)
    {
        yield return FadeToBlackRoutine();
        onBlack?.Invoke();
    }

    // ===== เฟด "ออก" จากดำ พร้อม callback ให้เรียกตอนสอง/ข้อความเล่นจบแล้วเท่านั้น =====
    // ใช้แทนการเฟดเข้าตอนเปิดเกม: จอเริ่มดำสนิททันที (ดู Awake) แล้วค่อยเรียกอันนี้ตอน narration เล่นจบ
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