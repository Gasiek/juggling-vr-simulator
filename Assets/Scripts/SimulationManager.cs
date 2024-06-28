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
    public int currentNumberOfBalls = 1;
    public Transform[] ballsOriginsOnTable;
    private List<GameObject> balls = new();
    [SerializeField] private TextMeshProUGUI numberOfBallsText;


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
        if (currentNumberOfBalls > ballsOriginsOnTable.Length)
        {
            currentNumberOfBalls = ballsOriginsOnTable.Length;
        }
        StartCoroutine(DestroyAndSpawnBalls());
    }

    private IEnumerator DestroyAndSpawnBalls()
    {
        DestroyAllBalls();
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < currentNumberOfBalls; i++)
        {
            var ball = Instantiate(ballPrefab, ballsOriginsOnTable[i].position, Quaternion.identity);
            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            balls.Add(ball);
        }
        numberOfBallsText.text = currentNumberOfBalls.ToString();
    }

    private void DestroyAllBalls()
    {
        foreach (var ball in balls)
        {
            Destroy(ball);
        }
        balls.Clear();
    }

    public void IncreaseNumberOfBalls()
    {
        currentNumberOfBalls++;
        SpawnBallsOnTable();
    }
}
