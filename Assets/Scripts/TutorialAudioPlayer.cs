using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TutorialAudioPlayer : MonoBehaviour
{
    public AudioSource backgroundAudioSource;
    public AudioClip[] praisesAudioClips;
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
        StopAllCoroutines();
        audioSource.Stop();
        StartCoroutine(PlayAudioClipDelayed(audioClip));
    }

    private IEnumerator PlayAudioClipDelayed(AudioClip audioClip)
    {
        backgroundAudioSource.DOFade(0.35f, 1);
        yield return new WaitForSeconds(delay);
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    public void PlayAudioClipIfFree(AudioClip audioClip)
    {
        if (!audioSource.isPlaying)
        {
            PlayAudioClip(audioClip);
        }
    }

    public void PlayPraiseAudioClip()
    {
        AudioClip audioClip = praisesAudioClips[Random.Range(0, praisesAudioClips.Length)];
        PlayAudioClip(audioClip);
    }
}
