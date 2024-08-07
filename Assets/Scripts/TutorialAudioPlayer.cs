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
    private int previousPraiseIndex = -1;

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
        StartCoroutine(PlayAudioClipDelayed(audioClip));
    }

    private IEnumerator PlayAudioClipDelayed(AudioClip audioClip)
    {
        backgroundAudioSource.DOFade(0.35f, 1);
        yield return new WaitForSeconds(delay);
        audioSource.Stop();
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    public void PlayAudioClipIfFree(AudioClip audioClip)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }

    public void PlayPraiseAudioClip()
    {
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, praisesAudioClips.Length);
        } while (previousPraiseIndex == randomIndex);
        previousPraiseIndex = randomIndex;
        AudioClip audioClip = praisesAudioClips[randomIndex];
        audioSource.Stop();
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}
