
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

        if (HospitalManager.Instance != null)
        {
            pontos = HospitalManager.Instance.pontosSandy;
        }
        else if (MercadoManager.Instance != null)
        {
            pontos = MercadoManager.Instance.pontosSandy;
        }
        else if (PracaManager.Instance != null)
        {
            pontos = PracaManager.Instance.diversaoConseguida;
        }

        textoPontos.text = "Pontos: " + pontos.ToString("0.##");
    }
}
