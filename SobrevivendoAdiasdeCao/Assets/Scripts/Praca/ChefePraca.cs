
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
        Fugindo
    }

    [Header("Estado")]
    public EstadoChefe estadoAtual = EstadoChefe.Parado;

    [Header("Alvo")]
    public Transform sandy;
    public EnergiaSandy energiaSandy;
    public Transform pontoDeAgarrar;

    [Header("Percepção")]
    public float distanciaPercepcao = 5f;

    [Header("Movimento")]
    public float velocidadePerseguicao = 3.5f;

    [Header("Agarrão")]
    public int latidosNecessarios = 6;
    public float tempoMaximoAgarrada = 4f;

    [Header("Fuga do chefe")]
    public float duracaoFuga = 3f;
    public float velocidadeFuga = 4.5f;

    [Header("HUD do agarrão")]
    public GameObject painelAgarramento;
    public TMP_Text textoAviso;
    public TMP_Text textoLatidos;
    public Slider barraTempo;

    private int latidosAtuais = 0;
    private float tempoAgarrada;
    private float tempoFugindo;
    private int direcaoFuga;

    private Rigidbody2D rb;
    private Vector2 velocidadeAtual;
    private ControleAgarramentoSandy controleSandy;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
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

            case EstadoChefe.Fugindo:
                AtualizarFuga();
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

    private void VerificarSandy()
    {
        if (sandy == null)
            return;

        float distancia = Vector2.Distance(
            transform.position,
            sandy.position
        );

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
            else
                Debug.LogError(
                    "ControleAgarramentoSandy não encontrado na Sandy!"
                );
        }

        Debug.Log("CHEFE DA PRAÇA AGARROU SANDY!");
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

            Debug.Log("TECLA Z DETECTADA");
            Debug.Log("LATIDOS: " + latidosAtuais + "/" + latidosNecessarios);
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

        direcaoFuga = sandy.position.x >= transform.position.x ? -1 : 1;

        tempoFugindo = duracaoFuga;
        estadoAtual = EstadoChefe.Fugindo;

        Debug.Log("SANDY ESCAPOU! CHEFE ESTÁ FUGINDO!");
        EsconderPainelAgarramento();
    }

    private void EscaparPorTempoEsgotado()
    {
        if (controleSandy != null)
            controleSandy.Soltar();

        if (FaseManager.Instance != null)
        {
            Debug.Log("TEMPO DE COLETA PERDIDO!");
        }

        direcaoFuga = sandy.position.x >= transform.position.x ? -1 : 1;

        tempoFugindo = duracaoFuga;
        estadoAtual = EstadoChefe.Fugindo;
        EsconderPainelAgarramento();
    }

    private void AtualizarFuga()
    {
        tempoFugindo -= Time.deltaTime;

        velocidadeAtual = new Vector2(
            direcaoFuga * velocidadeFuga,
            0f
        );

        if (tempoFugindo <= 0f)
        {
            PararMovimento();
            estadoAtual = EstadoChefe.Parado;

            Debug.Log("CHEFE VOLTOU A ESPERAR SANDY!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            distanciaPercepcao
        );
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
