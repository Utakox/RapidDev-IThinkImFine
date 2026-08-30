using UnityEngine;

// 1 บรรทัดบทพูด อาจมี "บทพูดสำรอง" เวลา Sanity ของตัวละคร ณ ตอนพูดบรรทัดนี้
// เท่ากับหรือต่ำกว่าค่าที่ตั้งไว้ (เช็คสดทุกครั้งที่บรรทัดนี้ถูกพูด ไม่ใช่ทริกเกอร์ครั้งเดียว)
// (จุดสำคัญ: คลาสนี้ต้อง "ไม่" อ้างกลับไปหา ChoiceOptionData เด็ดขาด ไม่งั้นจะวนเป็นวงกลมอีก)
[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 5)]
    public string text;

    [Header("(ไม่ใส่ก็ได้) บทพูดสำรองเวลา Sanity ตอนพูดบรรทัดนี้ เท่ากับหรือต่ำกว่าค่าที่ตั้ง")]
    [Tooltip("เช็คทุกอันที่ Sanity ปัจจุบัน <= Threshold แล้วเลือกอันที่ Threshold ต่ำสุด (เข้มงวด/วิกฤตที่สุดที่ยังตรงกับ Sanity ตอนนี้) มาใช้แทน Text ด้านบน\nถ้าไม่มีอันไหนผ่านเงื่อนไขเลย จะใช้ Text ปกติด้านบนตามเดิม")]
    public SanityOverride[] sanityOverrides;

    // คืนบทพูดที่ควรใช้จริงตาม Sanity ปัจจุบัน ณ ตอนพูดบรรทัดนี้ (เรียกจาก DialogueManager ตอนจะเริ่มพิมพ์)
    public string GetText(int currentSanity)
    {
        if (sanityOverrides == null || sanityOverrides.Length == 0)
            return text;

        SanityOverride best = null;

        foreach (var over in sanityOverrides)
        {
            if (currentSanity > over.threshold) continue; // ไม่เข้าเงื่อนไข "เท่ากับหรือต่ำกว่า"

            // มีหลายอันผ่านพร้อมกันได้ (เช่นตั้ง 50 กับ 25 ไว้ แล้ว sanity ตอนนี้ = 10) เลือกอันที่ threshold ต่ำสุด
            // เพราะถือว่า "ตรงกับสถานการณ์ตอนนี้มากที่สุด" (วิกฤตกว่า)
            if (best == null || over.threshold < best.threshold)
                best = over;
        }

        return best != null ? best.text : text;
    }
}

[System.Serializable]
public class SanityOverride
{
    public int threshold;

    [TextArea(2, 5)]
    public string text;
}