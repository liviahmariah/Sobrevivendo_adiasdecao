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

    private void Start()
    {
        energiaAtual = energiaMaxima;

        AtualizarBarra();

        if (painelAviso != null)
        {
            painelAviso.SetActive(false);
        }
    }

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

        if (energiaAtual <= 0)
        {
            FalharTutorial();
            return;
        }

        MostrarAviso(
            "VOLTE PARA SUA GAIOLA!"
        );
    }

    private void AtualizarBarra()
    {
        if (barraEnergia != null)
        {
            barraEnergia.fillAmount =
                (float)energiaAtual / energiaMaxima;
        }
    }

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

    private void EsconderAviso()
    {
        if (painelAviso != null)
        {
            painelAviso.SetActive(false);
        }
    }

    public void RetornarParaGaiolaPorCaptura()
    {
        if (pontoRetorno == null)
        {
            Debug.LogWarning(
                "O ponto de retorno não foi configurado."
            );

            return;
        }

        transform.position = pontoRetorno.position;

        Debug.Log(
            "Sandy foi colocada de volta na gaiola."
        );
    }

    private void FalharTutorial()
    {
        Debug.Log(
            "Sandy ficou sem energia! Tutorial finalizado."
        );

        MostrarAviso(
            "VOCÊ FICOU SEM ENERGIA!"
        );

        // O reinício completo será implementado depois.
    }
}