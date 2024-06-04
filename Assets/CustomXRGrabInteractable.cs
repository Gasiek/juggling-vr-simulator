using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CustomXRGrabInteractable : XRGrabInteractable
{
    public override bool IsSelectableBy(IXRSelectInteractor interactor)
    {
        bool isGrabbed = false;

        if (isSelected && !interactor.Equals(firstInteractorSelecting))
        {
            var grabbingInteractor = firstInteractorSelecting as XRDirectInteractor;
            if (grabbingInteractor != null)
            {
                isGrabbed = true;
            }
        }

        return base.IsSelectableBy(interactor) && !isGrabbed;
    }
}
