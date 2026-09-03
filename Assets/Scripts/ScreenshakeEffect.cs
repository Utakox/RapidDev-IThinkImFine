using UnityEngine;

public class ScreenShakeEffect : MonoBehaviour
{
    public static ScreenShakeEffect Instance;

    [Header("ความแรง/ความเร็วของอาการจอสั่น")]
    [SerializeField] private float intensity = 0.15f;
    [SerializeField] private float shakeSpeed = 20f;

    private Vector3 originalLocalPos;
    private bool isShaking;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        originalLocalPos = transform.localPosition;
    }

    public void SetShaking(bool on)
    {
        isShaking = on;
        if (!on) transform.localPosition = originalLocalPos;
    }

    private void LateUpdate()
    {
        if (!isShaking) return;

        float x = (Mathf.PerlinNoise(Time.unscaledTime * shakeSpeed, 0f) - 0.5f) * 2f * intensity;
        float y = (Mathf.PerlinNoise(0f, Time.unscaledTime * shakeSpeed) - 0.5f) * 2f * intensity;

        transform.localPosition = originalLocalPos + new Vector3(x, y, 0f);
    }
}