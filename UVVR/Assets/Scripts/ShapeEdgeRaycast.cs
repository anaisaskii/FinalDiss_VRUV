using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ShapeEdgeRaycast : MonoBehaviour
{
    [Header("Shapes")]
    public GameObject[] easyShapes;
    public GameObject[] difficultShapes;
    public GameObject[] cupPiecesInGame;
    public GameObject[] snowmanPiecesInGame;
    public GameObject[] easyChallengeParts;
    public GameObject[] difficultChallengeParts;
    public GameObject buttonPodium;

    private GameObject[] shapes;
    private GameObject[] challengeParts;
    private GameObject currentShape;
    private GameObject[] challengePiecesInGame;
    private MeshRenderer shapeMeshRenderer;
    private BoxCollider[] boxColliders;

    private Material[] shapematerials;

    private int currentChallengePart = 0;
    private int currentShapeIndex = 0;
    private int currentBoxColliderIndex = 0;
    private int currentEdgeVideo = 0;
    private int completedShapes;

    [Header("Shape Edge Materials")]
    public Material litEdge;
    public Material unlitEdge;
    public Material currentEdge;

    [Header("VR Controls")]
    public Transform controller;
    public InputActionProperty rightTriggerAction;
    public GameObject player;
    public GameObject userGuide;

    [Header("Video Clips")]
    public VideoPlayer videoPlayer;
    public VideoClip[] easyEdgeClips;
    public VideoClip[] difficultEdgeClips;
    public VideoClip[] easyUnwrapClips;
    public VideoClip[] difficultUnwrapClips;
    public VideoClip tutorialVideo;

    private VideoClip[] unwrapClips;
    private VideoClip[] edgeClips;

    [Header("User Guide")]
    public bool hideUserGuide = false;
    public bool tutVidComplete = false;
    public float pulseSpeed = 2.0f; // glowing material adjustments
    public float intensity = 5.0f;

    private Color baseEmissionColor;
    private static readonly string emissionProperty = "_EmissionColor";

    [Header("Timer")]
    public Timer timer;

    [Header("Audio")]
    public AudioSource selectAudioSource;
    public AudioSource errorAudioSource;
    public AudioSource videoAudioSource;

    private bool FinalChallenge = false;
    private bool triggerPreviouslyPressed = false;
    private List<GameObject> completedSnowmanParts = new List<GameObject>();

    // trying hash set as its faster + no dupes - stores challenge edges so that each can only be selected once
    private HashSet<int> completedChallengeEdges = new HashSet<int>();

    private void Start()
    {
        // get difficulty from selected choice in StartGame.cs
        int difficulty = StartGame.difficulty;
        baseEmissionColor = Color.cyan;
        currentEdge.EnableKeyword("_EMISSION");

        if (difficulty == 0) // easy
        {
            shapes = easyShapes;
            challengeParts = easyChallengeParts;
            edgeClips = easyEdgeClips;
            unwrapClips = easyUnwrapClips;
            challengePiecesInGame = cupPiecesInGame;
        }
        else // difficult
        {
            shapes = difficultShapes;
            challengeParts = difficultChallengeParts;
            edgeClips = difficultEdgeClips;
            unwrapClips = difficultUnwrapClips;
            challengePiecesInGame = snowmanPiecesInGame;
        }

        // Setup for video and audio (synced)
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, videoAudioSource);
        videoPlayer.clip = tutorialVideo;
        videoPlayer.loopPointReached += OnVidFinished;

        StartCoroutine(PrepareAndPlayVideo());
    }

    // Loads video before playing so that audio is not out of sync
    IEnumerator PrepareAndPlayVideo()
    {
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoPlayer.Play();
    }

    // called when the tutorial video finishes
    // starts game + timer in other scripts
    void OnVidFinished(VideoPlayer vp)
    {
        tutVidComplete = true;
        SpawnNewShape();
        spawnGuide();

        // stop listening for event (otherwise shapes will keep spawning)
        videoPlayer.loopPointReached -= OnVidFinished;

        timer.StartTimer();
    }

    private void Update()
    {
        //Hide the user guide for the challenges
        //unhide if the player needs it
        if (tutVidComplete)
        {
            if (!FinalChallenge)
            {
                ProcessStandardShapes();
            }
            else
            {
                ProcessChallengeShape();

                if (!hideUserGuide)
                {
                    spawnGuide();
                }
            }


            //if helper enabled, make appropriate edge glow/if not, edge is black
            if (shapeMeshRenderer != null && hideUserGuide && currentBoxColliderIndex >= 0 && currentBoxColliderIndex < shapeMeshRenderer.materials.Length)
            {
                Material[] currentMaterials = shapeMeshRenderer.materials;
                Material currentMat = currentMaterials[currentBoxColliderIndex];
                //Debug.Log(currentBoxColliderIndex);
                currentMat.SetColor(emissionProperty, Color.black);
            }
            else
            {
                if (shapeMeshRenderer != null && currentBoxColliderIndex >= 0 && currentBoxColliderIndex < shapeMeshRenderer.materials.Length)
                {
                    Material[] currentMaterials = shapeMeshRenderer.materials;
                    Material currentMat = currentMaterials[currentBoxColliderIndex];

                    float emission = Mathf.PingPong(Time.time * pulseSpeed, 1.0f); // goes back and forth
                    Color finalEmission = baseEmissionColor * emission * intensity;

                    currentMat.SetColor(emissionProperty, finalEmission);
                }
            }
        }
    }

    /// <summary> Standard game logic </summary>
    void ProcessStandardShapes()
    {
        if (currentBoxColliderIndex >= boxColliders.Length) return;

        if (CheckForEdgeHit())
        {
            HandleEdgeHit();

            if (currentBoxColliderIndex >= boxColliders.Length)
            {
                // All edges completed for current shape
                Destroy(currentShape);

                //play the unwrapping video for the shape!!
                StartCoroutine(PlayVideoThenSpawnNextShape());

                currentEdgeVideo += 1;
            }
            else
            {
                // Move guide for next edge
                spawnGuide();
            }
        }
    }

    IEnumerator PlayVideoThenSpawnNextShape()
    {
        if (completedShapes < unwrapClips.Length)
        {
            // stop looping so that the unwrap video can play once
            videoPlayer.isLooping = false;
            videoPlayer.clip = unwrapClips[completedShapes];
            videoPlayer.Play();

            //while video player loads up 
            while (!videoPlayer.isPlaying)
            {
                yield return null;  // Wait for one frame until the video starts
            }

            // Wait for video to finish
            while (videoPlayer.isPlaying)
            {
                yield return null;
            }

            completedShapes++;  // Only increment after the video is done
        }
        else
        {
            Debug.LogWarning("No matching video clip found for shape " + completedShapes);
        }

        currentShapeIndex++;

        if (currentShapeIndex < shapes.Length)
        {
            // Spawn the next shape and guide
            SpawnNewShape();
            spawnGuide();
            videoPlayer.isLooping = true; //loop the edge video
        }
        //if all primitive shapes done, do final challenge
        else
        {
            FinalChallenge = true;
            hideUserGuide = true;
            StartSnowmanChallenge();
            Instantiate(buttonPodium);
        }
    }

    //spawn in a new shape and update the video player
    void SpawnNewShape()
    {
        if (currentShapeIndex >= shapes.Length) return;

        currentShape = Instantiate(shapes[currentShapeIndex], new Vector3(0.6f, 1.9f, 2.1f), Quaternion.Euler(-90f, 0f, -180f));
        SetupShape(currentShape);

        videoPlayer.clip = edgeClips[currentEdgeVideo];
    }

    // set up the shape materials and colliders
    void SetupShape(GameObject shape)
    {
        shapeMeshRenderer = shape.GetComponent<MeshRenderer>();
        boxColliders = shape.GetComponentsInChildren<BoxCollider>();
        shapematerials = shapeMeshRenderer.materials;

        completedChallengeEdges.Clear();

        ResetEdges();
        currentBoxColliderIndex = 0;
    }

    // reset all the materials on the shape edges
    void ResetEdges()
    {
        shapeMeshRenderer.materials = shapematerials;
    }

    /// <summary>
    /// Cast ray from right controller.
    /// If it hits the currently active box collider, return true
    /// </summary>
    bool CheckForEdgeHit()
    {
        float triggerValue = rightTriggerAction.action.ReadValue<float>();
        bool triggerPressed = triggerValue > 0.1f;

        if (!triggerPressed)
        {
            triggerPreviouslyPressed = false;
            return false;
        }

        Ray ray = new Ray(controller.position, controller.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10f))
        {
            if (!FinalChallenge)
            {
                BoxCollider currentCollider = boxColliders[currentBoxColliderIndex];
                if (currentCollider.bounds.Contains(hit.point))
                {
                    triggerPreviouslyPressed = true;
                    return true;
                }
                else if (!triggerPreviouslyPressed)
                {
                    errorAudioSource.Play(0);
                }
            }
            else
            {
                for (int i = 0; i < boxColliders.Length; i++)
                {
                    if (boxColliders[i].enabled && boxColliders[i].bounds.Contains(hit.point))
                    {
                        currentBoxColliderIndex = i;
                        triggerPreviouslyPressed = true;
                        return true;
                    }
                }

                // play error audio if nothing is hit
                if (!triggerPreviouslyPressed)
                {
                    errorAudioSource.Play(0);
                }
            }
        }

        // Update press state only if raycast didn't result in success
        // Stops error sound from repeating when trigger held
        triggerPreviouslyPressed = true;
        return false;
    }

    // Logic for when player successfully hits an edge
    // update edge materials, play sound, and change active box collider
    void HandleEdgeHit()
    {
        Material[] updatedMaterials = shapeMeshRenderer.materials;

        selectAudioSource.Play(0);

        if (FinalChallenge)
        {
            completedChallengeEdges.Add(currentBoxColliderIndex);

            updatedMaterials[currentBoxColliderIndex] = litEdge;
            shapeMeshRenderer.materials = updatedMaterials;

            boxColliders[currentBoxColliderIndex].enabled = false;
        }
        else
        {
            updatedMaterials[currentBoxColliderIndex] = litEdge;
            shapeMeshRenderer.materials = updatedMaterials;

            boxColliders[currentBoxColliderIndex].enabled = false;
            currentBoxColliderIndex++;

            if (currentBoxColliderIndex < boxColliders.Length)
            {
                currentEdgeVideo = Mathf.Min(currentEdgeVideo + 1, edgeClips.Length - 1);
                videoPlayer.clip = edgeClips[currentEdgeVideo];
            }
        }

        spawnGuide();
    }

    //enable the assistive guide
    void spawnGuide()
    {
        if (shapeMeshRenderer == null) return;

        Material[] updatedMaterials = shapeMeshRenderer.materials;

        if (FinalChallenge)
        {
            if (!hideUserGuide)
            {
                int safeLength = Mathf.Min(updatedMaterials.Length, boxColliders.Length);

                // Only highlight the first uncompleted edge
                for (int i = 0; i < safeLength; i++)
                {
                    if (boxColliders[i] != null && boxColliders[i].enabled)
                    {
                        updatedMaterials[i] = currentEdge;
                        currentBoxColliderIndex = i; // So pulsing works
                        break;
                    }
                }

                shapeMeshRenderer.materials = updatedMaterials;
            }
            else
            {
                currentBoxColliderIndex = -1; // No pulsing
            }
        }
        else
        {
            // Standard shape - highlight current edge only
            if (currentBoxColliderIndex >= 0 && currentBoxColliderIndex < updatedMaterials.Length)
            {
                updatedMaterials[currentBoxColliderIndex] = currentEdge;
                shapeMeshRenderer.materials = updatedMaterials;
            }
        }
    }

    // --- Snowman Challenge Logic ---

    void StartSnowmanChallenge()
    {
        currentChallengePart = 0;
        SpawnNextSnowmanPart();
    }

    // Logic for the final challenge
    void ProcessChallengeShape()
    {
        if (completedChallengeEdges.Count >= boxColliders.Length) return;

        if (CheckForEdgeHit())
        {
            HandleEdgeHit();

            if (completedChallengeEdges.Count >= boxColliders.Length)
            {
                if (currentChallengePart < challengeParts.Length - 1)
                {
                    MoveCompletedPartToDesk(currentShape);
                }
                else
                {
                    MoveCompletedPartToDesk(currentShape);
                    StartCoroutine(PlayFinalVideo());
                }
            }
            else
            {
                spawnGuide();
            }
        }
    }

    void SpawnNextSnowmanPart()
    {
        // Loop videos again as they display which part to choose
        videoPlayer.isLooping = true;
        videoPlayer.clip = edgeClips[currentEdgeVideo];

        currentShape = Instantiate(challengeParts[currentChallengePart], new Vector3(0.6f, 1.9f, 2.1f), Quaternion.Euler(-90f, 0f, -180f));
        SetupShape(currentShape);
        spawnGuide();
    }

    // Unhide mesh once shape has been unwrapped
    // Progress to next part
    void BuildSnowman()
    {
        challengePiecesInGame[currentChallengePart].GetComponent<MeshRenderer>().enabled = true;

        completedSnowmanParts.Add(currentShape);

        currentChallengePart++;
        if (currentChallengePart < challengeParts.Length)
        {
            SpawnNextSnowmanPart();
        }
    }

    void MoveCompletedPartToDesk(GameObject part)
    {
        StartCoroutine(LerpToDeskPosition(part, new Vector3(1.4f, 0.8f, 2.27f)));
    }

    /// <summary>
    /// Move the completed challenge object to its assigned position on the table
    /// </summary>
    IEnumerator LerpToDeskPosition(GameObject part, Vector3 targetPosition)
    {
        Vector3 startPos = part.transform.position;
        float duration = 2.0f;
        float elapsed = 0;

        if (currentChallengePart < 2)
        {
            currentEdgeVideo += 1;
            Debug.Log(currentEdgeVideo);
            videoPlayer.clip = edgeClips[currentEdgeVideo];
        }

        while (elapsed < duration)
        {
            part.transform.position = Vector3.Lerp(startPos, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        part.transform.position = targetPosition;

        Destroy(part);
        //unhide relevant snowman part
        BuildSnowman();

        if (currentChallengePart == challengeParts.Length)
        {
            StartCoroutine(PlayFinalVideo());
        }

        hideUserGuide = true;

    }

    // Return shape to original position if it goes out of bounds
    public void TeleportShapeBack()
    {
        currentShape.transform.position = new Vector3(0.6f, 1.9f, 2.1f);
    }

    // maybe incorporate this into main logic..?
    IEnumerator PlayFinalVideo()
    {
        if (completedShapes < unwrapClips.Length)
        {
            //stop looping to unwrap video plays once
            videoPlayer.isLooping = false;
            videoPlayer.clip = unwrapClips[completedShapes]; //unwrap video
            videoPlayer.Play();

            //while video player loads up 
            while (!videoPlayer.isPlaying)
            {
                yield return null;  // Wait for one frame until the video starts
            }

            // Wait for video to finish
            while (videoPlayer.isPlaying)
            {
                yield return null;
            }

            SceneManager.LoadScene("MRTScene");
        }
        else
        {
            Debug.LogWarning("No matching video clip found for shape " + completedShapes);
        }
    }
}