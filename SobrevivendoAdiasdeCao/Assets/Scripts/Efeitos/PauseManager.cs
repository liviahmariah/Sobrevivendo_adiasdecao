
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("Painéis")]
    [SerializeField] private GameObject painelPause;
    [SerializeField] private GameObject painelConfiguracoes;

    [Header("Cena do menu")]
    [SerializeField] private string cenaMenu = "Menu";

    private bool pausado = false;

    public bool Pausado => pausado;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += AoCarregarCena;

        FecharTudo();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            AlternarPause();
        }
    }

    public void AlternarPause()
    {
        if (painelPause == null) return;

        if (!pausado)
        {
            pausado = true;
            painelPause.SetActive(true);
            painelConfiguracoes.SetActive(false);
            Time.timeScale = 0f;
        }
        else
        {
            Continuar();
        }
    }

    public void Continuar()
    {
        pausado = false;

        painelPause.SetActive(false);
        painelConfiguracoes.SetActive(false);

        Time.timeScale = 1f;
    }

    public void AbrirConfiguracoes()
    {
        painelPause.SetActive(false);
        painelConfiguracoes.SetActive(true);
    }

    public void VoltarParaPause()
    {
        painelConfiguracoes.SetActive(false);
        painelPause.SetActive(true);
    }

    public void VoltarMenuPrincipal()
    {
        Time.timeScale = 1f;
        pausado = false;

        SceneManager.LoadScene(cenaMenu);
    }

    private void FecharTudo()
    {
        pausado = false;

        if (painelPause != null)
            painelPause.SetActive(false);

        if (painelConfiguracoes != null)
            painelConfiguracoes.SetActive(false);

        Time.timeScale = 1f;
    }

    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        FecharTudo();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= AoCarregarCena;
            Instance = null;
            Time.timeScale = 1f;
        }
    }
}
