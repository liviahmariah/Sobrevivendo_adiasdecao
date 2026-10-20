
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

    [Header("Aviso de Início")]
    public RectTransform textoVai;
    public float duracaoVai = 0.8f;
    public float escalaVai = 1.6f;

    [Header("Informações")]
    public string fase = "MERCADO";
    [TextArea]
    public string objetivoFase =
        "OBJETIVO: Buscar alimento para a comunidade.";

    [Header("Velocidades")]
    public float velocidadeEntrada = 8f;
    public float velocidadeSaida = 10f;

    [Header("Configurações")]
    public float tempoExibicao = 1.4f;
    public float duracaoNumero = 0.8f;
    public float intervaloContagem = 0.2f;

    [Header("Efeitos Sonoros")]
    public AudioSource audioSource;

    public AudioClip somEntradaNome;
    public AudioClip somEntradaObjetivo;
    public AudioClip somContagem;
    public AudioClip somApitoFinal;

    [Range(0f, 1f)]
    public float volumeEntrada = 0.6f;

    [Range(0f, 1f)]
    public float volumeContagem = 0.7f;

    [Range(0f, 1f)]
    public float volumeApito = 1f;

    private Vector2 posNome;
    private Vector2 posObj;
    private Vector3 escalaOriginalVai;

    private bool iniciou = false;

    private void Start()
    {
        if (!ValidarReferencias())
            return;

        Time.timeScale = 0f;

        textoNome.text = fase;
        textoObjetivo.text = objetivoFase;

        posNome = nomeFase.anchoredPosition;
        posObj = objetivo.anchoredPosition;
        escalaOriginalVai = textoVai.localScale;

        nomeFase.anchoredPosition += Vector2.left * 1200;
        objetivo.anchoredPosition += Vector2.left * 1200;

        countdown.gameObject.SetActive(false);
        textoVai.gameObject.SetActive(false);

        StartCoroutine(Intro());
    }

    private bool ValidarReferencias()
    {
        bool valido = true;

        if (background == null ||
            nomeFase == null ||
            textoNome == null ||
            objetivo == null ||
            textoObjetivo == null ||
            countdown == null ||
            textoVai == null)
        {
            Debug.LogError(
                "SceneIntroSimple: Configure todas as referências " +
                "do painel, incluindo o texto VAI!"
            );

            valido = false;
        }

        if (audioSource == null)
        {
            Debug.LogWarning(
                "SceneIntroSimple: AudioSource não configurado. " +
                "A introdução funcionará sem os sons."
            );
        }

        return valido;
    }

    private void TocarSom(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    private IEnumerator Intro()
    {
        Color corFade = background.color;

        yield return new WaitForSecondsRealtime(0.3f);

        // Nome da fase
        TocarSom(somEntradaNome, volumeEntrada);

        yield return StartCoroutine(
            Mover(nomeFase, posNome)
        );

        yield return new WaitForSecondsRealtime(0.15f);

        // Objetivo da fase
        TocarSom(somEntradaObjetivo, volumeEntrada);

        yield return StartCoroutine(
            Mover(objetivo, posObj)
        );

        yield return new WaitForSecondsRealtime(tempoExibicao);

        // Saída dos textos
        yield return StartCoroutine(SairDireita(nomeFase));
        yield return StartCoroutine(SairDireita(objetivo));

        // Contagem: 3, 2, 1
        yield return StartCoroutine(Contagem());

        // Aviso separado: VAI!
        yield return StartCoroutine(MostrarVai());

        // Fade do fundo
        corFade = background.color;

        while (corFade.a > 0f)
        {
            corFade.a = Mathf.MoveTowards(
                corFade.a,
                0f,
                Time.unscaledDeltaTime * 2f
            );

            background.color = corFade;

            yield return null;
        }

        Time.timeScale = 1f;

        gameObject.SetActive(false);
    }

    private IEnumerator Mover(
        RectTransform alvo,
        Vector2 destino
    )
    {
        while (
            Vector2.Distance(
                alvo.anchoredPosition,
                destino
            ) > 1f
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

    private IEnumerator SairDireita(RectTransform alvo)
    {
        Vector2 destino =
            alvo.anchoredPosition + Vector2.right * 1800f;

        while (
            Vector2.Distance(
                alvo.anchoredPosition,
                destino
            ) > 5f
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

    private IEnumerator Contagem()
    {
        countdown.gameObject.SetActive(true);

        string[] numeros = { "3", "2", "1" };

        foreach (string numero in numeros)
        {
            countdown.text = numero;

            countdown.transform.localScale =
                Vector3.one * 1.8f;

            TocarSom(somContagem, volumeContagem);

            float tempo = 0f;

            while (tempo < duracaoNumero)
            {
                tempo += Time.unscaledDeltaTime;

                float progresso = duracaoNumero > 0f
                    ? Mathf.Clamp01(tempo / duracaoNumero)
                    : 1f;

                // Começa grande e termina no tamanho normal.
                float escala = Mathf.Lerp(
                    1.8f,
                    1f,
                    Mathf.SmoothStep(0f, 1f, progresso)
                );

                countdown.transform.localScale =
                    Vector3.one * escala;

                yield return null;
            }

            countdown.transform.localScale = Vector3.one;

            if (intervaloContagem > 0f)
            {
                yield return new WaitForSecondsRealtime(
                    intervaloContagem
                );
            }
        }

        countdown.gameObject.SetActive(false);
    }

    private IEnumerator MostrarVai()
    {
        textoVai.gameObject.SetActive(true);
        textoVai.localScale = escalaOriginalVai * escalaVai;

        // Apito marcando o início da fase.
        TocarSom(somApitoFinal, volumeApito);

        float tempo = 0f;

        while (tempo < duracaoVai)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = duracaoVai > 0f
                ? Mathf.Clamp01(tempo / duracaoVai)
                : 1f;

            float escala = Mathf.Lerp(
                escalaVai,
                1f,
                Mathf.SmoothStep(0f, 1f, progresso)
            );

            textoVai.localScale =
                escalaOriginalVai * escala;

            yield return null;
        }

        textoVai.localScale = escalaOriginalVai;

        textoVai.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
}
