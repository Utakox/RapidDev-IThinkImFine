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

        // การันตีว่า panel นี้อยู่บนสุดของ Canvas เสมอ ไม่ต้องพึ่งการลากจัด Hierarchy ด้วยมือ
        // (กันเคสมีใครมาเพิ่ม UI ใหม่ทีหลังแล้วดันไปอยู่ใต้ Hierarchy กว่านี้โดยไม่ตั้งใจ จนบัง fade ไม่มิด)
        transform.SetAsLastSibling();
    }

    // เฟดจอดำเข้ามาก่อน แล้วค่อยเรียก onBlack (เช่นสลับตัวละคร/เปลี่ยนฉาก) ตอนจอดำสนิทพอดี
    // จากนั้นเฟดจอดำออกกลับไปให้เห็นฉากใหม่ แล้วค่อยเรียก onComplete ตอนจอใสสนิทแล้วเท่านั้น
    //
    // สำคัญ: ถ้าจะทำอะไรที่ผู้เล่น "เห็น" ทันที (เช่นเริ่มพิมพ์ dialogue) ให้ใส่ใน onComplete ไม่ใช่ onBlack
    // เพราะ onBlack ทำงานตอนจอยังดำอยู่ ถ้าเริ่ม dialogue ตรงนั้นจะพิมพ์ไปบางส่วนโดยที่จอยังดำ ไม่มีใครเห็น
    public void PlayTransition(System.Action onBlack, System.Action onComplete = null)
    {
        StartCoroutine(TransitionRoutine(onBlack, onComplete));
    }

    private IEnumerator TransitionRoutine(System.Action onBlack, System.Action onComplete)
    {
        fadeGroup.blocksRaycasts = true; // กันคนกดอะไรระหว่างจอดำ

        yield return Fade(0f, 1f); // ดำเข้า

        onBlack?.Invoke();

        yield return Fade(1f, 0f); // ดำออก

        fadeGroup.blocksRaycasts = false;

        onComplete?.Invoke();
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