using UnityEngine;
using UnityEngine.UI;

// ติดกับ GameObject ไหนก็ได้ในซีน แล้วลาก UI Image ที่จะโชว์รูปตัวละครใส่ Portrait Image
public class CharacterPortraitDisplay : MonoBehaviour
{
    public static CharacterPortraitDisplay Instance;

    [SerializeField] private Image portraitImage;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // เรียกจาก CharacterManager ทุกครั้งที่ตัวละครปัจจุบันเปลี่ยน
    public void UpdatePortrait()
    {
        if (portraitImage == null) return;

        Sprite sprite = CharacterManager.Instance.GetCurrentPortraitSprite();

        portraitImage.sprite = sprite;
        portraitImage.enabled = sprite != null; // ตัวละครนี้ไม่มีรูป = ซ่อน Image ไปเลย ไม่โชว์รูปเก่าค้าง
    }
}