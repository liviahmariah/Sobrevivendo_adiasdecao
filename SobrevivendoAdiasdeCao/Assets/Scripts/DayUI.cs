using UnityEngine;
using TMPro;

public class DayUI : MonoBehaviour
{
    public TextMeshProUGUI textoDia;
    public TextMeshProUGUI textoTempo;

    private void Start()
    {
        Debug.Log("DayUI iniciado.");

        if (DayManager.Instance == null)
        {
            Debug.LogError(
                "DayUI: DayManager.Instance está NULL!"
            );

            return;
        }

        AtualizarUI();
    }

    private void Update()
    {
        if (DayManager.Instance == null)
            return;

        AtualizarUI();
    }

    private void AtualizarUI()
    {
        if (textoDia != null)
        {
            textoDia.text =
                "DIA " + DayManager.Instance.diaAtual;
        }

        if (textoTempo != null)
        {
            int segundos = Mathf.CeilToInt(
                DayManager.Instance.ObterTempoRestante()
            );

            textoTempo.text = segundos + "s";
        }
    }
}