using System.Collections;
using UnityEngine;

public class ChefeCarrocinhaTutorial : MonoBehaviour
{
    public enum EstadoChefe
    {
        Fora,
        Andando,
        Correndo,
        Capturando,
        Assustado
    }

    [Header("Estado Atual")]
    public EstadoChefe estadoAtual = EstadoChefe.Fora;

    [Header("Chaves")]
    public int chavesColetadas = 0;
    public int quantidadeNecessaria = 8;
    public GameObject chaveFinalPrefab;

    [Header("Tempo da Dinâmica")]
    public float tempoPatrulha = 20f;
    public float tempoFora = 20f;
    [Tooltip("Duração da animação de susto antes de voltar a correr atrás de Sandy.")]
    public float duracaoSusto = 1.2f;
    [Tooltip("Tempo abaixado prendendo Sandy antes de começar a puxá-la para a gaiola.")]
    public float tempoAbaixandoParaPrender = 0.6f;

    [Header("Velocidades")]
    public float velocidadeAndar = 2f;
    public float velocidadeCorrer = 4.2f;
    public float velocidadeTransporte = 2.2f;

    [Header("Detecção e Captura")]
    public Transform sandy;
    public float distanciaVisao = 8f;
    public float distanciaCaptura = 1.2f;
    public float distanciaGaiola = 0.35f;

    [Header("Pontos de Ronda e Gaiola")]
    public Transform[] pontosPatrulha;
    public float distanciaDoPonto = 0.25f;
    public Transform pontoInicioRonda;
    public Transform pontoRetornoChefe;
    public Transform pontoGaiolaSandy;
    public float raioAreaSegura = 2f;
    [Tooltip("Ponto na carrocinha de onde sai a corda e onde Sandy fica presa.")]
    public Transform pontoCordaChefe;

    [Header("Corda Visual (Conexão Chefe <-> Sandy)")]
    public Color corCorda = new Color(0.55f, 0.35f, 0.18f, 1f);
    public float larguraCorda = 0.08f;
    public int sortingOrderCorda = 12;

    [Header("Áudio")]
    public AudioSource audioSource;
    [Tooltip("Som tocado assim que o chefe vê Sandy fora da gaiola e começa a correr.")]
    public AudioClip somAlertaViuSandy;
    [Tooltip("Som tocado quando Sandy late e assusta o chefe.")]
    public AudioClip somSustoChefe;

    [Header("Referências Externas")]
    public EnergiaSandy energiaSandy;
    public TutorialTimer tutorialTimer;
    public IndicadorChefeHUD indicadorChefeHUD;
    public Animator animatorCarrocinha;

    private int pontoAtual = 0;
    private bool chefeAtivo = false;
    private bool podeCapturar = true;
    private bool transportandoSandy = false;
    private bool chaveJaSolta = false;

    private PlayerMovement movimentoSandy;
    private Transform paiOriginalSandy;
    private LineRenderer linhaCorda;
    private Coroutine cicloChefeCoroutine;
    private Coroutine rotinaCapturaCoroutine;
    private Coroutine rotinaSustoCoroutine;

    // =========================================================
    // INICIALIZAÇÃO
    // =========================================================

    private void Awake()
    {
        if (animatorCarrocinha == null)
            animatorCarrocinha = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        ConfigurarLinhaCorda();
    }

    private void Start()
    {
        estadoAtual = EstadoChefe.Fora;
        chefeAtivo = false;
        podeCapturar = true;
        transportandoSandy = false;

        if (sandy == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                sandy = playerObj.transform;
        }

        if (sandy != null)
            movimentoSandy = sandy.GetComponent<PlayerMovement>();

        AtualizarIndicadorChefe();
        AtualizarAnimacoes();
    }

    private void ConfigurarLinhaCorda()
    {
        linhaCorda = GetComponent<LineRenderer>();
        if (linhaCorda == null)
            linhaCorda = gameObject.AddComponent<LineRenderer>();

        linhaCorda.positionCount = 2;
        linhaCorda.startWidth = larguraCorda;
        linhaCorda.endWidth = larguraCorda;
        linhaCorda.material = new Material(Shader.Find("Sprites/Default"));
        linhaCorda.startColor = corCorda;
        linhaCorda.endColor = corCorda;
        linhaCorda.sortingOrder = sortingOrderCorda;
        linhaCorda.useWorldSpace = true;
        linhaCorda.enabled = false;
    }

    // =========================================================
    // LOOP PRINCIPAL
    // =========================================================

    private void Update()
    {
        AtualizarAnimacoes();
        AtualizarCordaVisual();

        if (!chefeAtivo || sandy == null)
            return;

        switch (estadoAtual)
        {
            case EstadoChefe.Andando:
                VerificarSeViuSandyForaDaGaiola();
                if (estadoAtual == EstadoChefe.Andando)
                    ExecutarPatrulha();
                break;

            case EstadoChefe.Correndo:
                ExecutarPerseguicao();
                break;

            case EstadoChefe.Capturando:
                // Gerenciado pela coroutine RotinaCapturaETransporte
                break;

            case EstadoChefe.Assustado:
                // Aguarda terminar a animação de susto para voltar a correr
                break;
        }
    }

    // =========================================================
    // ANIMAÇÕES E CORDA
    // =========================================================

    private void AtualizarAnimacoes()
    {
        if (animatorCarrocinha == null)
            return;

        animatorCarrocinha.SetBool("EstaParada", estadoAtual == EstadoChefe.Fora);
        animatorCarrocinha.SetBool("EstaAndando", estadoAtual == EstadoChefe.Andando);
        animatorCarrocinha.SetBool("EstaCorrendo", estadoAtual == EstadoChefe.Correndo);
        animatorCarrocinha.SetBool("Agarrando", estadoAtual == EstadoChefe.Capturando);
    }

    private void AtualizarCordaVisual()
    {
        if (linhaCorda == null)
            return;

        if (estadoAtual == EstadoChefe.Capturando && sandy != null)
        {
            linhaCorda.enabled = true;
            Vector3 origemCorda = pontoCordaChefe != null ? pontoCordaChefe.position : transform.position;
            linhaCorda.SetPosition(0, origemCorda);
            linhaCorda.SetPosition(1, sandy.position);
        }
        else
        {
            linhaCorda.enabled = false;
        }
    }

    private void TocarSom(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    // =========================================================
    // CICLO DE RONDA
    // =========================================================

    public void LiberarChefe()
    {
        if (cicloChefeCoroutine != null)
            StopCoroutine(cicloChefeCoroutine);

        chefeAtivo = true;
        podeCapturar = true;
        transportandoSandy = false;
        estadoAtual = EstadoChefe.Andando;
        pontoAtual = 0;

        AtualizarIndicadorChefe();

        if (pontoInicioRonda != null)
            transform.position = pontoInicioRonda.position;

        cicloChefeCoroutine = StartCoroutine(CicloDoChefe());
    }

    private IEnumerator CicloDoChefe()
    {
        while (chefeAtivo)
        {
            // FASE 1: CHEFE NA RONDA
            float tempoRestante = tempoPatrulha;

            while (tempoRestante > 0f && chefeAtivo)
            {
                AtualizarTimerUI("CHEFE EM RONDA", tempoRestante);
                tempoRestante -= Time.deltaTime;
                yield return null;
            }

            // Se estiver levando Sandy para a gaiola, espera terminar antes de ir embora
            while (transportandoSandy)
                yield return null;

            if (!chefeAtivo)
                yield break;

            // FASE 2: CHEFE SAI DA FASE
            chefeAtivo = false;
            podeCapturar = false;
            estadoAtual = EstadoChefe.Fora;
            AtualizarIndicadorChefe();

            if (pontoRetornoChefe != null)
                transform.position = pontoRetornoChefe.position;

            tempoRestante = tempoFora;
            while (tempoRestante > 0f)
            {
                AtualizarTimerUI("CHEFE FORA", tempoRestante);
                tempoRestante -= Time.deltaTime;
                yield return null;
            }

            if (sandy == null)
                yield break;

            // RETORNA PARA NOVA RONDA
            chefeAtivo = true;
            podeCapturar = true;
            transportandoSandy = false;
            estadoAtual = EstadoChefe.Andando;
            pontoAtual = 0;
            AtualizarIndicadorChefe();

            if (pontoInicioRonda != null)
                transform.position = pontoInicioRonda.position;
        }
    }

    // =========================================================
    // PATRULHA (ANDANDO)
    // =========================================================

    private void ExecutarPatrulha()
    {
        if (pontosPatrulha == null || pontosPatrulha.Length == 0)
            return;

        if (pontoAtual < 0 || pontoAtual >= pontosPatrulha.Length)
            pontoAtual = 0;

        Transform destino = pontosPatrulha[pontoAtual];
        if (destino == null)
            return;

        MoverAte(destino.position, velocidadeAndar);

        if (Vector2.Distance(transform.position, destino.position) <= distanciaDoPonto)
        {
            pontoAtual = (pontoAtual + 1) % pontosPatrulha.Length;
        }
    }

    private void VerificarSeViuSandyForaDaGaiola()
    {
        if (SandyEstaNaAreaSegura())
            return;

        float distancia = Vector2.Distance(transform.position, sandy.position);
        if (distancia <= distanciaVisao)
        {
            IniciarPerseguicao(tocarSomAlerta: true);
        }
    }

    private void IniciarPerseguicao(bool tocarSomAlerta)
    {
        if (estadoAtual == EstadoChefe.Correndo)
            return;

        estadoAtual = EstadoChefe.Correndo;

        if (tocarSomAlerta)
            TocarSom(somAlertaViuSandy);
    }

    // =========================================================
    // PERSEGUIÇÃO (CORRENDO)
    // =========================================================

    private void ExecutarPerseguicao()
    {
        // Se Sandy voltou para a gaiola, volta a andar na ronda
        if (SandyEstaNaAreaSegura())
        {
            estadoAtual = EstadoChefe.Andando;
            return;
        }

        MoverAte(sandy.position, velocidadeCorrer);

        if (podeCapturar && !transportandoSandy)
        {
            float distancia = Vector2.Distance(transform.position, sandy.position);
            if (distancia <= distanciaCaptura)
            {
                CapturarSandy();
            }
        }
    }

    // =========================================================
    // CAPTURA (ABAIXAR + CORDA + LEVAR ATÉ A GAIOLA)
    // =========================================================

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (!chefeAtivo || !podeCapturar || transportandoSandy)
            return;

        if (estadoAtual != EstadoChefe.Correndo && estadoAtual != EstadoChefe.Andando)
            return;

        if (!outro.CompareTag("Player") || SandyEstaNaAreaSegura())
            return;

        CapturarSandy();
    }

    public void CapturarSandy()
    {
        if (!chefeAtivo || !podeCapturar || transportandoSandy || SandyEstaNaAreaSegura())
            return;

        podeCapturar = false;
        transportandoSandy = true;
        estadoAtual = EstadoChefe.Capturando;

        if (energiaSandy != null)
            energiaSandy.PerderEnergia();

        if (rotinaCapturaCoroutine != null)
            StopCoroutine(rotinaCapturaCoroutine);

        rotinaCapturaCoroutine = StartCoroutine(RotinaCapturaETransporte());
    }

    private IEnumerator RotinaCapturaETransporte()
    {
        if (sandy == null)
        {
            FinalizarTransporte();
            yield break;
        }

        // Trava o movimento da Sandy
        if (movimentoSandy == null)
            movimentoSandy = sandy.GetComponent<PlayerMovement>();

        if (movimentoSandy != null)
            movimentoSandy.enabled = false;

        paiOriginalSandy = sandy.parent;

        // 1. Chefe abaixa até Sandy e prende a corda
        yield return new WaitForSeconds(tempoAbaixandoParaPrender);

        if (sandy != null)
        {
            Transform ancora = pontoCordaChefe != null ? pontoCordaChefe : transform;
            sandy.SetParent(ancora);
            sandy.localPosition = new Vector3(0.6f * Mathf.Sign(transform.localScale.x), -0.3f, 0f);
        }

        // 2. Arrasta Sandy presa na corda até a gaiola
        if (pontoGaiolaSandy != null)
        {
            while (Vector2.Distance(transform.position, pontoGaiolaSandy.position) > distanciaGaiola)
            {
                MoverAte(pontoGaiolaSandy.position, velocidadeTransporte);
                yield return null;
            }
        }

        FinalizarTransporte();
    }

    private void FinalizarTransporte()
    {
        transportandoSandy = false;

        if (sandy != null)
        {
            sandy.SetParent(paiOriginalSandy);
            if (pontoGaiolaSandy != null)
                sandy.position = pontoGaiolaSandy.position;
        }

        if (movimentoSandy != null)
            movimentoSandy.enabled = true;

        estadoAtual = EstadoChefe.Andando;
        podeCapturar = true;
    }

    // =========================================================
    // SUSTO PELO LATIDO (SOLTA CHAVE + VOLTA A CORRER)
    // =========================================================

    public void AssustarChefe()
    {
        if (!chefeAtivo || sandy == null)
            return;

        // Não interrompe se já estiver levando Sandy presa ou já assustado
        if (estadoAtual == EstadoChefe.Capturando || estadoAtual == EstadoChefe.Assustado)
            return;

        float distancia = Vector2.Distance(transform.position, sandy.position);
        if (distancia > distanciaVisao)
            return;

        if (chavesColetadas < quantidadeNecessaria)
            return;

        if (rotinaSustoCoroutine != null)
            StopCoroutine(rotinaSustoCoroutine);

        rotinaSustoCoroutine = StartCoroutine(RotinaSustoEDepoisCorrer());
    }

    private IEnumerator RotinaSustoEDepoisCorrer()
    {
        estadoAtual = EstadoChefe.Assustado;

        // Toca som e dispara animação de susto
        TocarSom(somSustoChefe);

        if (animatorCarrocinha != null)
            animatorCarrocinha.SetTrigger("Assustado");

        // Solta a chave final apenas uma vez
        if (!chaveJaSolta && chaveFinalPrefab != null)
        {
            chaveJaSolta = true;
            GameObject novaChave = Instantiate(chaveFinalPrefab, transform.position, Quaternion.identity);
            ChaveFinalTutorial chave = novaChave.GetComponent<ChaveFinalTutorial>();

            if (chave != null)
                chave.SoltarChave(transform.position);
        }

        // Espera a animação de susto terminar
        yield return new WaitForSeconds(duracaoSusto);

        // Depois de assustado, volta a correr para tentar capturar Sandy!
        if (chefeAtivo)
        {
            if (!SandyEstaNaAreaSegura())
                IniciarPerseguicao(tocarSomAlerta: false);
            else
                estadoAtual = EstadoChefe.Andando;
        }
    }

    // =========================================================
    // UTILITÁRIOS
    // =========================================================

    private void MoverAte(Vector3 destino, float velocidade)
    {
        float diffX = destino.x - transform.position.x;

        transform.position = Vector2.MoveTowards(
            transform.position,
            destino,
            velocidade * Time.deltaTime
        );

        if (Mathf.Abs(diffX) > 0.01f)
        {
            float sinal = diffX > 0f ? 1f : -1f;
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x) * sinal,
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    private bool SandyEstaNaAreaSegura()
    {
        if (sandy == null || pontoGaiolaSandy == null)
            return false;

        return Vector2.Distance(sandy.position, pontoGaiolaSandy.position) <= raioAreaSegura;
    }

    public void AtualizarChavesColetadas(int quantidade)
    {
        chavesColetadas = quantidade;
    }

    private void AtualizarIndicadorChefe()
    {
        if (indicadorChefeHUD != null)
            indicadorChefeHUD.AtualizarIndicadores(chefeAtivo);
    }

    private void AtualizarTimerUI(string mensagem, float tempo)
    {
        if (tutorialTimer != null)
            tutorialTimer.MostrarTimerChefe(mensagem, tempo);
    }

    public void DesativarChefe()
    {
        chefeAtivo = false;
        podeCapturar = false;
        transportandoSandy = false;
        estadoAtual = EstadoChefe.Fora;

        if (linhaCorda != null)
            linhaCorda.enabled = false;

        AtualizarIndicadorChefe();
        StopAllCoroutines();

        if (pontoRetornoChefe != null)
            transform.position = pontoRetornoChefe.position;
    }
}