using UnityEngine;
using System.Collections;

// จำลองการกระพริบตา: จอค่อยๆ ดำ (หลับตา) แล้วค่อยๆ สว่างกลับ (ลืมตา) วนไปเรื่อยๆ แบบสุ่มช่วงเวลา
// ติดกับ GameObject ที่มี CanvasGroup ครอบเต็มจอ (แยกต่างหากจาก TransitionManager เพื่อไม่ให้ชนกัน)
public class EyeBlinkEffect : MonoBehaviour
{
    [Header("UI จอดำเต็มจอสำหรับกระพริบตา")]
    [SerializeField] private CanvasGroup blinkGroup;

    [Header("ความเร็วกระพริบแต่ละครั้ง")]
    [Tooltip("เวลาที่ใช้หลับตา (จอค่อยๆ ดำลง)")]
    public float closeDuration = 0.15f;

    [Tooltip("ค้างหลับตาไว้นานแค่ไหนก่อนลืมตา")]
    public float holdClosedDuration = 0.05f;

    [Tooltip("เวลาที่ใช้ลืมตา (จอค่อยๆ สว่างกลับ)")]
    public float openDuration = 0.2f;

    [Header("ช่วงเวลาสุ่มระหว่างการกระพริบแต่ละรอบ")]
    public float minInterval = 2f;
    public float maxInterval = 5f;

    private bool isBlinking;
    private Coroutine blinkRoutine;

    private void Awake()
    {
        if (blinkGroup != null)
        {
            blinkGroup.alpha = 0f;
            blinkGroup.blocksRaycasts = false;
        }
    }

    // เรียกจากภายนอกตอนอยากเปิด/ปิดระบบกระพริบตา (เช่นตอนหมอ Glitch)
    public void SetBlinking(bool active)
    {
        if (isBlinking == active) return;
        isBlinking = active;

        if (blinkRoutine != null) StopCoroutine(blinkRoutine);

        if (isBlinking)
            blinkRoutine = StartCoroutine(BlinkLoop());
        else if (blinkGroup != null)
            blinkGroup.alpha = 0f; // ปิดกลางคันก็ให้จอใสทันที ไม่ค้างดำ
    }

    private IEnumerator BlinkLoop()
    {
        while (isBlinking)
        {
            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);

            yield return DoOneBlink();
        }
    }

    private IEnumerator DoOneBlink()
    {
        yield return Fade(0f, 1f, closeDuration);
        yield return new WaitForSeconds(holdClosedDuration);
        yield return Fade(1f, 0f, openDuration);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (blinkGroup == null) yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            blinkGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        blinkGroup.alpha = to;
    }

    // เผื่ออยากสั่งกระพริบทันที 1 ครั้งจากที่อื่น นอกเหนือจาก loop อัตโนมัติ
    public void BlinkOnce()
    {
        StartCoroutine(DoOneBlink());
    }
}