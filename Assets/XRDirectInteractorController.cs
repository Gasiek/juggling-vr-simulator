using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRDirectInteractorController : MonoBehaviour
{
    public SimulatorEvent ballGrabbedEvent;

    private void OnEnable()
    {
        if (TryGetComponent<XRDirectInteractor>(out var interactor))
        {
            interactor.selectEntered.AddListener(OnGrab);
        }
    }

    private void OnDisable()
    {
        if (TryGetComponent<XRDirectInteractor>(out var interactor))
        {
            interactor.selectEntered.RemoveListener(OnGrab);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (args.interactableObject.transform.CompareTag("Ball"))
        {
            ballGrabbedEvent.Raise();
        }
    }
}
