using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 6)] public string text;

    [Tooltip("ความเร็วพิมพ์เฉพาะบรรทัดนี้ ใส่ -1 หรือ 0 = ใช้ค่า default")]
    public float typeSpeedOverride = -1f;

    [Tooltip("หน่วงหลังบรรทัดนี้จบ ใส่ -1 = ใช้ค่า default")]
    public float delayAfterOverride = -1f;

    [Header("เอฟเฟกต์สั่น")]
    public bool shakeText;
    public float shakeIntensity = 1f;

    [Header("เสียงพิมพ์เฉพาะบรรทัดนี้ (ไม่ใส่ = ใช้ default ของ DialogueManager)")]
    public AudioClip typingLoopOverride;
}