using UnityEngine;
using TMPro;

public class DayManager : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================

    public static DayManager Instance;


    // =========================================================
    // CONFIGURAÇÃO DOS DIAS
    // =========================================================

    [Header("Configuração dos Dias")]

    [Tooltip("Quantidade total de dias da campanha.")]
    public int totalDias = 15;

    [Tooltip("Duração de cada dia no mapa, em segundos.")]
    public float duracaoDia = 90f;


    // =========================================================
    // ESTADO ATUAL
    // =========================================================

    [Header("Estado Atual")]

    public int diaAtual = 1;

    public float tempoRestante;

    private bool diaAtivo = true;


    // =========================================================
    // INTERFACE
    // =========================================================

    [Header("Interface")]

    public TextMeshProUGUI textoDia;
    public TextMeshProUGUI textoTempo;


    // =========================================================
    // HISTÓRICO DOS DIAS
    // =========================================================

    [Header("Histórico")]

    public float[] mediasDosDias;


    // =========================================================
    // MÉDIA FINAL
    // =========================================================

    [Header("Resultado Final")]

    public float mediaFinal;

    public string resultadoFinal;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        mediasDosDias = new float[totalDias];

        tempoRestante = duracaoDia;
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        AtualizarUI();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!diaAtivo)
            return;

        ContarTempo();

        AtualizarUI();
    }


    // =========================================================
    // CONTAGEM DO TEMPO
    // =========================================================

    private void ContarTempo()
    {
        tempoRestante -= Time.deltaTime;

        if (tempoRestante <= 0f)
        {
            tempoRestante = 0f;

            FinalizarDia();
        }
    }


    // =========================================================
    // FINALIZAR DIA
    // =========================================================

    public void FinalizarDia()
    {
        if (!diaAtivo)
            return;

        diaAtivo = false;

        RegistrarMediaDoDia();

        // Ainda existem dias?
        if (diaAtual < totalDias)
        {
            IniciarProximoDia();
        }
        else
        {
            FinalizarJogo();
        }
    }


    // =========================================================
    // REGISTRAR MÉDIA DO DIA
    // =========================================================

    private void RegistrarMediaDoDia()
    {
        if (CommunityManager.instance == null)
        {
            Debug.LogWarning(
                "DayManager: CommunityManager não encontrado."
            );

            return;
        }

        float media = CommunityManager.instance.MediaComunidade();

        int indice = diaAtual - 1;

        if (indice >= 0 && indice < mediasDosDias.Length)
        {
            mediasDosDias[indice] = media;
        }

        Debug.Log(
            "Dia " + diaAtual +
            " finalizado. Média: " + media.ToString("F1")
        );
    }


    // =========================================================
    // INICIAR PRÓXIMO DIA
    // =========================================================

    private void IniciarProximoDia()
    {
        diaAtual++;

        tempoRestante = duracaoDia;

        diaAtivo = true;

        AtualizarUI();

        Debug.Log("Iniciando Dia " + diaAtual);
    }


    // =========================================================
    // FINALIZAR JOGO
    // =========================================================

    private void FinalizarJogo()
    {
        mediaFinal = CalcularMediaFinal();

        resultadoFinal = DeterminarResultadoFinal();

        Debug.Log(
            "JOGO FINALIZADO!"
        );

        Debug.Log(
            "Média final: " + mediaFinal.ToString("F1")
        );

        Debug.Log(
            "Resultado: " + resultadoFinal
        );

        // A tela de final será implementada depois.
    }


    // =========================================================
    // CALCULAR MÉDIA FINAL
    // =========================================================

    private float CalcularMediaFinal()
    {
        float soma = 0f;
        int quantidadeDias = 0;

        for (int i = 0; i < mediasDosDias.Length; i++)
        {
            // Só contabiliza dias que realmente foram registrados.
            if (mediasDosDias[i] > 0f)
            {
                soma += mediasDosDias[i];
                quantidadeDias++;
            }
        }

        if (quantidadeDias == 0)
            return 0f;

        return soma / quantidadeDias;
    }


    // =========================================================
    // DEFINIR FINAL
    // =========================================================

    private string DeterminarResultadoFinal()
    {
        if (mediaFinal > 75f)
        {
            return "Final Bom";
        }

        if (mediaFinal >= 45f)
        {
            return "Final Médio";
        }

        return "Final Ruim";
    }


    // =========================================================
    // PAUSAR O TEMPO DO MAPA
    // =========================================================

    public void PausarDia()
    {
        diaAtivo = false;
    }


    // =========================================================
    // RETOMAR O TEMPO DO MAPA
    // =========================================================

    public void RetomarDia()
    {
        // Não permite retomar depois que o jogo terminou.
        if (diaAtual > totalDias)
            return;

        diaAtivo = true;
    }


    // =========================================================
    // VERIFICAR SE O DIA ESTÁ ATIVO
    // =========================================================

    public bool DiaEstaAtivo()
    {
        return diaAtivo;
    }


    // =========================================================
    // VERIFICAR TEMPO
    // =========================================================

    public float ObterTempoRestante()
    {
        return tempoRestante;
    }


    // =========================================================
    // ATUALIZAR INTERFACE
    // =========================================================

    private void AtualizarUI()
    {
        if (textoDia != null)
        {
            textoDia.text = "DIA " + diaAtual;
        }

        if (textoTempo != null)
        {
            int segundos = Mathf.CeilToInt(tempoRestante);

            textoTempo.text = segundos + "s";
        }
    }
}