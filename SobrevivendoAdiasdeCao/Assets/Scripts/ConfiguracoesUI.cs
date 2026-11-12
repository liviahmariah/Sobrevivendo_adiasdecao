using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfiguracoesUI : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject painelPause;
    public GameObject painelConfiguracoes;
    public GameObject painelAudio;
    public GameObject painelCreditos;

    [Header("Cena do Menu")]
    [Tooltip("Nome exato da cena do Menu Principal no Build Settings")]
    public string nomeCenaMenu = "Menu";

    // =====================================================
    // CONTROLE DE PAUSE E CONTINUAR
    // =====================================================

    /// <summary>
    /// Despausa o jogo e fecha todas as janelas da UI de Pause.
    /// Vincule este método ao botão "Continuar" / "Jogar".
    /// </summary>
    public void Continuar()
    {
        Time.timeScale = 1f;

        // Se o PauseManager existir na cena, atualiza o estado interno dele
        if (PauseManager.Instance != null && PauseManager.Instance.Pausado)
        {
            PauseManager.Instance.Continuar();
            return;
        }

        FecharTodosOsPaineis();
    }

    /// <summary>
    /// Restaura o tempo do jogo e carrega a cena do Menu Principal.
    /// Vincule este método ao botão "Menu Principal" / "Sair para o Menu".
    /// </summary>
    public void VoltarAoMenuPrincipal()
    {
        Time.timeScale = 1f;
        FecharTodosOsPaineis();

        // Toca a música do menu se o MusicManager existir
        if (MusicManager.instance != null)
        {
            MusicManager.instance.TocarMusicaSuave(MusicManager.instance.musicaMenu);
        }

        SceneManager.LoadScene(nomeCenaMenu);
    }

    // =====================================================
    // NAVEGAÇÃO ENTRE PAINÉIS
    // =====================================================

    public void AbrirConfiguracoes()
    {
        DesativarSeExistir(painelPause);
        DesativarSeExistir(painelAudio);
        DesativarSeExistir(painelCreditos);

        AtivarSeExistir(painelConfiguracoes);
    }

    public void FecharConfiguracoes()
    {
        FecharTodosOsPaineis();

        // Se estiver dentro do Pause, reabre a tela principal do Pause
        AtivarSeExistir(painelPause);
    }

    public void AbrirAudio()
    {
        DesativarSeExistir(painelPause);
        DesativarSeExistir(painelConfiguracoes);
        DesativarSeExistir(painelCreditos);

        AtivarSeExistir(painelAudio);
    }

    public void AbrirCreditos()
    {
        DesativarSeExistir(painelPause);
        DesativarSeExistir(painelConfiguracoes);
        DesativarSeExistir(painelAudio);

        AtivarSeExistir(painelCreditos);
    }

    public void VoltarConfiguracoes()
    {
        DesativarSeExistir(painelPause);
        DesativarSeExistir(painelAudio);
        DesativarSeExistir(painelCreditos);

        AtivarSeExistir(painelConfiguracoes);
    }

    public void VoltarPause()
    {
        DesativarSeExistir(painelConfiguracoes);
        DesativarSeExistir(painelAudio);
        DesativarSeExistir(painelCreditos);

        AtivarSeExistir(painelPause);
    }

    // =====================================================
    // MÉTODOS AUXILIARES DE SUPORTE
    // =====================================================

    private void FecharTodosOsPaineis()
    {
        DesativarSeExistir(painelPause);
        DesativarSeExistir(painelConfiguracoes);
        DesativarSeExistir(painelAudio);
        DesativarSeExistir(painelCreditos);
    }

    private void AtivarSeExistir(GameObject painel)
    {
        if (painel != null)
            painel.SetActive(true);
    }

    private void DesativarSeExistir(GameObject painel)
    {
        if (painel != null)
            painel.SetActive(false);
    }

    // =====================================================
    // SAIR DO JOGO
    // =====================================================

    public void SairDoJogo()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}