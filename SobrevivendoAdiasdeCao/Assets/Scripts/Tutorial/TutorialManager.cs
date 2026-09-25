using UnityEngine;
using TMPro;
using System.Collections;

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

    public GameObject tituloFase;
    public TextMeshProUGUI textoTitulo;

    public GameObject objetivo;
    public TextMeshProUGUI textoObjetivo;

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

    [Tooltip("Tempo que Duke espera antes de dar uma dica.")]
    public float tempoParaDica = 8f;

    [Tooltip("Tempo que cada fala fica visível.")]
    public float tempoExibicaoFala = 3.5f;

    private Coroutine rotinaDica;
    private Coroutine rotinaFala;

    private bool tutorialIniciado = false;
    private bool chefeLiberado = false;

    // =====================================================
    // FALAS DA INTRODUÇÃO
    // =====================================================

    [Header("Falas da Introdução")]

    [TextArea(2, 5)]
    public string[] falasIntroducao =
    {
        "As coisas não estão boas para nós...",

        "Precisamos fugir deste lugar!",

        "Sandy, você conseguiu sair da sua gaiola. Ajude os outros a saírem também!",

        "O chefe da carrocinha está no horário de intervalo agora. É a nossa chance!",

        "Mas o intervalo não vai durar para sempre. Quando ele voltar, vai começar a ronda pelas gaiolas.",

        "Precisamos libertar todos antes que ele termine a ronda!",

        "Cuidado, Sandy! Se a carrocinha conseguir pegar você, você perderá 1 ponto de energia.",

        "Você começa com 3 pontos de energia. Se sua energia chegar a zero, terá que recomeçar o tutorial.",

        "Você precisa coletar as 8 chaves do molho. A última está com a carrocinha!"
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

        if (objetivo != null)
            objetivo.SetActive(false);

        if (painelDialogo != null)
            painelDialogo.SetActive(false);

        if (tituloFase != null)
            tituloFase.SetActive(true);

        if (textoTitulo != null)
            textoTitulo.text = "A FUGA";

        StartCoroutine(IniciarTutorial());
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
            MostrarFala(falasIntroducao[i]);

            yield return new WaitForSeconds(tempoEntreFalase);
        }

        EsconderDialogo();
    }

    // =====================================================
    // SISTEMA DE FALAS
    // =====================================================

    public void MostrarFala(string mensagem)
    {
        if (painelDialogo != null)
            painelDialogo.SetActive(true);

        if (textoDuke != null)
            textoDuke.text = mensagem;
    }

    void EsconderDialogo()
    {
        if (painelDialogo != null)
            painelDialogo.SetActive(false);
    }

    // PÚBLICO para outros scripts poderem chamar falas.
    public void MostrarFalaTemporaria(string mensagem)
    {
        if (rotinaFala != null)
            StopCoroutine(rotinaFala);

        rotinaFala = StartCoroutine(FalaTemporaria(mensagem));
    }

    IEnumerator FalaTemporaria(string mensagem)
    {
        MostrarFala(mensagem);

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

        if (etapaAtual == EtapaTutorial.Finalizado)
            yield break;

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
        etapaAtual = EtapaTutorial.Corrida;

        MostrarObjetivo(
            "Use as setas direcionais para andar!"
        );

        MostrarFalaTemporaria(
            "Sandy, vamos começar! Use as setas direcionais para andar."
        );

        IniciarDica(
            "Sandy, tente usar as setas direcionais para se movimentar!"
        );
    }

    public void RegistrarCorrida()
    {
        if (etapaAtual != EtapaTutorial.Corrida)
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
        etapaAtual = EtapaTutorial.Pulo;

        MostrarObjetivo(
            "Use ESPAÇO para pular e fugir dos golpes!"
        );

        MostrarFalaTemporaria(
            "Agora vamos treinar o pulo! Aperte ESPAÇO para pular."
        );

        IniciarDica(
            "Sandy, aperte ESPAÇO para pular! Você também pode usar o pulo duplo."
        );
    }

    public void RegistrarPulo()
    {
        if (etapaAtual != EtapaTutorial.Pulo)
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
        etapaAtual = EtapaTutorial.Latido;

        MostrarObjetivo(
            "Use Z para latir e assustar o chefe!"
        );

        MostrarFalaTemporaria(
            "Agora vamos aprender a latir! Aperte Z para assustar a carrocinha."
        );

        IniciarDica(
            "Sandy, use Z para latir! Seu latido pode ajudar a afastar o perigo."
        );
    }

    public void RegistrarLatido()
    {
        if (etapaAtual != EtapaTutorial.Latido)
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
        etapaAtual = EtapaTutorial.Coleta;

        MostrarObjetivo(
            "Colete as 8 chaves e liberte os cães!"
        );

        MostrarFalaTemporaria(
            "O intervalo não vai durar para sempre! Colete as 8 chaves do molho e liberte os cães. A última chave está com a carrocinha."
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
    // MÉTODO PARA OUTROS SCRIPTS ALTERAREM O OBJETIVO
    // =====================================================

    public void MostrarObjetivoExterno(string mensagem)
    {
        MostrarObjetivo(mensagem);
    }

    // =====================================================
    // INTERVALO TERMINOU
    // =====================================================

    public void IntervaloTerminou()
    {
        if (chefeLiberado)
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
    // FINAL
    // =====================================================

    public void FinalizarTutorial()
    {
        if (etapaAtual == EtapaTutorial.Finalizado)
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
            textoDuke.text =
                "Conseguimos! Vamos libertar todos!";
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
}