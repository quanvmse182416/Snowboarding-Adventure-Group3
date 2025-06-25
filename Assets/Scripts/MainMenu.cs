using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    private void Play()
    {
        SceneManager.LoadScene("Game");
    }

    private void Quit()
    {
       Application.Quit();
    }
}
