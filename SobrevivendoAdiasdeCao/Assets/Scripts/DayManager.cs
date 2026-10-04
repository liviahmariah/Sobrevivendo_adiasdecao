using UnityEngine;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    [Header("Configuração")]
    public int totalDias = 15;
    public float duracaoDia = 90f;

    [Header("Estado Atual")]
    public int diaAtual = 1;
    public float tempoRestante;

    [Header("Histórico dos Dias")]
    public float[] mediasDosDias;

    private bool[] diasRegistrados;

    [Header("Resultado Final")]
    public float mediaFinal;
    public string resultadoFinal;

    private bool diaAtivo = true;

    private void Awake()
    {
        // Impede outro DayManager de existir
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "DayManager duplicado! Destruindo este objeto."
            );

            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        // Inicializa apenas uma vez
        tempoRestante = duracaoDia;

        mediasDosDias = new float[totalDias];
        diasRegistrados = new bool[totalDias];

        Debug.Log(
            "DAY MANAGER INICIADO | Dia: " +
            diaAtual +
            " | Tempo: " +
            tempoRestante
        );
    }

    private void Update()
    {
        if (!diaAtivo)
            return;

        tempoRestante -= Time.deltaTime;

        if (tempoRestante <= 0f)
        {
            tempoRestante = 0f;

            FinalizarDia();
        }
    }

    // =====================================================
    // FINALIZAR DIA
    // =====================================================

    private void FinalizarDia()
    {
        Debug.Log(
            "FINALIZANDO DIA " + diaAtual
        );

        // Primeiro registra o estado da comunidade
        RegistrarMediaDoDia();

        // Depois verifica se ainda existem dias
        if (diaAtual < totalDias)
        {
            diaAtual++;

            tempoRestante = duracaoDia;

            Debug.Log(
                "NOVO DIA: " +
                diaAtual +
                " | Tempo: " +
                tempoRestante
            );
        }
        else
        {
            FinalizarJogo();
        }
    }

    // =====================================================
    // REGISTRAR MÉDIA DO DIA
    // =====================================================

    private void RegistrarMediaDoDia()
    {
        if (CommunityManager.instance == null)
        {
            Debug.LogWarning(
                "DayManager: CommunityManager não encontrado."
            );

            return;
        }

        float media =
            CommunityManager.instance.MediaComunidade();

        int indice = diaAtual - 1;

        if (indice >= 0 && indice < mediasDosDias.Length)
        {
            mediasDosDias[indice] = media;
            diasRegistrados[indice] = true;

            Debug.Log(
                "DIA " +
                diaAtual +
                " REGISTRADO | " +
                "Alimentação: " +
                CommunityManager.instance.alimentacao.ToString("F1") +
                " | Saúde: " +
                CommunityManager.instance.saude.ToString("F1") +
                " | Felicidade: " +
                CommunityManager.instance.felicidade.ToString("F1") +
                " | MÉDIA: " +
                media.ToString("F1")
            );
        }
    }

    // =====================================================
    // FINAL DO JOGO
    // =====================================================

    private void FinalizarJogo()
    {
        diaAtivo = false;

        mediaFinal = CalcularMediaFinal();
        resultadoFinal = DeterminarResultadoFinal();

        Debug.Log("=================================");
        Debug.Log("JOGO FINALIZADO");
        Debug.Log(
            "MÉDIA FINAL: " +
            mediaFinal.ToString("F1")
        );
        Debug.Log(
            "RESULTADO: " +
            resultadoFinal
        );
        Debug.Log("=================================");
    }

    // =====================================================
    // CALCULAR MÉDIA DOS DIAS
    // =====================================================

    private float CalcularMediaFinal()
    {
        float soma = 0f;
        int quantidadeDias = 0;

        for (int i = 0; i < mediasDosDias.Length; i++)
        {
            if (diasRegistrados[i])
            {
                soma += mediasDosDias[i];
                quantidadeDias++;
            }
        }

        if (quantidadeDias == 0)
            return 0f;

        return soma / quantidadeDias;
    }

    // =====================================================
    // RESULTADO FINAL
    // =====================================================

    private string DeterminarResultadoFinal()
    {
        if (mediaFinal > 75f)
            return "Final Bom";

        if (mediaFinal >= 45f)
            return "Final Médio";

        return "Final Ruim";
    }

    // =====================================================
    // ACESSO
    // =====================================================

    public float ObterTempoRestante()
    {
        return tempoRestante;
    }

    public bool DiaEstaAtivo()
    {
        return diaAtivo;
    }

    public float ObterMediaDoDia(int numeroDoDia)
    {
        int indice = numeroDoDia - 1;

        if (indice < 0 || indice >= mediasDosDias.Length)
            return 0f;

        if (!diasRegistrados[indice])
            return 0f;

        return mediasDosDias[indice];
    }
}