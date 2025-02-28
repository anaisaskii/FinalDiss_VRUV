using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.InputSystem;

public class ShapeEdgeRaycast : MonoBehaviour
{
    public Material litEdge;    // Material when edge is hit
    public Material unlitEdge; // Default material
    public Transform controller;
    public GameObject[] shapes;

    private GameObject currentShape;
    private int currentShapeIndex = 0;
    private int currentBoxColliderIndex = 0;

    private MeshRenderer shapeMeshRenderer;
    private BoxCollider[] boxColliders;
    private Material[] shapematerials;

    public VideoPlayer VideoPlayer;
    public VideoClip[] cubeVideoClips;

    public InputActionProperty rightTriggerAction;

    private void Start()
    {
        SpawnNewShape();
    }

    void SpawnNewShape()
    {
        if (currentShapeIndex >= shapes.Length)
        {
            Debug.Log("All shapes completed!");
            return; // Prevent out-of-bounds error
        }

        // Instantiate and set up the new shape
        currentShape = Instantiate(shapes[currentShapeIndex], new Vector3(0.6f, 1.9f, 2.1f), Quaternion.Euler(-90f, 0f, -180f));
        shapeMeshRenderer = currentShape.GetComponent<MeshRenderer>();
        boxColliders = currentShape.GetComponentsInChildren<BoxCollider>();
        shapematerials = shapeMeshRenderer.materials;

        // Reset materials & collider index
        ResetEdges();
        currentBoxColliderIndex = 0;
    }

    void ResetEdges()
    {
        for (int i = 0; i < shapematerials.Length; i++)
        {
            //shapematerials[i] = unlitEdge; // Reset all materials to default
            Debug.Log($"Material Index {i}: {shapematerials[i].name.Replace("(Instance)", "").Trim()}");
        }
        shapeMeshRenderer.materials = shapematerials; //Update renderer
    }

    private void Update()
    {
        if (currentBoxColliderIndex >= boxColliders.Length)
            return; // Stop processing if all colliders are done

        BoxCollider currentCollider = boxColliders[currentBoxColliderIndex]; // Get the current collider in order

        float triggerValue = rightTriggerAction.action.ReadValue<float>();

        if (triggerValue > 0.1f) // Small threshold to detect a press
        {
            Ray ray = new Ray(controller.position, controller.forward);
            Debug.DrawRay(controller.position, controller.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 10f))
            {
                if (currentCollider.bounds.Contains(hit.point)) // Check only the current collider
                {
                    Debug.Log("Hit on collider: " + currentBoxColliderIndex);

                    shapematerials[currentBoxColliderIndex + 1] = litEdge; // Ensure correct index
                    
                    shapeMeshRenderer.materials = shapematerials; // Apply changes

                    currentCollider.enabled = false; // Disable this collider
                    currentBoxColliderIndex++; // Move to the next collider in order

                    if (currentBoxColliderIndex < boxColliders.Length)
                    {
                        VideoPlayer.clip = cubeVideoClips[currentBoxColliderIndex]; //change video
                    }

                    // If all colliders are processed, move to the next shape
                    if (currentBoxColliderIndex >= boxColliders.Length)
                    {
                        Destroy(currentShape);
                        currentShapeIndex++;
                        SpawnNewShape();
                    }
                }
            }
        }
    }
}