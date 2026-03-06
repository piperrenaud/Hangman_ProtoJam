using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static int SelectedDifficulty = 2; //easy default

    public void SwitchToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void SetDifficulty(int difficulty)
    {
        SelectedDifficulty = difficulty;
    }
}
