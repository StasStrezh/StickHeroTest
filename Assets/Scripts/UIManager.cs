using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    #region SerializeField
    [SerializeField]
    private Text soundText;
    [SerializeField]
    private GameObject points;
    [SerializeField]
    private GameObject restartDialoge;
    [SerializeField]
    private GameObject startDialoge;
    [SerializeField]
    private GameController gameController;
    [SerializeField]
    private AudioManager audioManager;
    #endregion


    #region methods
    void Start ()
    {
        gameController = Transform.FindObjectOfType<GameController>();
        soundText.text = PlayerPrefs.GetString("Music") != "no" ? "Sound: ON" : "Sound: OFF";
	}


    /// <summary>
    /// Making game start
    /// </summary>
    public void StartGame()
    {
        audioManager.Play(AudioManager.AudioState.BtnClick);
        gameController.isApply = true;
        gameController.isComplete = true;
        points.SetActive(true);
        startDialoge.SetActive(false);
    }


    /// <summary>
    /// Making game restart
    /// </summary>
    public void RestartGame()
    {
        audioManager.Play(AudioManager.AudioState.BtnClick);
        gameController.isApply = true;
        gameController.isComplete = true;
        restartDialoge.SetActive(false);
        points.SetActive(true);
    }


    /// <summary>
    /// Go to mane menu
    /// </summary>
    public void OnMenuBtnClick()
    {
        audioManager.Play(AudioManager.AudioState.BtnClick);
        SceneManager.LoadScene(0);
    }


    /// <summary>
    /// Changing sound setting
    /// </summary>
    public void OnSoundBtnClick()
    {
        audioManager.Play(AudioManager.AudioState.BtnClick);
        soundText.text = soundText.text == "Sound: OFF" ? "Sound: ON" : "Sound: OFF";
        if (PlayerPrefs.GetString("Music") != "no")
        {
            PlayerPrefs.SetString("Music", "no");
        }
        else
        {
            PlayerPrefs.SetString("Music", "yes");
        }
    }
    #endregion
}
