using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

public class CubeManager : MonoBehaviour
{
    string CurrentShape;

    [Header("Shapes and Materials")]
    public Material cubeDisplayMat; 
    public Texture2D[] cubeTexturesSet1;
    public Texture2D[] cubeTexturesSet2;

    [Header("Renderers")]
    // The cubes that may be the answer
    public Renderer planeRenderer;  // target cube image
    public Renderer[] answerRenderers;

    [Header("Chosen Set")]
    public int cubesChosenSet = 0;

    [Header("External Scripts")]
    public SaveDataToCSV savedatatocsv;
    public Timer timer;

    [Header("Audio")]
    public AudioSource selectSound;

    private Dictionary<string, List<Texture2D>> correctAnswersPerShape = new Dictionary<string, List<Texture2D>>();
    private Dictionary<string, List<Texture2D>> wrongAnswersPerShape = new Dictionary<string, List<Texture2D>>();


    private Queue<string> selectedShapesQueue = new Queue<string>();

    private Texture2D currentCorrectTexture; // Correct shape, correct angle (on target plane)
    private Texture2D currentCorrectDifferentAngle; // Correct shape, different angle (in answers)
    private Texture2D[] currentAnswerOptions = new Texture2D[4]; // answer options

    private int correctAnswers = 0;
    private int roundsCompleted = 0;

    void Start()
    {
        // Choose a random number between 0 and 1 to determine which cube set to use
        // (If none can be read from the CSV file)
        if (savedatatocsv.GetSetCompleted() == 3)
        {
            cubesChosenSet = Random.Range(0, 2);
        }
        else
        {
            // Set the previously completed set to be the one read from the file
            int previousSet = savedatatocsv.GetSetCompleted();
            Debug.Log("The previous set was: " + previousSet);
            //reversed because I messed up the order
            if (previousSet == 1)
            {
                cubesChosenSet = 0;
            }
            else
            {
                cubesChosenSet = 1;
            }
            Debug.Log("The chosen set is: " + cubesChosenSet);
        }

        timer.ResetTimer();
        OrganizeTextures();
        PrepareShapeQueue();
        ChooseNextShape();
    }

    //read from csv first
    void OrganizeTextures()
    {
        Texture2D[] selectedSet = (cubesChosenSet == 1) ? cubeTexturesSet1 : cubeTexturesSet2;

        foreach (Texture2D tex in selectedSet)
        {
            //textures are named so that they can be split at the '_' and organised
            string[] parts = tex.name.Split('_');
            if (parts.Length < 2) continue;

            string shapeName = parts[0];
            string type = parts[1];

            if (type == "Correct")
            {
                if (!correctAnswersPerShape.ContainsKey(shapeName))
                    correctAnswersPerShape[shapeName] = new List<Texture2D>();
                correctAnswersPerShape[shapeName].Add(tex);
            }
            else if (type == "Incorrect")
            {
                if (!wrongAnswersPerShape.ContainsKey(shapeName))
                    wrongAnswersPerShape[shapeName] = new List<Texture2D>();
                wrongAnswersPerShape[shapeName].Add(tex);
            }
        }
    }

    void ChooseNextShape()
    {
        if (selectedShapesQueue.Count == 0)
        {
            Debug.Log("All shapes completed!");
            return;
        }

        CurrentShape = selectedShapesQueue.Dequeue();

        // get correct texture (main image)
        var correctList = correctAnswersPerShape[CurrentShape];
        currentCorrectTexture = correctList[Random.Range(0, correctList.Count)];
        planeRenderer.material.mainTexture = currentCorrectTexture;

        // pick other one from the correct set as the correct answer
        do
        {
            currentCorrectDifferentAngle = correctList[Random.Range(0, correctList.Count)];
        } 
        while (currentCorrectDifferentAngle == currentCorrectTexture);

        // get 3 wrong answers for this specific shape
        List<Texture2D> wrongOptions = new List<Texture2D>();
        var wrongList = wrongAnswersPerShape[CurrentShape];
        ShuffleList(wrongList);
        wrongOptions.Add(wrongList[0]);
        wrongOptions.Add(wrongList[1]);
        wrongOptions.Add(wrongList[2]);

        // combine and assign to renderers
        List<Texture2D> allOptions = new List<Texture2D>
        {
            currentCorrectDifferentAngle,
            wrongOptions[0],
            wrongOptions[1],
            wrongOptions[2]
        };

        ShuffleList(allOptions);

        for (int i = 0; i < answerRenderers.Length; i++)
        {
            answerRenderers[i].material.mainTexture = allOptions[i];
        }

        currentAnswerOptions = allOptions.ToArray();
    }

    void PrepareShapeQueue()
    {
        List<string> allShapes = new List<string>(correctAnswersPerShape.Keys);
        ShuffleList(allShapes);
        foreach (string shape in allShapes)
        {
            selectedShapesQueue.Enqueue(shape);
        }
    }

    // check the chosen shapes material against the target one
    public void CheckAnswer(int selectedIndex)
    {
        Renderer clickedRenderer = answerRenderers[selectedIndex];
        Texture2D selectedTexture = (Texture2D)clickedRenderer.material.mainTexture;

        //play selection sound
        selectSound.Play(0);

        bool isCorrect = selectedTexture.name == currentCorrectDifferentAngle.name;
        string chosenShapeName = selectedTexture.name;

        timer.LogShapeTime(CurrentShape, chosenShapeName, isCorrect);

        roundsCompleted += 1;

        if (isCorrect)
        {
            correctAnswers += 1;
        }

        if (roundsCompleted == 6)
        {
            SaveDataToCSV();
        }

        ChooseNextShape();
    }

    public void SaveDataToCSV()
    {
        savedatatocsv.SaveData(correctAnswers);
    }

    // fisher-yates shuffle
    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
