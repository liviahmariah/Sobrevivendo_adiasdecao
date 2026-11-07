using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("Painéis (Buscados por Tag)")]
    [SerializeField] private GameObject painelPause;
    [SerializeField] private GameObject painelConfiguracoes;
    [SerializeField] private GameObject painelCreditos;

    [Header("Sliders de Áudio (No Painel de Configurações)")]
    [SerializeField] private Slider sliderMaster;
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderSFX;

    [Header("Nomes EXATOS das Cenas sem Pause")]
    [SerializeField] private string cenaMenu = "Menu";
    [SerializeField] private string cenaIntro1 = "Intro1";
    [SerializeField] private string cenaIntro2 = "Intro2";

    private bool pausado = false;
    public bool Pausado => pausado;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            enabled = false;
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            string cenaAtual = SceneManager.GetActiveScene().name;

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
        if (painelPause == null || painelConfiguracoes == null)
        {
            BuscarPaineisNaCena();
        }

        if (painelPause == null)
        {
            Debug.LogError("[PauseManager] ERRO CRÍTICO: Não existe nenhum GameObject na cena com a Tag 'PainelPause'!");
            return;
        }

        pausado = !pausado;

        if (pausado)
        {
            // Abre apenas o menu de Pause e esconde os demais
            painelPause.SetActive(true);
            if (painelConfiguracoes != null) painelConfiguracoes.SetActive(false);
            if (painelCreditos != null) painelCreditos.SetActive(false);

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

        FecharTodosOsPaineis();

        Time.timeScale = 1f;
    }

    // =====================================================
    // NAVEGAÇÃO DE PAINÉIS
    // =====================================================

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

    public void VoltarParaConfiguracoes()
    {
        if (painelCreditos != null) painelCreditos.SetActive(false);
        if (painelConfiguracoes != null) painelConfiguracoes.SetActive(true);
    }

    public void VoltarMenuPrincipal()
    {
        Time.timeScale = 1f;
        pausado = false;

        if (MusicManager.instance != null)
        {
            MusicManager.instance.TocarMusicaSuave(MusicManager.instance.musicaMenu);
        }

        SceneManager.LoadScene(cenaMenu);
    }

    private void FecharTodosOsPaineis()
    {
        if (painelPause != null) painelPause.SetActive(false);
        if (painelConfiguracoes != null) painelConfiguracoes.SetActive(false);
        if (painelCreditos != null) painelCreditos.SetActive(false);
    }

    private void FecharTudo()
    {
        pausado = false;
        Time.timeScale = 1f;
        FecharTodosOsPaineis();
    }

    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        FecharTudo();
        BuscarPaineisNaCena();
    }

    // =====================================================
    // BUSCA AUTOMÁTICA DE COMPONENTES POR TAG
    // =====================================================

    private void BuscarPaineisNaCena()
    {
        GameObject[] todosObjetos = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in todosObjetos)
        {
            if (obj.hideFlags == HideFlags.None && obj.scene.isLoaded)
            {
                if (obj.CompareTag("PainelPause"))
                {
                    painelPause = obj;
                }
                else if (obj.CompareTag("PainelConfig"))
                {
                    painelConfiguracoes = obj;
                }
                else if (obj.CompareTag("PainelCreditos"))
                {
                    painelCreditos = obj;
                }
            }
        }

        FecharTodosOsPaineis();
        ConectarSlidersDeAudio();
    }

    private void ConectarSlidersDeAudio()
    {
        if (AudioManager.Instance == null || painelConfiguracoes == null) return;

        // Procura os Sliders dentro do Painel de Configurações
        Slider[] sliders = painelConfiguracoes.GetComponentsInChildren<Slider>(true);

        foreach (Slider s in sliders)
        {
            if (s.gameObject.name.Contains("Master"))
            {
                sliderMaster = s;
                sliderMaster.value = AudioManager.Instance.GetMasterVolume();
                sliderMaster.onValueChanged.RemoveAllListeners();
                sliderMaster.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
            }
            else if (s.gameObject.name.Contains("Musica") || s.gameObject.name.Contains("Music"))
            {
                sliderMusica = s;
                sliderMusica.value = AudioManager.Instance.GetMusicVolume();
                sliderMusica.onValueChanged.RemoveAllListeners();
                sliderMusica.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
            }
            else if (s.gameObject.name.Contains("SFX") || s.gameObject.name.Contains("Efeitos"))
            {
                sliderSFX = s;
                sliderSFX.value = AudioManager.Instance.GetSFXVolume();
                sliderSFX.onValueChanged.RemoveAllListeners();
                sliderSFX.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
            }
        }
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