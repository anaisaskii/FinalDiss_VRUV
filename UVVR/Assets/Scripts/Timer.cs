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

    void Start()
    {
        shapeStartTime = Time.time;
    }

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

    public void LogShapeTime(string shapeName, string chosenShape, bool isCorrect)
    {
        float shapeCompletionTime = Time.time - shapeStartTime;
        shapeTimes.Add(shapeCompletionTime);

        string correctness = isCorrect ? "Correct" : "Incorrect";
        shapeDetails.Add($"{shapeName},{shapeCompletionTime:F2},{correctness},{chosenShape}");

        shapeStartTime = Time.time;
    }

    public void SaveData(int CorrectAnswers)
    {
        Debug.Log("Writing data as CSV!");

        string filePath = Path.Combine(Application.persistentDataPath, "TestTimes.csv");
        bool append = false;

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

        append = (RoundsCompleted == 1);

        using (StreamWriter writer = new StreamWriter(filePath, append: append))
        {
            if (!append)
            {
                writer.WriteLine("Shape Name,Time (seconds),Correct,Chosen Shape");
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
        }

        Debug.Log($"CSV file saved at: {filePath}");

        if (RoundsCompleted >= 2)
        {
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string newFilePath = Path.Combine(Application.persistentDataPath, $"TestTimes_{timestamp}.csv");
            File.Move(filePath, newFilePath);
            Debug.Log($"Test completed twice. Saved as new file: {newFilePath}");
            SceneManager.LoadScene("EndScene");
        }
        else
        {
            SceneManager.LoadScene("UnwrapScene");
        }
    }

    public int GetSetCompleted()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "TestTimes.csv");

        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(',');

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

        return 3; // Default value if no valid set is found
    }
}
