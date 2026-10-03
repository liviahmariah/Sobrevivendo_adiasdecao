using UnityEngine;

public class BotaoVoltarMapa : MonoBehaviour
{
    public void Voltar()
    {
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.VoltarParaMapa();
        }
        else
        {
            Debug.LogError(
                "BotaoVoltarMapa: PhaseManager não encontrado!"
            );
        }
    }
}