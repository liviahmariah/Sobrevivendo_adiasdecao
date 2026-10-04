using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CommunityUI : MonoBehaviour
{
    [Header("Barras")]
    public Slider barraAlimentacao;
    public Slider barraSaude;
    public Slider barraFelicidade;

    [Header("Textos")]
    public TextMeshProUGUI textoAlimentacao;
    public TextMeshProUGUI textoSaude;
    public TextMeshProUGUI textoFelicidade;

    private void Start()
    {
        ConfigurarBarras();
        AtualizarUI();
    }

    private void Update()
    {
        AtualizarUI();
    }

    private void ConfigurarBarras()
    {
        if (barraAlimentacao != null)
        {
            barraAlimentacao.minValue = 0;
            barraAlimentacao.maxValue = 100;
        }

        if (barraSaude != null)
        {
            barraSaude.minValue = 0;
            barraSaude.maxValue = 100;
        }

        if (barraFelicidade != null)
        {
            barraFelicidade.minValue = 0;
            barraFelicidade.maxValue = 100;
        }
    }

    private void AtualizarUI()
    {
        if (CommunityManager.instance == null)
            return;

        float alimentacao =
            CommunityManager.instance.alimentacao;

        float saude =
            CommunityManager.instance.saude;

        float felicidade =
            CommunityManager.instance.felicidade;

        // Barras
        if (barraAlimentacao != null)
            barraAlimentacao.value = alimentacao;

        if (barraSaude != null)
            barraSaude.value = saude;

        if (barraFelicidade != null)
            barraFelicidade.value = felicidade;

        // Textos
        if (textoAlimentacao != null)
            textoAlimentacao.text =
                Mathf.RoundToInt(alimentacao) + "%";

        if (textoSaude != null)
            textoSaude.text =
                Mathf.RoundToInt(saude) + "%";

        if (textoFelicidade != null)
            textoFelicidade.text =
                Mathf.RoundToInt(felicidade) + "%";
    }
}