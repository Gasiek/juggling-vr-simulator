using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeInteractorController : MonoBehaviour
{
    public Transform correctGazeTransform;
    public Transform headset;
    public CanvasGroup LookUpCanvasGroup;
    public CanvasGroup LookDownCanvasGroup;
    public CanvasGroup GazeCanvasGroup;
    public PulsatingCircles pulsatingCircles;
    private Coroutine currentLookDownCoroutine;
    private Coroutine hideLookDownCoroutine;
    private Coroutine currentLookUpCoroutine;
    private Coroutine hideLookUpCoroutine;
    private Coroutine currentGazeCoroutine;
    private Coroutine hideGazeCoroutine;
    public float fadeInDuration;
    public float fadeOutDuration;

    private void Update()
    {
        float angleDifference = correctGazeTransform.rotation.eulerAngles.x - headset.rotation.eulerAngles.x;
        if (angleDifference > 180) angleDifference -= 360;
        if (angleDifference < -180) angleDifference += 360;
        Debug.Log(angleDifference);

        if (angleDifference < 6 && angleDifference > -6)
        {
            StopAllCoroutines();
            hideLookDownCoroutine = StartCoroutine(HideImage(LookDownCanvasGroup));
            hideLookUpCoroutine = StartCoroutine(HideImage(LookUpCanvasGroup));
            hideGazeCoroutine = StartCoroutine(HideImage(GazeCanvasGroup));
            pulsatingCircles.StopPulsatingCircles();
            currentLookDownCoroutine = null;
            currentLookUpCoroutine = null;
            currentGazeCoroutine = null;
        }
        else
        {
            if (angleDifference > 10)
            {
                LookUpCanvasGroup.alpha = 0;
                if (currentLookDownCoroutine == null)
                {
                    if (hideLookDownCoroutine != null)
                    {
                        StopCoroutine(hideLookDownCoroutine);
                        hideLookDownCoroutine = null;
                    }
                    currentLookDownCoroutine = StartCoroutine(ShowImageAfterDelay(LookDownCanvasGroup, 2));
                    pulsatingCircles.StartPulsatingCirclesAfterDelay(2);
                }
                if (currentLookUpCoroutine != null)
                {
                    StopCoroutine(currentLookUpCoroutine);
                    currentLookUpCoroutine = null;
                }
            }
            else if (angleDifference < -10)
            {
                LookDownCanvasGroup.alpha = 0;
                if (currentLookUpCoroutine == null)
                {
                    if (hideLookUpCoroutine != null)
                    {
                        StopCoroutine(hideLookUpCoroutine);
                        hideLookUpCoroutine = null;
                    }
                    currentLookUpCoroutine = StartCoroutine(ShowImageAfterDelay(LookUpCanvasGroup, 2));
                    pulsatingCircles.StartPulsatingCirclesAfterDelay(2);
                }
                if (currentLookDownCoroutine != null)
                {
                    StopCoroutine(currentLookDownCoroutine);
                    currentLookDownCoroutine = null;
                }
            }
            if (currentGazeCoroutine == null)
            {
                if (hideGazeCoroutine != null)
                {
                    StopCoroutine(hideGazeCoroutine);
                    hideGazeCoroutine = null;
                }
                currentGazeCoroutine = StartCoroutine(ShowImageAfterDelay(GazeCanvasGroup, 0));
            }
        }
    }

    private IEnumerator ShowImageAfterDelay(CanvasGroup canvasGroup, float delay)
    {
        yield return new WaitForSeconds(delay);

        float startAlpha = canvasGroup.alpha;
        float endAlpha = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha; // Ensure it reaches the end alpha value
    }
    private IEnumerator HideImage(CanvasGroup canvasGroup)
    {
        float startAlpha = canvasGroup.alpha;
        float endAlpha = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha; // Ensure it reaches the end alpha value
    }
}
