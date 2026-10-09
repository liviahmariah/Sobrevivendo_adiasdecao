
using UnityEngine;

public class IndicadorChefeHUD : MonoBehaviour
{
    [Header("Chefe fora")]
    public GameObject botaoVerde;
    public GameObject quadradoVerde;

    [Header("Chefe na fase")]
    public GameObject botaoVermelho;
    public GameObject quadradoVermelho;

    public void AtualizarIndicadores(bool chefeNaFase)
    {
        if (botaoVerde != null)
            botaoVerde.SetActive(!chefeNaFase);

        if (quadradoVerde != null)
            quadradoVerde.SetActive(!chefeNaFase);

        if (botaoVermelho != null)
            botaoVermelho.SetActive(chefeNaFase);

        if (quadradoVermelho != null)
            quadradoVermelho.SetActive(chefeNaFase);
    }
}
