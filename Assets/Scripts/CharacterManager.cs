using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    [SerializeField] private CharacterRuntime[] characters;

    [Header("แฟ้มประวัติคนไข้ (เรียงลำดับให้ตรงกับ characters ด้านบนทุกช่อง)")]
    [Tooltip("ช่องที่ i คือหน้าประวัติของ characters[i] เอาเมาส์ไปวางที่ไอคอนแฟ้ม (ดู PatientFileHover) จะโชว์ช่องของตัวละครปัจจุบันให้เอง")]
    [SerializeField] private GameObject[] historyPanels;

    [Header("(ไม่ใส่ก็ได้) เรียกตอนเล่นครบทุกตัวละคร")]
    public UnityEngine.Events.UnityEvent onAllCharactersFinished;

    private int currentIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("[CharacterManager] ไม่มีตัวละครในระบบ");
            return;
        }

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] == null)
            {
                Debug.LogError($"[CharacterManager] ช่อง characters[{i}] ว่าง");
                continue;
            }
            characters[i].gameObject.SetActive(i == 0);
        }

        if (historyPanels != null && historyPanels.Length != characters.Length)
            Debug.LogError($"[CharacterManager] historyPanels ({historyPanels.Length}) กับ characters ({characters.Length}) จำนวนไม่เท่ากัน เช็ค index ให้ตรงกันด้วย");

        HideAllHistoryPanels();
    }

    private void Start()
    {
        if (characters == null || characters.Length == 0) return;

        // ตัวแรก: จอยังใสอยู่ ให้ NarrationManager เฟดดำเองแล้วเล่า intro
        BeginCharacter(alreadyBlack: false);
    }

    public CharacterRuntime GetCurrent()
    {
        if (characters == null || currentIndex < 0 || currentIndex >= characters.Length) return null;
        return characters[currentIndex];
    }

    // แฟ้มประวัติของตัวละครปัจจุบัน (ตาม currentIndex) เรียกจาก PatientFileHover ตอนเอาเมาส์ไปวาง
    public GameObject GetCurrentHistoryPanel()
    {
        if (historyPanels == null || currentIndex >= historyPanels.Length) return null;
        return historyPanels[currentIndex];
    }

    private void HideAllHistoryPanels()
    {
        if (historyPanels == null) return;
        foreach (var panel in historyPanels)
        {
            if (panel == null) continue;
            panel.SetActive(false);
        }
    }

    public void NextCharacter(bool wasCrisisEnding)
    {
        // เฟดดำเข้า -> เล่นจอดำสรุปของตัวละครที่เพิ่งจบ (ตาม good/bad ending) -> สลับตัวละครตอนยังดำอยู่ -> ต่อด้วย intro ตัวถัดไปเลย ไม่มีจอสว่างคั่นกลาง
        TransitionManager.Instance.FadeToBlack(() =>
        {
            CharacterRuntime finished = characters[currentIndex];
            NarrationSequence endingNarration = wasCrisisEnding
                ? finished.data.crisisEndingNarration
                : finished.data.goodEndingNarration;

            NarrationManager.Instance.PlaySequence(
                endingNarration,
                onComplete: () => SwitchToNextCharacterAlreadyBlack(),
                alreadyBlack: true,
                fadeOutAtEnd: false);
        });
    }

    private void SwitchToNextCharacterAlreadyBlack()
    {
        if (characters[currentIndex] != null)
            characters[currentIndex].gameObject.SetActive(false);
            
        currentIndex++;

        if (currentIndex >= characters.Length)
        {
            Debug.Log("ตัวละครหมดแล้ว จบเกม");
            TransitionManager.Instance.SetBlackInstant(true); // ค้างจอดำตอนจบ
            onAllCharactersFinished?.Invoke();
            return;
        }

        if (characters[currentIndex] != null)
            characters[currentIndex].gameObject.SetActive(true);
            
        BeginCharacter(alreadyBlack: true);
    }

    // เล่น narration จอดำก่อน แล้วค่อยเริ่มบทพูดปกติ "ตอนจอใสสนิทแล้วเท่านั้น"
    private void BeginCharacter(bool alreadyBlack)
    {
        CharacterRuntime target = characters[currentIndex];

        NarrationManager.Instance.PlaySequence(
            target.data.introNarration,
            onComplete: () => DialogueManager.Instance.StartCharacter(target),
            alreadyBlack: alreadyBlack);
    }
}