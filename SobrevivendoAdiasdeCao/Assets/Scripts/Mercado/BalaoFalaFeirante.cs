
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

    [Header("Tempo")]
    public float duracaoFala = 1.5f;

    private void Awake()
    {
        if (painel != null)
            painel.SetActive(false);
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
        texto.text = fala;
        painel.SetActive(true);
    }

    public void EsconderFala()
    {
        if (painel != null)
            painel.SetActive(false);
    }
}
