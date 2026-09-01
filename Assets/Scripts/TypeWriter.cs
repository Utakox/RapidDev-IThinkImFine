using System.Collections;
using UnityEngine;
using TMPro;

// ตัวกลางพิมพ์ข้อความทีละตัว + เสียงพิมพ์ที่ตัดพร้อมข้อความจบ ใช้ร่วมกันทั้ง DialogueManager และ NarrationManager
// (เดิมทั้ง 2 ไฟล์เขียน logic นี้แยกกัน ทำให้แก้ทีต้องแก้ 2 ที่)
public static class Typewriter
{
    public static IEnumerator TypeLine(
        TMP_Text text, string fullText, float charDelay,
        AudioSource audio, AudioClip loopClip, float volume, float fadeOut,
        int startVisible = 0, System.Func<bool> checkSkip = null, bool unscaled = false, bool stopSoundAtEnd = true)
    {
        text.text = fullText;
        text.ForceMeshUpdate(true, true);
        int total = text.textInfo.characterCount;
        text.maxVisibleCharacters = startVisible;

        if (audio != null && loopClip != null)
        {
            audio.clip = loopClip;
            audio.volume = volume;
            audio.loop = true;
            if (!audio.isPlaying) audio.Play();
        }

        for (int c = startVisible + 1; c <= total; c++)
        {
            if (checkSkip != null && checkSkip())
            {
                text.maxVisibleCharacters = total;
                break;
            }
            text.maxVisibleCharacters = c;
            yield return unscaled ? Wait(charDelay) : new WaitForSeconds(charDelay);
        }
        text.maxVisibleCharacters = total;

        if (stopSoundAtEnd)
            yield return StopSound(audio, fadeOut, unscaled);
    }

    public static IEnumerator StopSound(AudioSource source, float fadeOut, bool unscaled)
    {
        if (source == null || !source.isPlaying) yield break;

        float start = source.volume;
        float t = 0f;
        while (t < fadeOut)
        {
            t += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            source.volume = Mathf.Lerp(start, 0f, t / fadeOut);
            yield return null;
        }
        source.Stop();
        source.volume = start;
    }

    public static IEnumerator Wait(float seconds)
    {
        float t = 0f;
        while (t < seconds) { t += Time.unscaledDeltaTime; yield return null; }
    }
}