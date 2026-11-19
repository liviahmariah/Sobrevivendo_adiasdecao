using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PegadasCachorro : MonoBehaviour
{
    [Header("Sprite da Pegada")]
    public Sprite spritePegada;
    public Color corPegada = Color.white;
    public Vector2 tamanhoPegadaUI = new Vector2(50f, 50f);
    public Vector3 escalaPegada2D = Vector3.one;

    [Header("Trajeto (Ponto A até Ponto B)")]
    [Tooltip("Arraste um objeto vazio indicando onde os passos começam")]
    public Transform pontoInicial;
    [Tooltip("Arraste um objeto vazio indicando onde os passos terminam")]
    public Transform pontoFinal;

    [Header("Ritmo e Passada")]
    [Tooltip("Distância entre cada passo")]
    public float distanciaEntrePassos = 60f;
    [Tooltip("Afastamento lateral entre pata esquerda e direita")]
    public float aberturaLateral = 18f;
    [Tooltip("Tempo em segundos entre cada passo")]
    public float tempoEntrePassos = 0.35f;
    [Tooltip("Tempo que cada pegada leva para desaparecer suavemente")]
    public float duracaoFadeOut = 1.5f;
    [Tooltip("Repetir o caminho em loop infinito?")]
    public bool repetirEmLoop = true;

    [Header("Ajuste de Rotação do PNG")]
    [Tooltip("Ajuste caso o seu PNG da patinha esteja desenhado de lado ou para baixo")]
    public float offsetAngulo = -90f;

    private bool usandoUI;

    private void Start()
    {
        usandoUI = GetComponentInParent<Canvas>() != null;

        if (spritePegada != null && pontoInicial != null && pontoFinal != null)
        {
            StartCoroutine(RotinaCaminhada());
        }
        else
        {
            Debug.LogWarning("PegadasCachorro: Preencha o Sprite, Ponto Inicial e Ponto Final no Inspector.");
        }
    }

    private IEnumerator RotinaCaminhada()
    {
        do
        {
            Vector3 inicio = pontoInicial.position;
            Vector3 fim = pontoFinal.position;
            Vector3 direcao = (fim - inicio).normalized;
            float distanciaTotal = Vector3.Distance(inicio, fim);

            // Vetor perpendicular para alternar pata esquerda e direita
            Vector3 lateral = new Vector3(-direcao.y, direcao.x, 0f);
            float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg + offsetAngulo;

            int passo = 0;
            float distanciaPercorrida = 0f;

            while (distanciaPercorrida <= distanciaTotal)
            {
                float lado = (passo % 2 == 0) ? 1f : -1f;
                Vector3 posicaoPasso = inicio + (direcao * distanciaPercorrida) + (lateral * (aberturaLateral * lado));

                CriarPegada(posicaoPasso, angulo);

                passo++;
                distanciaPercorrida += distanciaEntrePassos;

                yield return new WaitForSeconds(tempoEntrePassos);
            }

            // Aguarda um instante antes de recomeçar o trajeto
            yield return new WaitForSeconds(duracaoFadeOut);

        } while (repetirEmLoop);
    }

    private void CriarPegada(Vector3 posicao, float anguloZ)
    {
        GameObject objPegada = new GameObject("Pegada");
        objPegada.transform.SetParent(transform, true);
        objPegada.transform.position = posicao;
        objPegada.transform.rotation = Quaternion.Euler(0f, 0f, anguloZ);

        if (usandoUI)
        {
            Image img = objPegada.AddComponent<Image>();
            img.sprite = spritePegada;
            img.color = corPegada;
            img.raycastTarget = false;

            RectTransform rect = objPegada.GetComponent<RectTransform>();
            rect.sizeDelta = tamanhoPegadaUI;

            StartCoroutine(FadeEDestruirUI(img));
        }
        else
        {
            SpriteRenderer sr = objPegada.AddComponent<SpriteRenderer>();
            sr.sprite = spritePegada;
            sr.color = corPegada;
            sr.sortingOrder = 10;
            objPegada.transform.localScale = escalaPegada2D;

            StartCoroutine(FadeEDestruir2D(sr));
        }
    }

    private IEnumerator FadeEDestruirUI(Image img)
    {
        float tempo = 0f;
        Color corInicial = img.color;

        while (tempo < duracaoFadeOut && img != null)
        {
            tempo += Time.deltaTime;
            float alpha = Mathf.Lerp(corInicial.a, 0f, tempo / duracaoFadeOut);
            img.color = new Color(corInicial.r, corInicial.g, corInicial.b, alpha);
            yield return null;
        }

        if (img != null) Destroy(img.gameObject);
    }

    private IEnumerator FadeEDestruir2D(SpriteRenderer sr)
    {
        float tempo = 0f;
        Color corInicial = sr.color;

        while (tempo < duracaoFadeOut && sr != null)
        {
            tempo += Time.deltaTime;
            float alpha = Mathf.Lerp(corInicial.a, 0f, tempo / duracaoFadeOut);
            sr.color = new Color(corInicial.r, corInicial.g, corInicial.b, alpha);
            yield return null;
        }

        if (sr != null) Destroy(sr.gameObject);
    }
}