using System.Collections;
using UnityEngine;
using TMPro;

public class BalaoFalaFeirante : MonoBehaviour
{
    [Header("Referências")]
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

    [Header("Animação")]
    public float alturaSubida = 0.5f;
    public float duracaoSubida = 0.25f;
    public float duracaoFala = 1.5f;

    private RectTransform textoRect;
    private Vector3 posicaoInicial;
    private Coroutine rotinaFala;

    private void Awake()
    {
        if (texto == null)
            texto = GetComponentInChildren<TMP_Text>(true);

        if (texto == null)
        {
            Debug.LogWarning("BalaoFalaFeirante: Texto não configurado!", this);
            return;
        }

        textoRect = texto.rectTransform;
        posicaoInicial = textoRect.localPosition;

        texto.alignment = TextAlignmentOptions.Center;
        texto.enableWordWrapping = true;
        texto.text = "";
        texto.gameObject.SetActive(false);
    }

    public void MostrarFala()
    {
        Debug.Log("MOSTRAR FALA DO FEIRANTE", this);

        if (texto == null || textoRect == null)
        {
            Debug.LogWarning("Texto do feirante não configurado!", this);
            return;
        }

        if (falas == null || falas.Length == 0)
            return;

        if (rotinaFala != null)
            StopCoroutine(rotinaFala);

        texto.text = falas[Random.Range(0, falas.Length)];
        texto.alpha = 1f;
        textoRect.localPosition = posicaoInicial;
        texto.gameObject.SetActive(true);

        rotinaFala = StartCoroutine(AnimarFala());
    }

    private IEnumerator AnimarFala()
    {
        Vector3 inicio = posicaoInicial;
        Vector3 destino = inicio + Vector3.up * alturaSubida;
        float tempo = 0f;

        while (tempo < duracaoSubida)
        {
            tempo += Time.deltaTime;

            float t = Mathf.Clamp01(tempo / duracaoSubida);
            float suave = 1f - Mathf.Pow(1f - t, 3f);

            textoRect.localPosition = Vector3.Lerp(inicio, destino, suave);

            yield return null;
        }

        textoRect.localPosition = destino;

        yield return new WaitForSeconds(duracaoFala);

        tempo = 0f;

        while (tempo < 0.2f)
        {
            tempo += Time.deltaTime;
            texto.alpha = 1f - Mathf.Clamp01(tempo / 0.2f);

            yield return null;
        }

        texto.gameObject.SetActive(false);
        textoRect.localPosition = posicaoInicial;
        texto.alpha = 1f;
        rotinaFala = null;
    }

    public void EsconderFala()
    {
        if (rotinaFala != null)
        {
            StopCoroutine(rotinaFala);
            rotinaFala = null;
        }

        if (texto != null)
        {
            texto.text = "";
            texto.alpha = 1f;
            texto.gameObject.SetActive(false);
        }

        if (textoRect != null)
            textoRect.localPosition = posicaoInicial;
    }
}