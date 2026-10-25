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

        if (MercadoManager.Instance == null)
        {
            textoPontos.text = "Pontos: 0";
            return;
        }

        float pontos = MercadoManager.Instance.pontosSandy;

        textoPontos.text = "Pontos: " + pontos.ToString("0.##");
    }
}