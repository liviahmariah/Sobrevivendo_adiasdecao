using UnityEngine;

public class BotaoEntrarFase : MonoBehaviour
{
    public enum TipoFase
    {
        Mercado,
        Praca,
        Hospital
    }

    public TipoFase fase;

    public void Entrar()
    {
        if (PhaseManager.Instance == null)
        {
            Debug.LogError("BotaoEntrarFase: PhaseManager não encontrado!");
            return;
        }

        switch (fase)
        {
            case TipoFase.Mercado:
                PhaseManager.Instance.EntrarNoMercado();
                break;

            case TipoFase.Praca:
                PhaseManager.Instance.EntrarNaPraca();
                break;

            case TipoFase.Hospital:
                PhaseManager.Instance.EntrarNoHospital();
                break;
        }
    }
}