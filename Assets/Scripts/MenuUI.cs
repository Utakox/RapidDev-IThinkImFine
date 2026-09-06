using UnityEngine;
using UnityEngine.SceneManagement;

// วิธีใช้: ลาก component นี้ไปใส่ GameObject ว่างในซีนเมนู (เช่นชื่อ "MenuManager")
// แล้วที่ปุ่ม UI แต่ละปุ่ม -> OnClick() -> ลาก object นี้ใส่ -> เลือกเมธอดจาก dropdown
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game";

    [SerializeField] private GameObject creditsPanel;

    private void Awake()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false); // กันลืมปิดไว้ใน Editor
    }

    // ผูกกับปุ่ม "Start" / "Play"
    public void PlayGame()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogWarning("[MainMenuController] ยังไม่ได้ตั้งชื่อ gameSceneName");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            Debug.LogError($"[MainMenuController] ไม่พบซีนชื่อ \"{gameSceneName}\" ใน Build Settings — เช็คว่าเพิ่มซีนนี้ใน File > Build Settings แล้วหรือยัง และชื่อสะกดตรงกันไหม");
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    // ผูกกับปุ่ม "Credit" (เปิด panel)
    public void ShowCredits()
    {
        if (creditsPanel == null)
        {
            Debug.LogWarning("[MainMenuController] ยังไม่ได้ลาก creditsPanel ใส่ใน Inspector");
            return;
        }

        creditsPanel.SetActive(true);
    }

    // ผูกกับปุ่ม "Back" / "X" ที่อยู่ในหน้า Credit เอง (ปิด panel)
    public void HideCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    // ผูกกับปุ่ม "Quit" / "Exit Game"
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // กด Quit ตอนอยู่ใน Editor จะแค่หยุด Play Mode แทนการปิดโปรแกรมจริง
#else
        Application.Quit();
#endif
    }
}