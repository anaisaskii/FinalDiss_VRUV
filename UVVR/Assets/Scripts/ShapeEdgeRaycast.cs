using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShapeEdgeRaycast : MonoBehaviour
{
    public Material cubeEdgeMaterial;
    //use for loop to get each box collider in order
    //then can check if hit
    public BoxCollider collider1;
    public Transform controller;
    private float offsetY = 0.1f;
    public GameObject[] shapes;

    private GameObject currentShape;
    private int currentShapeIndex = 0;
    private int currentboxcollider = 0;

    private void Start()
    {
        cubeEdgeMaterial.SetTextureOffset("_MainTex", new Vector2(0, 0));
    }

    private void Update()
    {
        //only display current shape
        currentShape = shapes[currentShapeIndex]; //increase by one once shape has been unwrapped
        currentShape.GetComponent<MeshRenderer>().enabled = true; //hide at end

        //get bounding boxes
        BoxCollider[] boxcolliders = currentShape.GetComponentsInChildren<BoxCollider>();

        Ray ray = new Ray(controller.position, controller.forward);
        Debug.DrawRay(controller.position, controller.forward);
        RaycastHit hit;

        //get shape bounds and iterate through each one

        if (Physics.Raycast(ray, out hit, 10f)) 
        {
            if (boxcolliders[currentboxcollider].bounds.Contains(hit.point)) // checks if inside bounding box
            {
                cubeEdgeMaterial.SetTextureOffset("_MainTex", new Vector2(0, offsetY));

                currentboxcollider += 1;
                if (currentboxcollider >= boxcolliders.Length)
                {
                    // Hide the shape and move to next
                    currentShape.GetComponent<MeshRenderer>().enabled = false;
                    currentShapeIndex += 1;
                    currentboxcollider = 0; // Reset for next shape
                }
            }
        }

    }
}