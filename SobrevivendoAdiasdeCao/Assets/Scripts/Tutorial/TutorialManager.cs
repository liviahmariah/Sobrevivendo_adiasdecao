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

    public GameObject tituloFase;
    public TextMeshProUGUI textoTitulo;

    public GameObject objetivo;
    public TextMeshProUGUI textoObjetivo;

    // =====================================================
    // ANIMAÇÃO DO BALÃO
    // =====================================================

    [Header("Animação do Balão")]

    [Tooltip("Objeto que será animado. Normalmente é o próprio balão.")]
    public RectTransform balãoAnimado;

    [Tooltip("Tempo para o balão aparecer.")]
    public float tempoEntradaBalao = 0.2f;

    [Tooltip("Tempo para o balão desaparecer.")]
    public float tempoSaidaBalao = 0.15f;

    [Tooltip("Tamanho máximo do efeito POP.")]
    public float escalaPop = 1.08f;

    [Tooltip("Velocidade da pequena flutuação do balão.")]
    public float velocidadeFlutuacao = 2f;

    [Tooltip("Quanto o balão sobe e desce.")]
    public float intensidadeFlutuacao = 2f;

    private Vector3 escalaOriginalBalao;
    private Vector2 posicaoOriginalBalao;

    private Coroutine rotinaAnimacaoBalao;

    // =====================================================
    // SOM DO DUKE
    // =====================================================

    [Header("Som do Duke")]

    [Tooltip("AudioSource usado para reproduzir o latido do Duke.")]
    public AudioSource audioSourceDuke;

    [Tooltip("Som de latido do Duke.")]
    public AudioClip somLatidoDuke;

    // =====================================================
    // EFEITO DE TEXTO
    // =====================================================

    [Header("Efeito de Texto")]

    [Tooltip("Tempo entre cada caractere.")]
    public float velocidadeTexto = 0.035f;

    [Tooltip("Pausa extra depois de vírgulas.")]
    public float pausaVirgula = 0.06f;

    [Tooltip("Pausa extra depois de pontos, ! e ?.")]
    public float pausaPontuacao = 0.15f;

    // =====================================================
    // PAINEL DE VITÓRIA
    // =====================================================

    [Header("Painel de Vitória")]
    public GameObject painelVitoria;
    public TextMeshProUGUI textoVitoria;

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

    [Tooltip("Tempo que Duke espera antes de dar uma dica.")]
    public float tempoParaDica = 8f;

    [Tooltip("Tempo que cada fala fica visível depois de terminar de escrever.")]
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
        tutorialTerminou = false;

        // ---------------------------------------------
        // BALÃO
        // ---------------------------------------------

        if (balãoAnimado != null)
        {
            escalaOriginalBalao =
                balãoAnimado.localScale;

            posicaoOriginalBalao =
                balãoAnimado.anchoredPosition;

            balãoAnimado.localScale =
                Vector3.zero;
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

        yield return StartCoroutine(
            MostrarDialogosIntroducao()
        );

        tutorialIniciado = true;

        ComecarCorrida();
    }

    // =====================================================
    // DIÁLOGOS DA INTRODUÇÃO
    // =====================================================

    IEnumerator MostrarDialogosIntroducao()
    {
        for (
            int i = 0;
            i < falasIntroducao.Length;
            i++
        )
        {
            yield return StartCoroutine(
                MostrarFalaEEsperar(
                    falasIntroducao[i]
                )
            );

            yield return new WaitForSeconds(
                tempoEntreFalase
            );
        }

        EsconderDialogo();
    }

    // =====================================================
    // MOSTRAR FALA E ESPERAR
    // =====================================================

    IEnumerator MostrarFalaEEsperar(
        string mensagem
    )
    {
        yield return StartCoroutine(
            MostrarFalaAnimada(mensagem)
        );

        yield return new WaitForSeconds(
            tempoExibicaoFala
        );
    }

    // =====================================================
    // SISTEMA DE FALAS
    // =====================================================

    public void MostrarFala(
        string mensagem
    )
    {
        if (painelDialogo != null)
            painelDialogo.SetActive(true);

        if (textoDuke != null)
        {
            textoDuke.text = mensagem;
            textoDuke.maxVisibleCharacters =
                int.MaxValue;
        }

        // Anima o balão
        if (balãoAnimado != null)
        {
            if (rotinaAnimacaoBalao != null)
            {
                StopCoroutine(
                    rotinaAnimacaoBalao
                );
            }

            rotinaAnimacaoBalao =
                StartCoroutine(
                    AnimarEntradaBalao()
                );
        }

        // Toca o latido
        TocarLatidoDuke();
    }

    // =====================================================
    // FALA ANIMADA
    // =====================================================

    IEnumerator MostrarFalaAnimada(
        string mensagem
    )
    {
        if (painelDialogo != null)
            painelDialogo.SetActive(true);

        if (textoDuke == null)
            yield break;

        // ---------------------------------------------
        // TEXTO
        // ---------------------------------------------

        textoDuke.text = mensagem;

        textoDuke.ForceMeshUpdate();

        int quantidadeCaracteres =
            textoDuke.textInfo.characterCount;

        textoDuke.maxVisibleCharacters = 0;

        textoSendoEscrito = true;

        // ---------------------------------------------
        // BALÃO + LATIDO
        // ---------------------------------------------

        if (rotinaAnimacaoBalao != null)
        {
            StopCoroutine(
                rotinaAnimacaoBalao
            );
        }

        rotinaAnimacaoBalao =
            StartCoroutine(
                AnimarEntradaBalao()
            );

        TocarLatidoDuke();

        // ---------------------------------------------
        // ESCREVER TEXTO
        // ---------------------------------------------

        for (
            int i = 0;
            i <= quantidadeCaracteres;
            i++
        )
        {
            textoDuke.maxVisibleCharacters = i;

            if (i < quantidadeCaracteres)
            {
                char caractere =
                    mensagem[i];

                // ---------------------------------
                // PONTUAÇÃO
                // ---------------------------------

                if (
                    caractere == '.' ||
                    caractere == '!' ||
                    caractere == '?'
                )
                {
                    yield return new WaitForSeconds(
                        pausaPontuacao
                    );
                }

                // ---------------------------------
                // VÍRGULA
                // ---------------------------------

                else if (
                    caractere == ','
                )
                {
                    yield return new WaitForSeconds(
                        pausaVirgula
                    );
                }

                // ---------------------------------
                // LETRA NORMAL
                // ---------------------------------

                else
                {
                    yield return new WaitForSeconds(
                        velocidadeTexto
                    );
                }
            }
        }

        textoSendoEscrito = false;
    }

    // =====================================================
    // SOM DO DUKE
    // =====================================================

    void TocarLatidoDuke()
    {
        if (
            audioSourceDuke != null &&
            somLatidoDuke != null
        )
        {
            audioSourceDuke.PlayOneShot(
                somLatidoDuke
            );
        }
    }

    // =====================================================
    // ENTRADA DO BALÃO
    // =====================================================

    IEnumerator AnimarEntradaBalao()
    {
        if (balãoAnimado == null)
            yield break;

        Vector3 inicio =
            Vector3.zero;

        Vector3 meio =
            escalaOriginalBalao *
            escalaPop;

        Vector3 fim =
            escalaOriginalBalao;

        float tempo = 0f;

        // ---------------------------------------------
        // POP
        // ---------------------------------------------

        while (
            tempo < tempoEntradaBalao
        )
        {
            tempo += Time.deltaTime;

            float progresso =
                tempo /
                tempoEntradaBalao;

            balãoAnimado.localScale =
                Vector3.Lerp(
                    inicio,
                    meio,
                    progresso
                );

            yield return null;
        }

        tempo = 0f;

        // ---------------------------------------------
        // VOLTA PARA O TAMANHO NORMAL
        // ---------------------------------------------

        while (
            tempo <
            tempoEntradaBalao * 0.4f
        )
        {
            tempo += Time.deltaTime;

            float progresso =
                tempo /
                (tempoEntradaBalao * 0.4f);

            balãoAnimado.localScale =
                Vector3.Lerp(
                    meio,
                    fim,
                    progresso
                );

            yield return null;
        }

        balãoAnimado.localScale =
            escalaOriginalBalao;

        rotinaAnimacaoBalao = null;
    }

    // =====================================================
    // FLUTUAÇÃO DO BALÃO
    // =====================================================

    void Update()
    {
        if (
            balãoAnimado != null &&
            painelDialogo != null &&
            painelDialogo.activeSelf
        )
        {
            float movimento =
                Mathf.Sin(
                    Time.time *
                    velocidadeFlutuacao
                ) *
                intensidadeFlutuacao;

            balãoAnimado.anchoredPosition =
                posicaoOriginalBalao +
                Vector2.up *
                movimento;
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
            balãoAnimado.localScale =
                escalaOriginalBalao;

            balãoAnimado.anchoredPosition =
                posicaoOriginalBalao;
        }

        if (textoDuke != null)
        {
            textoDuke.maxVisibleCharacters =
                int.MaxValue;
        }
    }

    // =====================================================
    // FALA TEMPORÁRIA
    // =====================================================

    public void MostrarFalaTemporaria(
        string mensagem
    )
    {
        if (rotinaFala != null)
        {
            StopCoroutine(
                rotinaFala
            );
        }

        rotinaFala =
            StartCoroutine(
                FalaTemporaria(
                    mensagem
                )
            );
    }

    IEnumerator FalaTemporaria(
        string mensagem
    )
    {
        yield return StartCoroutine(
            MostrarFalaAnimada(
                mensagem
            )
        );

        yield return new WaitForSeconds(
            tempoExibicaoFala
        );

        EsconderDialogo();

        rotinaFala = null;
    }

    // =====================================================
    // SISTEMA DE DICAS
    // =====================================================

    void IniciarDica(
        string mensagem
    )
    {
        PararDica();

        rotinaDica =
            StartCoroutine(
                AguardarDica(
                    mensagem
                )
            );
    }

    IEnumerator AguardarDica(
        string mensagem
    )
    {
        yield return new WaitForSeconds(
            tempoParaDica
        );

        if (
            etapaAtual ==
            EtapaTutorial.Finalizado
        )
        {
            yield break;
        }

        MostrarFalaTemporaria(
            mensagem
        );

        rotinaDica = null;
    }

    void PararDica()
    {
        if (rotinaDica != null)
        {
            StopCoroutine(
                rotinaDica
            );

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

        etapaAtual =
            EtapaTutorial.Corrida;

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
        if (tutorialTerminou)
            return;

        if (
            etapaAtual !=
            EtapaTutorial.Corrida
        )
            return;

        PararDica();

        Debug.Log(
            "Corrida concluída!"
        );

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

        etapaAtual =
            EtapaTutorial.Pulo;

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
        if (tutorialTerminou)
            return;

        if (
            etapaAtual !=
            EtapaTutorial.Pulo
        )
            return;

        PararDica();

        Debug.Log(
            "Pulo concluído!"
        );

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

        etapaAtual =
            EtapaTutorial.Latido;

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
        if (tutorialTerminou)
            return;

        if (
            etapaAtual !=
            EtapaTutorial.Latido
        )
            return;

        PararDica();

        Debug.Log(
            "Latido concluído!"
        );

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

        etapaAtual =
            EtapaTutorial.Coleta;

        MostrarObjetivo(
            "Colete as 8 chaves e liberte os cães!"
        );

        MostrarFalaTemporaria(
            "O intervalo não vai durar para sempre! Colete as 8 chaves do molho e liberte os cães. A última está com a carrocinha."
        );

        IniciarDica(
            "Sandy, procure as chaves das gaiolas! Precisamos libertar todos antes que o chefe volte."
        );

        Debug.Log(
            "Tutorial das mecânicas concluído!"
        );

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
    // MÉTODO PARA OUTROS SCRIPTS ALTERAREM O OBJETIVO
    // =====================================================

    public void MostrarObjetivoExterno(
        string mensagem
    )
    {
        if (tutorialTerminou)
            return;

        MostrarObjetivo(
            mensagem
        );
    }

    // =====================================================
    // INTERVALO TERMINOU
    // =====================================================

    public void IntervaloTerminou()
    {
        if (chefeLiberado)
            return;

        if (tutorialTerminou)
            return;

        chefeLiberado = true;

        PararDica();

        Debug.Log(
            "O CHEFE VOLTOU!"
        );

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
    // VITÓRIA
    // =====================================================

    public void VerificarVitoria()
    {
        if (tutorialTerminou)
            return;

        if (
            TutorialChavesManager.instance ==
            null
        )
        {
            Debug.LogWarning(
                "TutorialChavesManager não foi encontrado."
            );

            return;
        }

        int gaiolasAbertas =
            TutorialChavesManager
            .instance
            .GaiolasAbertas;

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

        etapaAtual =
            EtapaTutorial.Finalizado;

        PararDica();

        if (rotinaFala != null)
        {
            StopCoroutine(
                rotinaFala
            );

            rotinaFala = null;
        }

        if (tutorialTimer != null)
        {
            tutorialTimer.PararTimer();
        }

        if (chefe != null)
        {
            chefe.DesativarChefe();
        }

        if (objetivo != null)
            objetivo.SetActive(false);

        if (painelDialogo != null)
            painelDialogo.SetActive(false);

        if (painelVitoria != null)
        {
            painelVitoria.SetActive(true);
        }

        if (textoVitoria != null)
        {
            textoVitoria.text =
                "VOCÊ CONSEGUIU!\n" +
                "Todos os cães foram libertados!";
        }

        Debug.Log(
            "VITÓRIA! Sandy libertou os 9 cães."
        );
    }

    // =====================================================
    // DERROTA
    // =====================================================

    public void Derrota()
    {
        if (tutorialTerminou)
            return;

        tutorialTerminou = true;

        etapaAtual =
            EtapaTutorial.Finalizado;

        PararDica();

        if (rotinaFala != null)
        {
            StopCoroutine(
                rotinaFala
            );

            rotinaFala = null;
        }

        if (tutorialTimer != null)
        {
            tutorialTimer.PararTimer();
        }

        if (chefe != null)
        {
            chefe.DesativarChefe();
        }

        if (objetivo != null)
            objetivo.SetActive(false);

        if (painelDialogo != null)
            painelDialogo.SetActive(false);

        if (painelDerrota != null)
        {
            painelDerrota.SetActive(true);
        }

        if (textoDerrota != null)
        {
            textoDerrota.text =
                "A FUGA FALHOU\n" +
                "Sandy ficou sem energia.";
        }

        Debug.Log(
            "DERROTA! Sandy ficou sem energia."
        );
    }

    // =====================================================
    // REINICIAR TUTORIAL
    // =====================================================

    public void ReiniciarTutorial()
    {
        Debug.Log(
            "Reiniciando o tutorial..."
        );

        Time.timeScale = 1f;

        Scene cenaAtual =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            cenaAtual.name
        );
    }

    // =====================================================
    // FINAL
    // =====================================================

    public void FinalizarTutorial()
    {
        if (tutorialTerminou)
            return;

        etapaAtual =
            EtapaTutorial.Finalizado;

        PararDica();

        if (rotinaFala != null)
        {
            StopCoroutine(
                rotinaFala
            );

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

        Debug.Log(
            "Tutorial concluído!"
        );
    }

    // =====================================================
    // MOSTRAR OBJETIVO
    // =====================================================

    void MostrarObjetivo(
        string mensagem
    )
    {
        if (objetivo != null)
            objetivo.SetActive(true);

        if (textoObjetivo != null)
            textoObjetivo.text =
                mensagem;
    }

    // =====================================================
    // VERIFICAR ETAPA
    // =====================================================

    public bool EstaNaEtapa(
        EtapaTutorial etapa
    )
    {
        return etapaAtual == etapa;
    }
}