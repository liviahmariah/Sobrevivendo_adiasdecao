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

    [Header("Molho de chaves")]
    public MolhoChavesTutorial molhoChaves;

    [Header("Tutorial")]
    public TutorialManager tutorialManager;

    private int chavesDisponiveis = 0;
    private int chavesColetadas = 0;
    private int gaiolasAbertas = 0;

    private bool avisoUltimaChaveMostrado = false;

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
        avisoUltimaChaveMostrado = false;

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
            Debug.Log(
                "Sandy não possui chaves disponíveis."
            );

            return false;
        }

        if (gaiolasAbertas >= quantidadeGaiolasTotal)
        {
            Debug.Log(
                "Todas as gaiolas já foram abertas."
            );

            return false;
        }

        // Consome a chave
        chavesDisponiveis--;

        // Registra a gaiola aberta
        gaiolasAbertas++;

        // Avisa o molho que a chave foi usada
        if (molhoChaves != null)
        {
            molhoChaves.RegistrarUsoDaChave();
        }
        else
        {
            Debug.LogWarning(
                "MolhoChavesTutorial não foi configurado no Inspector."
            );
        }

        AtualizarUI();

        Debug.Log(
            "Gaiola aberta! Progresso: "
            + gaiolasAbertas
            + "/"
            + quantidadeGaiolasTotal
        );

        // Quando o 8º cão for libertado,
        // avisar que falta a chave da carrocinha
        if (gaiolasAbertas == 8 &&
            !avisoUltimaChaveMostrado)
        {
            avisoUltimaChaveMostrado = true;

            AvisarSobreUltimaChave();
        }

        return true;
    }

    private void AvisarSobreUltimaChave()
    {
        Debug.Log(
            "Os 8 primeiros cães foram libertados. " +
            "A última chave está com a carrocinha!"
        );

        if (tutorialManager != null)
        {
            tutorialManager.MostrarAvisoUltimaChave();
        }
        else
        {
            Debug.LogWarning(
                "TutorialManager não foi configurado no Inspector."
            );
        }
    }

    public void ColetarChaveFinal()
    {
        chavesDisponiveis++;
        chavesColetadas++;

        AtualizarUI();

        Debug.Log(
            "Chave final coletada!"
        );
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