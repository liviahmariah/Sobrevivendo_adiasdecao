using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CutsceneFinalManager : MonoBehaviour
{
    [System.Serializable]
    public class TextoCutscene
    {
        [Header("Texto")]
        public TMP_Text texto;

        [TextArea(3, 8)]
        public string conteudo;
    }

    [System.Serializable]
    public class CachorroHomenageado
    {
        [Header("Retrato do cachorro")]
        public Image retrato;

        [Header("Nome abaixo do retrato")]
        public TMP_Text nome;

        public string nomeCao;
    }

    [Header("1. Mensagens iniciais")]
    public TextoCutscene[] mensagensIniciais;

    [Header("2. Homenagem aos cachorros")]
    public CachorroHomenageado[] cachorros;

    [Header("3. Créditos finais")]
    public TextoCutscene[] creditos;

    [Header("Escrita dos textos")]
    [Min(0.01f)]
    public float velocidadeTexto = 0.09f;

    [Header("Fade dos textos")]
    public float tempoFadeEntrada = 0.8f;
    public float tempoFadeSaida = 0.6f;

    [Header("Tempo de leitura")]
    [Min(0f)]
    public float tempoExibicaoTexto = 3f;

    [Min(0f)]
    public float tempoExibicaoCachorro = 3f;

    [Header("Retratos dos cachorros")]
    public float escalaNormal = 1f;
    public float escalaAmpliada = 1.10f;
    public float velocidadeEscala = 2.5f;
    public float tempoFadeRetrato = 0.8f;

    [Header("Som das letras")]
    public AudioSource audioEscrita;
    public AudioClip somEscrita;

    [Header("Trilha sonora opcional")]
    [Tooltip("Deixe vazio se a música já estiver sendo controlada por outro sistema.")]
    public AudioSource musicaFundo;

    public AudioClip trilhaFinal;

    [Range(0f, 1f)]
    public float volumeTrilha = 0.5f;

    public float tempoFadeMusica = 2f;

    [Header("Aviso para pular")]
    public TMP_Text textoPular;

    [Header("Transição de tela")]
    public Image fade;
    public float tempoFadeTela = 1f;
    public string proximaCena;

    private bool finalizando;
    private bool pulou;

    void Start()
    {
        // A cena começa preta.
        if (fade != null)
        {
            Color cor = fade.color;
            cor.a = 1f;
            fade.color = cor;
            fade.gameObject.SetActive(true);
        }

        if (textoPular != null)
        {
            textoPular.gameObject.SetActive(true);
            textoPular.text = "Pressione ESPAÇO para pular";
        }

        PrepararTextos(mensagensIniciais);
        PrepararTextos(creditos);
        PrepararCachorros();

        // Configura o som das letras.
        if (audioEscrita != null)
        {
            audioEscrita.playOnAwake = false;
            audioEscrita.loop = true;
            audioEscrita.clip = somEscrita;
        }

        // A trilha é opcional.
        if (musicaFundo != null && trilhaFinal != null)
        {
            if (musicaFundo.clip != trilhaFinal ||
                !musicaFundo.isPlaying)
            {
                musicaFundo.clip = trilhaFinal;
                musicaFundo.loop = true;
                musicaFundo.volume = 0f;
                musicaFundo.Play();
            }

            StartCoroutine(
                FadeVolumeMusica(volumeTrilha, tempoFadeMusica)
            );
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

            StartCoroutine(EncerrarCutscene());
        }
    }

    // Prepara os textos sem mostrá-los.
    void PrepararTextos(TextoCutscene[] lista)
    {
        if (lista == null)
            return;

        foreach (TextoCutscene item in lista)
        {
            if (item == null || item.texto == null)
                continue;

            item.texto.text = "";
            item.texto.maxVisibleCharacters = 0;

            Color cor = item.texto.color;
            cor.a = 0f;
            item.texto.color = cor;

            item.texto.gameObject.SetActive(false);
        }
    }

    // Deixa todos os retratos inicialmente transparentes.
    void PrepararCachorros()
    {
        if (cachorros == null)
            return;

        foreach (CachorroHomenageado cao in cachorros)
        {
            if (cao == null)
                continue;

            if (cao.retrato != null)
            {
                cao.retrato.gameObject.SetActive(true);
                cao.retrato.canvasRenderer.SetAlpha(0f);
                cao.retrato.rectTransform.localScale =
                    Vector3.one * escalaNormal;
            }

            if (cao.nome != null)
            {
                cao.nome.text = "";
                cao.nome.maxVisibleCharacters = 0;

                Color cor = cao.nome.color;
                cor.a = 0f;
                cao.nome.color = cor;

                cao.nome.gameObject.SetActive(false);
            }
        }
    }

    IEnumerator ExecutarCutscene()
    {
        // Revela a cena.
        yield return StartCoroutine(
            FazerFadeTela(1f, 0f)
        );

        // ETAPA 1: agradecimento e reflexão.
        if (mensagensIniciais != null)
        {
            foreach (TextoCutscene mensagem in mensagensIniciais)
            {
                if (pulou)
                    yield break;

                if (mensagem == null || mensagem.texto == null)
                    continue;

                yield return StartCoroutine(
                    MostrarTexto(mensagem, tempoExibicaoTexto)
                );
            }
        }

        if (pulou)
            yield break;

        // ETAPA 2: retratos individuais.
        if (cachorros != null)
        {
            foreach (CachorroHomenageado cao in cachorros)
            {
                if (pulou)
                    yield break;

                if (cao == null || cao.retrato == null)
                {
                    Debug.LogWarning(
                        "Um cachorro está sem retrato na cutscene.",
                        this
                    );
                    continue;
                }

                yield return StartCoroutine(
                    MostrarCachorro(cao)
                );
            }
        }

        if (pulou)
            yield break;

        // ETAPA 3: créditos.
        if (creditos != null)
        {
            foreach (TextoCutscene credito in creditos)
            {
                if (pulou)
                    yield break;

                if (credito == null || credito.texto == null)
                    continue;

                yield return StartCoroutine(
                    MostrarTexto(credito, tempoExibicaoTexto)
                );
            }
        }

        if (pulou)
            yield break;

        finalizando = true;

        yield return StartCoroutine(EncerrarCutscene());
    }

    // Exibe um texto com fade e escrita letra por letra.
    IEnumerator MostrarTexto(TextoCutscene item, float tempoLeitura)
    {
        TMP_Text texto = item.texto;

        texto.gameObject.SetActive(true);
        texto.text = item.conteudo;
        texto.maxVisibleCharacters = 0;
        texto.ForceMeshUpdate();

        Color corOriginal = texto.color;
        corOriginal.a = 1f;

        Color cor = corOriginal;
        cor.a = 0f;
        texto.color = cor;

        int totalCaracteres = texto.textInfo.characterCount;

        IniciarSomEscrita();

        float tempo = 0f;
        float duracaoEscrita = totalCaracteres * velocidadeTexto;
        float duracao = Mathf.Max(
            duracaoEscrita,
            tempoFadeEntrada
        );

        while (tempo < duracao)
        {
            if (pulou)
                yield break;

            tempo += Time.deltaTime;

            texto.maxVisibleCharacters = Mathf.Clamp(
                Mathf.FloorToInt(tempo / velocidadeTexto),
                0,
                totalCaracteres
            );

            float alpha = tempoFadeEntrada <= 0f
                ? 1f
                : Mathf.Clamp01(tempo / tempoFadeEntrada);

            cor.a = alpha;
            texto.color = cor;

            yield return null;
        }

        texto.maxVisibleCharacters = totalCaracteres;
        texto.color = corOriginal;

        PararSomEscrita();

        if (tempoLeitura > 0f)
            yield return new WaitForSeconds(tempoLeitura);

        if (pulou)
            yield break;

        yield return StartCoroutine(
            AnimarAlphaTexto(
                texto,
                corOriginal.a,
                0f,
                tempoFadeSaida
            )
        );

        texto.text = "";
        texto.gameObject.SetActive(false);
    }

    // Mostra um retrato e escreve o nome do cachorro.
    IEnumerator MostrarCachorro(CachorroHomenageado cao)
    {
        Image retrato = cao.retrato;
        RectTransform imagem = retrato.rectTransform;
        TMP_Text nome = cao.nome;

        // O retrato começa transparente e na escala normal.
        retrato.canvasRenderer.SetAlpha(0f);
        imagem.localScale = Vector3.one * escalaNormal;

        if (nome != null)
        {
            nome.gameObject.SetActive(true);
            nome.text = string.IsNullOrWhiteSpace(cao.nomeCao)
                ? ""
                : cao.nomeCao;

            nome.maxVisibleCharacters = 0;
            nome.ForceMeshUpdate();

            Color corNome = nome.color;
            corNome.a = 0f;
            nome.color = corNome;
        }

        int totalCaracteres = nome != null
            ? nome.textInfo.characterCount
            : 0;

        float tempoEscrita = totalCaracteres * velocidadeTexto;

        float tempoZoom = velocidadeEscala > 0f
            ? Mathf.Abs(escalaAmpliada - escalaNormal)
                / velocidadeEscala
            : 0f;

        float duracao = Mathf.Max(
            tempoFadeRetrato,
            tempoEscrita,
            tempoZoom
        );

        IniciarSomEscrita();

        float tempo = 0f;

        while (tempo < duracao)
        {
            if (pulou)
                yield break;

            tempo += Time.deltaTime;

            // Fade de entrada do retrato.
            float alphaRetrato = tempoFadeRetrato <= 0f
                ? 1f
                : Mathf.Clamp01(tempo / tempoFadeRetrato);

            retrato.canvasRenderer.SetAlpha(alphaRetrato);

            // Ampliação suave.
            imagem.localScale = Vector3.MoveTowards(
                imagem.localScale,
                Vector3.one * escalaAmpliada,
                velocidadeEscala * Time.deltaTime
            );

            // Escrita e fade do nome.
            if (nome != null)
            {
                nome.maxVisibleCharacters = Mathf.Clamp(
                    Mathf.FloorToInt(tempo / velocidadeTexto),
                    0,
                    totalCaracteres
                );

                Color corNome = nome.color;
                corNome.a = tempoFadeEntrada <= 0f
                    ? 1f
                    : Mathf.Clamp01(tempo / tempoFadeEntrada);

                nome.color = corNome;
            }

            yield return null;
        }

        retrato.canvasRenderer.SetAlpha(1f);
        imagem.localScale = Vector3.one * escalaAmpliada;

        if (nome != null)
        {
            nome.maxVisibleCharacters = totalCaracteres;

            Color corNome = nome.color;
            corNome.a = 1f;
            nome.color = corNome;
        }

        PararSomEscrita();

        // O retrato permanece na tela durante a homenagem.
        if (tempoExibicaoCachorro > 0f)
            yield return new WaitForSeconds(tempoExibicaoCachorro);

        if (pulou)
            yield break;

        // Antes do próximo retrato, este desaparece em fade.
        yield return StartCoroutine(
            EsconderCachorro(cao)
        );
    }

    IEnumerator EsconderCachorro(CachorroHomenageado cao)
    {
        float duracao = Mathf.Max(0f, tempoFadeRetrato);
        float tempo = 0f;

        Image retrato = cao.retrato;
        TMP_Text nome = cao.nome;

        float alphaInicial = 1f;

        Color corNome = Color.white;
        if (nome != null)
        {
            corNome = nome.color;
            alphaInicial = corNome.a;
        }

        if (duracao <= 0f)
        {
            retrato.canvasRenderer.SetAlpha(0f);

            if (nome != null)
            {
                corNome.a = 0f;
                nome.color = corNome;
                nome.text = "";
                nome.gameObject.SetActive(false);
            }

            yield break;
        }

        while (tempo < duracao)
        {
            if (pulou)
                yield break;

            tempo += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempo / duracao);

            retrato.canvasRenderer.SetAlpha(1f - progresso);

            if (nome != null)
            {
                corNome.a = Mathf.Lerp(
                    alphaInicial,
                    0f,
                    progresso
                );

                nome.color = corNome;
            }

            yield return null;
        }

        retrato.canvasRenderer.SetAlpha(0f);

        if (nome != null)
        {
            corNome.a = 0f;
            nome.color = corNome;
            nome.text = "";
            nome.gameObject.SetActive(false);
        }

        // O retrato fica pronto para ser usado novamente.
        retrato.rectTransform.localScale =
            Vector3.one * escalaNormal;
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

            cor.a = Mathf.Lerp(
                alphaInicial,
                alphaFinal,
                Mathf.Clamp01(tempo / duracao)
            );

            texto.color = cor;

            yield return null;
        }

        cor.a = alphaFinal;
        texto.color = cor;
    }

    void IniciarSomEscrita()
    {
        if (audioEscrita == null || somEscrita == null)
            return;

        audioEscrita.clip = somEscrita;
        audioEscrita.loop = true;

        if (!audioEscrita.isPlaying)
            audioEscrita.Play();
    }

    void PararSomEscrita()
    {
        if (audioEscrita != null && audioEscrita.isPlaying)
            audioEscrita.Stop();
    }

    IEnumerator FadeVolumeMusica(float volumeAlvo, float duracao)
    {
        if (musicaFundo == null)
            yield break;

        float volumeInicial = musicaFundo.volume;

        if (duracao <= 0f)
        {
            musicaFundo.volume = volumeAlvo;
            yield break;
        }

        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;

            musicaFundo.volume = Mathf.Lerp(
                volumeInicial,
                volumeAlvo,
                Mathf.Clamp01(tempo / duracao)
            );

            yield return null;
        }

        musicaFundo.volume = volumeAlvo;
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

    IEnumerator EncerrarCutscene()
    {
        PararSomEscrita();

        if (musicaFundo != null && musicaFundo.isPlaying)
        {
            yield return StartCoroutine(
                FadeVolumeMusica(0f, tempoFadeMusica)
            );
        }

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
