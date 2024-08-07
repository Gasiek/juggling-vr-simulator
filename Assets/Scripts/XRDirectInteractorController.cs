using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public enum Hand
{
    Left,
    Right,
    None
}

public class XRDirectInteractorController : MonoBehaviour
{
    public bool shouldTrackHandHeight;
    public SimulatorEvent ballGrabbedEvent;
    public SimulatorEvent ballGrabbedCorrectlyEvent;
    public SimulatorEvent ballGrabbedWithWrongHandEvent;
    public SimulatorEvent ballReleasedEvent;
    public SimulatorEvent wrongBallThrownEvent;
    [SerializeField] private Hand thisHand;
    [SerializeField] private SimulationManager simulationManager;
    [SerializeField] private Transform headset;
    private float heightThreshold = 0.55f;
    private float aboveThresholdTime = 0f;
    private float warningDelay = 3f;

    private void OnEnable()
    {
        if (TryGetComponent<XRDirectInteractor>(out var interactor))
        {
            interactor.selectEntered.AddListener(OnGrab);
            interactor.selectExited.AddListener(OnRelease);
        }
    }

    private void OnDisable()
    {
        if (TryGetComponent<XRDirectInteractor>(out var interactor))
        {
            interactor.selectEntered.RemoveListener(OnGrab);
            interactor.selectExited.RemoveListener(OnRelease);
        }
    }

    private void Update()
    {
        if (shouldTrackHandHeight)
        {
            CheckHandPosition();
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (args.interactableObject.transform.CompareTag("Ball"))
        {
            BallController ballController = args.interactableObject.transform.GetComponent<BallController>();
            ballController.OnBallGrabbed();
            if (thisHand == Hand.Left)
            {
                InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).SendHapticImpulse(0, .75f, 0.2f);
            }
            else
            {
                InputDevices.GetDeviceAtXRNode(XRNode.RightHand).SendHapticImpulse(0, .75f, 0.2f);
            }
            if (ballController.GetPreviousHand() == Hand.None)
            {
                ballGrabbedEvent.Raise();
            }
            else if (ballController.GetPreviousHand() == thisHand)
            {
                ballGrabbedWithWrongHandEvent.Raise();
            }
            else
            {
                ballGrabbedCorrectlyEvent.Raise();
            }
            ballController.SetPreviousHand(thisHand);
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (args.interactableObject.transform.CompareTag("Ball"))
        {
            ballReleasedEvent.Raise();
            BallController ballController = args.interactableObject.transform.GetComponent<BallController>();
            ballController.PlayThrowSound();
            ballController.AdjustVelocityOnRelease(thisHand);
            string currentBallId = args.interactableObject.transform.GetInstanceID().ToString();
            if (currentBallId == simulationManager.GetPreviouslyThrownBallId() && simulationManager.GetCurrentNumberOfBalls() > 1)
            {
                wrongBallThrownEvent.Raise();
            }
            simulationManager.RegisterReleasedBall(currentBallId);
        }
    }


    private void CheckHandPosition()
    {
        float handHeight = transform.position.y;
        float headsetHeight = headset.position.y;

        if (headsetHeight - handHeight < heightThreshold)
        {
            aboveThresholdTime += Time.deltaTime;

            if (aboveThresholdTime >= warningDelay)
            {
                simulationManager.PlayAudioTutorialHandsLower();
                aboveThresholdTime = 0.0f;
            }
        }
        else
        {
            aboveThresholdTime = 0.0f;
        }
    }
}