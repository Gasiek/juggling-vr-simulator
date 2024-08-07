using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GazeInteractorController : MonoBehaviour
{
    public SimulationManager simulationManager;
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
    public CanvasGroup gazeCanvas;
    public CanvasGroup correctGazeCanvas;
    public Material fadeToBlackMaterial;


    private void Update()
    {
        float angleDifference = correctGazeTransform.rotation.eulerAngles.x - headset.rotation.eulerAngles.x;
        if (angleDifference > 180) angleDifference -= 360;
        if (angleDifference < -180) angleDifference += 360;

        if (angleDifference < 8 && angleDifference > -8)
        {
            simulationManager.SetWaitingForCorrectGazeToFalseAndCheckProgress();
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
            if (angleDifference > 16)
            {
                LookUpCanvasGroup.alpha = 0;
                if (currentLookDownCoroutine == null)
                {
                    if (hideLookDownCoroutine != null)
                    {
                        StopCoroutine(hideLookDownCoroutine);
                        hideLookDownCoroutine = null;
                    }
                    currentLookDownCoroutine = StartCoroutine(ShowImageAfterDelay(LookDownCanvasGroup, 1));
                    pulsatingCircles.StartPulsatingCirclesAfterDelay(2);
                }
                if (currentLookUpCoroutine != null)
                {
                    StopCoroutine(currentLookUpCoroutine);
                    currentLookUpCoroutine = null;
                }
            }
            else if (angleDifference < -15)
            {
                LookDownCanvasGroup.alpha = 0;
                if (currentLookUpCoroutine == null)
                {
                    if (hideLookUpCoroutine != null)
                    {
                        StopCoroutine(hideLookUpCoroutine);
                        hideLookUpCoroutine = null;
                    }
                    currentLookUpCoroutine = StartCoroutine(ShowImageAfterDelay(LookUpCanvasGroup, 1));
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
            float fadeAlpha = 0f;
            if (angleDifference > 16)
            {
                fadeAlpha = Mathf.Clamp01((Mathf.Abs(angleDifference) - 16) / 3);
            }
            else if (angleDifference < -15)
            {
                fadeAlpha = Mathf.Clamp01((Mathf.Abs(angleDifference) - 15) / 3);
            }
            fadeToBlackMaterial.color = new Color(0, 0, 0, fadeAlpha);
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

    private IEnumerator EnableGazeCheck()
    {
        yield return new WaitForSeconds(2);
        gazeCanvas.DOFade(1, 1f);
        correctGazeCanvas.DOFade(1, 1f);
        enabled = true;
    }

    private void OnEnable()
    {
        fadeToBlackMaterial.DOFade(0, 0.5f);
        if (!simulationManager.GetShouldTrackGaze() || simulationManager.IsBallGrounded())
        {
            this.enabled = false;
            return;
        }
        StartCoroutine(EnableGazeCheck());
    }

    private void OnDisable()
    {
        fadeToBlackMaterial.DOFade(0, 0.5f);
        StopAllCoroutines();
        currentLookDownCoroutine = null;
        currentLookUpCoroutine = null;
        currentGazeCoroutine = null;
        hideGazeCoroutine = null;
        hideLookDownCoroutine = null;
        hideLookUpCoroutine = null;
        DisableGazeCheck();
    }

    public void DisableGazeCheck()
    {
        gazeCanvas.DOFade(0, 0.5f);
        correctGazeCanvas.DOFade(0, 0.5f);
        enabled = false;
    }
}
