using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] int targetScore = 10, lives = 3;
    [SerializeField] float respawnDelay = 0.6f, spawnProtection = 1.5f;
    [SerializeField] Health player;
    [SerializeField] Transform respawnPoint;
    [SerializeField] TMP_Text scoreText, livesText;
    [SerializeField] GameObject winPanel, losePanel;
    [SerializeField] GameObject nextLevelButton;   // optional: hidden automatically on the last level

    [Header("Locked final coin (optional)")]
    [SerializeField] GameObject finalCoin;         // leave it disabled in the scene; it appears once enough coins are collected
    [SerializeField] int coinsToUnlock = 3;
    int score;

    void Awake() => Instance = this;
    void Start() => UpdateUI();

    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
        if (finalCoin != null && score >= coinsToUnlock) finalCoin.SetActive(true);
        if (score >= targetScore) Win();
    }

    public void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {score} / {targetScore}";
        if (livesText != null && player != null)
            livesText.text = $"Lives: {lives}   Health: {Mathf.Max(player.Current, 0)} / {player.Max}";
    }

    public void PlayerDied()
    {
        lives--;
        UpdateUI();
        if (lives <= 0) Lose();
        else StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay);
        Rigidbody body = player.GetComponent<Rigidbody>();
        body.position = respawnPoint.position;
        body.linearVelocity = Vector3.zero;
        player.Restore(spawnProtection);
    }

    void Win()
    {
        // No scene after this one in the build list = last level, so hide the Next Level button
        if (nextLevelButton != null)
            nextLevelButton.SetActive(SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings);

        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void Lose() { losePanel.SetActive(true); Time.timeScale = 0f; }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;   // Win() froze time, and it survives a scene load
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(next);
        else SceneManager.LoadScene(0);   // finished the last level: back to the first
    }
}