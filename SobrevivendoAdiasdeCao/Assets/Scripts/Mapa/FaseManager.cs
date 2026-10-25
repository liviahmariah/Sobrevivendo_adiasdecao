
using UnityEngine;

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
    public TMPro.TextMeshProUGUI textoRecado;
    public TMPro.TextMeshProUGUI textoItensBons;
    public TMPro.TextMeshProUGUI textoItensRuins;
    public TMPro.TextMeshProUGUI textoSaldoFinal;
    public TMPro.TextMeshProUGUI textoEntregaComunidade;
    public TMPro.TextMeshProUGUI textoContinuar;

    [Header("Áudio do Resultado")]
    public AudioSource audioResultado;
    public AudioClip audioVitoria;
    public AudioClip audioDerrota;

    [Header("Confetes da Vitória")]
    public GameObject confetesVitoriaA;
    public GameObject confetesVitoriaB;

    [Header("Resultado")]
    public bool resultadoFoiVitoria = false;

    protected float tempoRestante;
    protected float tempoFugaRestante;

    private bool aguardandoContinuar = false;

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
        // Depois do resultado, só permite continuar com Espaço.
        if (faseFinalizada)
        {
            if (aguardandoContinuar &&
                Input.GetKeyDown(KeyCode.Space))
            {
                ContinuarParaMapa();
            }

            return;
        }

        if (coletaAtiva)
        {
            AtualizarTimerColeta();
        }
        else if (fugaAtiva)
        {
            AtualizarTimerFuga();
        }
    }

    protected virtual void IniciarFase()
    {
        tempoRestante = CalcularTempoDaFase();
        tempoFugaRestante = tempoFuga;

        faseAtiva = true;
        coletaAtiva = true;
        fugaAtiva = false;
        faseFinalizada = false;
        resultadoFoiVitoria = false;
        aguardandoContinuar = false;

        if (carrocinha != null)
            carrocinha.Desativar();

        if (painelFuga != null)
            painelFuga.SetActive(false);

        if (painelResultado != null)
            painelResultado.SetActive(false);

        if (confetesVitoriaA != null)
            confetesVitoriaA.SetActive(false);

        if (confetesVitoriaB != null)
            confetesVitoriaB.SetActive(false);

        Debug.Log("FASE INICIADA: " + tipoFase);
        Debug.Log("Tempo de coleta: " + tempoRestante.ToString("F1") + " segundos");
    }

    protected virtual float CalcularTempoDaFase()
    {
        return Mathf.Clamp(tempoMaximo, tempoMinimo, tempoMaximo);
    }

    protected virtual void AtualizarTimerColeta()
    {
        tempoRestante -= Time.deltaTime;

        if (tempoRestante <= 0f)
        {
            tempoRestante = 0f;
            IniciarFuga();
        }
    }

    protected virtual void IniciarFuga()
    {
        if (faseFinalizada)
            return;

        coletaAtiva = false;
        fugaAtiva = true;
        tempoFugaRestante = tempoFuga;

        if (painelFuga != null)
            painelFuga.SetActive(true);

        if (textoAviso != null)
            textoAviso.text = "FUJA DA CARROCINHA!";

        AtualizarTextoFuga();

        if (carrocinha != null)
            carrocinha.Ativar();

        Debug.Log("FASE: CARROCINHA ATIVADA!");
    }

    protected virtual void AtualizarTimerFuga()
    {
        tempoFugaRestante -= Time.deltaTime;

        if (tempoFugaRestante < 0f)
            tempoFugaRestante = 0f;

        AtualizarTextoFuga();

        if (tempoFugaRestante <= 0f)
            Vitoria();
    }

    protected void AtualizarTextoFuga()
    {
        if (textoContador != null)
        {
            int segundos = Mathf.CeilToInt(tempoFugaRestante);
            textoContador.text = segundos + "s";
        }
    }

    protected virtual void Vitoria()
    {
        if (faseFinalizada)
            return;

        resultadoFoiVitoria = true;

        Debug.Log("FASE: VITÓRIA!");

        if (carrocinha != null)
            carrocinha.Desativar();

        if (painelFuga != null)
            painelFuga.SetActive(false);

        MostrarResultado("VITÓRIA!");
    }

    public virtual void Derrota()
    {
        if (faseFinalizada)
            return;

        resultadoFoiVitoria = false;

        Debug.Log("FASE: DERROTA!");

        if (carrocinha != null)
            carrocinha.Desativar();

        if (painelFuga != null)
            painelFuga.SetActive(false);

        MostrarResultado("DERROTA!");
    }

    protected virtual void AtualizarEstatisticasResultado()
    {
        if (textoRecado != null)
        {
            textoRecado.text = resultadoFoiVitoria
                ? "Missão concluída!"
                : "Missão não concluída.";
            textoRecado.gameObject.SetActive(true);
        }

        if (textoItensBons != null)
            textoItensBons.gameObject.SetActive(false);

        if (textoItensRuins != null)
            textoItensRuins.gameObject.SetActive(false);

        if (textoSaldoFinal != null)
            textoSaldoFinal.gameObject.SetActive(false);

        if (textoEntregaComunidade != null)
            textoEntregaComunidade.gameObject.SetActive(false);
    }

    protected void MostrarResultado(string resultado)
    {
        faseFinalizada = true;
        faseAtiva = false;
        coletaAtiva = false;
        fugaAtiva = false;
        aguardandoContinuar = true;

        if (painelResultado != null)
            painelResultado.SetActive(true);

        if (textoResultado != null)
            textoResultado.text = resultado;

        if (textoContinuar != null)
        {
            textoContinuar.text = "APERTE ESPAÇO PARA CONTINUAR";
            textoContinuar.gameObject.SetActive(true);
        }

        AtualizarEstatisticasResultado();
        TocarAudioResultado();
        AtualizarConfetesResultado();
    }

    private void TocarAudioResultado()
    {
        if (audioResultado == null)
        {
            Debug.LogWarning("FaseManager: AudioSource do resultado não conectado.");
            return;
        }

        AudioClip clip = resultadoFoiVitoria ? audioVitoria : audioDerrota;

        if (clip != null)
        {
            audioResultado.Stop();
            audioResultado.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("FaseManager: áudio do resultado não conectado.");
        }
    }

    private void AtualizarConfetesResultado()
    {
        if (confetesVitoriaA != null)
            confetesVitoriaA.SetActive(resultadoFoiVitoria);

        if (confetesVitoriaB != null)
            confetesVitoriaB.SetActive(resultadoFoiVitoria);
    }

    private void ContinuarParaMapa()
    {
        if (!aguardandoContinuar)
            return;

        aguardandoContinuar = false;

        if (painelResultado != null)
            painelResultado.SetActive(false);

        if (confetesVitoriaA != null)
            confetesVitoriaA.SetActive(false);

        if (confetesVitoriaB != null)
            confetesVitoriaB.SetActive(false);

        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.VoltarParaMapa();
        }
        else
        {
            Debug.LogWarning("FaseManager: PhaseManager não encontrado!");
        }
    }

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
        return coletaAtiva && !faseFinalizada;
    }

    public bool EstaEmFuga()
    {
        return fugaAtiva && !faseFinalizada;
    }
}
