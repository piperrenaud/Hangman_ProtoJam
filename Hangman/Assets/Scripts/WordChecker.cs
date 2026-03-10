using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class WordChecker : MonoBehaviour
{
    public TMP_InputField inputField;
    public ScrambledWord scrambledWordScript;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("Lives - Hearts")]
    public List<Image> hearts;
    private int lives;

    [Header("Hangman versions")]
    public Image displayImage;
    public List<Sprite> spriteVersions;
    private int wrongIndex = 0;

    private List<string> remainingWords;

    private void Start()
    {
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);

        inputField.onEndEdit.AddListener(CheckWord);
        lives = hearts.Count;
        if (spriteVersions.Count > 0) displayImage.sprite = spriteVersions[0];


    }

    void CheckWord(string playerInput)
    {
        if (scrambledWordScript == null || string.IsNullOrEmpty(scrambledWordScript.chosenWord)) return;

        if (playerInput.Trim().ToLower() == scrambledWordScript.chosenWord.ToLower())
        {
            Debug.Log("Correct!");

            scrambledWordScript.currentWords.Remove(scrambledWordScript.chosenWord);

            if (scrambledWordScript.currentWords.Count == 0)
            {
                Debug.Log("Player Wins");
                winPanel.SetActive(true);
                return;
            }

            scrambledWordScript.PickRandomWord();
            inputField.text = "";
        }
        else
        {
            Debug.Log("Wrong!");
            LoseLife();
            ShowNextVersion();
            inputField.text = "";
        }
    }

    void LoseLife()
    {
        if (lives <= 0) return;
        lives--;
        Color heartColor = Color.white;
        heartColor.a = 0.4f;
        hearts[lives].color = heartColor;

        if (lives == 0)
        {
            Debug.Log("Game Over");
            gameOverPanel.SetActive(true);
        }
    }

    void ShowNextVersion()
    {
        if (spriteVersions.Count == 0) return;

        wrongIndex++;
        if (wrongIndex >= spriteVersions.Count) wrongIndex = spriteVersions.Count - 1;

        displayImage.sprite = spriteVersions[wrongIndex];
    }
}