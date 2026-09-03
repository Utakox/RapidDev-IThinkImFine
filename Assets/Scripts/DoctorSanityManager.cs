using UnityEngine;
using TMPro;

public class DoctorSanityManager : MonoBehaviour
{
    public static DoctorSanityManager Instance;

    [Header("Sanity หมอ (รวมทั้งเกม)")]
    [Range(0, 100)] [SerializeField] private int startingSanity = 100;

    [Tooltip("Sanity หมอ < ค่านี้ = เริ่มเกิดอาการ Glitch (จอสั่น/ตัวอักษรเพี้ยน/เมาส์ฝืด/ตัดจบกลางคัน)")]
    [SerializeField] private int glitchThreshold = 40;

    [Header("อัตราการลดของ Sanity หมอ")]
    [Tooltip("Sanity หมอที่จะลดลงอัตโนมัติทุกครั้งที่ผู้เล่นเลือกตอบ 1 ข้อ (ใส่ 0 ถ้าไม่ต้องการให้ลดอัตโนมัติ)")]
    [SerializeField] private int baseSanityLossPerChoice = 2;

    [Header("(ไม่ใส่ก็ได้) TMP โชว์ค่า Sanity หมอตอนเทส")]
    [SerializeField] private TextMeshProUGUI sanityDebugText;

    public int Sanity { get; private set; }
    public bool IsGlitching => Sanity < glitchThreshold;
    public int BaseSanityLossPerChoice => baseSanityLossPerChoice;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        Sanity = Mathf.Clamp(startingSanity, 0, 100);
        UpdateDebugText();

        if (ScreenShakeEffect.Instance != null)
            ScreenShakeEffect.Instance.SetShaking(IsGlitching);
    }

    public void ChangeSanity(int amount)
    {
        if (amount == 0) return;

        bool wasGlitching = IsGlitching;
        Sanity = Mathf.Clamp(Sanity + amount, 0, 100);
        UpdateDebugText();

        if (IsGlitching != wasGlitching && ScreenShakeEffect.Instance != null)
            ScreenShakeEffect.Instance.SetShaking(IsGlitching);
    }

    private void UpdateDebugText()
    {
        if (sanityDebugText == null) return;
        sanityDebugText.text = $"Doctor Sanity: {Sanity}";
    }
}