using UnityEngine;

public class HospitalManager : FaseManager
{
    public static HospitalManager Instance;

    [Header("Remédios conseguidos nesta fase")]
    public float remediosConseguidos = 0f;

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
        tipoFase = TipoFase.Hospital;

        remediosConseguidos = 0f;

        base.Start();
    }

    // ==========================================
    // REMÉDIOS
    // ==========================================

    public void AdicionarRemedios(float valor)
    {
        if (faseFinalizada)
            return;

        if (!coletaAtiva)
            return;

        remediosConseguidos += valor;

        if (remediosConseguidos < 0f)
        {
            remediosConseguidos = 0f;
        }

        Debug.Log(
            "HOSPITAL | Remédios conseguidos: " +
            remediosConseguidos
        );
    }

    public float ObterRemediosConseguidos()
    {
        return remediosConseguidos;
    }

    // ==========================================
    // VITÓRIA
    // ==========================================

    protected override void Vitoria()
    {
        if (faseFinalizada)
            return;

        Debug.Log("=================================");
        Debug.Log("HOSPITAL: VITÓRIA!");
        Debug.Log(
            "Remédios conseguidos: " +
            remediosConseguidos
        );
        Debug.Log("=================================");

        if (CommunityManager.instance != null)
        {
            CommunityManager.instance.AlterarSaude(
                remediosConseguidos
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
        Debug.Log("HOSPITAL: DERROTA!");
        Debug.Log(
            "Remédios perdidos: " +
            remediosConseguidos
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