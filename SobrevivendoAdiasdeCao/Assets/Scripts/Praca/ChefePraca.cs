using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class ChefePraca : MonoBehaviour
{
    public enum EstadoChefe
    {
        Parado,
        Perseguindo,
        Agarrando,
        Assustado,
        Fugindo,
        AguardandoRetorno
    }

    [Header("Estado")]
    public EstadoChefe estadoAtual = EstadoChefe.Parado;

    [Header("Alvo")]
    public Transform sandy;
    public EnergiaSandy energiaSandy;
    public Transform pontoDeAgarrar;

    [Header("Percepção")]
    public float distanciaPercepcao = 6f;

    [Header("Movimento")]
    public float velocidadePerseguicao = 3.5f;

    [Header("Agarrão")]
    public int latidosNecessarios = 6;
    public float tempoMaximoAgarrada = 4f;

    [Header("Susto, Fuga e Retorno")]
    public float duracaoSusto = 0.8f;
    public float duracaoFuga = 2.5f;
    public float velocidadeFuga = 4.5f;
    [Tooltip("Tempo que ele fica fora antes de voltar para a praça.")]
    public float tempoParaVoltar = 3f;
    [Tooltip("Se marcado, volta para o ponto inicial na praça e já recomeça a perseguir.")]
    public bool voltarParaPosicaoInicial = true;

    [Header("Animação e Áudio")]
    public Animator animatorVigia;
    public AudioSource audioSource;
    public AudioClip somSustoVigia;

    [Header("HUD do agarrão")]
    public GameObject painelAgarramento;
    public TMP_Text textoAviso;
    public TMP_Text textoLatidos;
    public Slider barraTempo;

    private int latidosAtuais = 0;
    private float tempoAgarrada;
    private float tempoSustoRestante;
    private float tempoFugindo;
    private float tempoEsperaRetorno;
    private int direcaoFuga;

    private Vector3 posicaoInicial;
    private Rigidbody2D rb;
    private Vector2 velocidadeAtual;
    private ControleAgarramentoSandy controleSandy;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        posicaoInicial = transform.position;

        if (animatorVigia == null)
            animatorVigia = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (sandy == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                sandy = playerObj.transform;
        }
    }

    private void Update()
    {
        AtualizarAnimacoes();

        if (FaseManager.Instance == null ||
            !FaseManager.Instance.PodeColetar())
        {
            PararMovimento();
            return;
        }

        switch (estadoAtual)
        {
            case EstadoChefe.Parado:
                VerificarSandy();
                break;

            case EstadoChefe.Perseguindo:
                PerseguirSandy();
                break;

            case EstadoChefe.Agarrando:
                PararMovimento();
                AtualizarAgarrão();
                break;

            case EstadoChefe.Assustado:
                PararMovimento();
                AtualizarSusto();
                break;

            case EstadoChefe.Fugindo:
                AtualizarFuga();
                break;

            case EstadoChefe.AguardandoRetorno:
                PararMovimento();
                AtualizarRetorno();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                velocidadeAtual.x,
                rb.linearVelocity.y
            );
        }
    }

    private void AtualizarAnimacoes()
    {
        if (animatorVigia == null)
            return;

        bool estaParado = estadoAtual == EstadoChefe.Parado ||
                          estadoAtual == EstadoChefe.Agarrando ||
                          estadoAtual == EstadoChefe.AguardandoRetorno;

        animatorVigia.SetBool("EstaParado", estaParado);
        animatorVigia.SetBool("EstaCorrendo", estadoAtual == EstadoChefe.Perseguindo || estadoAtual == EstadoChefe.Fugindo);
        animatorVigia.SetBool("Agarrando", estadoAtual == EstadoChefe.Agarrando);
        animatorVigia.SetBool("Assustado", estadoAtual == EstadoChefe.Assustado);
    }

    private void VirarParaDirecao(float direcaoX)
    {
        if (direcaoX == 0f)
            return;

        float sinal = Mathf.Sign(direcaoX);
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x) * sinal,
            transform.localScale.y,
            transform.localScale.z
        );
    }

    private void VerificarSandy()
    {
        if (sandy == null)
            return;

        float distancia = Mathf.Abs(sandy.position.x - transform.position.x);

        if (distancia <= distanciaPercepcao)
        {
            estadoAtual = EstadoChefe.Perseguindo;
        }
    }

    private void PerseguirSandy()
    {
        if (sandy == null)
        {
            PararMovimento();
            return;
        }

        float direcaoX = Mathf.Sign(
            sandy.position.x - transform.position.x
        );

        VirarParaDirecao(direcaoX);

        velocidadeAtual = new Vector2(
            direcaoX * velocidadePerseguicao,
            0f
        );
    }

    private void PararMovimento()
    {
        velocidadeAtual = Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D colisao)
    {
        if (estadoAtual != EstadoChefe.Perseguindo)
            return;

        if (colisao.GetComponentInParent<PlayerMovement>() == null)
            return;

        IniciarAgarrão();
    }

    private void IniciarAgarrão()
    {
        estadoAtual = EstadoChefe.Agarrando;

        PararMovimento();

        latidosAtuais = 0;
        tempoAgarrada = tempoMaximoAgarrada;
        MostrarPainelAgarramento();

        if (energiaSandy != null)
            energiaSandy.PerderEnergia();

        if (sandy != null && pontoDeAgarrar != null)
            sandy.position = pontoDeAgarrar.position;

        if (sandy != null)
        {
            controleSandy =
                sandy.GetComponent<ControleAgarramentoSandy>();

            if (controleSandy != null)
                controleSandy.Prender();
        }
    }

    private void AtualizarAgarrão()
    {
        tempoAgarrada -= Time.deltaTime;

        if (barraTempo != null)
        {
            barraTempo.value = Mathf.Clamp(
                tempoAgarrada,
                0f,
                tempoMaximoAgarrada
            );
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            latidosAtuais++;
            AtualizarTextoLatidos();
        }

        if (latidosAtuais >= latidosNecessarios)
        {
            EscaparDoAgarrão();
            return;
        }

        if (tempoAgarrada <= 0f)
        {
            EscaparPorTempoEsgotado();
        }
    }

    private void EscaparDoAgarrão()
    {
        if (controleSandy != null)
            controleSandy.Soltar();

        EsconderPainelAgarramento();

        direcaoFuga = sandy != null && sandy.position.x >= transform.position.x ? -1 : 1;
        VirarParaDirecao(direcaoFuga);

        tempoSustoRestante = duracaoSusto;
        estadoAtual = EstadoChefe.Assustado;

        if (audioSource != null && somSustoVigia != null)
            audioSource.PlayOneShot(somSustoVigia);
    }

    private void AtualizarSusto()
    {
        tempoSustoRestante -= Time.deltaTime;

        if (tempoSustoRestante <= 0f)
        {
            tempoFugindo = duracaoFuga;
            estadoAtual = EstadoChefe.Fugindo;
        }
    }

    private void EscaparPorTempoEsgotado()
    {
        if (controleSandy != null)
            controleSandy.Soltar();

        direcaoFuga = sandy != null && sandy.position.x >= transform.position.x ? -1 : 1;
        VirarParaDirecao(direcaoFuga);

        tempoFugindo = duracaoFuga;
        estadoAtual = EstadoChefe.Fugindo;
        EsconderPainelAgarramento();
    }

    private void AtualizarFuga()
    {
        tempoFugindo -= Time.deltaTime;

        VirarParaDirecao(direcaoFuga);

        velocidadeAtual = new Vector2(
            direcaoFuga * velocidadeFuga,
            0f
        );

        if (tempoFugindo <= 0f)
        {
            PararMovimento();
            tempoEsperaRetorno = tempoParaVoltar;
            estadoAtual = EstadoChefe.AguardandoRetorno;
        }
    }

    private void AtualizarRetorno()
    {
        tempoEsperaRetorno -= Time.deltaTime;

        if (tempoEsperaRetorno <= 0f)
        {
            if (voltarParaPosicaoInicial)
            {
                transform.position = posicaoInicial;
                if (rb != null)
                    rb.position = posicaoInicial;
            }

            estadoAtual = EstadoChefe.Perseguindo;
            Debug.Log("CHEFE VOLTOU PARA A PRAÇA!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaPercepcao);
    }

    private void MostrarPainelAgarramento()
    {
        if (painelAgarramento != null)
            painelAgarramento.SetActive(true);

        if (textoAviso != null)
            textoAviso.text = "SANDY FOI AGARRADA!";

        AtualizarTextoLatidos();

        if (barraTempo != null)
        {
            barraTempo.minValue = 0;
            barraTempo.maxValue = tempoMaximoAgarrada;
            barraTempo.value = tempoMaximoAgarrada;
        }
    }

    private void AtualizarTextoLatidos()
    {
        if (textoLatidos != null)
        {
            textoLatidos.text =
                "LATIDOS: " + latidosAtuais + " / " + latidosNecessarios;
        }
    }

    private void EsconderPainelAgarramento()
    {
        if (painelAgarramento != null)
            painelAgarramento.SetActive(false);
    }
}