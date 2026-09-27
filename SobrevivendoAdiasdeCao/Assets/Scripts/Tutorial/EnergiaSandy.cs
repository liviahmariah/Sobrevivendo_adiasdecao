using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnergiaSandy : MonoBehaviour
{
    [Header("Energia")]
    public int energiaMaxima = 3;
    public int energiaAtual = 3;

    [Header("Barra de Energia")]
    public Image barraEnergia;

    [Header("Configuração da Gaiola")]
    public Transform pontoRetorno;

    [Header("Aviso ao Jogador")]
    public GameObject painelAviso;
    public TextMeshProUGUI textoAviso;
    public float duracaoAviso = 3f;

    private bool podePerderEnergia = true;

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        energiaAtual = energiaMaxima;

        podePerderEnergia = true;

        AtualizarBarra();

        if (painelAviso != null)
        {
            painelAviso.SetActive(false);
        }
    }

    // =====================================================
    // PERDER ENERGIA
    // =====================================================

    public void PerderEnergia()
    {
        if (!podePerderEnergia)
            return;

        energiaAtual--;

        if (energiaAtual < 0)
        {
            energiaAtual = 0;
        }

        AtualizarBarra();

        Debug.Log(
            "Energia perdida! Atual: " + energiaAtual
        );

        // =================================================
        // DERROTA
        // =================================================

        if (energiaAtual <= 0)
        {
            FalharTutorial();
            return;
        }

        // =================================================
        // AINDA TEM ENERGIA
        // =================================================

        MostrarAviso(
            "VOLTE PARA SUA GAIOLA!"
        );
    }

    // =====================================================
    // ATUALIZAR BARRA
    // =====================================================

    private void AtualizarBarra()
    {
        if (barraEnergia != null)
        {
            if (energiaMaxima > 0)
            {
                barraEnergia.fillAmount =
                    (float)energiaAtual / energiaMaxima;
            }
            else
            {
                barraEnergia.fillAmount = 0f;
            }
        }
    }

    // =====================================================
    // MOSTRAR AVISO
    // =====================================================

    private void MostrarAviso(string mensagem)
    {
        if (painelAviso != null)
        {
            painelAviso.SetActive(true);
        }

        if (textoAviso != null)
        {
            textoAviso.text = mensagem;
        }

        CancelInvoke(nameof(EsconderAviso));

        Invoke(
            nameof(EsconderAviso),
            duracaoAviso
        );
    }

    // =====================================================
    // ESCONDER AVISO
    // =====================================================

    private void EsconderAviso()
    {
        if (painelAviso != null)
        {
            painelAviso.SetActive(false);
        }
    }

    // =====================================================
    // RETORNAR PARA A GAIOLA
    // =====================================================

    public void RetornarParaGaiolaPorCaptura()
    {
        if (pontoRetorno == null)
        {
            Debug.LogWarning(
                "O ponto de retorno não foi configurado."
            );

            return;
        }

        transform.position =
            pontoRetorno.position;

        Debug.Log(
            "Sandy foi colocada de volta na gaiola."
        );
    }

    // =====================================================
    // DERROTA
    // =====================================================

    private void FalharTutorial()
    {
        // Impede novas perdas de energia
        podePerderEnergia = false;

        Debug.Log(
            "Sandy ficou sem energia! Tutorial finalizado."
        );

        MostrarAviso(
            "VOCÊ FICOU SEM ENERGIA!"
        );

        // =================================================
        // CHAMA O PAINEL DE DERROTA
        // =================================================

        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.Derrota();
        }
        else
        {
            Debug.LogWarning(
                "TutorialManager não foi encontrado. " +
                "Não foi possível mostrar o painel de derrota."
            );
        }
    }
}