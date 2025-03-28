using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    public float timePassed = 0;
    public int setCompleted;

    public TextMeshProUGUI TimerTextDisplay;

    private float timeRemaining = 180f;

    public CubeManager cubeManager;

    private float shapeStartTime;

    private List<float> shapeTimes = new List<float>();

    private List<string> shapeDetails = new List<string>();


    int RoundsCompleted = 0;

    // Start is called before the first frame update
    void Start()
    {
        shapeStartTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            Debug.Log("Time is up!");
            cubeManager.SaveDataToCSV();
        }

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        TimerTextDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    //adds the completed shapes time to the array
    //call this when a shape is completed
    public void LogShapeTime(string shapeName, bool isCorrect)
    {
        float shapeCompletionTime = Time.time - shapeStartTime;
        shapeTimes.Add(shapeCompletionTime);

        string correctness = isCorrect ? "Correct" : "Incorrect";
        shapeDetails.Add($"{shapeName},{shapeCompletionTime:F2},{correctness}");

        shapeStartTime = Time.time;
    }

    //write time taken to complete each shape to a text file
    public void SaveData(int CorrectAnswers)
    {
        Debug.Log("Writing data as CSV!");

        string filePath = Path.Combine(Application.dataPath, "../TestTimes.csv");
        bool append = false;

        // Check if the file exists before reading
        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 2 && parts[0].Trim() == "Rounds Completed")
                    {
                        if (int.TryParse(parts[1].Trim(), out int rounds))
                        {
                            RoundsCompleted = rounds;
                        }
                    }
                }
            }
        }


        //if the test has only been completed once, append to the existing file
        //if the test has been completed twice (new player) overwrite existing file to store new scores
        append = (RoundsCompleted == 1) ? true : false;

        Debug.Log("Appending Mode: " + append);

        //only append if second round has been completed
        using (StreamWriter writer = new StreamWriter(filePath, append: append))
        {
            if (!append)
            {
                writer.WriteLine("Shape Name,Time (seconds),Correct");
                RoundsCompleted = 0;
            }

            foreach (string shapeDetail in shapeDetails)
            {
                writer.WriteLine(shapeDetail);
            }

            writer.WriteLine($"\nCorrect Answers,{CorrectAnswers}");
            RoundsCompleted++;
            writer.WriteLine($"Rounds Completed, {RoundsCompleted}");
            writer.WriteLine($"Set Completed, {cubeManager.cubesChosenSet}");
            writer.WriteLine();

            if (!append)
            {
                SceneManager.LoadScene("UnwrapScene");
            }
            else
            {
                SceneManager.LoadScene("EndScene");
            }
        }


        Debug.Log($"CSV file saved at: {filePath}");

        
    }

    public int GetSetCompleted()
    {
        string filePath = Path.Combine(Application.dataPath, "../TestTimes.csv");

        // Check if the file exists before reading
        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(',');

                    //get which set was completed and pass that back
                    if (parts.Length == 2 && parts[0].Trim() == "Set Completed")
                    {
                        if (int.TryParse(parts[1].Trim(), out int set))
                        {
                            setCompleted = set;
                        }
                    }
                }
            }

            return setCompleted;
        }
        //the set can only be 0 or 1 so if it returns 3 then it has to be null
        //(this is a terrible way of doing things sorry)
        return 3;
    }

}
