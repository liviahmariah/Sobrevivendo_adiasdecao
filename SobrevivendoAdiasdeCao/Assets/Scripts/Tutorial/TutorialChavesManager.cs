using UnityEngine;
using TMPro;

public class TutorialChavesManager : MonoBehaviour
{
    public static TutorialChavesManager instance;

    [Header("Configuração")]
    public int quantidadeGaiolasTotal = 9;

    [Header("Interface")]
    public TextMeshProUGUI textoChaves;

    [Header("Referência da carrocinha")]
    public ChefeCarrocinhaTutorial chefe;

    private int chavesDisponiveis = 0;
    private int chavesColetadas = 0;
    private int gaiolasAbertas = 0;

    public int ChavesDisponiveis => chavesDisponiveis;
    public int GaiolasAbertas => gaiolasAbertas;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        chavesDisponiveis = 0;
        chavesColetadas = 0;
        gaiolasAbertas = 0;

        AtualizarUI();
    }

    public void ColetarChaveComum()
    {
        chavesDisponiveis++;
        chavesColetadas++;

        AtualizarUI();

        if (chefe != null)
        {
            chefe.AtualizarChavesColetadas(chavesColetadas);
        }

        Debug.Log(
            "Chave coletada! Disponíveis: "
            + chavesDisponiveis
        );
    }

    public bool TemChaveDisponivel()
    {
        return chavesDisponiveis > 0;
    }

    public bool UsarChaveParaAbrirGaiola()
    {
        if (chavesDisponiveis <= 0)
        {
            Debug.Log("Sandy não possui chaves disponíveis.");
            return false;
        }

        if (gaiolasAbertas >= quantidadeGaiolasTotal)
        {
            Debug.Log("Todas as gaiolas já foram abertas.");
            return false;
        }

        chavesDisponiveis--;
        gaiolasAbertas++;

        AtualizarUI();

        Debug.Log(
            "Gaiola aberta! Progresso: "
            + gaiolasAbertas
            + "/"
            + quantidadeGaiolasTotal
        );

        return true;
    }

    public void ColetarChaveFinal()
    {
        chavesDisponiveis++;
        chavesColetadas++;

        AtualizarUI();

        Debug.Log("Chave final coletada!");
    }

    private void AtualizarUI()
    {
        if (textoChaves != null)
        {
            textoChaves.text =
                gaiolasAbertas + "/" + quantidadeGaiolasTotal;
        }
    }
}