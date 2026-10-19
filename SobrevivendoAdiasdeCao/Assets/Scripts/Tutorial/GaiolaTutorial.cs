
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GaiolaTutorial : MonoBehaviour
{
    [Header("Referências")]
    public Transform sandy;
    public Transform pontoInteracao;

    [Header("Elementos da gaiola")]
    public GameObject[] grades;
    public GameObject cao;

    [Header("Interface de interação")]
    public GameObject painelInteracao;
    public TextMeshProUGUI textoInteracao;

    [Header("Interface da barra")]
    public GameObject fundoBarra;
    public Image barraProgresso;

    [Header("Configuração")]
    public float distanciaInteracao = 2f;
    public float tempoParaAbrir = 1.5f;

    [Header("Animação das grades")]
    public float duracaoSacudida = 0.4f;
    public float intensidadeSacudida = 0.06f;
    public float velocidadeSacudida = 35f;

    private float progresso = 0f;
    private bool gaiolaAberta = false;
    private bool sandyPerto = false;
    private bool pressionando = false;
    private bool abrindoGaiola = false;

    private void Start()
    {
        if (cao != null)
            cao.SetActive(false);

        if (painelInteracao != null)
            painelInteracao.SetActive(false);

        if (fundoBarra != null)
            fundoBarra.SetActive(true);

        if (barraProgresso != null)
        {
            barraProgresso.gameObject.SetActive(true);
            barraProgresso.fillAmount = 0f;
        }
    }

    private void Update()
    {
        if (gaiolaAberta || abrindoGaiola || sandy == null)
            return;

        Vector3 origemInteracao = pontoInteracao != null
            ? pontoInteracao.position
            : transform.position;

        float distancia = Vector2.Distance(
            origemInteracao,
            sandy.position
        );

        sandyPerto = distancia <= distanciaInteracao;

        if (!sandyPerto)
        {
            pressionando = false;
            EsconderInteracao();
            return;
        }

        MostrarInteracao();

        if (Input.GetMouseButtonDown(0))
        {
            pressionando = true;
            MostrarBarra();
        }

        if (Input.GetMouseButton(0) && pressionando)
            AumentarProgresso();

        if (Input.GetMouseButtonUp(0))
            pressionando = false;

        if (!pressionando && !abrindoGaiola)
            DiminuirProgresso();
    }

    private void MostrarInteracao()
    {
        if (painelInteracao != null &&
            !painelInteracao.activeSelf)
        {
            painelInteracao.SetActive(true);
        }

        if (textoInteracao != null)
            textoInteracao.text = "SEGURE O CLIQUE";
    }

    private void EsconderInteracao()
    {
        if (painelInteracao != null)
            painelInteracao.SetActive(false);

        progresso = 0f;
        pressionando = false;

        if (barraProgresso != null)
            barraProgresso.fillAmount = 0f;
    }

    private void MostrarBarra()
    {
        if (fundoBarra != null)
            fundoBarra.SetActive(true);

        if (barraProgresso != null)
        {
            barraProgresso.gameObject.SetActive(true);
            barraProgresso.fillAmount = progresso;
        }
    }

    private void AumentarProgresso()
    {
        if (TutorialChavesManager.instance == null)
        {
            Debug.LogError(
                "TutorialChavesManager não foi encontrado."
            );
            return;
        }

        if (!TutorialChavesManager.instance.TemChaveDisponivel())
        {
            if (textoInteracao != null)
                textoInteracao.text = "VOCÊ NÃO TEM CHAVE";

            return;
        }

        progresso += Time.deltaTime / Mathf.Max(tempoParaAbrir, 0.01f);
        progresso = Mathf.Clamp01(progresso);

        if (barraProgresso != null)
            barraProgresso.fillAmount = progresso;

        if (progresso >= 1f)
            TentarAbrirGaiola();
    }

    private void DiminuirProgresso()
    {
        if (progresso <= 0f)
            return;

        progresso -= Time.deltaTime * 2f;
        progresso = Mathf.Clamp01(progresso);

        if (barraProgresso != null)
            barraProgresso.fillAmount = progresso;
    }

    private void TentarAbrirGaiola()
    {
        if (gaiolaAberta || abrindoGaiola)
            return;

        if (TutorialChavesManager.instance == null)
            return;

        bool abriu =
            TutorialChavesManager.instance
            .UsarChaveParaAbrirGaiola();

        if (!abriu)
        {
            progresso = 0f;

            if (barraProgresso != null)
                barraProgresso.fillAmount = 0f;

            return;
        }

        abrindoGaiola = true;
        pressionando = false;

        if (painelInteracao != null)
            painelInteracao.SetActive(false);

        if (barraProgresso != null)
            barraProgresso.fillAmount = 0f;

        StartCoroutine(AnimarAberturaGaiola());
    }

    private IEnumerator AnimarAberturaGaiola()
    {
        // Guarda as posições originais das grades.
        Vector3[] posicoesOriginais = new Vector3[grades.Length];

        for (int i = 0; i < grades.Length; i++)
        {
            if (grades[i] != null)
                posicoesOriginais[i] = grades[i].transform.localPosition;
        }

        float tempo = 0f;

        // Sacode todas as grades juntas.
        while (tempo < duracaoSacudida)
        {
            tempo += Time.deltaTime;

            float intensidade = intensidadeSacudida *
                (1f - Mathf.Clamp01(tempo / duracaoSacudida));

            for (int i = 0; i < grades.Length; i++)
            {
                if (grades[i] == null)
                    continue;

                float deslocamentoX = Mathf.Sin(
                    tempo * velocidadeSacudida
                ) * intensidade;

                float deslocamentoY = Mathf.Cos(
                    tempo * velocidadeSacudida * 0.8f
                ) * intensidade * 0.35f;

                grades[i].transform.localPosition =
                    posicoesOriginais[i] +
                    new Vector3(deslocamentoX, deslocamentoY, 0f);
            }

            yield return null;
        }

        // Restaura as posições antes de desativar as grades.
        for (int i = 0; i < grades.Length; i++)
        {
            if (grades[i] != null)
            {
                grades[i].transform.localPosition =
                    posicoesOriginais[i];

                grades[i].SetActive(false);
            }
        }

        // Liberta o cão.
        if (cao != null)
            cao.SetActive(true);

        gaiolaAberta = true;
        abrindoGaiola = false;

        Debug.Log("Gaiola aberta! Cão libertado.");

        if (TutorialManager.instance != null)
            TutorialManager.instance.VerificarVitoria();
    }
}
