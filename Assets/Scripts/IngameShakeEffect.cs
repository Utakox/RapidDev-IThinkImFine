using UnityEngine;
using System.Collections;

// ติดกับ UI Image ที่อยากให้สั่นได้ (เช่น ตัวคนไข้) ต้องมี RectTransform (Image ทุกตัวมีอยู่แล้ว)
[RequireComponent(typeof(RectTransform))]
public class ImageShakeEffect : MonoBehaviour
{
    [Header("ความแรงการสั่น (ยิ่งเยอะยิ่งสั่นแรง)")]
    public float intensity = 8f;

    [Header("ความเร็วการสั่น")]
    public float shakeSpeed = 25f;

    [Header("สั่นแต่ละรอบนานกี่วินาที ก่อนพัก")]
    public float shakeDuration = 0.5f;

    [Tooltip("พักกี่วินาทีก่อนสั่นรอบถัดไป ใส่ 0 = สั่นต่อเนื่องไม่พักเลย")]
    public float pauseBetweenShakes = 0.3f;

    private RectTransform rect;
    private Vector2 originalPos;
    private bool isShaking;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalPos = rect.anchoredPosition;
    }

    // เรียกจากภายนอกตอนจะเปิด/ปิดการสั่น
    public void SetShaking(bool shake)
    {
        if (isShaking == shake) return;
        isShaking = shake;

        if (shakeRoutine != null) StopCoroutine(shakeRoutine);

        if (isShaking)
            shakeRoutine = StartCoroutine(ShakeLoop());
        else
            rect.anchoredPosition = originalPos; // ปิดแล้วกลับตำแหน่งเดิมทันที ไม่ค้างเบี้ยว
    }

    private IEnumerator ShakeLoop()
    {
        if (pauseBetweenShakes <= 0f)
        {
            // สั่นต่อเนื่องไม่มีพักเลย
            while (isShaking)
            {
                ApplyShakeOffset();
                yield return null;
            }
        }
        else
        {
            // สั่นเป็นช่วงๆ แล้วพักสลับกันไปเรื่อยๆ
            while (isShaking)
            {
                float t = 0f;
                while (t < shakeDuration && isShaking)
                {
                    t += Time.deltaTime;
                    ApplyShakeOffset();
                    yield return null;
                }
                rect.anchoredPosition = originalPos;

                float p = 0f;
                while (p < pauseBetweenShakes && isShaking)
                {
                    p += Time.deltaTime;
                    yield return null;
                }
            }
        }

        rect.anchoredPosition = originalPos;
    }

    private void ApplyShakeOffset()
    {
        Vector2 offset = new Vector2(
            (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * intensity,
            (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f) * intensity
        );

        rect.anchoredPosition = originalPos + offset;
    }
}