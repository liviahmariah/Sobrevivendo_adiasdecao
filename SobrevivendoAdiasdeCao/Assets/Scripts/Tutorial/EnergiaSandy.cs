
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnergiaSandy : MonoBehaviour
{
    [Header("Energia")]
    public int energiaMaxima = 5;
    public int energiaAtual = 5;

    [Header("Barra de Energia")]
    public Image barraEnergia;

    [Header("Configuração da Captura")]
    public Transform pontoRetorno;
    public float tempoRetorno = 1f;

    private bool podePerderEnergia = true;

    void Start()
    {
        energiaAtual = energiaMaxima;
        AtualizarBarra();
    }

    public void PerderEnergia()
    {
        if (!podePerderEnergia)
            return;

        energiaAtual--;

        if (energiaAtual < 0)
            energiaAtual = 0;

        AtualizarBarra();

        Debug.Log("Energia perdida! Atual: " + energiaAtual);

        if (energiaAtual <= 0)
        {
            FalharTutorial();
        }
        else
        {
            StartCoroutine(RetornarParaGaiola());
        }
    }

    void AtualizarBarra()
    {
        if (barraEnergia != null)
        {
            barraEnergia.fillAmount =
                (float)energiaAtual / energiaMaxima;
        }
    }

    IEnumerator RetornarParaGaiola()
    {
        podePerderEnergia = false;

        yield return new WaitForSeconds(tempoRetorno);

        if (pontoRetorno != null)
        {
            transform.position = pontoRetorno.position;
        }

        podePerderEnergia = true;
    }

    void FalharTutorial()
    {
        Debug.Log("Sandy ficou sem energia! Tutorial reiniciado.");

        energiaAtual = energiaMaxima;
        AtualizarBarra();

        // Na próxima etapa vamos conectar
        // o reinício completo do tutorial.
    }
}