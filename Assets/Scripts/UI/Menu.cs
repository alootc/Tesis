using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void menu()
    {
        SceneManager.LoadScene("MainVR");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Salir");
    }
}

