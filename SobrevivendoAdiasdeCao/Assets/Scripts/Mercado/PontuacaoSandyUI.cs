
using TMPro;
using UnityEngine;

public class PontuacaoSandyUI : MonoBehaviour
{
    [Header("Texto da pontuação")]
    [SerializeField] private TMP_Text textoPontos;

    private void Start()
    {
        AtualizarPontuacao();
    }

    public void AtualizarPontuacao()
    {
        if (textoPontos == null)
            return;

        float pontos = 0f;

        // Hospital
        if (HospitalManager.Instance != null)
        {
            pontos = HospitalManager.Instance.pontosSandy;
        }
        // Mercado
        else if (MercadoManager.Instance != null)
        {
            pontos = MercadoManager.Instance.pontosSandy;
        }
        else
        {
            pontos = 0f;
        }

        textoPontos.text = "Pontos: " + pontos.ToString("0.##");
    }
}
