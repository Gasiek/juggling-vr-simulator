using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class InteractorWithController
{
    public XRDirectInteractor interactor;
    public ActionBasedController controller;
    public Transform attachmentPoint;
    public bool isGrabbing = false;
}

public class BallResetHandler : MonoBehaviour
{
    public List<InteractorWithController> interactorWithControllers = new();
    public List<XRGrabInteractable> balls;
    public float resetTime;
    public SimulatorEvent ballsResetEvent;
    private Coroutine resetCoroutine;
    private int grabbingCount = 0;
    private SimulationManager simulationManager;

    private void Awake()
    {
        simulationManager = GetComponent<SimulationManager>();
    }

    void Start()
    {
        foreach (var interactorWithController in interactorWithControllers)
        {
            interactorWithController.controller.selectAction.action.Enable();
        }
    }

    void Update()
    {
        grabbingCount = 0;
        bool alreadyHolding = false;
        foreach (var interactorWithController in interactorWithControllers)
        {
            bool isPressed = interactorWithController.controller.selectAction.action.IsPressed();
            interactorWithController.isGrabbing = isPressed;
            if (isPressed)
            {
                grabbingCount++;
            }
            if (interactorWithController.interactor.hasSelection)
            {
                alreadyHolding = true;
            }
        }
        if (grabbingCount >= simulationManager.GetCurrentNumberOfBalls() && !alreadyHolding)
        {
            if (resetCoroutine == null)
            {
                resetCoroutine = StartCoroutine(ResetBallsAfterDelay());
            }
        }
        else
        {
            if (resetCoroutine != null)
            {
                StopCoroutine(resetCoroutine);
                resetCoroutine = null;
            }
        }
    }

    private IEnumerator ResetBallsAfterDelay()
    {
        yield return new WaitForSeconds(resetTime);
        // if (grabbingCount < simulationManager.GetCurrentNumberOfBalls())
        // {
        //     yield break;
        // }
        ballsResetEvent.Raise();
        int grabbedCount = 0;
        for (int i = 0; i < interactorWithControllers.Count; i++)
        {
            if (interactorWithControllers[i].isGrabbing)
            {
                ResetBall(balls[grabbedCount], interactorWithControllers[i].interactor, interactorWithControllers[i].attachmentPoint);
                grabbedCount++;
                if (grabbedCount >= simulationManager.GetCurrentNumberOfBalls())
                {
                    break;
                }
            }
        }
        resetCoroutine = null;
    }

    private void ResetBall(XRGrabInteractable ballGrabInteractable, XRDirectInteractor interactor, Transform attachmentPoint)
    {
        ballGrabInteractable.transform.SetPositionAndRotation(attachmentPoint.position, attachmentPoint.rotation); // TODO: Use Rigidbody instead of Transform
        interactor.attachTransform = attachmentPoint;
        string tempTag = ballGrabInteractable.transform.tag;
        ballGrabInteractable.transform.tag = "Untagged";
        Rigidbody rb = ballGrabInteractable.GetComponent<Rigidbody>();
        interactor.StartManualInteraction((IXRSelectInteractable)ballGrabInteractable);
        interactor.EndManualInteraction();
        ballGrabInteractable.transform.tag = tempTag;
    }
}
