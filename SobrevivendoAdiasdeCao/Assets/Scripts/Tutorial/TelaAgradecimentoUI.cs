using UnityEngine;
using UnityEngine.SceneManagement;

public class TelaAgradecimentoUI : MonoBehaviour
{
    [Header("Nomes das Cenas")]
    public string cenaPrimeiraFase = "Comunidade";
    public string cenaMenu = "Menu";

    private void Start()
    {
        Time.timeScale = 1f;
    }

    public void BotaoJogarNovamente()
    {
        ResetarComunidade();
        SceneManager.LoadScene(cenaPrimeiraFase);
    }

    public void BotaoVoltarMenu()
    {
        ResetarComunidade();

        if (MusicManager.instance != null)
        {
            MusicManager.instance.TocarMusicaSuave(MusicManager.instance.musicaMenu);
        }

        SceneManager.LoadScene(cenaMenu);
    }

    private void ResetarComunidade()
    {
        if (CommunityManager.instance != null)
        {
            CommunityManager.instance.alimentacao = 100f;
            CommunityManager.instance.saude = 100f;
            CommunityManager.instance.felicidade = 100f;
        }
    }
}