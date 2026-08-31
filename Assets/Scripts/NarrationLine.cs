using UnityEngine;

// 1 บรรทัดของ narration จอดำ
[System.Serializable]
public class NarrationLine
{
    [TextArea(2, 6)] public string text;

    [Tooltip("ความเร็วพิมพ์เฉพาะบรรทัดนี้ (วินาที/ตัวอักษร) ใส่ -1 = ใช้ค่า default ของ NarrationManager")]
    public float typeSpeedOverride = -1f;

    [Tooltip("หน่วงหลังพิมพ์บรรทัดนี้จบ ใส่ -1 = ใช้ค่า default")]
    public float delayAfterOverride = -1f;

    [Tooltip("ติ๊ก = ล้างจอก่อนพิมพ์บรรทัดนี้ / ไม่ติ๊ก = พิมพ์ต่อท้ายบรรทัดเดิม (ใช้ทำย่อหน้ายาว)")]
    public bool clearBefore = true;
}

// ชุด narration ทั้งก้อน ตั้งค่าได้ต่อตัวละคร
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

    [Header("เวลาค้างจอหลังข้อความขึ้นครบ (วินาที) ใส่ -1 = ใช้ค่า default")]
    public float holdAfterFinishOverride = -1f;

    [Header("เสียงประกอบฉาก (ไม่ใส่ก็ได้) เล่นครั้งเดียวตอนจอดำเริ่ม")]
    public AudioClip ambienceOneShot;

    public bool HasContent => enabled && lines != null && lines.Length > 0;
}