using UnityEngine;
using System.Collections;

public class FaseManager : MonoBehaviour
{
    public static FaseManager Instance;

    public enum TipoFase
    {
        Mercado,
        Praca,
        Hospital
    }

    [Header("Identificação")]
    public TipoFase tipoFase;

    [Header("Tempo da fase")]
    public float tempoMinimo = 10f;
    public float tempoMaximo = 60f;

    [Header("Tempo de fuga")]
    public float tempoFuga = 15f;

    [Header("Estado")]
    public bool faseAtiva = false;
    public bool coletaAtiva = false;
    public bool fugaAtiva = false;
    public bool faseFinalizada = false;

    [Header("Carrocinha")]
    public CarrocinhaMercado carrocinha;

    [Header("UI da Fuga")]
    public GameObject painelFuga;
    public TMPro.TextMeshProUGUI textoAviso;
    public TMPro.TextMeshProUGUI textoContador;

    [Header("UI do Resultado")]
    public GameObject painelResultado;
    public TMPro.TextMeshProUGUI textoResultado;

    [Header("Resultado")]
    public float tempoTelaResultado = 2.5f;

    protected float tempoRestante;
    protected float tempoFugaRestante;

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void Start()
    {
        IniciarFase();
    }

    protected virtual void Update()
    {
        if (faseFinalizada)
            return;

        if (coletaAtiva)
        {
            AtualizarTimerColeta();
        }
        else if (fugaAtiva)
        {
            AtualizarTimerFuga();
        }
    }

    // ==========================================
    // INÍCIO DA FASE
    // ==========================================

    protected virtual void IniciarFase()
    {
        tempoRestante = CalcularTempoDaFase();
        tempoFugaRestante = tempoFuga;

        faseAtiva = true;
        coletaAtiva = true;
        fugaAtiva = false;
        faseFinalizada = false;

        if (carrocinha != null)
        {
            carrocinha.Desativar();
        }

        if (painelFuga != null)
        {
            painelFuga.SetActive(false);
        }

        if (painelResultado != null)
        {
            painelResultado.SetActive(false);
        }

        Debug.Log(
            "FASE INICIADA: " +
            tipoFase
        );

        Debug.Log(
            "Tempo de coleta: " +
            tempoRestante.ToString("F1") +
            " segundos"
        );
    }

    protected virtual float CalcularTempoDaFase()
    {
        return Mathf.Clamp(
            tempoMaximo,
            tempoMinimo,
            tempoMaximo
        );
    }

    // ==========================================
    // TIMER DE COLETA
    // ==========================================

    protected virtual void AtualizarTimerColeta()
    {
        tempoRestante -= Time.deltaTime;

        if (tempoRestante <= 0f)
        {
            tempoRestante = 0f;

            IniciarFuga();
        }
    }

    // ==========================================
    // INÍCIO DA FUGA
    // ==========================================

    protected virtual void IniciarFuga()
    {
        if (faseFinalizada)
            return;

        coletaAtiva = false;
        fugaAtiva = true;

        tempoFugaRestante = tempoFuga;

        if (painelFuga != null)
        {
            painelFuga.SetActive(true);
        }

        if (textoAviso != null)
        {
            textoAviso.text =
                "FUJA DA CARROCINHA!";
        }

        AtualizarTextoFuga();

        if (carrocinha != null)
        {
            carrocinha.Ativar();
        }

        Debug.Log(
            "FASE: CARROCINHA ATIVADA!"
        );
    }

    // ==========================================
    // TIMER DE FUGA
    // ==========================================

    protected virtual void AtualizarTimerFuga()
    {
        tempoFugaRestante -= Time.deltaTime;

        if (tempoFugaRestante < 0f)
        {
            tempoFugaRestante = 0f;
        }

        AtualizarTextoFuga();

        if (tempoFugaRestante <= 0f)
        {
            Vitoria();
        }
    }

    protected void AtualizarTextoFuga()
    {
        if (textoContador != null)
        {
            int segundos =
                Mathf.CeilToInt(
                    tempoFugaRestante
                );

            textoContador.text =
                segundos + "s";
        }
    }

    // ==========================================
    // VITÓRIA
    // ==========================================

    protected virtual void Vitoria()
    {
        if (faseFinalizada)
            return;

        Debug.Log(
            "FASE: VITÓRIA!"
        );

        if (carrocinha != null)
        {
            carrocinha.Desativar();
        }

        if (painelFuga != null)
        {
            painelFuga.SetActive(false);
        }

        MostrarResultado(
            "VITÓRIA!"
        );
    }

    // ==========================================
    // DERROTA
    // ==========================================

    public virtual void Derrota()
    {
        if (faseFinalizada)
            return;

        Debug.Log(
            "FASE: DERROTA!"
        );

        if (carrocinha != null)
        {
            carrocinha.Desativar();
        }

        if (painelFuga != null)
        {
            painelFuga.SetActive(false);
        }

        MostrarResultado(
            "DERROTA!"
        );
    }

    // ==========================================
    // RESULTADO
    // ==========================================

    protected void MostrarResultado(string resultado)
    {
        faseFinalizada = true;
        faseAtiva = false;
        coletaAtiva = false;
        fugaAtiva = false;

        if (painelResultado != null)
        {
            painelResultado.SetActive(true);
        }

        if (textoResultado != null)
        {
            textoResultado.text = resultado;
        }

        StartCoroutine(
            VoltarAoMapaDepoisDoResultado()
        );
    }

    private IEnumerator VoltarAoMapaDepoisDoResultado()
    {
        yield return new WaitForSeconds(
            tempoTelaResultado
        );

        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.VoltarParaMapa();
        }
        else
        {
            Debug.LogWarning(
                "FaseManager: PhaseManager não encontrado!"
            );
        }
    }

    // ==========================================
    // ACESSO AO TIMER
    // ==========================================

    public float ObterTempoRestante()
    {
        if (coletaAtiva)
            return tempoRestante;

        return 0f;
    }

    public float ObterTempoFugaRestante()
    {
        return tempoFugaRestante;
    }

    public bool PodeColetar()
    {
        return coletaAtiva &&
               !faseFinalizada;
    }

    public bool EstaEmFuga()
    {
        return fugaAtiva &&
               !faseFinalizada;
    }
}