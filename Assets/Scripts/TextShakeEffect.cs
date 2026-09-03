using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TextShakeEffect : MonoBehaviour
{
    [Header("ค่า default ถ้าไม่ได้ตั้ง intensity เอง (เช่นตอน Meltdown บังคับสั่น)")]
    public float defaultIntensity = 2f;
    public float shakeSpeed = 25f;

    private TMP_Text tmp;
    private bool isShaking;
    private float intensity;

    private void Awake()
    {
        tmp = GetComponent<TMP_Text>();
    }

    public void SetShaking(bool shake, float customIntensity = 0f)
    {
        isShaking = shake;
        intensity = customIntensity > 0f ? customIntensity : defaultIntensity;

        if (!shake)
            ResetToNormal();
    }

    private void LateUpdate()
    {
        if (!isShaking || tmp == null || string.IsNullOrEmpty(tmp.text)) return;

        ShakeText();
    }

    private void ShakeText()
    {
        tmp.ForceMeshUpdate();
        TMP_TextInfo textInfo = tmp.textInfo;

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
        tmp.ForceMeshUpdate(true, true);
    }
}