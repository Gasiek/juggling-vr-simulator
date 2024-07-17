using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public enum Hand
{
    Left,
    Right,
    None
}

public class XRDirectInteractorController : MonoBehaviour
{
    public SimulatorEvent ballGrabbedEvent;
    public SimulatorEvent ballGrabbedCorrectlyEvent;
    public SimulatorEvent ballGrabbedWithWrongHandEvent;
    public SimulatorEvent ballReleasedEvent;
    public SimulatorEvent wrongBallThrownEvent;
    [SerializeField] private Hand thisHand;
    [SerializeField] private SimulationManager simulationManager;

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

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (args.interactableObject.transform.CompareTag("Ball"))
        {
            if (args.interactorObject.transform.TryGetComponent<XRBaseController>(out var controller))
            {
                controller.SendHapticImpulse(0.8f, 0.2f);
            }
            BallController ballController = args.interactableObject.transform.GetComponent<BallController>();
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
            ballController.AdjustVelocityOnRelease();
            string currentBallId = args.interactableObject.transform.GetInstanceID().ToString();
            if (currentBallId == simulationManager.GetPreviouslyThrownBallId() && simulationManager.GetCurrentNumberOfBalls() > 1)
            {
                wrongBallThrownEvent.Raise();
            }
            simulationManager.SetPreviouslyThrownBallId(currentBallId);
        }
    }
}