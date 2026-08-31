using UnityEngine;

// ติดกับ Choice ซ้าย และ Choice ขวา (คนละอัน) ตั้ง Is Left Side ใน Inspector ให้ตรงฝั่งด้วย
// สืบทอด HoldInteractable มาแล้ว ไม่ต้องเขียนระบบ hover/countdown เองอีก
public class ChoiceOption : HoldInteractable
{
    public bool isLeftSide = true;

    protected override void Confirm()
    {
        ChoiceManager.Instance.OnChoiceConfirmed(isLeftSide);
    }

    // เก็บชื่อเดิมไว้เผื่อ ChoiceManager เรียกอยู่ แค่ส่งต่อไปฟังก์ชันของ base
    public void ResetChoice() => ResetInteractable();
}