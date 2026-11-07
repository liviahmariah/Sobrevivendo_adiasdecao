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
    public QuadroCutscene[] quadros;

    [Header("Escrita do texto")]
    [Min(0.01f)]
    public float velocidadeTexto = 0.09f;

    [Header("Fade dos textos")]
    public float tempoFadeEntrada = 0.8f;
    public float tempoFadeSaida = 0.6f;

    [Header("Tempo de exibição de cada quadro")]
    [Min(0f)]
    public float tempoExibicaoQuadro = 8f;

    [Header("Ampliação das imagens")]
    public float escalaNormal = 1f;
    public float escalaAmpliada = 1.10f;
    public float velocidadeEscala = 2.5f;

    [Header("Som de escrita (SFX)")]
    public AudioSource audioEscrita;
    public AudioClip somEscrita;

    [Header("Aviso para pular/avançar")]
    public TMP_Text textoPular;

    [Header("Transição entre cenas")]
    public Image fade;
    public float tempoFadeTela = 1f;
    public string proximaCena;

    [Header("Configuração de Continuidade")]
    [Tooltip("Marque TRUE se a próxima cena também for parte da cutscene para não parar a música.")]
    public bool proximaCenaEParteDaCutscene = false;

    private bool avancarPressionado = false;
    private bool finalizando = false;

    private TMP_Text textoAtual;
    private Color corOriginalTexto;

    void Start()
    {
        // Registra o efeito sonoro de escrita no AudioManager global
        if (audioEscrita != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.RegisterSFXSource(audioEscrita);
        }

        // Inicia a tela em preto
        if (fade != null)
        {
            Color cor = fade.color;
            cor.a = 1f;
            fade.color = cor;
        }

        // Texto de aviso
        if (textoPular != null)
        {
            textoPular.gameObject.SetActive(true);
            textoPular.text = "Pressione ESPAÇO para avançar";
        }

        // Prepara quadros
        foreach (QuadroCutscene quadro in quadros)
        {
            if (quadro == null) continue;

            if (quadro.imagem != null)
            {
                quadro.imagem.gameObject.SetActive(true);
                quadro.imagem.rectTransform.localScale = Vector3.one * escalaNormal;
            }

            if (quadro.texto != null)
            {
                quadro.texto.text = "";
                quadro.texto.maxVisibleCharacters = 0;
                quadro.texto.gameObject.SetActive(false);
            }
        }

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
            avancarPressionado = true;
        }
    }

    IEnumerator ExecutarCutscene()
    {
        // Revela a tela
        yield return StartCoroutine(FazerFadeTela(1f, 0f));

        for (int i = 0; i < quadros.Length; i++)
        {
            QuadroCutscene quadro = quadros[i];

            if (quadro == null || quadro.imagem == null || quadro.texto == null)
                continue;

            // Fade Out do texto anterior
            if (textoAtual != null)
            {
                yield return StartCoroutine(AnimarAlphaTexto(textoAtual, corOriginalTexto.a, 0f, tempoFadeSaida));
                textoAtual.text = "";
                textoAtual.gameObject.SetActive(false);
            }

            avancarPressionado = false;

            // Configura o texto atual
            textoAtual = quadro.texto;
            corOriginalTexto = textoAtual.color;

            textoAtual.gameObject.SetActive(true);
            textoAtual.text = quadro.conteudo;
            textoAtual.maxVisibleCharacters = 0;

            Color cor = corOriginalTexto;
            cor.a = 0f;
            textoAtual.color = cor;

            // Inicia áudio de digitação
            if (audioEscrita != null && somEscrita != null)
            {
                audioEscrita.clip = somEscrita;
                audioEscrita.loop = true;
                audioEscrita.Play();
            }

            // Escreve e expande a imagem
            yield return StartCoroutine(EscreverTextoEAmpliarImagem(quadro));

            PararSomEscrita();

            // Espera o tempo padrão ou o acionamento da tecla ESPAÇO
            float tempoEsperado = 0f;
            avancarPressionado = false;

            while (tempoEsperado < tempoExibicaoQuadro && !avancarPressionado)
            {
                tempoEsperado += Time.deltaTime;
                yield return null;
            }

            avancarPressionado = false;
        }

        // Esconde a última fala
        if (textoAtual != null)
        {
            yield return StartCoroutine(AnimarAlphaTexto(textoAtual, corOriginalTexto.a, 0f, tempoFadeSaida));
            textoAtual.text = "";
            textoAtual.gameObject.SetActive(false);
        }

        finalizando = true;
        yield return StartCoroutine(CarregarProximaCena());
    }

    IEnumerator EscreverTextoEAmpliarImagem(QuadroCutscene quadro)
    {
        TMP_Text texto = quadro.texto;
        RectTransform imagem = quadro.imagem.rectTransform;

        string conteudo = quadro.conteudo;
        int totalCaracteres = conteudo.Length;

        texto.text = conteudo;
        texto.maxVisibleCharacters = 0;

        float tempoDecorrido = 0f;
        float tempoEscritaTotal = totalCaracteres * velocidadeTexto;
        float duracao = Mathf.Max(tempoEscritaTotal, tempoFadeEntrada);

        Vector3 escalaAlvo = Vector3.one * escalaAmpliada;

        Color cor = corOriginalTexto;
        cor.a = 0f;
        texto.color = cor;

        while (tempoDecorrido < duracao || Vector3.Distance(imagem.localScale, escalaAlvo) > 0.001f)
        {
            // Pressionar Espaço durante a digitação exibe o texto completo
            if (avancarPressionado)
            {
                avancarPressionado = false;
                break;
            }

            tempoDecorrido += Time.deltaTime;

            int caracteresVisiveis = Mathf.Clamp(
                Mathf.FloorToInt(tempoDecorrido / velocidadeTexto),
                0,
                totalCaracteres
            );

            texto.maxVisibleCharacters = caracteresVisiveis;

            float progressoFade = tempoFadeEntrada <= 0f ? 1f : Mathf.Clamp01(tempoDecorrido / tempoFadeEntrada);

            cor = corOriginalTexto;
            cor.a = Mathf.Lerp(0f, corOriginalTexto.a, progressoFade);
            texto.color = cor;

            imagem.localScale = Vector3.MoveTowards(
                imagem.localScale,
                escalaAlvo,
                velocidadeEscala * Time.deltaTime
            );

            yield return null;
        }

        texto.maxVisibleCharacters = totalCaracteres;
        texto.color = corOriginalTexto;
        imagem.localScale = escalaAlvo;
    }

    IEnumerator AnimarAlphaTexto(TMP_Text texto, float alphaInicial, float alphaFinal, float duracao)
    {
        if (texto == null) yield break;

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
            tempo += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempo / duracao);

            cor.a = Mathf.Lerp(alphaInicial, alphaFinal, progresso);
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
        if (fade == null) yield break;

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
            cor.a = Mathf.Lerp(inicio, fim, Mathf.Clamp01(tempo / tempoFadeTela));
            fade.color = cor;
            yield return null;
        }

        cor.a = fim;
        fade.color = cor;
    }

    IEnumerator CarregarProximaCena()
    {
        PararSomEscrita();

        // Fade da tela para preto
        yield return StartCoroutine(FazerFadeTela(0f, 1f));

        if (string.IsNullOrWhiteSpace(proximaCena))
        {
            Debug.LogError("Defina o nome da próxima cena no Inspector!", this);
            yield break;
        }

        // Se a próxima cena ainda é cutscene (ex: Intro1 -> Intro2)
        if (proximaCenaEParteDaCutscene)
        {
            if (MusicManager.instance != null)
            {
                MusicManager.instance.IrParaIntro2(proximaCena);
            }
            else
            {
                SceneManager.LoadScene(proximaCena);
            }
        }
        else // Se a cutscene acabou e vai para o jogo principal
        {
            if (MusicManager.instance != null)
            {
                MusicManager.instance.PararMusicaCutsceneEEntrarNoJogo(proximaCena);
            }
            else
            {
                SceneManager.LoadScene(proximaCena);
            }
        }
    }
}