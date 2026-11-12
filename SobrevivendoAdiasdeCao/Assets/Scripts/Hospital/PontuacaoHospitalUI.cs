using TMPro;
using UnityEngine;

public class PontuacaoHospitalUI : MonoBehaviour
{
    public static PontuacaoHospitalUI Instance { get; private set; }

    [Header("Texto da pontuação (Hospital)")]
    [SerializeField] private TMP_Text textoPontos;

    [Header("Formatacao")]
    [Tooltip("Prefixo que aparece antes da pontuação.")]
    public string prefixoTexto = "Pontos: ";

    private void Awake()
    {
        // Instância única para fácil acesso na cena do Hospital
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        AtualizarPontuacao();
    }

    /// <summary>
    /// Atualiza o texto da pontuação com base no valor armazenado no HospitalManager.
    /// Pode ser chamado via script ao coletar um medicamento ou alterar os pontos.
    /// </summary>
    public void AtualizarPontuacao()
    {
        if (textoPontos == null)
        {
            Debug.LogWarning("PontuacaoHospitalUI: Componente TMP_Text não atribuído no Inspector!", this);
            return;
        }

        float pontos = 0f;

        if (HospitalManager.Instance != null)
        {
            pontos = HospitalManager.Instance.pontosSandy;
        }
        else
        {
            Debug.LogWarning("PontuacaoHospitalUI: HospitalManager.Instance não foi encontrado na cena!", this);
        }

        // Formata a pontuação sem casas decimais desnecessárias (ex: "Pontos: 15" ou "Pontos: 15.5")
        textoPontos.text = prefixoTexto + pontos.ToString("0.##");
    }

    /// <summary>
    /// Permite passar um valor de pontuação diretamente, se necessário.
    /// </summary>
    public void DefinirPontuacaoDireta(float valorPontos)
    {
        if (textoPontos != null)
        {
            textoPontos.text = prefixoTexto + valorPontos.ToString("0.##");
        }
    }
}