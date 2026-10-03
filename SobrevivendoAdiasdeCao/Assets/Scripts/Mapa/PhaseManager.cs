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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AoCarregarCena;
    }

    // =====================================================
    // ENTRADA NAS FASES
    // =====================================================

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
        faseAtual = nomeCena;
        emFase = true;

        Debug.Log("PhaseManager: Entrando na fase " + nomeCena);

        SceneManager.LoadScene(nomeCena);
    }

    // =====================================================
    // VOLTAR PARA O MAPA
    // =====================================================

    public void VoltarParaMapa()
    {
        string cenaAtual = SceneManager.GetActiveScene().name;

        Debug.Log("PhaseManager: Tentando voltar para o Mapa.");
        Debug.Log("PhaseManager: Cena atual = " + cenaAtual);

        // Se estamos em uma das fases, pode voltar.
        if (cenaAtual == cenaMercado ||
            cenaAtual == cenaPraca ||
            cenaAtual == cenaHospital)
        {
            Debug.Log("PhaseManager: Saindo da fase " + cenaAtual);

            emFase = false;
            faseAtual = "";

            SceneManager.LoadScene("Mapa");
        }
        else
        {
            Debug.LogWarning(
                "PhaseManager: A cena atual não é uma fase."
            );
        }
    }

    // =====================================================
    // DETECTAR CENA CARREGADA
    // =====================================================

    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        Debug.Log(
            "PhaseManager: Cena carregada = " + cena.name
        );

        if (cena.name == cenaMercado ||
            cena.name == cenaPraca ||
            cena.name == cenaHospital)
        {
            emFase = true;
            faseAtual = cena.name;

            Debug.Log(
                "PhaseManager: Fase detectada = " + faseAtual
            );
        }

        if (cena.name == "Mapa")
        {
            emFase = false;
            faseAtual = "";

            Debug.Log(
                "PhaseManager: Voltamos para o Mapa."
            );
        }
    }

    // =====================================================
    // CONSULTAS
    // =====================================================

    public bool EstaEmFase()
    {
        return emFase;
    }

    public string ObterFaseAtual()
    {
        return faseAtual;
    }
}