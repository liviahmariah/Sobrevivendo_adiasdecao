using UnityEngine;
using TMPro;

public class TutorialChavesManager : MonoBehaviour
{
    public static TutorialChavesManager instance;

    [Header("Configuração")]
    public int quantidadeChavesComuns = 8;
    public int quantidadeChavesTotal = 9;

    [Header("Interface")]
    public TextMeshProUGUI textoChaves;

    [Header("Referência da carrocinha")]
    public ChefeCarrocinhaTutorial chefe;

    private int chavesColetadas = 0;

    public int ChavesColetadas => chavesColetadas;

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
        chavesColetadas = 0;
        AtualizarUI();
    }

    // =====================================================
    // COLETAR CHAVE COMUM
    // =====================================================

    public void ColetarChaveComum()
    {
        if (chavesColetadas >= quantidadeChavesComuns)
            return;

        chavesColetadas++;

        AtualizarUI();

        if (chefe != null)
        {
            chefe.AtualizarChavesColetadas(chavesColetadas);
        }

        Debug.Log(
            "Chave comum coletada: "
            + chavesColetadas
            + "/"
            + quantidadeChavesTotal
        );

        if (chavesColetadas >= quantidadeChavesComuns)
        {
            LiberarChaveFinal();
        }
    }

    // =====================================================
    // COLETAR CHAVE FINAL
    // =====================================================

    public void ColetarChaveFinal()
    {
        if (chavesColetadas < quantidadeChavesComuns)
        {
            Debug.LogWarning(
                "As chaves comuns ainda não foram coletadas."
            );

            return;
        }

        if (chavesColetadas >= quantidadeChavesTotal)
        {
            return;
        }

        chavesColetadas++;

        AtualizarUI();

        Debug.Log(
            "Chave final coletada: "
            + chavesColetadas
            + "/"
            + quantidadeChavesTotal
        );

        if (chavesColetadas >= quantidadeChavesTotal)
        {
            Debug.Log("TODAS AS 9 CHAVES FORAM COLETADAS!");
        }
    }

    // =====================================================
    // LIBERAR CHAVE FINAL
    // =====================================================

    private void LiberarChaveFinal()
    {
        Debug.Log("As 8 chaves comuns foram coletadas!");
        Debug.Log("Agora Sandy pode assustar a carrocinha.");
    }

    // =====================================================
    // ATUALIZAR UI
    // =====================================================

    private void AtualizarUI()
    {
        if (textoChaves != null)
        {
            textoChaves.text =
                chavesColetadas + "/" + quantidadeChavesTotal;
        }
    }
}