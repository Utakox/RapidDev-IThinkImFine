using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    [SerializeField] private CharacterRuntime[] characters;

    [Header("(ไม่ใส่ก็ได้) เรียกตอนเล่นครบทุกตัวละคร")]
    public UnityEngine.Events.UnityEvent onAllCharactersFinished;

    private int currentIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] == null)
            {
                Debug.LogError($"[CharacterManager] ช่อง characters[{i}] ว่าง");
                continue;
            }
            characters[i].gameObject.SetActive(i == 0);
        }
    }

    private void Start()
    {
        if (characters.Length == 0)
        {
            Debug.LogError("[CharacterManager] ไม่มีตัวละครเลย");
            return;
        }

        // ตัวแรก: จอยังใสอยู่ ให้ NarrationManager เฟดดำเองแล้วเล่า intro
        BeginCharacter(alreadyBlack: false);
    }

    public CharacterRuntime GetCurrent()
    {
        return characters[currentIndex];
    }

    public void NextCharacter()
    {
        // เฟดดำเข้า -> สลับตัวละครตอนมองไม่เห็น -> ค้างดำไว้ให้ narration เล่นต่อทันที
        TransitionManager.Instance.FadeToBlack(() =>
        {
            characters[currentIndex].gameObject.SetActive(false);
            currentIndex++;

            if (currentIndex >= characters.Length)
            {
                Debug.Log("ตัวละครหมดแล้ว จบเกม");
                TransitionManager.Instance.SetBlackInstant(true); // ค้างจอดำตอนจบ
                onAllCharactersFinished?.Invoke();
                return;
            }

            characters[currentIndex].gameObject.SetActive(true);
            BeginCharacter(alreadyBlack: true);
        });
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