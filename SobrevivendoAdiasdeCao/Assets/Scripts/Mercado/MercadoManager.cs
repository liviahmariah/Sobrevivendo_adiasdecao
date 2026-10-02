using UnityEngine;

public class MercadoManager : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("FASE MERCADO INICIADA!");

        if (DayManager.Instance != null)
        {
            Debug.Log("Tempo restante do dia: " +
                      DayManager.Instance.ObterTempoRestante() + "s");
        }
    }

    public void FinalizarMercado()
    {
        Debug.Log("Mercado finalizado.");

        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.VoltarParaMapa();
        }
    }
}