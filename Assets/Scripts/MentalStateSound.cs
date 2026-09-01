using UnityEngine;

// เสียงเดียว + volume ของมันเอง ใช้ทั้งฝั่ง default (DialogueManager) และ override ต่อตัวละคร (CharacterData)
// ไม่ต้องเป็น list/array แล้ว เพราะแต่ละอันปรับ volume เองได้อยู่แล้ว ไม่ต้องสุ่ม
[System.Serializable]
public struct MentalStateSound
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume;
}