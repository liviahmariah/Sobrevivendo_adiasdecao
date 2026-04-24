using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Itens")]
    public int itensColetados = 0;
    public int meta = 10;

    [Header("Tempo")]
    public float tempoMax = 60f;
    private float tempoAtual;

    [Header("UI")]
    public TextMeshProUGUI textoItens;
    public TextMeshProUGUI textoTempo;
    public GameObject painelVitoria;
    public GameObject painelDerrota;

    private bool jogoAcabou = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        tempoAtual = tempoMax;

        painelVitoria.SetActive(false);
        painelDerrota.SetActive(false);

        AtualizarUI();
    }

    void Update()
    {
        if (jogoAcabou) return;

        tempoAtual -= Time.deltaTime;

        if (tempoAtual <= 0)
        {
            tempoAtual = 0;
            Derrota();
        }

        AtualizarTempoUI();
    }

    public void Coletar(int valor)
    {
        if (jogoAcabou) return;

        itensColetados += valor;
        AtualizarUI();

        if (itensColetados >= meta)
        {
            Vitoria();
        }
    }

    void AtualizarUI()
    {
        textoItens.text = "Itens: " + itensColetados + "/" + meta;
    }

    void AtualizarTempoUI()
    {
        textoTempo.text = "Tempo: " + Mathf.Ceil(tempoAtual);
    }

    void Vitoria()
    {
        jogoAcabou = true;
        painelVitoria.SetActive(true);
        Time.timeScale = 0f;
    }

    void Derrota()
    {
        jogoAcabou = true;
        painelDerrota.SetActive(true);
        Time.timeScale = 0f;
    }
}