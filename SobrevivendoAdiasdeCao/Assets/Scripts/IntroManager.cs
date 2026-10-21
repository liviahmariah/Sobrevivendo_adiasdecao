
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class IntroManager : MonoBehaviour
{
    [System.Serializable]
    public class QuadroCutscene
    {
        [Header("Imagem e texto")]
        public Image imagem;
        public TMP_Text texto;

        [TextArea(3, 6)]
        public string conteudo;
    }

    [Header("Quadros da cutscene")]
    public QuadroCutscene[] quadros = new QuadroCutscene[3];

    [Header("Escrita do texto")]
    [Min(0.01f)]
    public float velocidadeTexto = 0.09f;

    [Header("Fade dos textos")]
    public float tempoFadeEntrada = 0.8f;
    public float tempoFadeSaida = 0.6f;

    [Header("Pausa entre falas")]
    public float pausaEntreQuadros = 0.5f;

    [Header("Ampliação das imagens")]
    public float escalaNormal = 1f;
    public float escalaAmpliada = 1.10f;
    public float velocidadeEscala = 2.5f;

    [Header("Som de escrita")]
    public AudioSource audioEscrita;
    public AudioClip somEscrita;

    [Header("Aviso para pular")]
    public TMP_Text textoPular;

    [Header("Transição entre cenas")]
    public Image fade;
    public float tempoFadeTela = 1f;
    public string proximaCena;

    private bool pulou = false;
    private bool finalizando = false;

    private TMP_Text textoAtual;
    private Color corOriginalTexto;

    void Start()
    {
        // Começa com a tela preta.
        if (fade != null)
        {
            Color cor = fade.color;
            cor.a = 1f;
            fade.color = cor;
        }

        // Aviso para pular a cutscene.
        if (textoPular != null)
        {
            textoPular.gameObject.SetActive(true);
            textoPular.text = "Pressione ESPAÇO para pular";
        }

        // Todas as imagens ficam visíveis desde o início.
        foreach (QuadroCutscene quadro in quadros)
        {
            if (quadro == null)
                continue;

            if (quadro.imagem != null)
            {
                quadro.imagem.gameObject.SetActive(true);

                quadro.imagem.rectTransform.localScale =
                    Vector3.one * escalaNormal;
            }

            if (quadro.texto != null)
            {
                quadro.texto.text = "";
                quadro.texto.maxVisibleCharacters = 0;
                quadro.texto.gameObject.SetActive(false);
            }
        }

        // Configura o áudio.
        if (audioEscrita != null)
        {
            audioEscrita.playOnAwake = false;
            audioEscrita.loop = true;
            audioEscrita.clip = somEscrita;
        }

        StartCoroutine(ExecutarCutscene());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !finalizando)
        {
            pulou = true;
            finalizando = true;

            StopAllCoroutines();
            PararSomEscrita();

            StartCoroutine(CarregarProximaCena());
        }
    }

    IEnumerator ExecutarCutscene()
    {
        // Revela a cena.
        yield return StartCoroutine(FazerFadeTela(1f, 0f));

        foreach (QuadroCutscene quadro in quadros)
        {
            if (pulou)
                yield break;

            if (quadro == null)
                continue;

            if (quadro.imagem == null || quadro.texto == null)
            {
                Debug.LogWarning(
                    "Um quadro está sem imagem ou texto.",
                    this
                );

                continue;
            }

            // A fala anterior desaparece suavemente.
            if (textoAtual != null)
            {
                yield return StartCoroutine(
                    AnimarAlphaTexto(
                        textoAtual,
                        corOriginalTexto.a,
                        0f,
                        tempoFadeSaida
                    )
                );

                textoAtual.text = "";
                textoAtual.gameObject.SetActive(false);
            }

            if (pulou)
                yield break;

            // Prepara a nova fala.
            textoAtual = quadro.texto;
            corOriginalTexto = textoAtual.color;

            textoAtual.gameObject.SetActive(true);
            textoAtual.text = quadro.conteudo;
            textoAtual.maxVisibleCharacters = 0;

            Color cor = corOriginalTexto;
            cor.a = 0f;
            textoAtual.color = cor;

            // Inicia o som de escrita.
            if (audioEscrita != null && somEscrita != null)
            {
                audioEscrita.clip = somEscrita;
                audioEscrita.loop = true;
                audioEscrita.Play();
            }

            // Digita lentamente, faz o fade de entrada
            // e amplia a imagem simultaneamente.
            yield return StartCoroutine(
                EscreverTextoEAmpliarImagem(quadro)
            );

            PararSomEscrita();

            if (pulou)
                yield break;

            // Mantém a fala na tela antes da próxima transição.
            yield return new WaitForSeconds(pausaEntreQuadros);
        }

        // Faz a última fala desaparecer.
        if (!pulou && textoAtual != null)
        {
            yield return StartCoroutine(
                AnimarAlphaTexto(
                    textoAtual,
                    corOriginalTexto.a,
                    0f,
                    tempoFadeSaida
                )
            );

            textoAtual.text = "";
            textoAtual.gameObject.SetActive(false);
        }

        if (!pulou)
        {
            finalizando = true;
            yield return StartCoroutine(CarregarProximaCena());
        }
    }

    IEnumerator EscreverTextoEAmpliarImagem(
        QuadroCutscene quadro)
    {
        TMP_Text texto = quadro.texto;
        RectTransform imagem = quadro.imagem.rectTransform;

        string conteudo = quadro.conteudo;
        int totalCaracteres = conteudo.Length;

        texto.text = conteudo;
        texto.maxVisibleCharacters = 0;

        float tempoDecorrido = 0f;
        float tempoProximaLetra = 0f;

        float tempoEscritaTotal =
            totalCaracteres * velocidadeTexto;

        float duracao = Mathf.Max(
            tempoEscritaTotal,
            tempoFadeEntrada
        );

        Vector3 escalaAlvo =
            Vector3.one * escalaAmpliada;

        Color cor = corOriginalTexto;
        cor.a = 0f;
        texto.color = cor;

        while (tempoDecorrido < duracao ||
               Vector3.Distance(imagem.localScale, escalaAlvo) > 0.001f)
        {
            if (pulou)
                yield break;

            tempoDecorrido += Time.deltaTime;
            tempoProximaLetra += Time.deltaTime;

            // Revela as letras no ritmo configurado.
            int caracteresVisiveis = Mathf.Clamp(
                Mathf.FloorToInt(
                    tempoDecorrido / velocidadeTexto
                ),
                0,
                totalCaracteres
            );

            texto.maxVisibleCharacters = caracteresVisiveis;

            // Fade de entrada da fala.
            float progressoFade = tempoFadeEntrada <= 0f
                ? 1f
                : Mathf.Clamp01(
                    tempoDecorrido / tempoFadeEntrada
                );

            cor = corOriginalTexto;
            cor.a = Mathf.Lerp(
                0f,
                corOriginalTexto.a,
                progressoFade
            );

            texto.color = cor;

            // Amplia a imagem atual suavemente.
            imagem.localScale = Vector3.MoveTowards(
                imagem.localScale,
                escalaAlvo,
                velocidadeEscala * Time.deltaTime
            );

            yield return null;
        }

        // Garante que todo o texto e a ampliação terminem.
        texto.maxVisibleCharacters = totalCaracteres;
        texto.color = corOriginalTexto;
        imagem.localScale = escalaAlvo;
    }

    IEnumerator AnimarAlphaTexto(
        TMP_Text texto,
        float alphaInicial,
        float alphaFinal,
        float duracao)
    {
        if (texto == null)
            yield break;

        Color cor = texto.color;

        if (duracao <= 0f)
        {
            cor.a = alphaFinal;
            texto.color = cor;
            yield break;
        }

        float tempo = 0f;

        while (tempo < duracao)
        {
            if (pulou)
                yield break;

            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(tempo / duracao);

            cor.a = Mathf.Lerp(
                alphaInicial,
                alphaFinal,
                progresso
            );

            texto.color = cor;

            yield return null;
        }

        cor.a = alphaFinal;
        texto.color = cor;
    }

    void PararSomEscrita()
    {
        if (audioEscrita != null && audioEscrita.isPlaying)
            audioEscrita.Stop();
    }

    IEnumerator FazerFadeTela(float inicio, float fim)
    {
        if (fade == null)
            yield break;

        Color cor = fade.color;

        if (tempoFadeTela <= 0f)
        {
            cor.a = fim;
            fade.color = cor;
            yield break;
        }

        float tempo = 0f;

        while (tempo < tempoFadeTela)
        {
            tempo += Time.deltaTime;

            cor.a = Mathf.Lerp(
                inicio,
                fim,
                Mathf.Clamp01(tempo / tempoFadeTela)
            );

            fade.color = cor;

            yield return null;
        }

        cor.a = fim;
        fade.color = cor;
    }

    IEnumerator CarregarProximaCena()
    {
        PararSomEscrita();

        yield return StartCoroutine(
            FazerFadeTela(0f, 1f)
        );

        if (string.IsNullOrWhiteSpace(proximaCena))
        {
            Debug.LogError(
                "Defina o nome da próxima cena no Inspector!",
                this
            );

            yield break;
        }

        SceneManager.LoadScene(proximaCena);
    }
}
