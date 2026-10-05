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

        base.Start();
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