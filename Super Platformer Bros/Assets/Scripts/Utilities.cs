using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static GameObject GetChildByName(this GameObject self, string name)
    {
        foreach (Transform trans in self.transform)
        {
            if (trans.name == name)
                return trans.gameObject;
        }

        return null;
    }

    public static IEnumerator FadeOutAudio(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }
}
