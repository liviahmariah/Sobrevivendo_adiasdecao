using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("Painéis de UI (Arraste os objetos do Prefab)")]
    [SerializeField] private GameObject painelPause;
    [SerializeField] private GameObject painelConfiguracoes;
    [SerializeField] private GameObject painelCreditos;

    [Header("Cenas onde o Pause é BLOQUEADO")]
    [SerializeField] private string cenaMenu = "Menu";
    [SerializeField] private string cenaIntro1 = "Intro1";
    [SerializeField] private string cenaIntro2 = "Intro2";

    private bool pausado = false;
    public bool Pausado => pausado;

    private void Awake()
    {
        // Garante instância única
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Se estiver preso dentro de algum filho, desanexa para a raiz e aplica DontDestroyOnLoad
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private void Start()
    {
        FecharTodosOsPaineis();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            string cenaAtual = SceneManager.GetActiveScene().name;

            // Não abre pause nas telas de Menu/Intro
            if (!PodePausarNaCenaAtual(cenaAtual))
                return;

            AlternarPause();
        }
    }

    private bool PodePausarNaCenaAtual(string cenaAtual)
    {
        return cenaAtual != cenaMenu && cenaAtual != cenaIntro1 && cenaAtual != cenaIntro2;
    }

    public void AlternarPause()
    {
        pausado = !pausado;

        if (pausado)
        {
            Pausar();
        }
        else
        {
            Continuar();
        }
    }

    public void Pausar()
    {
        pausado = true;
        Time.timeScale = 0f;

        if (painelPause != null) painelPause.SetActive(true);
        if (painelConfiguracoes != null) painelConfiguracoes.SetActive(false);
        if (painelCreditos != null) painelCreditos.SetActive(false);
    }

    public void Continuar()
    {
        pausado = false;
        Time.timeScale = 1f;

        FecharTodosOsPaineis();
    }

    // Navegação de Painéis
    public void AbrirConfiguracoes()
    {
        if (painelPause != null) painelPause.SetActive(false);
        if (painelCreditos != null) painelCreditos.SetActive(false);
        if (painelConfiguracoes != null) painelConfiguracoes.SetActive(true);
    }

    public void VoltarParaPause()
    {
        if (painelConfiguracoes != null) painelConfiguracoes.SetActive(false);
        if (painelCreditos != null) painelCreditos.SetActive(false);
        if (painelPause != null) painelPause.SetActive(true);
    }

    public void AbrirCreditos()
    {
        if (painelConfiguracoes != null) painelConfiguracoes.SetActive(false);
        if (painelPause != null) painelPause.SetActive(false);
        if (painelCreditos != null) painelCreditos.SetActive(true);
    }

    public void VoltarMenuPrincipal()
    {
        Time.timeScale = 1f;
        pausado = false;
        FecharTodosOsPaineis();

        SceneManager.LoadScene(cenaMenu);
    }

    private void FecharTodosOsPaineis()
    {
        if (painelPause != null) painelPause.SetActive(false);
        if (painelConfiguracoes != null) painelConfiguracoes.SetActive(false);
        if (painelCreditos != null) painelCreditos.SetActive(false);
    }

    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        Time.timeScale = 1f;
        pausado = false;
        FecharTodosOsPaineis();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= AoCarregarCena;
            Time.timeScale = 1f;
        }
    }
}