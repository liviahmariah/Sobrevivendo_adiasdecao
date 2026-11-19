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

        if (totalDias < 1) totalDias = 15;
        if (duracaoDia <= 0f) duracaoDia = 90f;

        tempoRestante = duracaoDia;
        mediasDosDias = new float[totalDias];
        diasRegistrados = new bool[totalDias];
    }

    private void Update()
    {
        if (!diaAtivo || jogoFinalizado || diaAtual >= totalDias)
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
        RegistrarMediaDoDia(diaAtual);
        TentarIniciarTransicaoPendente();
    }

    public void TentarIniciarTransicaoPendente()
    {
        if (!fimDoDiaPendente || transicaoEmAndamento || jogoFinalizado)
            return;

        if (SceneManager.GetActiveScene().name != "Mapa")
            return;

        if (PhaseManager.Instance != null && PhaseManager.Instance.EstaEmFase())
            return;

        TransicaoDiaUI transicao = FindFirstObjectByType<TransicaoDiaUI>(FindObjectsInactive.Include);
        if (transicao == null)
            return;

        transicaoEmAndamento = true;
        transicao.IniciarTransicao(ConcluirTransicao);
    }

    private void ConcluirTransicao()
    {
        if (jogoFinalizado)
            return;

        transicaoEmAndamento = false;
        fimDoDiaPendente = false;

        diaAtual++;

        if (diaAtual >= totalDias)
        {
            PrepararUltimoDia();
            return;
        }

        tempoRestante = duracaoDia;
        diaAtivo = true;
    }

    private void PrepararUltimoDia()
    {
        diaAtivo = false;
        tempoRestante = 0f;

        RegistrarMediaDoDia(totalDias);
        mediaFinal = CalcularMediaFinal();
        resultadoFinal = DeterminarResultadoFinal();

        MapDialogueManager dialogoMapa = FindFirstObjectByType<MapDialogueManager>(FindObjectsInactive.Include);
        if (dialogoMapa != null)
        {
            dialogoMapa.IniciarDiscursoFinalDia15();
        }
        else
        {
            Debug.LogWarning("MapDialogueManager não encontrado na cena Mapa. Indo direto para CenaFinal.");
            IrParaCenaFinal();
        }
    }

    public void IrParaCenaFinal()
    {
        if (jogoFinalizado)
            return;

        jogoFinalizado = true;
        SceneManager.LoadScene("CenaFinal");
    }

    private void RegistrarMediaDoDia(int dia)
    {
        if (CommunityManager.instance == null)
            return;

        int indice = dia - 1;
        if (indice < 0 || indice >= mediasDosDias.Length)
            return;

        mediasDosDias[indice] = CommunityManager.instance.MediaComunidade();
        diasRegistrados[indice] = true;
    }

    private float CalcularMediaFinal()
    {
        float soma = 0f;
        int quantidadeDias = 0;

        for (int i = 0; i < mediasDosDias.Length; i++)
        {
            if (!diasRegistrados[i]) continue;
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
    public bool EhUltimoDia() => diaAtual >= totalDias;

    public float ObterMediaDoDia(int numeroDoDia)
    {
        int indice = numeroDoDia - 1;

        if (indice < 0 || indice >= mediasDosDias.Length)
            return 0f;

        return diasRegistrados[indice] ? mediasDosDias[indice] : 0f;
    }
}