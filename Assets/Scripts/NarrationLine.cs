using UnityEngine;

[System.Serializable]
public class NarrationLine
{
    [TextArea(2, 6)] public string text;

    [Tooltip("วินาที/ตัวอักษร")]
    public float typeSpeed = 0.04f;

    [Tooltip("หน่วงเวลาหลังบรรทัดนี้จบ ก่อนไปบรรทัดถัดไป (บรรทัดสุดท้ายไม่ใช้ค่านี้)")]
    public float delayAfter = 0.8f;

    [Tooltip("ติ๊ก = ล้างจอก่อนพิมพ์บรรทัดนี้ / ไม่ติ๊ก = พิมพ์ต่อท้ายบรรทัดเดิม (ใช้ทำย่อหน้ายาว)")]
    public bool clearBefore = true;
}

[System.Serializable]
public class NarrationSequence
{
    [Tooltip("ไม่ติ๊ก = ข้ามจอดำเล่าเรื่องของตัวละครนี้ไปเลย")]
    public bool enabled = true;

    [Header("ข้อความ (custom ได้เต็มที่ ใส่กี่บรรทัดก็ได้)")]
    public NarrationLine[] lines;

    [Header("เสียงตอนข้อความกำลังพิมพ์")]
    [Tooltip("ควรเป็นคลิปสั้นที่ loop ได้เนียน สคริปต์จะสั่งหยุดเองตอนข้อความพิมพ์จบพอดี")]
    public AudioClip typingLoopClip;
    [Range(0f, 1f)] public float typingVolume = 1f;

    [Tooltip("ติ๊ก = หยุดเสียงระหว่างพักคั่นบรรทัดด้วย / ไม่ติ๊ก = เสียงวิ่งยาวจนกว่าข้อความทั้งชุดจะจบ (ค่าแนะนำ)")]
    public bool stopSoundBetweenLines = false;

    [Header("เวลาค้างจอหลังข้อความขึ้นครบ (วินาที) ก่อนเฟดออก")]
    public float holdAfterFinish = 2.5f;

    [Header("เสียงประกอบฉาก (ไม่ใส่ก็ได้) เล่นครั้งเดียวตอนจอดำเริ่ม")]
    public AudioClip ambienceOneShot;

    public bool HasContent => enabled && lines != null && lines.Length > 0;
}