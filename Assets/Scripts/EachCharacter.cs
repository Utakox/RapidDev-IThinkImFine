using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ติดสคริปต์นี้กับ Image ของตัวละครแต่ละตัว
public class EachCharacter : CharacterBase
{
    // ไม่ต้องเขียน sanity, ChangeSanity, UpdateFace ซ้ำแล้ว
    // เพราะ base class ทำให้หมดแล้ว
    // ถ้าตัวละครไหนมี behavior พิเศษ ค่อย override ทีหลังได้

    protected override void Awake()
    {
        base.Awake(); // เรียกของ base ก่อน (setup faceImage, UpdateFace)
        // ใส่ logic เฉพาะตัวละครนี้เพิ่มได้ตรงนี้ ถ้ามี
    }
}