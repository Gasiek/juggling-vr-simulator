using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PulsatingCircles : MonoBehaviour
{
    public GameObject circlePrefab;
    public float pulseInterval = 2f;  // Total interval between starting pulses
    public float circleDelay = 0.5f;  // Delay between each circle's appearance
    public float scaleAndFadeTime = 1f;
    public int numberOfCirclesPerPulse = 3;
    public Vector3 initialScale;
    private Coroutine pulsatingCirclesCoroutine;
    // public CatchCounter catchCounter;

    public void StartPulsatingCirclesAfterDelay(float delay)
    {
        if (pulsatingCirclesCoroutine == null)
        {
            pulsatingCirclesCoroutine = StartCoroutine(SpawnCircles(delay));
        }
    }

    public void StopPulsatingCircles()
    {
        if (pulsatingCirclesCoroutine != null)
        {
            StopCoroutine(pulsatingCirclesCoroutine);
            pulsatingCirclesCoroutine = null;
        }
    }

    private IEnumerator SpawnCircles(float delay)
    {
        yield return new WaitForSeconds(delay);
        while (true)
        {
            // catchCounter.ResetCurrentScore();
            for (int i = 0; i < numberOfCirclesPerPulse; i++)
            {
                StartCoroutine(SpawnCircleWithDelay(i * circleDelay));
            }
            yield return new WaitForSeconds(pulseInterval);
        }
    }

    private IEnumerator SpawnCircleWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject circle = Instantiate(circlePrefab, transform.position, transform.rotation, transform);
        circle.transform.localScale = initialScale;

        // Apply DOTween animations
        RectTransform rectTransform = circle.GetComponent<RectTransform>();
        Image image = circle.GetComponent<Image>();

        // Scale and fade animation using DOTween
        Sequence sequence = DOTween.Sequence();
        sequence.Append(rectTransform.DOScale(1.75f, scaleAndFadeTime).SetEase(Ease.OutQuad));
        sequence.Join(image.DOFade(0, scaleAndFadeTime).SetEase(Ease.OutQuad));
        sequence.AppendCallback(() => Destroy(circle));
    }
}
