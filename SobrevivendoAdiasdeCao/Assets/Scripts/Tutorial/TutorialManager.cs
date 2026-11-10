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

    [Header("Estado")]
    public EtapaTutorial etapaAtual = EtapaTutorial.Introducao;

    // =====================================================
    // UI
    // =====================================================

    [Header("UI do Tutorial")]
    public GameObject painelDialogo;
    public TextMeshProUGUI textoDuke;

    [Header("Visual do Duke")]
    public DukeVisualController dukeVisualController;

    [Header("Identificação do Duke")]
    public TextMeshProUGUI textoIdentificacaoDuke;

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
    // EFEITO DE TEXTO
    // =====================================================

    [Header("Efeito de Texto")]
    public float velocidadeTexto = 0.035f;
    public float pausaVirgula = 0.06f;
    public float pausaPontuacao = 0.15f;

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
        "<mark=#FFFFFFFF padding=\"5,5,3,3\">Você precisa coletar as 8 chaves do molho, uma de cada vez. A última está com a carrocinha!</mark>"
    };

    // Variável estática (não se perde ao recarregar a cena)
    public static bool ehReinicioRapido = false;

    void Start()
    {
        if (textoIdentificacaoDuke != null)
        {
            textoIdentificacaoDuke.text = "DUKYE - LÍDER DA REVOLTA CANINA";
        }

        etapaAtual = EtapaTutorial.Introducao;

        tutorialIniciado = false;
        chefeLiberado = false;
        tutorialTerminou = false;

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

        if (textoTitulo != null)
            textoTitulo.text = "A FUGA";

        ConfigurarTextoDuke();

        // VERIFICA SE É UM REINÍCIO RÁPIDO
        if (ehReinicioRapido)
        {
            StartCoroutine(IniciarTutorialRapido());
        }
        else
        {
            StartCoroutine(IniciarTutorial());
        }
    }

    // Corrotina para quando o jogador clica em "Reiniciar"
    IEnumerator IniciarTutorialRapido()
    {
        yield return new WaitForSeconds(0.5f);

        if (tituloFase != null)
            tituloFase.SetActive(false);

        tutorialIniciado = true;

        // Vai DIRETO para o início da corrida com a dica simples!
        ComecarCorrida();
    }

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
    // CONFIGURAÇÃO DO TEXTO
    // =====================================================

    void ConfigurarTextoDuke()
    {
        if (textoDuke == null)
            return;

        textoDuke.geometrySortingOrder = VertexSortingOrder.Reverse;
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

            yield return StartCoroutine(MostrarFalaEEsperar(falasIntroducao[i]));

            yield return new WaitForSeconds(tempoEntreFalase);
        }

        yield return StartCoroutine(EsconderDialogoAnimado());
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

        if (dukeVisualController != null)
            dukeVisualController.TocarLatido();
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

        if (dukeVisualController != null)
        {
            dukeVisualController.GarantaAparicao(this);
            dukeVisualController.TocarLatido();
        }

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

                if (caractere == '.' || caractere == '!' || caractere == '?')
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
    // MOSTRAR BALÃO
    // =====================================================

    void MostrarBalao()
    {
        if (painelDialogo != null)
            painelDialogo.SetActive(true);

        if (balãoAnimado == null)
            return;

        if (rotinaAnimacaoBalao != null)
            StopCoroutine(rotinaAnimacaoBalao);

        balãoAnimado.anchoredPosition = posicaoOriginalBalao;
        balãoAnimado.localScale = Vector3.zero;

        rotinaAnimacaoBalao = StartCoroutine(AnimarEntradaBalao());
    }

    // =====================================================
    // ANIMAÇÃO DE ENTRADA
    // =====================================================

    IEnumerator AnimarEntradaBalao()
    {
        if (balãoAnimado == null)
            yield break;

        Vector3 escondido = Vector3.zero;
        Vector3 aumentado = escalaOriginalBalao * escalaPop;
        Vector3 normal = escalaOriginalBalao;

        float tempo = 0f;

        while (tempo < tempoEntradaBalao)
        {
            tempo += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempo / tempoEntradaBalao);
            progresso = Mathf.SmoothStep(0f, 1f, progresso);

            balãoAnimado.localScale = Vector3.Lerp(escondido, aumentado, progresso);
            yield return null;
        }

        tempo = 0f;
        float duracaoPop = tempoEntradaBalao * 0.35f;

        while (tempo < duracaoPop)
        {
            tempo += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempo / duracaoPop);

            balãoAnimado.localScale = Vector3.Lerp(aumentado, normal, Mathf.SmoothStep(0f, 1f, progresso));
            yield return null;
        }

        balãoAnimado.localScale = escalaOriginalBalao;
        rotinaAnimacaoBalao = null;
    }

    // =====================================================
    // ANIMAÇÃO DE SAÍDA
    // =====================================================

    IEnumerator AnimarSaidaBalao()
    {
        if (balãoAnimado == null)
        {
            if (painelDialogo != null)
                painelDialogo.SetActive(false);

            yield break;
        }

        Vector3 escalaInicial = balãoAnimado.localScale;
        float tempo = 0f;

        while (tempo < tempoSaidaBalao)
        {
            tempo += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempo / tempoSaidaBalao);
            progresso = Mathf.SmoothStep(0f, 1f, progresso);

            balãoAnimado.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, progresso);
            yield return null;
        }

        balãoAnimado.localScale = Vector3.zero;
        balãoAnimado.anchoredPosition = posicaoOriginalBalao;

        if (painelDialogo != null)
            painelDialogo.SetActive(false);

        rotinaAnimacaoBalao = null;
    }

    // =====================================================
    // ESCONDER BALÃO
    // =====================================================

    IEnumerator EsconderDialogoAnimado()
    {
        if (rotinaAnimacaoBalao != null)
            StopCoroutine(rotinaAnimacaoBalao);

        rotinaAnimacaoBalao = StartCoroutine(AnimarSaidaBalao());
        yield return rotinaAnimacaoBalao;
    }

    // =====================================================
    // FLUTUAÇÃO DO BALÃO
    // =====================================================

    void Update()
    {
        if (balãoAnimado != null && painelDialogo != null && painelDialogo.activeSelf)
        {
            float movimento = Mathf.Sin(Time.time * velocidadeFlutuacao) * intensidadeFlutuacao;
            balãoAnimado.anchoredPosition = posicaoOriginalBalao + Vector2.up * movimento;
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
        yield return StartCoroutine(EsconderDialogoAnimado());

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

        if (etapaAtual == EtapaTutorial.Finalizado || tutorialTerminou)
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

        MostrarFalaTemporaria("Sandy, vamos começar! Use as setas direcionais para andar.");
        IniciarDica("Sandy, tente usar as setas direcionais para se movimentar!");
    }

    public void RegistrarCorrida()
    {
        if (tutorialTerminou || etapaAtual != EtapaTutorial.Corrida)
            return;

        PararDica();
        Debug.Log("Corrida concluída!");

        MostrarFalaTemporaria("Muito bem, Sandy! Você já sabe andar. Agora vamos aprender a pular!");
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

        MostrarFalaTemporaria("Agora vamos treinar o pulo! Aperte ESPAÇO para pular.");
        IniciarDica("Sandy, aperte ESPAÇO para pular! Você também pode usar o pulo duplo.");
    }

    public void RegistrarPulo()
    {
        if (tutorialTerminou || etapaAtual != EtapaTutorial.Pulo)
            return;

        PararDica();
        Debug.Log("Pulo concluído!");

        MostrarFalaTemporaria("Muito bem! O pulo vai ajudar você a escapar dos obstáculos e dos golpes!");
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

        MostrarFalaTemporaria("Agora vamos aprender a latir! Aperte Z para assustar a carrocinha.");
        IniciarDica("Sandy, use Z para latir! Seu latido pode ajudar a afastar o perigo.");
    }

    public void RegistrarLatido()
    {
        if (tutorialTerminou || etapaAtual != EtapaTutorial.Latido)
            return;

        PararDica();
        Debug.Log("Latido concluído!");

        // Adiciona o aviso sobre a tecla P junto com a conclusão das mecânicas básicas
        MostrarFalaTemporaria(
            "Isso aí, Sandy! Se precisar de uma pausa a qualquer momento, aperte P para pausar o jogo. Agora você está pronta para libertar os cães!"
        );

        ComecarColeta();
    }

    // =====================================================
    // ETAPA DE COLETA E LIBERTAÇÃO (PASSO A PASSO DIRETO)
    // =====================================================

    private int avisosChaveColetada = 0;
    private int avisosGaiolaAberta = 0;

    void ComecarColeta()
    {
        if (tutorialTerminou)
            return;

        etapaAtual = EtapaTutorial.Coleta;

        avisosChaveColetada = 0;
        avisosGaiolaAberta = 0;

        MostrarObjetivo("Vá até o molho de chaves e aperte Z para derrubar a chave!");
        MostrarFalaTemporaria("Sandy, veja aquele molho de chaves na parede! Chegue perto dele e use seu latido (tecla Z) para fazer a chave cair!");
        IniciarDica("Aproxime-se do molho de chaves na parede e aperte Z para latir!");

        Debug.Log("Tutorial de mecânicas concluído. Iniciando fase de libertação!");

        if (tutorialTimer != null)
        {
            tutorialTimer.IniciarTimer();
        }
        else
        {
            Debug.LogWarning("TutorialTimer não foi configurado no Inspector!");
        }
    }

    public void NotificarChaveColetada()
    {
        if (tutorialTerminou) return;

        PararDica();
        avisosChaveColetada++;

        if (avisosChaveColetada <= 2)
        {
            MostrarObjetivo("Vá até uma gaiola e aperte o BOTÃO DIREITO DO MOUSE para abri-la!");
            MostrarFalaTemporaria("Boa! A chave já está com você! Agora vá até uma das gaiolas trancadas e use o BOTÃO DIREITO DO MOUSE para libertar seu amigo!");
            IniciarDica("Chegue perto de uma gaiola trancada e clique com o BOTÃO DIREITO DO MOUSE para abrir.");
        }
    }

    public void NotificarGaiolaAberta()
    {
        if (tutorialTerminou) return;

        PararDica();
        avisosGaiolaAberta++;

        int abertas = TutorialChavesManager.instance != null ? TutorialChavesManager.instance.GaiolasAbertas : 0;

        if (abertas < 8)
        {
            MostrarObjetivo($"Liberte os cães! ({abertas}/8 gaiolas abertas)");
        }

        if (avisosGaiolaAberta <= 2 && abertas < 8)
        {
            MostrarFalaTemporaria("Perfeito! Cão libertado! Lembre-se: você só pode carregar UMA chave por vez. Volte ao molho, lata com Z para pegar outra e use o BOTÃO DIREITO DO MOUSE na próxima gaiola!");
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
        MostrarFalaTemporaria("Muito bem! Você libertou os oito cães! Agora falta apenas uma chave. A carrocinha está com ela. Assuste o chefe para conseguir a última chave!");
        MostrarObjetivo("Assuste a carrocinha e pegue a última chave!");

        Debug.Log("Objetivo atualizado: conseguir a última chave com a carrocinha.");
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

        MostrarFalaTemporaria("Sandy! O chefe voltou! Volte para sua gaiola! Se ele pegar você, você perderá 1 ponto de energia.");
        MostrarObjetivo("O chefe voltou! Volte para sua gaiola!");

        if (chefe != null)
        {
            chefe.LiberarChefe();
        }
        else
        {
            Debug.LogWarning("O Chefe Carrocinha não foi configurado no TutorialManager!");
        }
    }

    // =====================================================
    // VERIFICAR VITÓRIA & DERROTA (INTEGRADO COM ENDGAME)
    // =====================================================

    public void VerificarVitoria()
    {
        if (tutorialTerminou)
            return;

        if (TutorialChavesManager.instance == null)
            return;

        int gaiolasAbertas = TutorialChavesManager.instance.GaiolasAbertas;

        if (gaiolasAbertas >= 9)
        {
            Vitoria();
        }
    }

    public void Vitoria()
    {
        if (tutorialTerminou)
            return;

        tutorialTerminou = true;
        etapaAtual = EtapaTutorial.Finalizado;

        PararDica();

        if (rotinaFala != null) { StopCoroutine(rotinaFala); rotinaFala = null; }
        if (rotinaAnimacaoBalao != null) { StopCoroutine(rotinaAnimacaoBalao); rotinaAnimacaoBalao = null; }

        textoSendoEscrito = false;

        if (tutorialTimer != null) tutorialTimer.PararTimer();
        if (chefe != null) chefe.DesativarChefe();

        if (objetivo != null) objetivo.SetActive(false);
        if (painelDialogo != null) painelDialogo.SetActive(false);
        if (tituloFase != null) tituloFase.SetActive(false);

        if (TutorialEndGameManager.instance != null)
        {
            TutorialEndGameManager.instance.ExecutarVitoria();
        }
    }

    public void Derrota()
    {
        if (tutorialTerminou)
            return;

        tutorialTerminou = true;
        etapaAtual = EtapaTutorial.Finalizado;

        PararDica();

        if (rotinaFala != null) { StopCoroutine(rotinaFala); rotinaFala = null; }
        if (rotinaAnimacaoBalao != null) { StopCoroutine(rotinaAnimacaoBalao); rotinaAnimacaoBalao = null; }

        if (tutorialTimer != null) tutorialTimer.PararTimer();
        if (chefe != null) chefe.DesativarChefe();

        if (objetivo != null) objetivo.SetActive(false);
        if (painelDialogo != null) painelDialogo.SetActive(false);

        if (TutorialEndGameManager.instance != null)
        {
            TutorialEndGameManager.instance.ExecutarDerrota();
        }
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

        MostrarBalao();

        if (textoDuke != null)
        {
            textoDuke.geometrySortingOrder = VertexSortingOrder.Reverse;
            textoDuke.text = "Conseguimos! Vamos libertar todos!";
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
}