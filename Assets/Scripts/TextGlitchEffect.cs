using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TextGlitchEffect : MonoBehaviour
{
    [Header("สัญลักษณ์สุ่มรบกวน (ตัดตัวอักษรภาษาอังกฤษออก ป้องกันสุ่มออกมาเป็นคำใบ้)")]
    [SerializeField] private char[] glitchPalette = { '@', '#', '$', '%', '&', '*', '!', '?' };

    [Header("สัดส่วนตัวอักษรที่โดนแทนต่อครั้ง (0-1)")]
    [Range(0f, 1f)] [SerializeField] private float corruptRatio = 0.15f;

    [Header("ความถี่ในการสุ่มใหม่ (วินาที)")]
    [SerializeField] private float glitchInterval = 0.12f;

    private TMP_Text tmp;
    private string cleanText = "";
    private bool isGlitching;
    private float timer;

    private void Awake()
    {
        tmp = GetComponent<TMP_Text>();
    }

    public void SetBaseText(string text)
    {
        cleanText = text ?? "";
    }

    public void SetGlitching(bool on)
    {
        if (isGlitching == on) return;
        isGlitching = on;
        timer = 0f;

        if (!on && tmp != null)
        {
            tmp.text = cleanText;
            tmp.ForceMeshUpdate(true, true);
        }
    }

    private void Update()
    {
        if (!isGlitching || tmp == null || string.IsNullOrEmpty(cleanText)) return;

        timer += Time.unscaledDeltaTime;
        if (timer < glitchInterval) return;
        timer = 0f;

        ApplyGlitchFrame();
    }

    private void ApplyGlitchFrame()
    {
        if (glitchPalette == null || glitchPalette.Length == 0) return;

        char[] chars = cleanText.ToCharArray();
        int visibleCount = Mathf.Min(tmp.maxVisibleCharacters, chars.Length);

        for (int i = 0; i < visibleCount; i++)
        {
            if (char.IsWhiteSpace(chars[i])) continue;
            if (Random.value > corruptRatio) continue;

            chars[i] = glitchPalette[Random.Range(0, glitchPalette.Length)];
        }

        tmp.text = new string(chars);
        tmp.ForceMeshUpdate(true, true);
    }
}