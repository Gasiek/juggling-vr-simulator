using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRDirectInteractorController : MonoBehaviour
{
    public SimulatorEvent ballGrabbedEvent;
    public SimulatorEvent ballReleasedEvent;

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
            ballGrabbedEvent.Raise();
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (args.interactableObject.transform.CompareTag("Ball"))
        {
            ballReleasedEvent.Raise();
        }
    }
}