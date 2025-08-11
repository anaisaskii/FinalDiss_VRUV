using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    private SphereCollider sphereCollider;

    public static int difficulty;

    public Button startButton;

    public Button easyButton;
    public Button hardButton;
    public Button continueButton;

    public GameObject tutorialPlayer;
    public VideoPlayer videoPlayer;
    public AudioSource videoAudio;
    public VideoClip tutorialVid;

    public GameObject startMenu;
    public Sprite difficultyMenu;

    void Start()
    {
        // Get button collider
        sphereCollider = GetComponent<SphereCollider>();

        startButton.gameObject.SetActive(true);

        easyButton.gameObject.SetActive(false);
        hardButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
        tutorialPlayer.SetActive(false);

    }

    private void Update()
    {
        if (videoPlayer.frame >= (long)(videoPlayer.frameCount - 1)) {
            // game only progresses if player clicks continue
            // player can rewatch video if they want
            continueButton.gameObject.SetActive(true);
        }
    }

    // preload the video so the audio syncs up
    public void LoadTutorialVideo()
    {
        startButton.gameObject.SetActive(false);

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, videoAudio);
        videoPlayer.clip = tutorialVid;

        StartCoroutine(PrepareAndPlayVideo());

        tutorialPlayer.SetActive(true);
    }

    IEnumerator PrepareAndPlayVideo()
    {
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoPlayer.Play();
    }

    public void ShowDifficultyOptions()
    {
        tutorialPlayer.SetActive(false);
        continueButton.gameObject.SetActive(false);

        startMenu.GetComponent<SpriteRenderer>().sprite = difficultyMenu;

        easyButton.gameObject.SetActive(true);
        hardButton.gameObject.SetActive(true);
    }

    public void SetDifficulty(int diff)
    {
        //difficulty accessed again in shapeedgeraycast script
        difficulty = diff;
        LoadGame();
    }

    //If the player hits the button, start the game
    void LoadGame()
    {
       SceneManager.LoadScene("MRTScene");
    }
}
