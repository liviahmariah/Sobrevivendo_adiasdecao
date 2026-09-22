
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneIntroSimple : MonoBehaviour
{
    [Header("Painel")]
    public Image background;

    [Header("Nome da Fase")]
    public RectTransform nomeFase;
    public TextMeshProUGUI textoNome;

    [Header("Objetivo")]
    public RectTransform objetivo;
    public TextMeshProUGUI textoObjetivo;

    [Header("Contagem Regressiva")]
    public TextMeshProUGUI countdown;

    [Header("Informações")]
    public string fase = "MERCADO";
    public string objetivoFase =
        "OBJETIVO: Buscar alimento para a comunidade.";

    [Header("Velocidades")]
    public float velocidadeEntrada = 8f;
    public float velocidadeSaida = 10f;

    [Header("Configurações")]
    public float tempoExibicao = 1.4f;

    Vector2 posNome;
    Vector2 posObj;

    void Start()
    {
        if (!ValidarReferencias())
            return;

        Time.timeScale = 0;

        textoNome.text = fase;
        textoObjetivo.text = objetivoFase;

        posNome = nomeFase.anchoredPosition;
        posObj = objetivo.anchoredPosition;

        nomeFase.anchoredPosition += Vector2.left * 1200;
        objetivo.anchoredPosition += Vector2.left * 1200;

        countdown.gameObject.SetActive(false);

        StartCoroutine(Intro());
    }

    bool ValidarReferencias()
    {
        bool valido = true;

        if (background == null)
        {
            Debug.LogError("SceneIntroSimple: Background não configurado!");
            valido = false;
        }

        if (nomeFase == null)
        {
            Debug.LogError("SceneIntroSimple: NomeFase não configurado!");
            valido = false;
        }

        if (textoNome == null)
        {
            Debug.LogError("SceneIntroSimple: TextoNome não configurado!");
            valido = false;
        }

        if (objetivo == null)
        {
            Debug.LogError("SceneIntroSimple: Objetivo não configurado!");
            valido = false;
        }

        if (textoObjetivo == null)
        {
            Debug.LogError("SceneIntroSimple: TextoObjetivo não configurado!");
            valido = false;
        }

        if (countdown == null)
        {
            Debug.LogError("SceneIntroSimple: Countdown não configurado!");
            valido = false;
        }

        return valido;
    }

    IEnumerator Intro()
    {
        // Preserva a cor e o Alpha do Inspector
        Color corOriginal = background.color;

        float alphaOriginal = corOriginal.a;

        // Mantém a cor original durante a introdução
        background.color = corOriginal;

        yield return new WaitForSecondsRealtime(.3f);

        // Entrada do nome
        yield return StartCoroutine(
            Mover(nomeFase, posNome)
        );

        yield return new WaitForSecondsRealtime(.15f);

        // Entrada do objetivo
        yield return StartCoroutine(
            Mover(objetivo, posObj)
        );

        // Mantém o painel durante a introdução
        yield return new WaitForSecondsRealtime(tempoExibicao);

        // Saída do nome
        yield return StartCoroutine(
            SairDireita(nomeFase)
        );

        // Saída do objetivo
        yield return StartCoroutine(
            SairDireita(objetivo)
        );

        // Contagem regressiva
        yield return StartCoroutine(Contagem());

        // Fade final: altera somente o Alpha
        Color corFade = background.color;

        while (corFade.a > 0)
        {
            corFade.a -= Time.unscaledDeltaTime * 2f;
            corFade.a = Mathf.Max(corFade.a, 0);

            background.color = corFade;

            yield return null;
        }

        gameObject.SetActive(false);

        Time.timeScale = 1;
    }

    IEnumerator Mover(
        RectTransform alvo,
        Vector2 destino
    )
    {
        while (
            Vector2.Distance(
                alvo.anchoredPosition,
                destino
            ) > 1
        )
        {
            alvo.anchoredPosition = Vector2.Lerp(
                alvo.anchoredPosition,
                destino,
                velocidadeEntrada * Time.unscaledDeltaTime
            );

            yield return null;
        }

        alvo.anchoredPosition = destino;
    }

    IEnumerator SairDireita(RectTransform alvo)
    {
        Vector2 destino =
            alvo.anchoredPosition + Vector2.right * 1800;

        while (
            Vector2.Distance(
                alvo.anchoredPosition,
                destino
            ) > 5
        )
        {
            alvo.anchoredPosition = Vector2.Lerp(
                alvo.anchoredPosition,
                destino,
                velocidadeSaida * Time.unscaledDeltaTime
            );

            yield return null;
        }

        alvo.gameObject.SetActive(false);
    }

    IEnumerator Contagem()
    {
        countdown.gameObject.SetActive(true);

        string[] numeros =
        {
            "3",
            "2",
            "1",
            "VAI!"
        };

        foreach (string numero in numeros)
        {
            countdown.text = numero;

            countdown.transform.localScale =
                Vector3.one * 2f;

            float tempo = 0;

            while (tempo < 1)
            {
                tempo += Time.unscaledDeltaTime * 6;

                countdown.transform.localScale =
                    Vector3.Lerp(
                        Vector3.one * 2f,
                        Vector3.one,
                        tempo
                    );

                yield return null;
            }

            yield return new WaitForSecondsRealtime(.6f);
        }

        countdown.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        Time.timeScale = 1;
    }
}