using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// ติดสคริปต์นี้กับ Choice ซ้าย และ Choice ขวา (คนละอัน)
public class ChoiceOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float holdDuration = 2f;

    private int sanityChange = 0; // ChoiceManager เซ็ตให้อัตโนมัติทุกครั้งที่สุ่ม choice ใหม่
    private Coroutine holdRoutine;
    private bool isConfirmed = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isConfirmed) return;
        holdRoutine = StartCoroutine(CountdownRoutine());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isConfirmed) return;
        if (holdRoutine != null)
            StopCoroutine(holdRoutine);
    }

    private IEnumerator CountdownRoutine()
    {
        yield return new WaitForSeconds(holdDuration);
        Confirm();
    }

    private void Confirm()
    {
        isConfirmed = true;

        SanityManager.Instance.ChangeSanity(sanityChange);
        ChoiceManager.Instance.OnChoiceConfirmed();
    }

    // เรียกจาก ChoiceManager.SetupOneChoice() ตอนสุ่ม choice รอบใหม่
    public void SetChoice(int newSanityChange)
    {
        sanityChange = newSanityChange;
        isConfirmed = false;

        if (holdRoutine != null)
            StopCoroutine(holdRoutine);
    }
}