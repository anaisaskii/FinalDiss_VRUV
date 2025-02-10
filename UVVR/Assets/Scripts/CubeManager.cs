using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CubeManager : MonoBehaviour
{
    string CurrentCube;

    public Renderer PlaneRenderer;
    public Material cubeDisplayMat;
    public Texture2D[] cubeTextures;

    private Dictionary<string, Texture2D> textureDict = new Dictionary<string, Texture2D>();

    //make this into a queue so that cubes can be in random orders
    string[] cubeNames = { "Cube_1", "Cube_2", "Cube_3" };

    public void LogObjectPickup(SelectEnterEventArgs args)
    {
        Debug.Log("Picked up: " + args.interactableObject.transform.name);
        CheckCube(args);
    }

    //get reference to cubepickupcheck
    void Start()
    {
        //create an array of cube images
        //match up images to cubes
        
        foreach(Texture2D tex in cubeTextures)
        {
            textureDict[tex.name] = tex;
        }

        ChooseCube();
    }

    void ChooseCube()
    {
        //Chooses a random cube from the array 
        CurrentCube = cubeNames[Random.Range(0, cubeNames.Length)];

        Debug.Log("Pickup: " + CurrentCube);
        //make sure it isn't the same as previous cube
        //have random order in queue
        //(pop then push to queue???)

        if (textureDict.TryGetValue(CurrentCube, out Texture2D selectedTexture))
        {
            PlaneRenderer.material.mainTexture = selectedTexture;
        }
        else
        {
            Debug.LogError("Texture not found for " + CurrentCube);
        }
    }

    void Update()
    {
        //choose a cube image
        //display 4 random cubes ENSURING one of them is correct
        //randomise order

        //check if a cube has been picked up
        //check if cube was right or wrong
        //progress
    }

    void CheckCube(SelectEnterEventArgs cube)
    {
        string pickedCubeName = cube.interactableObject.transform.name;

        if (pickedCubeName == CurrentCube)
        {
            Debug.Log("Correct!");
        }
        else
        {
            Debug.Log("Incorrect!");
        }

        ChooseCube();
    }
}
