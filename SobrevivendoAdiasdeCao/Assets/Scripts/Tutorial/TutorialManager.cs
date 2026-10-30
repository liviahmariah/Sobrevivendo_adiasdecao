
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    // =====================================================
    // ETAPAS DO TUTORIAL
    // =====================================================


    public enum EtapaTutorial
    {
        Introducao,
        Corrida,
        Pulo,
        Latido,
        Coleta,
        Finalizado
    }

    private void ConfigurarMusicaFinal()
    {
        if (audioSourceMusicaFinal == null)
        {
            audioSourceMusicaFinal = GetComponent<AudioSource>();

            if (audioSourceMusicaFinal == null)
            {
                audioSourceMusicaFinal = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSourceMusicaFinal.playOnAwake = false;
        audioSourceMusicaFinal.loop = true;
        audioSourceMusicaFinal.spatialBlend = 0f;
        audioSourceMusicaFinal.Stop();
    }

    private void TocarMusicaFinal(AudioClip musica, float volume)
    {
        if (audioSourceMusicaFinal == null || musica == null)
            return;

        audioSourceMusicaFinal.Stop();
        audioSourceMusicaFinal.clip = musica;
        audioSourceMusicaFinal.volume = volume;
        audioSourceMusicaFinal.loop = true;
        audioSourceMusicaFinal.Play();
    }

    private void PararMusicaFinal()
    {
        if (audioSourceMusicaFinal == null)
            return;

        audioSourceMusicaFinal.Stop();
        audioSourceMusicaFinal.clip = null;
    }


    [Header("Estado")]
    public EtapaTutorial etapaAtual = EtapaTutorial.Introducao;

    // =====================================================
    // UI
    // =====================================================

    [Header("UI do Tutorial")]
    public GameObject painelDialogo;
    public TextMeshProUGUI textoDuke;
    [Header("Aparição do Duke")]
    public GameObject dukeVisual;
    public float duracaoAparicaoDuke = 0.5f;
    private SpriteRenderer[] spritesDuke;
    private Color[] coresOriginaisDuke;
    private bool dukeJaApareceu = false;

    public GameObject tituloFase;
    public TextMeshProUGUI textoTitulo;

    public GameObject objetivo;
    public TextMeshProUGUI textoObjetivo;

    // =====================================================
    // ANIMAÇÃO DO BALÃO
    // =====================================================

    [Header("Animação do Balão")]
    public RectTransform balãoAnimado;

    public float tempoEntradaBalao = 0.2f;
    public float tempoSaidaBalao = 0.15f;
    public float escalaPop = 1.08f;
    public float velocidadeFlutuacao = 2f;
    public float intensidadeFlutuacao = 2f;

    private Vector3 escalaOriginalBalao;
    private Vector2 posicaoOriginalBalao;
    private Coroutine rotinaAnimacaoBalao;

    // =====================================================
    // SOM DO DUKE
    // =====================================================

    [Header("Som do Duke")]
    public AudioSource audioSourceDuke;
    public AudioClip somLatidoDuke;
    public Animator animatorDuke;

    [Header("Músicas de Vitória e Derrota")]
    [SerializeField] private AudioSource audioSourceMusicaFinal;
    [SerializeField] private AudioClip musicaVitoria;
    [SerializeField] private AudioClip musicaDerrota;

    [Range(0f, 1f)]
    [SerializeField] private float volumeMusicaVitoria = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float volumeMusicaDerrota = 0.7f;

    // =====================================================
    // EFEITO DE TEXTO
    // =====================================================

    [Header("Efeito de Texto")]
    public float velocidadeTexto = 0.035f;
    public float pausaVirgula = 0.06f;
    public float pausaPontuacao = 0.15f;

    // =====================================================
    // PAINEL DE VITÓRIA
    // =====================================================

    [Header("Painel de Vitória")]
    public GameObject painelVitoria;
    public TextMeshProUGUI textoVitoria;

    // =====================================================
    // IMAGEM ANIMADA DA VITÓRIA
    // =====================================================

    [Header("Imagem Animada da Vitória")]
    [Tooltip("Imagem que desliza e ocupa a tela ao vencer.")]
    public RectTransform imagemVitoriaAnimada;

    [Tooltip("Distância inicial da imagem fora da tela, em unidades de UI.")]
    public float distanciaEntradaVitoria = 1920f;

    [Tooltip("Duração do deslizamento da imagem.")]
    public float duracaoEntradaVitoria = 0.9f;

    [Tooltip("Tempo que a imagem permanece na tela após a entrada.")]
    public float tempoImagemVitoria = 0.5f;

    private Vector2 posicaoFinalVitoria;
    private Vector2 posicaoInicialVitoria;
    private Coroutine rotinaVitoria;

    // =====================================================
    // CONFETES DA VITÓRIA
    // =====================================================

    [Header("Confetes da Vitória")]
    public ParticleSystem confetesVitoria1;
    public ParticleSystem confetesVitoria2;

    // =====================================================
    // SONS DA VITÓRIA
    // =====================================================

    [Header("Sons da Vitória")]
    public AudioSource audioSourceVitoria;

    [Tooltip("Som tocado enquanto a imagem de vitória entra.")]
    public AudioClip somEntradaVitoria;

    [Tooltip("Som tocado quando os confetes são ativados.")]
    public AudioClip somConfetesVitoria;

    [Range(0f, 1f)]
    public float volumeEntradaVitoria = 0.8f;

    [Range(0f, 1f)]
    public float volumeConfetesVitoria = 1f;

    // =====================================================
    // PAINEL DE DERROTA
    // =====================================================

    [Header("Painel de Derrota")]
    public GameObject painelDerrota;
    public TextMeshProUGUI textoDerrota;

    // =====================================================
    // TIMER
    // =====================================================

    [Header("Timer")]
    public TutorialTimer tutorialTimer;

    // =====================================================
    // CHEFE
    // =====================================================

    [Header("Chefe")]
    public ChefeCarrocinhaTutorial chefe;

    // =====================================================
    // CONFIGURAÇÃO
    // =====================================================

    [Header("Configuração")]
    public float tempoEntreFalase = 2f;
    public float tempoParaDica = 8f;
    public float tempoExibicaoFala = 3.5f;

    private Coroutine rotinaDica;
    private Coroutine rotinaFala;

    private bool tutorialIniciado = false;
    private bool chefeLiberado = false;
    private bool tutorialTerminou = false;
    private bool textoSendoEscrito = false;

    // =====================================================
    // FALAS DA INTRODUÇÃO
    // =====================================================

    [Header("Falas da Introdução")]
    [TextArea(2, 5)]
    public string[] falasIntroducao =
    {
        "<mark=#FFFFFFFF padding=\"5,5,3,3\">As coisas não estão boas para nós...</mark>",
        "<mark=#FFFFFFFF padding=\"5,5,3,3\">Precisamos fugir deste lugar!</mark>",
        "<mark=#FFFFFFFF padding=\"5,5,3,3\">Sandy, você conseguiu sair da sua gaiola. Ajude os outros a saírem também!</mark>",
        "<mark=#FFFFFFFF padding=\"5,5,3,3\">O chefe da carrocinha está no horário de intervalo agora. É a nossa chance!</mark>",
        "<mark=#FFFFFFFF padding=\"5,5,3,3\">Mas o intervalo não vai durar para sempre. Quando ele voltar, vai começar a ronda pelas gaiolas.</mark>",
        "<mark=#FFFFFFFF padding=\"5,5,3,3\">Precisamos libertar todos antes que ele termine a ronda!</mark>",
        "<mark=#FFFFFFFF padding=\"5,5,3,3\">Cuidado, Sandy! Se a carrocinha conseguir pegar você, você perderá 1 ponto de energia.</mark>",
        "<mark=#FFFFFFFF padding=\"5,5,3,3\">Você começa com 3 pontos de energia. Se sua energia chegar a zero, terá que recomeçar o tutorial.</mark>",
        "<mark=#FFFFFFFF padding=\"5,5,3,3\">Você precisa coletar as 8 chaves do molho. A última está com a carrocinha!</mark>"
    };

    // =====================================================
    // AWAKE
    // =====================================================

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    // =====================================================
    // START
    // =====================================================

    void Start()
    {
        etapaAtual = EtapaTutorial.Introducao;

        tutorialIniciado = false;
        chefeLiberado = false;
        tutorialTerminou = false;
        if (dukeVisual != null)
        {
            spritesDuke = dukeVisual
                .GetComponentsInChildren<SpriteRenderer>(true);

            coresOriginaisDuke = new Color[spritesDuke.Length];

            for (int i = 0; i < spritesDuke.Length; i++)
            {
                coresOriginaisDuke[i] = spritesDuke[i].color;
                spritesDuke[i].enabled = false;
            }
        }

        if (balãoAnimado != null)
        {
            escalaOriginalBalao = balãoAnimado.localScale;
            posicaoOriginalBalao = balãoAnimado.anchoredPosition;
            balãoAnimado.localScale = Vector3.zero;
        }

        if (objetivo != null)
            objetivo.SetActive(false);

        if (painelDialogo != null)
            painelDialogo.SetActive(false);

        if (tituloFase != null)
            tituloFase.SetActive(true);

        if (painelVitoria != null)
            painelVitoria.SetActive(false);

        if (painelDerrota != null)
            painelDerrota.SetActive(false);

        // A imagem de vitória começa escondida.
        if (imagemVitoriaAnimada != null)
        {
            posicaoFinalVitoria =
                imagemVitoriaAnimada.anchoredPosition;

            posicaoInicialVitoria =
                posicaoFinalVitoria
                + Vector2.left * distanciaEntradaVitoria;

            imagemVitoriaAnimada.anchoredPosition =
                posicaoInicialVitoria;

            imagemVitoriaAnimada.gameObject.SetActive(false);
        }

        // Os efeitos não aparecem antes da vitória.
        PararConfetes();

        if (textoTitulo != null)
            textoTitulo.text = "A FUGA";

        ConfigurarTextoDuke();
        ConfigurarMusicaFinal();

        StartCoroutine(IniciarTutorial());
    }

    // =====================================================
    // CONTROLE DOS CONFETES
    // =====================================================

    void PararConfetes()
    {
        if (confetesVitoria1 != null)
        {
            confetesVitoria1.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        if (confetesVitoria2 != null)
        {
            confetesVitoria2.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }

    void ReproduzirConfetes()
    {
        if (confetesVitoria1 != null)
        {
            confetesVitoria1.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            confetesVitoria1.Play(true);
        }

        if (confetesVitoria2 != null)
        {
            confetesVitoria2.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            confetesVitoria2.Play(true);
        }
    }

    // =====================================================
    // SOM DA VITÓRIA
    // =====================================================

    void TocarSomVitoria(AudioClip clip, float volume)
    {
        if (audioSourceVitoria != null && clip != null)
        {
            audioSourceVitoria.PlayOneShot(clip, volume);
        }
    }

    // =====================================================
    // CONFIGURAÇÃO DO TEXTO
    // =====================================================

    void ConfigurarTextoDuke()
    {
        if (textoDuke == null)
            return;

        textoDuke.geometrySortingOrder =
            VertexSortingOrder.Reverse;

        textoDuke.maxVisibleCharacters = int.MaxValue;
        textoDuke.ForceMeshUpdate();
    }

    // =====================================================
    // INÍCIO DO TUTORIAL
    // =====================================================

    IEnumerator IniciarTutorial()
    {
        yield return new WaitForSeconds(1f);

        if (tituloFase != null)
            tituloFase.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(MostrarDialogosIntroducao());

        if (tutorialTerminou)
            yield break;

        tutorialIniciado = true;

        ComecarCorrida();
    }

    // =====================================================
    // DIÁLOGOS DA INTRODUÇÃO
    // =====================================================

    IEnumerator MostrarDialogosIntroducao()
    {
        for (int i = 0; i < falasIntroducao.Length; i++)
        {
            if (tutorialTerminou)
                yield break;

            yield return StartCoroutine(
                MostrarFalaEEsperar(falasIntroducao[i])
            );

            yield return new WaitForSeconds(tempoEntreFalase);
        }

        EsconderDialogo();
    }

    // =====================================================
    // MOSTRAR FALA E ESPERAR
    // =====================================================

    IEnumerator MostrarFalaEEsperar(string mensagem)
    {
        yield return StartCoroutine(MostrarFalaAnimada(mensagem));

        yield return new WaitForSeconds(tempoExibicaoFala);
    }

    // =====================================================
    // SISTEMA DE FALAS
    // =====================================================

    public void MostrarFala(string mensagem)
    {
        if (painelDialogo != null)
            painelDialogo.SetActive(true);

        if (textoDuke != null)
        {
            textoDuke.text = mensagem;
            textoDuke.geometrySortingOrder = VertexSortingOrder.Reverse;
            textoDuke.ForceMeshUpdate();
            textoDuke.maxVisibleCharacters = int.MaxValue;
        }

        if (balãoAnimado != null)
        {
            if (rotinaAnimacaoBalao != null)
                StopCoroutine(rotinaAnimacaoBalao);

            rotinaAnimacaoBalao = StartCoroutine(AnimarEntradaBalao());
        }

        TocarLatidoDuke();
    }

    // =====================================================
    // FALA ANIMADA
    // =====================================================

    IEnumerator MostrarFalaAnimada(string mensagem)
    {
        if (painelDialogo != null)
            painelDialogo.SetActive(true);

        if (textoDuke == null)
            yield break;

        textoDuke.text = mensagem;
        textoDuke.geometrySortingOrder = VertexSortingOrder.Reverse;
        textoDuke.ForceMeshUpdate();

        int quantidadeCaracteres = textoDuke.textInfo.characterCount;

        textoDuke.maxVisibleCharacters = 0;
        textoSendoEscrito = true;

        if (rotinaAnimacaoBalao != null)
            StopCoroutine(rotinaAnimacaoBalao);

        rotinaAnimacaoBalao = StartCoroutine(AnimarEntradaBalao());

        if (!dukeJaApareceu)
        {
            yield return StartCoroutine(AnimarAparicaoDuke());
        }

        TocarLatidoDuke();

        for (int i = 0; i <= quantidadeCaracteres; i++)
        {
            if (tutorialTerminou)
                yield break;

            textoDuke.maxVisibleCharacters = i;

            if (i < quantidadeCaracteres)
            {
                char caractere = '\0';

                if (i < mensagem.Length)
                    caractere = mensagem[i];

                if (caractere == '.' ||
                    caractere == '!' ||
                    caractere == '?')
                {
                    yield return new WaitForSeconds(pausaPontuacao);
                }
                else if (caractere == ',')
                {
                    yield return new WaitForSeconds(pausaVirgula);
                }
                else
                {
                    yield return new WaitForSeconds(velocidadeTexto);
                }
            }
        }

        textoDuke.geometrySortingOrder = VertexSortingOrder.Reverse;
        textoDuke.maxVisibleCharacters = int.MaxValue;
        textoSendoEscrito = false;
    }

    // =====================================================
    // SOM DO DUKE
    // =====================================================

    void TocarLatidoDuke()
    {
        Debug.Log("Tentando ativar o latido do Duke!");

        if (animatorDuke == null)
        {
            Debug.LogError("Animator do Duke está NULL!");
            return;
        }

        Debug.Log("Animator encontrado: " + animatorDuke.gameObject.name);

        animatorDuke.ResetTrigger("Latir");
        animatorDuke.SetTrigger("Latir");

        Debug.Log("Trigger Latir enviado!");

        if (audioSourceDuke != null && somLatidoDuke != null)
        {
            audioSourceDuke.PlayOneShot(somLatidoDuke);
        }
    }

    // =====================================================
    // ENTRADA DO BALÃO
    // =====================================================

    IEnumerator AnimarEntradaBalao()
    {
        if (balãoAnimado == null)
            yield break;

        Vector3 inicio = Vector3.zero;
        Vector3 meio = escalaOriginalBalao * escalaPop;
        Vector3 fim = escalaOriginalBalao;

        float tempo = 0f;

        while (tempo < tempoEntradaBalao)
        {
            tempo += Time.deltaTime;

            float progresso = tempoEntradaBalao > 0f
                ? Mathf.Clamp01(tempo / tempoEntradaBalao)
                : 1f;

            balãoAnimado.localScale =
                Vector3.Lerp(inicio, meio, progresso);

            yield return null;
        }

        tempo = 0f;

        float duracaoPop = tempoEntradaBalao * 0.4f;

        while (tempo < duracaoPop)
        {
            tempo += Time.deltaTime;

            float progresso = duracaoPop > 0f
                ? Mathf.Clamp01(tempo / duracaoPop)
                : 1f;

            balãoAnimado.localScale =
                Vector3.Lerp(meio, fim, progresso);

            yield return null;
        }

        balãoAnimado.localScale = escalaOriginalBalao;
        rotinaAnimacaoBalao = null;
    }

    // =====================================================
    // FLUTUAÇÃO DO BALÃO
    // =====================================================

    void Update()
    {
        if (balãoAnimado != null &&
            painelDialogo != null &&
            painelDialogo.activeSelf)
        {
            float movimento =
                Mathf.Sin(Time.time * velocidadeFlutuacao) *
                intensidadeFlutuacao;

            balãoAnimado.anchoredPosition =
                posicaoOriginalBalao + Vector2.up * movimento;
        }
    }

    // =====================================================
    // ESCONDER BALÃO
    // =====================================================

    void EsconderDialogo()
    {
        if (painelDialogo != null)
            painelDialogo.SetActive(false);

        if (balãoAnimado != null)
        {
            balãoAnimado.localScale = escalaOriginalBalao;
            balãoAnimado.anchoredPosition = posicaoOriginalBalao;
        }

        if (textoDuke != null)
        {
            textoDuke.geometrySortingOrder = VertexSortingOrder.Reverse;
            textoDuke.maxVisibleCharacters = int.MaxValue;
        }
    }

    // =====================================================
    // FALA TEMPORÁRIA
    // =====================================================

    public void MostrarFalaTemporaria(string mensagem)
    {
        if (tutorialTerminou)
            return;

        if (rotinaFala != null)
            StopCoroutine(rotinaFala);

        rotinaFala = StartCoroutine(FalaTemporaria(mensagem));
    }

    IEnumerator FalaTemporaria(string mensagem)
    {
        yield return StartCoroutine(MostrarFalaAnimada(mensagem));

        if (tutorialTerminou)
            yield break;

        yield return new WaitForSeconds(tempoExibicaoFala);

        EsconderDialogo();

        rotinaFala = null;
    }

    // =====================================================
    // SISTEMA DE DICAS
    // =====================================================

    void IniciarDica(string mensagem)
    {
        PararDica();

        rotinaDica = StartCoroutine(AguardarDica(mensagem));
    }

    IEnumerator AguardarDica(string mensagem)
    {
        yield return new WaitForSeconds(tempoParaDica);

        if (etapaAtual == EtapaTutorial.Finalizado ||
            tutorialTerminou)
        {
            yield break;
        }

        MostrarFalaTemporaria(mensagem);

        rotinaDica = null;
    }

    void PararDica()
    {
        if (rotinaDica != null)
        {
            StopCoroutine(rotinaDica);
            rotinaDica = null;
        }
    }

    // =====================================================
    // CORRIDA
    // =====================================================

    void ComecarCorrida()
    {
        if (tutorialTerminou)
            return;

        etapaAtual = EtapaTutorial.Corrida;

        MostrarObjetivo("Use as setas direcionais para andar!");

        MostrarFalaTemporaria(
            "Sandy, vamos começar! Use as setas direcionais para andar."
        );

        IniciarDica(
            "Sandy, tente usar as setas direcionais para se movimentar!"
        );
    }

    public void RegistrarCorrida()
    {
        if (tutorialTerminou ||
            etapaAtual != EtapaTutorial.Corrida)
            return;

        PararDica();

        Debug.Log("Corrida concluída!");

        MostrarFalaTemporaria(
            "Muito bem, Sandy! Você já sabe andar. Agora vamos aprender a pular!"
        );

        ComecarPulo();
    }

    // =====================================================
    // PULO
    // =====================================================

    void ComecarPulo()
    {
        if (tutorialTerminou)
            return;

        etapaAtual = EtapaTutorial.Pulo;

        MostrarObjetivo("Use ESPAÇO para pular e fugir dos golpes!");

        MostrarFalaTemporaria(
            "Agora vamos treinar o pulo! Aperte ESPAÇO para pular."
        );

        IniciarDica(
            "Sandy, aperte ESPAÇO para pular! Você também pode usar o pulo duplo."
        );
    }

    public void RegistrarPulo()
    {
        if (tutorialTerminou ||
            etapaAtual != EtapaTutorial.Pulo)
            return;

        PararDica();

        Debug.Log("Pulo concluído!");

        MostrarFalaTemporaria(
            "Muito bem! O pulo vai ajudar você a escapar dos obstáculos e dos golpes!"
        );

        ComecarLatido();
    }

    // =====================================================
    // LATIDO
    // =====================================================

    void ComecarLatido()
    {
        if (tutorialTerminou)
            return;

        etapaAtual = EtapaTutorial.Latido;

        MostrarObjetivo("Use Z para latir e assustar o chefe!");

        MostrarFalaTemporaria(
            "Agora vamos aprender a latir! Aperte Z para assustar a carrocinha."
        );

        IniciarDica(
            "Sandy, use Z para latir! Seu latido pode ajudar a afastar o perigo."
        );
    }

    public void RegistrarLatido()
    {
        if (tutorialTerminou ||
            etapaAtual != EtapaTutorial.Latido)
            return;

        PararDica();

        Debug.Log("Latido concluído!");

        MostrarFalaTemporaria(
            "Isso aí, Sandy! Agora você está pronta para ajudar os outros cães!"
        );

        ComecarColeta();
    }

    // =====================================================
    // COLETA / INÍCIO DO INTERVALO
    // =====================================================

    void ComecarColeta()
    {
        if (tutorialTerminou)
            return;

        etapaAtual = EtapaTutorial.Coleta;

        MostrarObjetivo("Colete as 8 chaves e liberte os cães!");

        MostrarFalaTemporaria(
            "O intervalo não vai durar para sempre! Colete as 8 chaves do molho e liberte os cães. A última está com a carrocinha."
        );

        IniciarDica(
            "Sandy, procure as chaves das gaiolas! Precisamos libertar todos antes que o chefe volte."
        );

        Debug.Log("Tutorial das mecânicas concluído!");

        if (tutorialTimer != null)
        {
            tutorialTimer.IniciarTimer();
        }
        else
        {
            Debug.LogWarning(
                "TutorialTimer não foi configurado no Inspector!"
            );
        }
    }

    // =====================================================
    // AVISO DA ÚLTIMA CHAVE
    // =====================================================

    public void MostrarAvisoUltimaChave()
    {
        if (tutorialTerminou)
            return;

        PararDica();

        MostrarFalaTemporaria(
            "Muito bem! Você libertou os oito cães! Agora falta apenas uma chave. A carrocinha está com ela. Assuste o chefe para conseguir a última chave!"
        );

        MostrarObjetivo(
            "Assuste a carrocinha e pegue a última chave!"
        );

        Debug.Log(
            "Objetivo atualizado: conseguir a última chave com a carrocinha."
        );
    }

    // =====================================================
    // ALTERAR OBJETIVO EXTERNAMENTE
    // =====================================================

    public void MostrarObjetivoExterno(string mensagem)
    {
        if (tutorialTerminou)
            return;

        MostrarObjetivo(mensagem);
    }

    // =====================================================
    // INTERVALO TERMINOU
    // =====================================================

    public void IntervaloTerminou()
    {
        if (chefeLiberado || tutorialTerminou)
            return;

        chefeLiberado = true;

        PararDica();

        Debug.Log("O CHEFE VOLTOU!");

        MostrarFalaTemporaria(
            "Sandy! O chefe voltou! Cuidado: se ele pegar você, você perderá 1 ponto de energia."
        );

        MostrarObjetivo(
            "O chefe voltou! Liberte os cães antes que seja tarde!"
        );

        if (chefe != null)
        {
            chefe.LiberarChefe();
        }
        else
        {
            Debug.LogWarning(
                "O Chefe Carrocinha não foi configurado no TutorialManager!"
            );
        }
    }

    // =====================================================
    // VERIFICAR VITÓRIA
    // =====================================================

    public void VerificarVitoria()
    {
        if (tutorialTerminou)
            return;

        if (TutorialChavesManager.instance == null)
        {
            Debug.LogWarning(
                "TutorialChavesManager não foi encontrado."
            );

            return;
        }

        int gaiolasAbertas =
            TutorialChavesManager.instance.GaiolasAbertas;

        if (gaiolasAbertas >= 9)
            Vitoria();
    }

    // =====================================================
    // VITÓRIA
    // =====================================================


    public void Vitoria()
    {
        if (tutorialTerminou)
            return;

        tutorialTerminou = true;
        etapaAtual = EtapaTutorial.Finalizado;

        PararMusicaFinal();
        TocarMusicaFinal(musicaVitoria, volumeMusicaVitoria);

        PararDica();

        if (rotinaFala != null)
        {
            StopCoroutine(rotinaFala);
            rotinaFala = null;
        }

        if (rotinaAnimacaoBalao != null)
        {
            StopCoroutine(rotinaAnimacaoBalao);
            rotinaAnimacaoBalao = null;
        }

        textoSendoEscrito = false;

        if (tutorialTimer != null)
            tutorialTimer.PararTimer();

        if (chefe != null)
            chefe.DesativarChefe();

        if (objetivo != null)
            objetivo.SetActive(false);

        if (painelDialogo != null)
            painelDialogo.SetActive(false);

        if (tituloFase != null)
            tituloFase.SetActive(false);

        // Não mostrar painel ou texto de vitória.
        if (painelVitoria != null)
            painelVitoria.SetActive(false);

        if (painelDerrota != null)
            painelDerrota.SetActive(false);

        // Esconder a imagem até a animação começar.
        if (imagemVitoriaAnimada != null)
            imagemVitoriaAnimada.gameObject.SetActive(false);
        else
            Debug.LogError(
                "ERRO: imagemVitoriaAnimada não foi configurada no Inspector!"
            );

        PararConfetes();

        if (rotinaVitoria != null)
            StopCoroutine(rotinaVitoria);

        rotinaVitoria = StartCoroutine(AnimarSequenciaVitoria());

        Debug.Log("VITÓRIA: iniciando animação da imagem.");
    }


    // =====================================================
    // ANIMAÇÃO DA SEQUÊNCIA DE VITÓRIA
    // =====================================================


    IEnumerator AnimarSequenciaVitoria()
    {
        Debug.Log("VITÓRIA: coroutine da imagem iniciada.");

        if (imagemVitoriaAnimada == null)
        {
            Debug.LogError(
                "VITÓRIA: o campo Imagem Vitoria Animada está vazio no Inspector!"
            );
            yield break;
        }

        GameObject objetoImagem = imagemVitoriaAnimada.gameObject;

        // Ativa a imagem.
        objetoImagem.SetActive(true);

        // Coloca a imagem acima dos elementos irmãos.
        imagemVitoriaAnimada.SetAsLastSibling();

        // Verifica se a imagem possui um Canvas ativo.
        Canvas canvas = objetoImagem.GetComponentInParent<Canvas>();

        Debug.Log(
            "VITÓRIA: imagem ativa = " + objetoImagem.activeSelf +
            " | ativa na hierarquia = " + objetoImagem.activeInHierarchy
        );

        Debug.Log(
            "VITÓRIA: posição = " + imagemVitoriaAnimada.anchoredPosition +
            " | tamanho = " + imagemVitoriaAnimada.rect.size
        );

        if (canvas == null)
        {
            Debug.LogError(
                "VITÓRIA: a imagem não está dentro de nenhum Canvas!"
            );
        }
        else
        {
            Debug.Log(
                "VITÓRIA: Canvas encontrado: " + canvas.name +
                " | ativo = " + canvas.isActiveAndEnabled
            );
        }

        if (!objetoImagem.activeInHierarchy)
        {
            Debug.LogError(
                "VITÓRIA: a imagem está ativa, mas algum objeto pai está desativado!"
            );
            yield break;
        }

        // Posiciona a imagem fora da tela, à esquerda.
        imagemVitoriaAnimada.anchoredPosition = posicaoInicialVitoria;

        TocarSomVitoria(
            somEntradaVitoria,
            volumeEntradaVitoria
        );

        // Anima a entrada.
        float duracao = Mathf.Max(0.01f, duracaoEntradaVitoria);
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = Mathf.Clamp01(tempo / duracao);
            float suavizado = Mathf.SmoothStep(0f, 1f, progresso);

            imagemVitoriaAnimada.anchoredPosition = Vector2.Lerp(
                posicaoInicialVitoria,
                posicaoFinalVitoria,
                suavizado
            );

            yield return null;
        }

        imagemVitoriaAnimada.anchoredPosition = posicaoFinalVitoria;

        Debug.Log("VITÓRIA: imagem chegou à posição final.");

        if (tempoImagemVitoria > 0f)
            yield return new WaitForSecondsRealtime(tempoImagemVitoria);

        ReproduzirConfetes();

        TocarSomVitoria(
            somConfetesVitoria,
            volumeConfetesVitoria
        );

        rotinaVitoria = null;
    }


    // =====================================================
    // DERROTA
    // =====================================================

    public void Derrota()
    {
        if (tutorialTerminou)
            return;

        tutorialTerminou = true;
        etapaAtual = EtapaTutorial.Finalizado;
        PararMusicaFinal();
        TocarMusicaFinal(musicaDerrota, volumeMusicaDerrota);

        PararDica();

        if (rotinaFala != null)
        {
            StopCoroutine(rotinaFala);
            rotinaFala = null;
        }

        if (rotinaAnimacaoBalao != null)
        {
            StopCoroutine(rotinaAnimacaoBalao);
            rotinaAnimacaoBalao = null;
        }

        if (tutorialTimer != null)
            tutorialTimer.PararTimer();

        if (chefe != null)
            chefe.DesativarChefe();

        if (objetivo != null)
            objetivo.SetActive(false);

        if (painelDialogo != null)
            painelDialogo.SetActive(false);

        // Cancela uma eventual sequência de vitória.
        if (rotinaVitoria != null)
        {
            StopCoroutine(rotinaVitoria);
            rotinaVitoria = null;
        }

        if (imagemVitoriaAnimada != null)
            imagemVitoriaAnimada.gameObject.SetActive(false);

        if (painelDerrota != null)
            painelDerrota.SetActive(true);

        PararConfetes();

        if (textoDerrota != null)
        {
            textoDerrota.text =
                "A FUGA FALHOU\n" +
                "Sandy ficou sem energia.";
        }

        Debug.Log("DERROTA! Sandy ficou sem energia.");
    }

    // =====================================================
    // REINICIAR TUTORIAL
    // =====================================================

    public void ReiniciarTutorial()
    {
        Debug.Log("Reiniciando o tutorial...");

        Time.timeScale = 1f;
        PararMusicaFinal();

        Scene cenaAtual = SceneManager.GetActiveScene();

        SceneManager.LoadScene(cenaAtual.name);
    }

    // =====================================================
    // FINALIZAR TUTORIAL
    // =====================================================

    public void FinalizarTutorial()
    {
        if (tutorialTerminou)
            return;

        etapaAtual = EtapaTutorial.Finalizado;

        PararDica();

        if (rotinaFala != null)
        {
            StopCoroutine(rotinaFala);
            rotinaFala = null;
        }

        if (objetivo != null)
            objetivo.SetActive(false);

        if (painelDialogo != null)
            painelDialogo.SetActive(true);

        if (textoDuke != null)
        {
            textoDuke.geometrySortingOrder =
                VertexSortingOrder.Reverse;

            textoDuke.text =
                "Conseguimos! Vamos libertar todos!";

            textoDuke.ForceMeshUpdate();
            textoDuke.maxVisibleCharacters = int.MaxValue;
        }

        Debug.Log("Tutorial concluído!");
    }

    // =====================================================
    // MOSTRAR OBJETIVO
    // =====================================================

    void MostrarObjetivo(string mensagem)
    {
        if (objetivo != null)
            objetivo.SetActive(true);

        if (textoObjetivo != null)
            textoObjetivo.text = mensagem;
    }

    // =====================================================
    // VERIFICAR ETAPA
    // =====================================================

    public bool EstaNaEtapa(EtapaTutorial etapa)
    {
        return etapaAtual == etapa;
    }

    // =====================================================
    // LIMPEZA
    // =====================================================

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    IEnumerator AnimarAparicaoDuke()
    {
        if (spritesDuke == null || spritesDuke.Length == 0)
            yield break;

        dukeJaApareceu = true;

        // Torna os sprites visíveis, inicialmente transparentes.
        for (int i = 0; i < spritesDuke.Length; i++)
        {
            if (spritesDuke[i] == null)
                continue;

            spritesDuke[i].enabled = true;

            Color cor = coresOriginaisDuke[i];
            cor.a = 0f;
            spritesDuke[i].color = cor;
        }

        float duracao = Mathf.Max(0.01f, duracaoAparicaoDuke);
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempo / duracao);

            for (int i = 0; i < spritesDuke.Length; i++)
            {
                if (spritesDuke[i] == null)
                    continue;

                Color cor = coresOriginaisDuke[i];
                cor.a = Mathf.Lerp(0f, coresOriginaisDuke[i].a, progresso);
                spritesDuke[i].color = cor;
            }

            yield return null;
        }

        // Garante que as cores originais sejam restauradas.
        for (int i = 0; i < spritesDuke.Length; i++)
        {
            if (spritesDuke[i] != null)
                spritesDuke[i].color = coresOriginaisDuke[i];
        }
    }
}
