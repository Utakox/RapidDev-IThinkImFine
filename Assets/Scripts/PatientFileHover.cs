using UnityEngine;
using UnityEngine.EventSystems;

public class PatientFileHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject shownPanel;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CharacterManager.Instance == null) return;

        shownPanel = CharacterManager.Instance.GetCurrentHistoryPanel();
        if (shownPanel != null) shownPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (shownPanel != null)
        {
            shownPanel.SetActive(false);
            shownPanel = null;
        }
    }

    private void OnDisable()
    {
        if (shownPanel != null)
        {
            shownPanel.SetActive(false);
            shownPanel = null;
        }
    }
}