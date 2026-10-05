using UnityEngine;
using TMPro;

public class MercadoUI : MonoBehaviour
{
    [Header("Texto do Timer")]
    public TextMeshProUGUI textoTempo;

    private void Start()
    {
        AtualizarUI();
    }

    private void Update()
    {
        AtualizarUI();
    }

    private void AtualizarUI()
    {
        if (MercadoManager.Instance == null)
            return;

        float tempo =
            MercadoManager.Instance.ObterTempoRestante();

        int segundos =
            Mathf.CeilToInt(tempo);

        if (textoTempo != null)
        {
            textoTempo.text = segundos + ":00";
        }
    }
}