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

    private float shapeStartTime;
    public SaveDataToCSV dataManager; // reference to DataManager
    public CubeManager cubeManager; // still needed to pass cubesChosenSet info

    void Start()
    {
        shapeStartTime = Time.time;
    }

    void Update()
    {
        //count down timer each second and display as text
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            Debug.Log("Time is up!");
            if(SceneManager.GetActiveScene().name == "BasicScene")
            {
                //when timer hits 0 and in mental rotations test
                //save data
                cubeManager.SaveDataToCSV();
            }
            else
            {
                //if not in basic scene (in sample scene) load up mental rotations test again
                SceneManager.LoadScene("BasicScene");
            }
            
        }

        //seperate time into seconds/minutes for readability
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        TimerTextDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void LogShapeTime(string shapeName, string chosenShape, bool isCorrect)
    {
        float shapeCompletionTime = Time.time - shapeStartTime;
        dataManager.AddShapeData(shapeName, shapeCompletionTime, isCorrect, chosenShape);
        shapeStartTime = Time.time;
    }
}