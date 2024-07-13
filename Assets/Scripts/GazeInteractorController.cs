using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeInteractorController : MonoBehaviour
{
    public Transform correctGazeTransform;
    public Transform headset;
    public Material gazeMaterial;
    public Material correctGazeMaterial;
    private Color originalGazeColor;
    private Color originalCorrectGazeColor;

    private void Awake()
    {
        originalGazeColor = gazeMaterial.color;
        originalCorrectGazeColor = correctGazeMaterial.color;
    }

    private void Update()
    {
        float angleDifference = Mathf.Abs(correctGazeTransform.rotation.eulerAngles.x - headset.rotation.eulerAngles.x);
        float normalizedDifference = Mathf.InverseLerp(0, 10, angleDifference);
        float alpha = Mathf.Lerp(0, 1, normalizedDifference);

        gazeMaterial.color = new Color(originalGazeColor.r, originalGazeColor.g, originalGazeColor.b, alpha);
        correctGazeMaterial.color = new Color(originalCorrectGazeColor.r, originalCorrectGazeColor.g, originalCorrectGazeColor.b, alpha);
    }
}
