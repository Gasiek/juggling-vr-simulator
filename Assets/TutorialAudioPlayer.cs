using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialAudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private float delay = 2f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayAudioClip(AudioClip audioClip)
    {
        if (audioClip == null)
        {
            return;
        }
        StartCoroutine(PlayAudioClipDelayed(audioClip));
    }

    private IEnumerator PlayAudioClipDelayed(AudioClip audioClip)
    {
        yield return new WaitForSeconds(delay);
        audioSource.clip = audioClip;
        audioSource.Play();
        PlayAudioClip(audioClip);
    }
}
