using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveDataToCSV : MonoBehaviour
{
    private List<string> shapeDetails = new List<string>();
    private int roundsCompleted = 0;
    private int setCompleted;

    public CubeManager cubeManager; // needed for saving setCompleted info

    public void AddShapeData(string shapeName, float completionTime, bool isCorrect, string chosenShape)
    {
        string correctness = isCorrect ? "Correct" : "Incorrect";
        shapeDetails.Add($"{shapeName},{completionTime:F2},{correctness},{chosenShape}");
    }

    public void SaveData(int correctAnswers)
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
                            roundsCompleted = rounds;
                        }
                    }
                }
            }
        }

        append = (roundsCompleted == 1);

        using (StreamWriter writer = new StreamWriter(filePath, append: append))
        {
            if (!append)
            {
                writer.WriteLine("Shape Name,Time (seconds),Correct,Chosen Shape");
                roundsCompleted = 0;
            }

            foreach (string shapeDetail in shapeDetails)
            {
                writer.WriteLine(shapeDetail);
            }

            writer.WriteLine($"\nCorrect Answers,{correctAnswers}");
            roundsCompleted++;
            writer.WriteLine($"Rounds Completed, {roundsCompleted}");
            writer.WriteLine($"Set Completed, {cubeManager.cubesChosenSet}");
            writer.WriteLine();
        }

        Debug.Log($"CSV file saved at: {filePath}");

        if (roundsCompleted >= 2)
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

        return 3; // Default value
    }
}
