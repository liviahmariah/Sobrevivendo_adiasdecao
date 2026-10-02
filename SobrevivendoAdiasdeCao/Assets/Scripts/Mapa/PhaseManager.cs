using UnityEngine;
using UnityEngine.SceneManagement;

public class PhaseManager : MonoBehaviour
{
    public static PhaseManager Instance;

    [Header("Cenas das Fases")]
    public string cenaMercado = "Mercado";
    public string cenaPraca = "Praca";
    public string cenaHospital = "Hospital";

    [Header("Estado Atual")]
    public string faseAtual = "";
    public bool emFase = false;

    // Indica que estamos voltando de uma fase para o mapa.
    private bool voltandoParaMapa = false;


    // =========================================================
    // INICIALIZAÇÃO
    // =========================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }


    private void Start()
    {
        // Se o jogo acabou de voltar de uma fase,
        // retoma o relógio do dia.
        if (voltandoParaMapa)
        {
            RetomarDia();
        }
    }


    // =========================================================
    // ENTRADA NAS FASES
    // =========================================================

    public void EntrarNoMercado()
    {
        EntrarNaFase(cenaMercado);
    }


    public void EntrarNaPraca()
    {
        EntrarNaFase(cenaPraca);
    }


    public void EntrarNoHospital()
    {
        EntrarNaFase(cenaHospital);
    }


    private void EntrarNaFase(string nomeCena)
    {
        // Impede entrar em outra fase enquanto já estiver
        // dentro de uma.
        if (emFase)
        {
            Debug.LogWarning(
                "PhaseManager: Sandy já está em uma fase."
            );

            return;
        }


        // -----------------------------------------------------
        // PAUSAR O DIA
        // -----------------------------------------------------

        if (DayManager.Instance != null)
        {
            DayManager.Instance.PausarDia();

            Debug.Log(
                "Dia pausado. Tempo restante: " +
                DayManager.Instance.ObterTempoRestante() +
                "s"
            );
        }


        // -----------------------------------------------------
        // REGISTRAR A FASE
        // -----------------------------------------------------

        faseAtual = nomeCena;
        emFase = true;
        voltandoParaMapa = false;


        Debug.Log(
            "Entrando na fase: " + nomeCena
        );


        // -----------------------------------------------------
        // CARREGAR A CENA
        // -----------------------------------------------------

        SceneManager.LoadScene(nomeCena);
    }


    // =========================================================
    // SAÍDA DA FASE
    // =========================================================

    public void VoltarParaMapa()
    {
        if (!emFase)
        {
            Debug.LogWarning(
                "PhaseManager: não há nenhuma fase ativa."
            );

            return;
        }


        Debug.Log(
            "Finalizando fase: " + faseAtual
        );


        // A fase terminou.
        emFase = false;

        // Guarda a informação de que estamos voltando
        // para o mapa.
        voltandoParaMapa = true;


        // Limpa a fase atual.
        faseAtual = "";


        // Carrega o mapa.
        SceneManager.LoadScene("Mapa");
    }


    // =========================================================
    // RETOMAR O DIA
    // =========================================================

    private void RetomarDia()
    {
        if (DayManager.Instance == null)
        {
            Debug.LogWarning(
                "PhaseManager: DayManager não encontrado."
            );

            return;
        }


        DayManager.Instance.RetomarDia();

        Debug.Log(
            "Dia retomado. Tempo restante: " +
            DayManager.Instance.ObterTempoRestante() +
            "s"
        );


        // Agora que o mapa foi carregado e o dia foi retomado,
        // não precisamos mais dessa informação.
        voltandoParaMapa = false;
    }


    // =========================================================
    // INFORMAÇÕES PARA OUTROS SISTEMAS
    // =========================================================

    public bool EstaEmFase()
    {
        return emFase;
    }


    public string ObterFaseAtual()
    {
        return faseAtual;
    }
}