
using UnityEngine;
using TMPro;

public class BalaoFalaFeirante : MonoBehaviour
{
    [Header("Referências")]
    public GameObject painel;
    public TMP_Text texto;

    [Header("Falas raivosas")]
    [TextArea(1, 3)]
    public string[] falas =
    {
        "EI! SAIA DAQUI!",
        "NÃO TOQUE NAS MINHAS MERCADORIAS!",
        "VOCÊ VAI SE ARREPENDER!",
        "SUMA DAQUI, CACHORRO!",
        "PARE AGORA MESMO!"
    };

    [Header("Tamanho do balão")]
    [Min(50f)]
    public float larguraMaxima = 250f;

    [Min(0f)]
    public float margemHorizontal = 12f;

    [Min(0f)]
    public float margemVertical = 8f;

    [Header("Tempo")]
    public float duracaoFala = 1.5f;

    private RectTransform painelRect;
    private RectTransform textoRect;
    private float tempoParaEsconder;
    private bool escondendoPorTempo;

    private void Awake()
    {
        if (painel != null)
        {
            painelRect = painel.GetComponent<RectTransform>();
            painel.SetActive(false);
        }

        if (texto != null)
        {
            textoRect = texto.GetComponent<RectTransform>();
            texto.enableWordWrapping = true;
            texto.overflowMode = TextOverflowModes.Overflow;
        }
    }

    private void Update()
    {
        if (!escondendoPorTempo)
            return;

        tempoParaEsconder -= Time.deltaTime;

        if (tempoParaEsconder <= 0f)
        {
            escondendoPorTempo = false;
            EsconderFala();
        }
    }

    public void MostrarFala()
    {
        if (painel == null || texto == null)
        {
            Debug.LogWarning(
                "BalaoFalaFeirante: configure o painel e o texto.",
                this
            );
            return;
        }

        if (falas == null || falas.Length == 0)
            return;

        string fala = falas[Random.Range(0, falas.Length)];

        painel.SetActive(true);
        texto.text = fala;

        AjustarTamanho();

        tempoParaEsconder = duracaoFala;
        escondendoPorTempo = true;
    }

    private void AjustarTamanho()
    {
        if (painelRect == null || textoRect == null)
            return;

        // Impede o painel de controlar o tamanho do texto.
        texto.enableWordWrapping = true;

        // Mede a fala respeitando a largura máxima.
        Vector2 tamanhoTexto = texto.GetPreferredValues(
            texto.text,
            larguraMaxima,
            0f
        );

        float larguraTexto = Mathf.Min(
            tamanhoTexto.x,
            larguraMaxima
        );

        // Mede novamente com a largura final para calcular
        // a altura necessária quando o texto quebra de linha.
        tamanhoTexto = texto.GetPreferredValues(
            texto.text,
            larguraTexto,
            0f
        );

        float larguraPainel = larguraTexto + margemHorizontal * 2f;
        float alturaPainel = tamanhoTexto.y + margemVertical * 2f;

        painelRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            larguraPainel
        );

        painelRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            alturaPainel
        );

        textoRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            larguraTexto
        );

        textoRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            tamanhoTexto.y
        );
    }

    public void EsconderFala()
    {
        escondendoPorTempo = false;

        if (painel != null)
            painel.SetActive(false);
    }
}
