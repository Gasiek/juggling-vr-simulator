using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tutorial Step", menuName = "Tutorial Step")]
public class TutorialStep : ScriptableObject
{
    public int ballsToProgress;
    public int moveToNextStepAfterSeconds;
    public AudioClip audioGuideTrack;
    public bool trackGaze;
    public int numberOfBalls;
    public float speedMultiplier;
    public bool showTutorial;
    public bool isTutorialPOV;
    public bool shouldBallStopAtThePeak;
    public bool showGrabBallsTutorial;
    public bool showResetBallsTutorial;
    [TextArea(2, 5)]
    [SerializeField] private string notes;
}
