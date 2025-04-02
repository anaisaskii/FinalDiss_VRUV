using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class tenminutetimer : MonoBehaviour
{
    //make proper timer script for final project
    //if you see this script no you didn't :)
    // Start is called before the first frame update

    private float timeRemaining = 600f;

    public TextMeshProUGUI TimerTextDisplay;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            SceneManager.LoadScene("BasicScene");
        }

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);

        TimerTextDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
