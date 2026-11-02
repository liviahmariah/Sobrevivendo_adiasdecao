using UnityEngine;

public class PracaManager : FaseManager
{
    public static PracaManager Instance;

    [Header("Diversão conseguida nesta fase")]
    public float diversaoConseguida = 0f;

    [Header("Penalidade por ser pega")]
    public float perdaFelicidadeAoSerPega = 10f;

    protected override void Awake()
    {
        base.Awake();

        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected override void Start()
    {
        tipoFase = TipoFase.Praca;
        diversaoConseguida = 0f;

        Debug.Log("PRAÇA | Start iniciado.");
        Debug.Log("PRAÇA | CommunityManager: " +
            (CommunityManager.instance != null));

        base.Start();

        Debug.Log("PRAÇA | Fase ativa: " + faseAtiva);
        Debug.Log("PRAÇA | Coleta ativa: " + coletaAtiva);
        Debug.Log("PRAÇA | Tempo restante: " + ObterTempoRestante());
    }

    protected override float CalcularTempoDaFase()
    {
        if (CommunityManager.instance == null)
        {
            Debug.LogWarning(
                "PracaManager: CommunityManager não encontrado!"
            );

            return tempoMaximo;
        }

        float felicidade = Mathf.Clamp(
            CommunityManager.instance.felicidade,
            0f,
            100f
        );

        float percentual = felicidade / 100f;

        float tempo = Mathf.Lerp(
            tempoMinimo,
            tempoMaximo,
            percentual
        );

        Debug.Log(
            "PRAÇA | Felicidade: " + felicidade +
            " | Tempo calculado: " + tempo
        );

        return tempo;
    }


    // ==========================================
    // DIVERSÃO
    // ==========================================

    public void AdicionarDiversao(float valor)
    {
        if (faseFinalizada)
            return;

        if (!coletaAtiva)
            return;

        diversaoConseguida += valor;

        if (diversaoConseguida < 0f)
        {
            diversaoConseguida = 0f;
        }

        Debug.Log(
            "PRAÇA | Diversão conseguida: " +
            diversaoConseguida
        );

        if (FindFirstObjectByType<PontuacaoSandyUI>() != null)
        {
            FindFirstObjectByType<PontuacaoSandyUI>()
                .AtualizarPontuacao();
        }
    }

    // ==========================================
    // VITÓRIA
    // ==========================================

    protected override void Vitoria()
    {
        if (faseFinalizada)
            return;

        Debug.Log("=================================");
        Debug.Log("PRAÇA: VITÓRIA!");
        Debug.Log(
            "Diversão conseguida: " +
            diversaoConseguida
        );
        Debug.Log("=================================");

        if (CommunityManager.instance != null)
        {
            CommunityManager.instance.AlterarFelicidade(
                diversaoConseguida
            );
        }

        base.Vitoria();
    }

    // ==========================================
    // DERROTA
    // ==========================================

    public override void Derrota()
    {
        if (faseFinalizada)
            return;

        Debug.Log("=================================");
        Debug.Log("PRAÇA: DERROTA!");
        Debug.Log(
            "Diversão perdida: " +
            diversaoConseguida
        );

        if (CommunityManager.instance != null)
        {
            CommunityManager.instance.AlterarFelicidade(
                -perdaFelicidadeAoSerPega
            );

            Debug.Log(
                "Felicidade perdida: " +
                perdaFelicidadeAoSerPega
            );
        }

        Debug.Log("=================================");

        base.Derrota();
    }
}