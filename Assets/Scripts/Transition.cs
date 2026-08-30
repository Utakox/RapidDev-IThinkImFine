using System.Collections;
using UnityEngine;

// ติดกับ GameObject ที่มี CanvasGroup ครอบเต็มจอ (Image สีดำ) วางบนสุดของ Canvas
// เริ่มต้นให้ alpha = 0 และปิด raycast ไว้ก่อน (เดี๋ยวสคริปต์เปิด/ปิดเองตอน fade)
public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    public CanvasGroup fadeGroup;
    public float fadeDuration = 0.5f;

    private void Awake()
    {
        Instance = this;
        fadeGroup.alpha = 0f;// เริ่มต้นให้จอดำโปร่งใส
        fadeGroup.blocksRaycasts = false;
    }
    
    // เฟดจอดำเข้ามาก่อน แล้วค่อยเรียก onBlack (เช่นสลับตัวละคร/เปลี่ยนฉาก) ตอนจอดำสนิทพอดี
    // จากนั้นเฟดจอดำออกกลับไปให้เห็นฉากใหม่
    public void PlayTransition(System.Action onBlack)
    {
        StartCoroutine(TransitionRoutine(onBlack));
    }

    // เฟดจอดำเข้ามาก่อน แล้วค่อยเรียก onBlack (เช่นสลับตัวละคร/เปลี่ยนฉาก) ตอนจอดำสนิทพอดี
    private IEnumerator TransitionRoutine(System.Action onBlack)
    {
        fadeGroup.blocksRaycasts = true; // กันคนกดอะไรระหว่างจอดำ

        yield return Fade(0f, 1f); // ดำเข้า

        onBlack?.Invoke();
        yield return Fade(1f, 0f); // ดำออก

        fadeGroup.blocksRaycasts = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = to;
    }
}
