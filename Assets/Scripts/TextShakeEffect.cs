using UnityEngine;
using TMPro;

// ติดกับ object เดียวกับ TextMeshProUGUI ที่ใช้โชว์ dialogue (ตัวเดียวกับ dialogueText ใน DialogueManager)
[RequireComponent(typeof(TMP_Text))]
public class TextShakeEffect : MonoBehaviour
{
    public static TextShakeEffect Instance;

    [Header("ค่า default ถ้าบรรทัดไหนสั่นแต่ไม่ได้ตั้ง intensity เอง")]
    public float defaultIntensity = 2f;
    public float shakeSpeed = 25f;

    private TMP_Text tmp;
    private bool isShaking;
    private float intensity;

    private void Awake()
    {
        Instance = this;
        tmp = GetComponent<TMP_Text>();
    }

    // เรียกจาก DialogueManager ทุกครั้งที่ขึ้นบรรทัดใหม่ ส่ง intensity มาด้วย (0 = ใช้ default)
    public void SetShaking(bool shake, float customIntensity = 0f)
    {
        isShaking = shake;
        intensity = customIntensity > 0f ? customIntensity : defaultIntensity;

        if (!shake)
            ResetToNormal(); // ปิดแล้วให้ตัวอักษรกลับตำแหน่งปกติทันที ไม่ค้างเบี้ยว
    }

    private void LateUpdate()
    {
        if (!isShaking) return;

        // ข้อความว่างอยู่ ไม่ต้องทำอะไรเลย
        if (tmp == null || string.IsNullOrEmpty(tmp.text)) return;

        ShakeText();
    }

    private void ShakeText()
    {
        tmp.ForceMeshUpdate();
        TMP_TextInfo textInfo = tmp.textInfo;

        // กันเคสไม่มีตัวอักษรที่มองเห็นได้
        // ถ้าไม่ return ลูปล่างจะเอา vertices เก่าที่ค้างใน meshInfo อัปโหลดทับ
        // = ข้อความเดิมเด้งกลับมาบนจอ ทั้งที่ .text = "" ไปแล้ว (ต้นเหตุบั๊กข้อความไม่หาย)
        if (textInfo == null || textInfo.characterCount == 0)
            return;

        bool anyVisible = false;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            anyVisible = true;

            int matIndex = textInfo.characterInfo[i].materialReferenceIndex;
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[matIndex].vertices;

            // สุ่มด้วย Perlin noise ต่อตัวอักษร (index i เป็นตัวขยับเฟสให้แต่ละตัวสั่นไม่พร้อมกัน)
            Vector3 offset = new Vector3(
                (Mathf.PerlinNoise(Time.time * shakeSpeed, i) - 0.5f) * intensity,
                (Mathf.PerlinNoise(i, Time.time * shakeSpeed) - 0.5f) * intensity,
                0f
            );

            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        if (!anyVisible) return;

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            tmp.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    private void ResetToNormal()
    {
        if (tmp == null) return;
        tmp.ForceMeshUpdate(true, true); // rebuild ทั้ง geometry ไม่ให้เหลือ offset ค้าง
    }
}