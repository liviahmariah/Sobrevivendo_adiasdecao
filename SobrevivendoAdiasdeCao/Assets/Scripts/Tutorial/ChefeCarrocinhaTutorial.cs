using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
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

    [Header("Chão e Física")]
    [Tooltip("Selecione a Layer Ground.")]
    public LayerMask layerChao;
    [Tooltip("Ponto nos pés do chefe para checar contato com o chão. Se vazio, usa a base do collider.")]
    public Transform verificadorChao;
    public float raioVerificacaoChao = 0.2f;
    [SerializeField] private bool estaNoChao;

    [Header("Chaves")]
    public int chavesColetadas = 0;
    public int quantidadeNecessaria = 8;
    public GameObject chaveFinalPrefab;

    [Header("Tempo da Dinâmica")]
    public float tempoPatrulha = 20f;
    public float tempoFora = 20f;
    public float duracaoSusto = 1.2f;
    public float tempoAbaixandoParaPrender = 0.6f;

    [Header("Velocidades")]
    public float velocidadeAndar = 2f;
    public float velocidadeCorrer = 4.2f;
    public float velocidadeTransporte = 2.2f;

    [Header("Detecção e Captura")]
    public Transform sandy;
    public float distanciaVisao = 8f;
    public float distanciaCaptura = 1.2f;
    public float distanciaGaiola = 0.4f;

    [Header("Pontos de Ronda e Gaiola")]
    public Transform[] pontosPatrulha;
    public float distanciaDoPonto = 0.35f;
    public Transform pontoInicioRonda;
    public Transform pontoRetornoChefe;
    public Transform pontoGaiolaSandy;
    public float raioAreaSegura = 2f;
    public Transform pontoCordaChefe;

    [Header("Corda Visual (Conexão Chefe <-> Sandy)")]
    public Color corCorda = new Color(0.55f, 0.35f, 0.18f, 1f);
    public float larguraCorda = 0.08f;
    public int sortingOrderCorda = 12;

    [Header("Áudio")]
    public AudioSource audioSource;
    public AudioClip somAlertaViuSandy;
    public AudioClip somSustoChefe;

    [Header("Referências Externas")]
    public EnergiaSandy energiaSandy;
    public TutorialTimer tutorialTimer;
    public IndicadorChefeHUD indicadorChefeHUD;
    public Animator animatorCarrocinha;

    private Rigidbody2D rb;
    private Collider2D colisorPrincipal;
    private LineRenderer linhaCorda;

    private int pontoAtual = 0;
    private bool chefeAtivo = false;
    private bool podeCapturar = true;
    private bool transportandoSandy = false;
    private bool chaveJaSolta = false;
    private float velocidadeHorizontalAtual = 0f;

    private PlayerMovement movimentoSandy;
    private Transform paiOriginalSandy;
    private Coroutine cicloChefeCoroutine;
    private Coroutine rotinaCapturaCoroutine;
    private Coroutine rotinaSustoCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            if (!col.isTrigger)
            {
                colisorPrincipal = col;
                break;
            }
        }

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

    private void FixedUpdate()
    {
        VerificarChao();

        if (!chefeAtivo || estadoAtual == EstadoChefe.Fora || estadoAtual == EstadoChefe.Assustado)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(velocidadeHorizontalAtual, rb.linearVelocity.y);
    }

    private void VerificarChao()
    {
        Vector2 pontoPe = verificadorChao != null
            ? (Vector2)verificadorChao.position
            : (colisorPrincipal != null
                ? new Vector2(colisorPrincipal.bounds.center.x, colisorPrincipal.bounds.min.y)
                : (Vector2)transform.position);

        estaNoChao = Physics2D.OverlapCircle(pontoPe, raioVerificacaoChao, layerChao);
    }

    private void Update()
    {
        AtualizarAnimacoes();
        AtualizarCordaVisual();

        if (!chefeAtivo || sandy == null)
        {
            velocidadeHorizontalAtual = 0f;
            return;
        }

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
                break;

            case EstadoChefe.Assustado:
                velocidadeHorizontalAtual = 0f;
                break;
        }
    }

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
            PosicionarSobreOChao(pontoInicioRonda.position);

        cicloChefeCoroutine = StartCoroutine(CicloDoChefe());
    }

    private void PosicionarSobreOChao(Vector3 posicaoAlvo)
    {
        RaycastHit2D hit = Physics2D.Raycast(posicaoAlvo + Vector3.up * 2f, Vector2.down, 10f, layerChao);
        if (hit.collider != null && colisorPrincipal != null)
        {
            float metadeAltura = colisorPrincipal.bounds.extents.y;
            float offsetCentro = transform.position.y - colisorPrincipal.bounds.center.y;
            transform.position = new Vector3(posicaoAlvo.x, hit.point.y + metadeAltura + offsetCentro, transform.position.z);
        }
        else
        {
            transform.position = posicaoAlvo;
        }

        rb.linearVelocity = Vector2.zero;
    }

    private IEnumerator CicloDoChefe()
    {
        while (chefeAtivo)
        {
            float tempoRestante = tempoPatrulha;

            while (tempoRestante > 0f && chefeAtivo)
            {
                AtualizarTimerUI("CHEFE EM RONDA", tempoRestante);
                tempoRestante -= Time.deltaTime;
                yield return null;
            }

            while (transportandoSandy)
                yield return null;

            if (!chefeAtivo)
                yield break;

            chefeAtivo = false;
            podeCapturar = false;
            velocidadeHorizontalAtual = 0f;
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

            chefeAtivo = true;
            podeCapturar = true;
            transportandoSandy = false;
            estadoAtual = EstadoChefe.Andando;
            pontoAtual = 0;
            AtualizarIndicadorChefe();

            if (pontoInicioRonda != null)
                PosicionarSobreOChao(pontoInicioRonda.position);
        }
    }

    private void ExecutarPatrulha()
    {
        if (pontosPatrulha == null || pontosPatrulha.Length == 0)
        {
            velocidadeHorizontalAtual = 0f;
            return;
        }

        if (pontoAtual < 0 || pontoAtual >= pontosPatrulha.Length)
            pontoAtual = 0;

        Transform destino = pontosPatrulha[pontoAtual];
        if (destino == null)
            return;

        DefinirMovimentoHorizontal(destino.position.x, velocidadeAndar);

        if (Mathf.Abs(transform.position.x - destino.position.x) <= distanciaDoPonto)
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

    private void ExecutarPerseguicao()
    {
        if (SandyEstaNaAreaSegura())
        {
            estadoAtual = EstadoChefe.Andando;
            return;
        }

        DefinirMovimentoHorizontal(sandy.position.x, velocidadeCorrer);

        if (podeCapturar && !transportandoSandy)
        {
            float distancia = Vector2.Distance(transform.position, sandy.position);
            if (distancia <= distanciaCaptura)
            {
                CapturarSandy();
            }
        }
    }

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
        velocidadeHorizontalAtual = 0f;
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

        while (!estaNoChao)
            yield return null;

        if (movimentoSandy == null)
            movimentoSandy = sandy.GetComponent<PlayerMovement>();

        if (movimentoSandy != null)
            movimentoSandy.enabled = false;

        Rigidbody2D rbSandy = sandy.GetComponent<Rigidbody2D>();
        bool tinhaSimulacaoSandy = rbSandy != null && rbSandy.simulated;
        if (rbSandy != null)
        {
            rbSandy.linearVelocity = Vector2.zero;
            rbSandy.simulated = false;
        }

        paiOriginalSandy = sandy.parent;

        yield return new WaitForSeconds(tempoAbaixandoParaPrender);

        if (sandy != null)
        {
            Transform ancora = pontoCordaChefe != null ? pontoCordaChefe : transform;
            sandy.SetParent(ancora);
            sandy.localPosition = new Vector3(0.6f * Mathf.Sign(transform.localScale.x), -0.3f, 0f);
        }

        if (pontoGaiolaSandy != null)
        {
            while (Mathf.Abs(transform.position.x - pontoGaiolaSandy.position.x) > distanciaGaiola)
            {
                DefinirMovimentoHorizontal(pontoGaiolaSandy.position.x, velocidadeTransporte);
                yield return null;
            }
        }

        velocidadeHorizontalAtual = 0f;

        if (rbSandy != null)
            rbSandy.simulated = tinhaSimulacaoSandy;

        FinalizarTransporte();
    }

    private void FinalizarTransporte()
    {
        transportandoSandy = false;
        velocidadeHorizontalAtual = 0f;

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

    public void AssustarChefe()
    {
        if (!chefeAtivo || sandy == null)
            return;

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
        while (!estaNoChao)
            yield return null;

        velocidadeHorizontalAtual = 0f;
        estadoAtual = EstadoChefe.Assustado;

        TocarSom(somSustoChefe);

        if (animatorCarrocinha != null)
            animatorCarrocinha.SetTrigger("Assustado");

        if (!chaveJaSolta && chaveFinalPrefab != null)
        {
            chaveJaSolta = true;
            Vector3 posSpawnChave = transform.position + Vector3.up * 0.5f;
            GameObject novaChave = Instantiate(chaveFinalPrefab, posSpawnChave, Quaternion.identity);
            ChaveFinalTutorial chave = novaChave.GetComponent<ChaveFinalTutorial>();

            if (chave != null)
                chave.SoltarChave(posSpawnChave);
        }

        yield return new WaitForSeconds(duracaoSusto);

        if (chefeAtivo)
        {
            if (!SandyEstaNaAreaSegura())
                IniciarPerseguicao(tocarSomAlerta: false);
            else
                estadoAtual = EstadoChefe.Andando;
        }
    }

    private void DefinirMovimentoHorizontal(float destinoX, float velocidade)
    {
        float diffX = destinoX - transform.position.x;

        if (Mathf.Abs(diffX) <= 0.05f)
        {
            velocidadeHorizontalAtual = 0f;
            return;
        }

        float direcao = Mathf.Sign(diffX);
        velocidadeHorizontalAtual = direcao * velocidade;

        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x) * direcao,
            transform.localScale.y,
            transform.localScale.z
        );
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
        velocidadeHorizontalAtual = 0f;
        estadoAtual = EstadoChefe.Fora;

        if (linhaCorda != null)
            linhaCorda.enabled = false;

        AtualizarIndicadorChefe();
        StopAllCoroutines();

        if (pontoRetornoChefe != null)
            transform.position = pontoRetornoChefe.position;
    }
}