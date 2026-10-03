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

    [Header("Histórico")]
    public float[] mediasDosDias;

    [Header("Resultado Final")]
    public float mediaFinal;
    public string resultadoFinal;

    private bool diaAtivo = true;

    private void Awake()
    {
        // Impede a criação de outro DayManager
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

        // Inicialização feita SOMENTE uma vez
        tempoRestante = duracaoDia;
        mediasDosDias = new float[totalDias];

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
            diaAtivo = false;

            Debug.Log("TODOS OS DIAS TERMINARAM.");

            mediaFinal = CalcularMediaFinal();
            resultadoFinal = DeterminarResultadoFinal();
        }
    }

    // =====================================================
    // MÉDIA
    // =====================================================

    private float CalcularMediaFinal()
    {
        if (CommunityManager.instance == null)
            return 0f;

        return CommunityManager.instance.MediaComunidade();
    }

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
}