using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioShot;
    [SerializeField] private AudioClip audioCoin;
    [SerializeField] private AudioClip audioHit;
    [SerializeField] private AudioClip audioGameOver;
    [SerializeField] private AudioClip audioGameWin;
    [SerializeField] private AudioClip audioRestart;
    [SerializeField] private AudioClip audioDash;
    [SerializeField] private AudioClip audioDamage;
    [SerializeField] private AudioClip audioBeep;

    private bool playState = false;
    private bool pauseState = false;
    private bool restartState = false;

    void Start()
    {
        updateUI();
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        healthBar.value = currentHealth / maxHealth;
        audioSource.PlayOneShot(audioHit);
    }

    public void UpdateScore(int currentScore)
    {
        scoreText.text = $"$ {currentScore}";
        audioSource.PlayOneShot(audioCoin);
    }

    public void ActionButtonPlay()
    {
        if (restartState)
        {
             RestartGame();
             audioSource.PlayOneShot(audioRestart);
        }
        else
        {
             PlayGame();
             audioSource.PlayOneShot(audioBeep);
        }
    }

    public void ActionButtonPause()
    {
        audioSource.PlayOneShot(audioBeep);
        pauseState = true;
        playState = false;
        updateUI("Paused");
    }

    public void PlayGame()
    {
        pauseState = false;
        restartState = false;
        playState = true;
        updateUI();
    }

    public void GameOver()
    {
        pauseState = false;
        restartState = true;
        playState = false;
        updateUI("Game Over");
        audioSource.PlayOneShot(audioGameOver);
    }

    public void GameWin()
    {
        pauseState = false;
        restartState = true;
        playState = false;
        updateUI("Game Win");
        audioSource.PlayOneShot(audioGameWin);
    }

    private void updateUI(String title = "Project Trodon")
    {
        TextMeshProUGUI buttonText = playButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = restartState ? "Restart" : pauseState ? "Continue" : "Play";
        titleText.text = title;
        playButton.gameObject.SetActive(!playState);
        titleText.gameObject.SetActive(!playState);
        healthBar.gameObject.SetActive(playState);
        scoreText.gameObject.SetActive(playState);
        pauseButton.gameObject.SetActive(playState);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public bool IsPlayState()
    {
        return playState;
    }

    public void soundShot()
    {
        audioSource.PlayOneShot(audioShot);
    }

    public void soundDash()
    {
        audioSource.PlayOneShot(audioDash);
    }

    public void soundProjectilHit()
    {
        audioSource.PlayOneShot(audioDamage);
    }
}
