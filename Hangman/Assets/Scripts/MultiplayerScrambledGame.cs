using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine.UI;

public class MultiplayerScrambledGame : NetworkBehaviour
{
    [Header("Host UI")]
    public TextMeshProUGUI hostScrambledWordText;
    public GameObject winPopupUI;
    public TextMeshProUGUI winPlayerName;
    public TextMeshProUGUI answerWord;
    public GameObject levelOverUI;
    public TextMeshProUGUI difficultyUI;
    public TextMeshProUGUI wordCountUI;

    [Header("Player UI")]
    public GameObject playerCanvas;
    public TMP_InputField playerInputField;
    public GameObject gameOverUI;

    [Header("Lives - Hearts")]
    public List<Image> hearts;
    private int lives;

    [Header("Sprite Versions")]
    public Image displayImage;
    public List<Sprite> spriteVersions;
    private int wrongIndex = 0;

    [Header("Level Over UI Texts")]
    public TextMeshProUGUI firstPlaceText;
    public TextMeshProUGUI secondPlaceText;
    public TextMeshProUGUI thirdPlaceText;

    [Header("Word Lists")]
    public List<string> easyWords = new();
    public List<string> mediumWords = new();
    public List<string> hardWords = new();

    private PlayerData playerData;
    private List<string> currentWords = new();
    private NetworkVariable<FixedString128Bytes> currentWord = new NetworkVariable<FixedString128Bytes>("");
    private List<string> allWordsForCurrentDifficulty = new();

    private void Start()
    {
        winPopupUI.SetActive(false);
        gameOverUI.SetActive(false);

        lives = hearts.Count;
        wrongIndex = 0;
        if (spriteVersions.Count > 0 && displayImage != null)
        {
            displayImage.sprite = spriteVersions[0];
        }

        if (IsHost)
        {
            ChooseDifficulty();
            PickNewWord();
        }

        if (playerInputField != null)
        {
            playerInputField.onEndEdit.AddListener(OnSubmitGuess);
        }

        currentWord.OnValueChanged += OnWordChanged;
    }

    void ChooseDifficulty()
    {
        switch (SceneLoader.SelectedDifficulty)
        {
            case 0: currentWords = new List<string>(hardWords); difficultyUI.text = "Difficulty: Hard" ; break;
            case 1: currentWords = new List<string>(mediumWords); difficultyUI.text = "Difficulty: Medium"; break;
            case 2: currentWords = new List<string>(easyWords); difficultyUI.text = "Difficulty: Easy"; break;
        }

        allWordsForCurrentDifficulty = new List<string>(currentWords);
        UpdateWordCountUI();
    }

    void UpdateWordCountUI()
    {
        if (wordCountUI != null)
        {
            int wordsDone = allWordsForCurrentDifficulty.Count - currentWords.Count;
            int total = allWordsForCurrentDifficulty.Count;
            wordCountUI.text = $"{wordsDone}/{total}";
        }
    }

    void  PickNewWord()
    {
        if (!IsHost) return;
        if (currentWords.Count == 0)
        {
            ShowLevelOverServerRpc();
            UpdateWordCountUI();
            return;
        }

        int index = Random.Range(0, currentWords.Count);
        string chosen = currentWords[index];
        currentWords.RemoveAt(index);

        currentWord.Value = chosen;
        hostScrambledWordText.text = Scramble(chosen);
        UpdateWordCountUI();
    }

    void OnWordChanged(FixedString128Bytes oldWord, FixedString128Bytes newWord)
    {
        if (IsHost) return;
        //players dont see word, only host
    }

    void OnSubmitGuess(string guess)
    {
        if (string.IsNullOrEmpty(guess)) return;
        SubmitGuessServerRpc(guess.Trim().ToLower());
        playerInputField.text = "";
    }

    [ServerRpc(RequireOwnership = false)]
    void SubmitGuessServerRpc(string guess, ServerRpcParams rpcParams = default)
    {
        string correct = currentWord.Value.ToString().ToLower();

        ulong senderId = rpcParams.Receive.SenderClientId;
        var playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(senderId);
        if (playerObj == null) return;

        var pd = playerObj.GetComponent<PlayerData>();
        if (pd == null) return;

        if (guess == correct)
        {
            ShowWinClientRpc(senderId, pd.playerName.Value.ToString(), correct);
            PickNewWord();
        }
        else
        {
            LoseLifeClientRpc(senderId);
        }
    }

    [ClientRpc]
    void ShowWinClientRpc(ulong winnerClientId, string winnerName,string correctWord)
    {
        if (NetworkManager.Singleton.LocalClientId == winnerClientId)
        {
            StartCoroutine(ShowWinRoutine(winnerClientId, winnerName, correctWord));
        }
    }

    IEnumerator ShowWinRoutine(ulong winnerID, string winnerName, string correctWord)
    {
        winPlayerName.text = winnerName;
        answerWord.text = correctWord;
        winPopupUI.SetActive(true);
        yield return new WaitForSeconds(3f);
        winPopupUI.SetActive(false);
    }

    [ClientRpc]
    void LoseLifeClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        if (lives <= 0) return;

        lives--;
        if (hearts.Count > lives)
        {
            Color heartColor = Color.white;
            heartColor.a = 0.4f;
            hearts[lives].color = heartColor;
        }

        wrongIndex++;
        if (wrongIndex >= spriteVersions.Count) wrongIndex = spriteVersions.Count - 1;
        if (displayImage != null && spriteVersions.Count > 0)
        {
            displayImage.sprite = spriteVersions[wrongIndex];
        }

        if (lives <= 0)
        {
            gameOverUI.SetActive(true);
        }
    }

    string Scramble(string word)
    {
        char[] letters = word.ToCharArray();
        for (int i = letters.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (letters[i], letters[j]) = (letters[j], letters[i]);
        }

        return new string(letters);
    }

    [ServerRpc(RequireOwnership = false)]
    void ShowLevelOverServerRpc()
    {
        List<NetworkClient> players = new List<NetworkClient>(NetworkManager.Singleton.ConnectedClientsList);
        players.Sort((a, b) =>
        {
            var pa = a.PlayerObject.GetComponent<PlayerData>();
            var pb = b.PlayerObject.GetComponent<PlayerData>();
            return pb.Score.Value.CompareTo(pa.Score.Value);
        });

        string first = players.Count > 0 ? players[0].PlayerObject.GetComponent<PlayerData>().playerName.Value.ToString() : "N/A";
        string second = players.Count > 1 ? players[1].PlayerObject.GetComponent<PlayerData>().playerName.Value.ToString() : "N/A";
        string third = players.Count > 2 ? players[2].PlayerObject.GetComponent<PlayerData>().playerName.Value.ToString() : "N/A";

        ShowLevelOverClientRpc(first, second, third);
    }

    [ClientRpc]
    void ShowLevelOverClientRpc(string first, string second, string third)
    {
        if (firstPlaceText != null) firstPlaceText.text = "1st: " + first;
        if (secondPlaceText != null) secondPlaceText.text = "2nd: " + second;
        if (thirdPlaceText != null) thirdPlaceText.text = "3rd: " + third;

        if (levelOverUI != null) levelOverUI.SetActive(true);
        if (playerCanvas != null) playerCanvas.SetActive(false);
    }
}
