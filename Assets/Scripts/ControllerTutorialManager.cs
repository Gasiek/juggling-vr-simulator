using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ControllerTutorialManager : MonoBehaviour
{
    public CanvasGroup leftControllerTriggerCanvasGroup;
    public CanvasGroup rightControllerTriggerCanvasGroup;
    public CanvasGroup leftControllerGripCanvasGroup;
    public CanvasGroup rightControllerGripCanvasGroup;
    public CanvasGroup leftControllerJoystickCanvasGroup;
    public CanvasGroup rightControllerJoystickCanvasGroup;
    public CanvasGroup leftControllerPrimaryButtonCanvasGroup;
    public CanvasGroup rightControllerPrimaryButtonCanvasGroup;
    public CanvasGroup leftControllerSecondaryButtonCanvasGroup;
    public CanvasGroup rightControllerSecondaryButtonCanvasGroup;
    private CanvasGroup tutorialCanvasGroup;
    private Coroutine grabTutorialCoroutine;
    private Coroutine ballResetTutorialCoroutine;
    private Coroutine infiniteBallResetTutorialCoroutine;

    private void Awake()
    {
        tutorialCanvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowMovementTutorial()
    {
        tutorialCanvasGroup.DOFade(1, 0.5f);
        leftControllerJoystickCanvasGroup.DOFade(1, 0.5f).SetLoops(8, LoopType.Yoyo).SetDelay(.5f);
    }

    public void ShowGrabTutorial()
    {
        if (grabTutorialCoroutine != null)
        {
            StopCoroutine(grabTutorialCoroutine);
        }
        grabTutorialCoroutine = StartCoroutine(GrabTutorial());
    }

    private IEnumerator GrabTutorial()
    {
        tutorialCanvasGroup.DOFade(1, 0.5f);

        yield return new WaitForSeconds(4.5f);

        leftControllerGripCanvasGroup.DOFade(1, 0.5f).SetLoops(3, LoopType.Yoyo);
        rightControllerGripCanvasGroup.DOFade(1, 0.5f).SetLoops(3, LoopType.Yoyo);
        leftControllerGripCanvasGroup.DOFade(0, 0.5f).SetDelay(2f);
        rightControllerGripCanvasGroup.DOFade(0, 0.5f).SetDelay(2f);

        yield return new WaitForSeconds(2f);

        leftControllerTriggerCanvasGroup.DOFade(1, 0.5f).SetLoops(3, LoopType.Yoyo);
        rightControllerTriggerCanvasGroup.DOFade(1, 0.5f).SetLoops(3, LoopType.Yoyo);
        leftControllerTriggerCanvasGroup.DOFade(0, 0.5f).SetDelay(2f);
        rightControllerTriggerCanvasGroup.DOFade(0, 0.5f).SetDelay(2f);

        yield return new WaitForSeconds(7f);

        rightControllerGripCanvasGroup.DOFade(1, 0.5f).SetLoops(3, LoopType.Yoyo);
        rightControllerTriggerCanvasGroup.DOFade(1, 0.5f).SetLoops(3, LoopType.Yoyo);
        rightControllerGripCanvasGroup.DOFade(0, 0.5f).SetDelay(2f);
        rightControllerTriggerCanvasGroup.DOFade(0, 0.5f).SetDelay(2f);

        yield return new WaitForSeconds(4f);

        tutorialCanvasGroup.DOFade(0, 0.5f);
    }

    public void ShowBallResetTutorial()
    {
        if (ballResetTutorialCoroutine != null)
        {
            StopCoroutine(ballResetTutorialCoroutine);
        }
        ballResetTutorialCoroutine = StartCoroutine(BallResetTutorial());
    }

    public void HideAllTutorials()
    {
        tutorialCanvasGroup.DOFade(0, 0.5f);
        if (grabTutorialCoroutine != null)
        {
            StopCoroutine(grabTutorialCoroutine);
            grabTutorialCoroutine = null;
        }
        if (ballResetTutorialCoroutine != null)
        {
            StopCoroutine(ballResetTutorialCoroutine);
            ballResetTutorialCoroutine = null;
        }
    }

    private IEnumerator BallResetTutorial()
    {
        tutorialCanvasGroup.DOFade(1, 0.5f);
        yield return new WaitForSeconds(4.5f);

        rightControllerGripCanvasGroup.DOFade(1, 0.5f);
        rightControllerPrimaryButtonCanvasGroup.DOFade(1, 0.5f).SetLoops(2, LoopType.Yoyo).SetDelay(2f);
        rightControllerGripCanvasGroup.DOFade(0, 0.5f).SetDelay(3.5f);

        yield return new WaitForSeconds(4f);

        leftControllerGripCanvasGroup.DOFade(1, 0.5f);
        rightControllerGripCanvasGroup.DOFade(1, 0.5f);
        rightControllerPrimaryButtonCanvasGroup.DOFade(1, 0.5f).SetLoops(2, LoopType.Yoyo).SetDelay(2f);
        leftControllerGripCanvasGroup.DOFade(0, 0.5f).SetDelay(3.5f);
        rightControllerGripCanvasGroup.DOFade(0, 0.5f).SetDelay(3.5f);

        yield return new WaitForSeconds(4f);

        leftControllerGripCanvasGroup.DOFade(1, 0.5f);
        rightControllerTriggerCanvasGroup.DOFade(1, 0.5f);
        rightControllerGripCanvasGroup.DOFade(1, 0.5f);
        rightControllerPrimaryButtonCanvasGroup.DOFade(1, 0.5f).SetLoops(2, LoopType.Yoyo).SetDelay(2f);
        leftControllerGripCanvasGroup.DOFade(0, 0.5f).SetDelay(3.5f);
        rightControllerTriggerCanvasGroup.DOFade(0, 0.5f).SetDelay(3.5f);
        rightControllerGripCanvasGroup.DOFade(0, 0.5f).SetDelay(3.5f);

        yield return new WaitForSeconds(5f);
        tutorialCanvasGroup.DOFade(0, 0.5f);
    }

    public void ShowInfiniteBallResetTutorial()
    {
        tutorialCanvasGroup.DOFade(1, 0.5f).OnComplete(() =>
        {
            if (infiniteBallResetTutorialCoroutine != null)
            {
                StopCoroutine(infiniteBallResetTutorialCoroutine);
            }
            infiniteBallResetTutorialCoroutine = StartCoroutine(InfiniteBallResetTutorial());
        });
    }

    public void HideInfiniteBallResetTutorial()
    {
        tutorialCanvasGroup.DOFade(0, 0.5f);
        if (infiniteBallResetTutorialCoroutine != null)
        {
            StopCoroutine(infiniteBallResetTutorialCoroutine);
            infiniteBallResetTutorialCoroutine = null;
        }
    }

    private IEnumerator InfiniteBallResetTutorial()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);

            rightControllerGripCanvasGroup.DOFade(1, 0.5f);
            rightControllerPrimaryButtonCanvasGroup.DOFade(1, 0.5f).SetLoops(2, LoopType.Yoyo).SetDelay(2f);
            rightControllerGripCanvasGroup.DOFade(0, 0.5f).SetDelay(3.5f);

            yield return new WaitForSeconds(4f);

            leftControllerGripCanvasGroup.DOFade(1, 0.5f);
            rightControllerGripCanvasGroup.DOFade(1, 0.5f);
            rightControllerPrimaryButtonCanvasGroup.DOFade(1, 0.5f).SetLoops(2, LoopType.Yoyo).SetDelay(2f);
            leftControllerGripCanvasGroup.DOFade(0, 0.5f).SetDelay(3.5f);
            rightControllerGripCanvasGroup.DOFade(0, 0.5f).SetDelay(3.5f);

            yield return new WaitForSeconds(4f);

            leftControllerGripCanvasGroup.DOFade(1, 0.5f);
            rightControllerTriggerCanvasGroup.DOFade(1, 0.5f);
            rightControllerGripCanvasGroup.DOFade(1, 0.5f);
            rightControllerPrimaryButtonCanvasGroup.DOFade(1, 0.5f).SetLoops(2, LoopType.Yoyo).SetDelay(2f);
            leftControllerGripCanvasGroup.DOFade(0, 0.5f).SetDelay(3.5f);
            rightControllerTriggerCanvasGroup.DOFade(0, 0.5f).SetDelay(3.5f);
            rightControllerGripCanvasGroup.DOFade(0, 0.5f).SetDelay(3.5f);

            yield return new WaitForSeconds(2f);
        }
    }
}
