using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ScrambledWord : MonoBehaviour
{
    public TextMeshProUGUI textUI;

    public List<string> easyWords = new List<string>();
    public List<string> mediumWords = new List<string>();
    public List<string> hardWords = new List<string>();

    public string chosenWord;
    public List<string> currentWords = new List<string>();

    private void Start()
    {
        switch (SceneLoader.SelectedDifficulty)
        {
            case 0: currentWords = hardWords; break;
            case 1: currentWords = mediumWords; break;
            case 2: currentWords = easyWords; break;
        }

        PickRandomWord();
    }

    public void PickRandomWord()
    {
        if (currentWords.Count == 0)
        {
            Debug.Log("All words guessed");
            return;
        }

        int index = Random.Range(0, currentWords.Count);
        chosenWord = currentWords[index];
        textUI.text = Scramble(chosenWord);
    }

    string Scramble(string word)
    {
        char[] letters = word.ToCharArray();

        for (int i = letters.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            char temp = letters[i];
            letters[i] = letters[j];
            letters[j] = temp;
        }

        return new string(letters);
    }
}
