using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class SimulationManager : MonoBehaviour
{
    public GameObject ballPrefab;
    private float speed;
    public Slider slider;
    public float originalVelocityThrow;
    public TextMeshProUGUI speedText;
    public int currentNumberOfBallsInGame = 1;
    public SimulatorEvent levelPassed;
    public Transform[] ballsOriginsOnTable;
    private List<GameObject> balls = new();
    [SerializeField] private TextMeshProUGUI numberOfBallsText;
    private int ballsToProgress = 10;
    private int currentlyHeldBalls = 0;
    private CatchCounter catchCounter;

    private void Awake()
    {
        catchCounter = GetComponent<CatchCounter>();
    }

    void Start()
    {
        SpawnBallsOnTable();
        UpdateSpeed();
    }

    public void UpdateSpeed()
    {
        speed = slider.value / 10f;
        speedText.text = "Speed: " + speed.ToString();
        Time.timeScale = speed;
    }

    public void SpawnBallsOnTable()
    {
        if (currentNumberOfBallsInGame > ballsOriginsOnTable.Length)
        {
            currentNumberOfBallsInGame = ballsOriginsOnTable.Length;
        }
        StartCoroutine(DestroyAndSpawnBalls());
    }

    private IEnumerator DestroyAndSpawnBalls()
    {
        DestroyAllBalls();
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < currentNumberOfBallsInGame; i++)
        {
            var ball = Instantiate(ballPrefab, ballsOriginsOnTable[i].position, Quaternion.identity);
            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            balls.Add(ball);
        }
        numberOfBallsText.text = currentNumberOfBallsInGame.ToString();
    }

    private void DestroyAllBalls()
    {
        foreach (var ball in balls)
        {
            Destroy(ball);
        }
        balls.Clear();
    }

    public void StartNextLevel()
    {
        StartCoroutine(IncreaseNumberOfBalls());
    }

    private IEnumerator IncreaseNumberOfBalls()
    {
        yield return new WaitForSeconds(1f); // here should be some info that a person finished the level
        currentNumberOfBallsInGame++;
        SpawnBallsOnTable();
    }

    private void CheckProgress()
    {
        if (currentlyHeldBalls == currentNumberOfBallsInGame)
        {
            if (catchCounter.GetCurrentScore() == ballsToProgress)
            {
                levelPassed.Raise();
            }
        }
    }

    public void OnBallGrabbed()
    {
        currentlyHeldBalls++;
        CheckProgress();
    }

    public void OnBallReleased()
    {
        currentlyHeldBalls--;
    }
}
