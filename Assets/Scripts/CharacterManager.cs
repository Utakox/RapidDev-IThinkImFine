using UnityEngine;

// จัดการว่าตอนนี้ตัวละครคนไหนกำลังเล่นอยู่ และสลับไปตัวถัดไปเมื่อจบตา
public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    public CharacterRuntime[] characters; // ลากตัวละครทุกตัวใส่เรียงลำดับ

    private int currentIndex = 0;

    private void Awake()
    {
        Instance = this;

        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].gameObject.SetActive(i == 0);
        }
    }

    private void Start()
    {
        // Start() รอให้ Awake ของทุกสคริปต์เสร็จหมดก่อน ปลอดภัยที่จะเรียกข้ามสคริปต์ตรงนี้
        DialogueManager.Instance.StartCharacter(characters[currentIndex]);
    }

    public CharacterRuntime GetCurrent()
    {
        return characters[currentIndex];
    }

    // เรียกจาก DialogueManager ตอนตัวละครคนปัจจุบันจบตาแล้ว
    public void NextCharacter()
    {
        characters[currentIndex].gameObject.SetActive(false);

        currentIndex++;
        if (currentIndex >= characters.Length)
        {
            Debug.Log("ตัวละครหมดแล้ว จบเกม");
            return;
        }

        characters[currentIndex].gameObject.SetActive(true);
        DialogueManager.Instance.StartCharacter(characters[currentIndex]);
    }
}