using UnityEngine;

public enum SanityCompare
{
    ต่ำกว่าหรือเท่ากับ,   // Sanity <= threshold
    สูงกว่าหรือเท่ากับ,   // Sanity >= threshold
    เท่ากับพอดี           // Sanity == threshold
}

[System.Serializable]
public class SanityDialogueTrigger
{
    [Tooltip("ชื่อกำกับไว้ดูเองใน Inspector เฉยๆ ไม่มีผลกับเกม")]
    public string note;

    [Tooltip("ค่า Sanity ที่ใช้เทียบ")]
    [Range(0, 100)] public int threshold = 30;

    [Tooltip("เทียบแบบไหน")]
    public SanityCompare compare = SanityCompare.ต่ำกว่าหรือเท่ากับ;

    [Tooltip("บทพูดที่จะเล่นเมื่อเงื่อนไขเป็นจริง (เล่นครั้งเดียวต่อตัวละคร)")]
    public DialogueLine[] dialogue;

    public bool IsMet(int sanity)
    {
        switch (compare)
        {
            case SanityCompare.ต่ำกว่าหรือเท่ากับ: return sanity <= threshold;
            case SanityCompare.สูงกว่าหรือเท่ากับ: return sanity >= threshold;
            case SanityCompare.เท่ากับพอดี:        return sanity == threshold;
            default: return false;
        }
    }
}