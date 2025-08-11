using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    public float timePassed = 0;
    public TextMeshProUGUI TimerTextDisplay;
    public float timeRemaining = 180f;

    public ShapeEdgeRaycast shapeEdgeRaycast;

    private bool startTimer = false;

    private float shapeStartTime;
    public SaveDataToCSV dataManager; // Reference to DataManager
    public CubeManager cubeManager; // Needed to pass cubesChosenSet info

    // wait until tutorial video is finished to start if in uv unwrapping scene
    // start automatically for MRT
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "MRTScene")
        {
            startTimer = true;
        }
    }

    public void StartTimer()
    {
        shapeStartTime = Time.time;
        startTimer = true;
    }

    // had bug where timer wouldn't reset, this forces it to
    public void ResetTimer()
    {
        shapeStartTime = Time.time;
    }

    void Update()
    {
        if(startTimer == true) //tutorial finished
        {
            // Count down timer each second and display as text
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                // If in MRT save data to CSV
                // If not, progress to the mental rotations test
                if (SceneManager.GetActiveScene().name == "MRTScene")
                {
                    cubeManager.SaveDataToCSV();
                }
                else
                {
                    SceneManager.LoadScene("MRTScene");
                }

            }

            // Seperate time into seconds/minutes for readability
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            TimerTextDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    /// <summary> Log the time it took to complete a MRT question </summary>
    public void LogShapeTime(string shapeName, string chosenShape, bool isCorrect)
    {
        float shapeCompletionTime = Time.time - shapeStartTime;
        dataManager.AddShapeData(shapeName, shapeCompletionTime, isCorrect, chosenShape);
        shapeStartTime = Time.time; // Reset timer
    }
}