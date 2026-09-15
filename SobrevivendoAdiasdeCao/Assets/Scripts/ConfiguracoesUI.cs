using UnityEngine;

public class ConfiguracoesUI : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject painelConfiguracoes;
    public GameObject painelAudio;
    public GameObject painelCreditos;


    // =====================================================
    // ABRIR CONFIGURAÇÕES
    // =====================================================

    public void AbrirConfiguracoes()
    {
        painelConfiguracoes.SetActive(true);

        painelAudio.SetActive(false);
        painelCreditos.SetActive(false);
    }


    // =====================================================
    // FECHAR CONFIGURAÇÕES
    // =====================================================

    public void FecharConfiguracoes()
    {
        painelConfiguracoes.SetActive(false);

        painelAudio.SetActive(false);
        painelCreditos.SetActive(false);
    }


    // =====================================================
    // ABRIR ÁUDIO
    // =====================================================

    public void AbrirAudio()
    {
        painelConfiguracoes.SetActive(false);
        painelAudio.SetActive(true);
        painelCreditos.SetActive(false);
    }


    // =====================================================
    // ABRIR CRÉDITOS
    // =====================================================

    public void AbrirCreditos()
    {
        painelConfiguracoes.SetActive(false);
        painelAudio.SetActive(false);
        painelCreditos.SetActive(true);
    }


    // =====================================================
    // VOLTAR PARA CONFIGURAÇÕES
    // =====================================================

    public void VoltarConfiguracoes()
    {
        painelConfiguracoes.SetActive(true);

        painelAudio.SetActive(false);
        painelCreditos.SetActive(false);
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
