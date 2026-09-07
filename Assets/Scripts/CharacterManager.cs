using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [SerializeField] private CharacterRuntime[] characters;

    [Header("(ไม่ใส่ก็ได้) เรียกตอนเล่นครบทุกตัวละคร")]
    public UnityEngine.Events.UnityEvent onAllCharactersFinished;

    private int currentIndex = 0;

    public int CurrentIndex => currentIndex;

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
            if (characters[i] == null) continue;
            characters[i].gameObject.SetActive(i == 0);
        }
    }

    private void Start()
    {
        if (characters == null || characters.Length == 0) return;
        BeginCharacter(alreadyBlack: false);
    }

    public CharacterRuntime GetCurrent()
    {
        if (characters == null || currentIndex < 0 || currentIndex >= characters.Length) return null;
        return characters[currentIndex];
    }

    // ดึงรูป inspectSprite จาก CharacterData ของตัวละครปัจจุบัน (เฉพาะระบบ Patient History)
    public Sprite GetCurrentInspectSprite()
    {
        CharacterRuntime current = GetCurrent();
        return (current != null && current.data != null) ? current.data.inspectSprite : null;
    }

    // ดึงรูป portraitSprite จาก CharacterData ของตัวละครปัจจุบัน (คนละตัวกับ inspectSprite)
    public Sprite GetCurrentPortraitSprite()
    {
        CharacterRuntime current = GetCurrent();
        return (current != null && current.data != null) ? current.data.portraitSprite : null;
    }

    public void NextCharacter(bool wasCrisisEnding)
    {
        TransitionManager.Instance.FadeToBlack(() =>
        {
            CharacterRuntime finished = characters[currentIndex];
            NarrationSequence endingNarration = wasCrisisEnding
                ? finished.data.crisisEndingNarration
                : finished.data.goodEndingNarration;

            NarrationManager.Instance.PlaySequence(
                endingNarration,
                onComplete: () =>
                {
                    // ซ่อนตัวละครที่จบไปเลยทันทีตรงนี้ ก่อนทำอะไรต่อ (สลับ index / โชว์ตัวถัดไป)
                    // ไม่ต้องพึ่งพาว่าจอจะดำสนิทหรือเปล่า ต่อให้เฟดมีปัญหายังไง ก็ไม่มีตัวละครให้เห็นอยู่ดี
                    if (finished != null)
                        finished.gameObject.SetActive(false);

                    SwitchToNextCharacterAlreadyBlack();
                },
                alreadyBlack: true,

                fadeOutAtEnd: false); 
                Destroy(finished.gameObject); // ลบตัวละครที่จบไปแล้วทิ้ง (ไม่ต้องเก็บไว้ในซีน)
        });
    }

    private void SwitchToNextCharacterAlreadyBlack()
    {
        // ตัวเก่าถูกซ่อนไปแล้วใน NextCharacter() ด้านบน ไม่ต้อง SetActive(false) ซ้ำตรงนี้อีก
        currentIndex++;

        if (currentIndex >= characters.Length)
        {
            Debug.Log("ตัวละครหมดแล้ว จบเกม");
            TransitionManager.Instance.SetBlackInstant(true);
            onAllCharactersFinished?.Invoke();
            return;
        }

        if (characters[currentIndex] != null)
            characters[currentIndex].gameObject.SetActive(true);
            
        BeginCharacter(alreadyBlack: true);
    }

    private void BeginCharacter(bool alreadyBlack)
    {
        CharacterRuntime target = characters[currentIndex];

        if (CharacterPortraitDisplay.Instance != null)
            CharacterPortraitDisplay.Instance.UpdatePortrait();

        NarrationManager.Instance.PlaySequence(
            target.data.introNarration,
            onComplete: () => DialogueManager.Instance.StartCharacter(target),
            alreadyBlack: alreadyBlack);
    }
}