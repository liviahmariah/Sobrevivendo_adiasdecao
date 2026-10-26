
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Resultado Final")]
    public float mediaFinal;
    public string resultadoFinal;

    private bool[] diasRegistrados;
    private bool diaAtivo = true;
    private bool fimDoDiaPendente;
    private bool transicaoEmAndamento;
    private bool jogoFinalizado;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (totalDias < 1)
            totalDias = 15;

        if (duracaoDia <= 0f)
            duracaoDia = 90f;

        tempoRestante = duracaoDia;
        mediasDosDias = new float[totalDias];
        diasRegistrados = new bool[totalDias];

        Debug.Log("DayManager iniciado. Dia " + diaAtual);
    }

    private void Update()
    {
        if (!diaAtivo || jogoFinalizado)
            return;

        tempoRestante -= Time.deltaTime;

        if (tempoRestante <= 0f)
        {
            tempoRestante = 0f;
            diaAtivo = false;

            PrepararFimDoDia();
        }
    }

    private void PrepararFimDoDia()
    {
        if (fimDoDiaPendente || jogoFinalizado)
            return;

        fimDoDiaPendente = true;

        RegistrarMediaDoDia();

        Debug.Log(
            "Fim do dia " + diaAtual +
            ". Aguardando momento seguro para a transição."
        );

        TentarIniciarTransicaoPendente();
    }

    public void TentarIniciarTransicaoPendente()
    {
        if (!fimDoDiaPendente ||
            transicaoEmAndamento ||
            jogoFinalizado)
            return;

        // A transição só começa no mapa.
        if (SceneManager.GetActiveScene().name != "Mapa")
            return;

        if (PhaseManager.Instance != null &&
            PhaseManager.Instance.EstaEmFase())
            return;

        // Inclui objetos inativos na busca.
        TransicaoDiaUI transicao =
            FindObjectOfType<TransicaoDiaUI>(true);

        if (transicao == null)
        {
            Debug.LogError(
                "DayManager: TransicaoDiaUI não encontrada na cena Mapa."
            );
            return;
        }

        transicaoEmAndamento = true;

        Debug.Log("Iniciando transição do dia " + diaAtual);

        transicao.IniciarTransicao(ConcluirTransicao);
    }

    private void ConcluirTransicao()
    {
        if (jogoFinalizado)
            return;

        Debug.Log("Transição do dia " + diaAtual + " concluída.");

        transicaoEmAndamento = false;
        fimDoDiaPendente = false;

        // O último dia termina sem criar um dia 16.
        if (diaAtual >= totalDias)
        {
            FinalizarJogo();
            return;
        }

        diaAtual++;
        tempoRestante = duracaoDia;
        diaAtivo = true;

        Debug.Log(
            "Novo dia: " + diaAtual +
            " | Tempo: " + tempoRestante
        );
    }

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

        if (indice < 0 || indice >= mediasDosDias.Length)
            return;

        mediasDosDias[indice] = media;
        diasRegistrados[indice] = true;

        Debug.Log(
            "Dia " + diaAtual + " registrado. " +
            "Alimentação: " +
            CommunityManager.instance.alimentacao.ToString("F1") +
            " | Saúde: " +
            CommunityManager.instance.saude.ToString("F1") +
            " | Felicidade: " +
            CommunityManager.instance.felicidade.ToString("F1") +
            " | Média: " + media.ToString("F1")
        );
    }

    private void FinalizarJogo()
    {
        diaAtivo = false;
        jogoFinalizado = true;

        mediaFinal = CalcularMediaFinal();
        resultadoFinal = DeterminarResultadoFinal();

        Debug.Log("JOGO FINALIZADO");
        Debug.Log("Média final: " + mediaFinal.ToString("F1"));
        Debug.Log("Resultado: " + resultadoFinal);
    }

    private float CalcularMediaFinal()
    {
        float soma = 0f;
        int quantidadeDias = 0;

        for (int i = 0; i < mediasDosDias.Length; i++)
        {
            if (!diasRegistrados[i])
                continue;

            soma += mediasDosDias[i];
            quantidadeDias++;
        }

        return quantidadeDias == 0 ? 0f : soma / quantidadeDias;
    }

    private string DeterminarResultadoFinal()
    {
        if (mediaFinal > 75f) return "Final Bom";
        if (mediaFinal >= 45f) return "Final Médio";
        return "Final Ruim";
    }

    public float ObterTempoRestante() => tempoRestante;
    public bool DiaEstaAtivo() => diaAtivo;

    public float ObterMediaDoDia(int numeroDoDia)
    {
        int indice = numeroDoDia - 1;

        if (indice < 0 || indice >= mediasDosDias.Length)
            return 0f;

        return diasRegistrados[indice]
            ? mediasDosDias[indice]
            : 0f;
    }
}
