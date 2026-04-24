using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void Jogar()
    {
        SceneManager.LoadScene("Mapa");
    }

    public void Sair()
    {
        Application.Quit();
    }
}