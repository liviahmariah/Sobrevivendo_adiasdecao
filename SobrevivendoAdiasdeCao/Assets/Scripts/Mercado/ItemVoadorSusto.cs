using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemVoadorSusto : MonoBehaviour
{
    [Header("Configurações do Arremesso")]
    [Tooltip("Altura máxima do arco em pixels/unidades.")]
    public float alturaArco = 2.5f;
    [Tooltip("Distância horizontal que o item vai voar.")]
    public float distanciaHorizontal = 3.0f;
    [Tooltip("Tempo total do voo em segundos.")]
    public float duracaoVoo = 0.5f;
    [Tooltip("Velocidade de rotação do item enquanto voa.")]
    public float velocidadeRotacao = 720f;

    [Header("Aparência da Trilha (Pixel Art)")]
    [Tooltip("Sprite do pontinho individual da trilha (ex: quadrado 2x2 bege).")]
    public Sprite spritePontinho;
    [Tooltip("Cor do pontinho da trilha.")]
    public Color corPontinho = Color.white;
    [Tooltip("Tamanho/Escala do pontinho da trilha.")]
    public Vector3 escalaPontinho = new Vector3(0.15f, 0.15f, 1f);
    [Tooltip("Distância percorrida para instanciar um novo pontinho.")]
    public float intervaloPontinhos = 0.15f;
    [Tooltip("Tempo que cada pontinho fica visível antes de sumir.")]
    public float tempoVidaPontinho = 0.6f;

    private List<GameObject> pontinhosCriados = new List<GameObject>();

    /// <summary>
    /// Chama o efeito de arremesso a partir da posição inicial.
    /// </summary>
    /// <param name="posicaoInicial">Posição da mão do NPC.</param>
    /// <param name="olhandoParaDireita">Direção em que o item deve voar.</param>
    public void IniciarVoo(Vector3 posicaoInicial, bool olhandoParaDireita)
    {
        transform.position = posicaoInicial;
        float direcao = olhandoParaDireita ? 1f : -1f;
        Vector3 posicaoFinal = posicaoInicial + new Vector3(distanciaHorizontal * direcao, -0.5f, 0f);

        StartCoroutine(RotinaVooComTrilha(posicaoInicial, posicaoFinal));
    }

    private IEnumerator RotinaVooComTrilha(Vector3 inicio, Vector3 fim)
    {
        float tempo = 0f;
        Vector3 ultimaPosicaoPontinho = inicio;

        while (tempo < duracaoVoo)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / duracaoVoo);

            // 1. Cálculo da Trajetória Parabólica (X linear, Y em arco de seno)
            float progressoX = Mathf.Lerp(inicio.x, fim.x, t);
            float progressoY = Mathf.Lerp(inicio.y, fim.y, t) + (Mathf.Sin(t * Mathf.PI) * alturaArco);

            transform.position = new Vector3(progressoX, progressoY, inicio.z);

            // 2. Rotação do Sprite do Item no Ar
            transform.Rotate(Vector3.forward, -velocidadeRotacao * Time.deltaTime);

            // 3. Geração dos Pontinhos da Trilha por Código
            if (Vector3.Distance(transform.position, ultimaPosicaoPontinho) >= intervaloPontinhos)
            {
                CriarPontinhoTrilha(transform.position);
                ultimaPosicaoPontinho = transform.position;
            }

            yield return null;
        }

        // Finaliza o voo exatamente na posição do chão
        transform.position = fim;
        transform.rotation = Quaternion.identity; // Reseta rotação ao cair no chão
    }

    private void CriarPontinhoTrilha(Vector3 posicao)
    {
        // Cria um GameObject vazio para ser o pontinho
        GameObject pontinho = new GameObject("PontinhoTrilha");
        pontinho.transform.position = posicao;
        pontinho.transform.localScale = escalaPontinho;

        // Adiciona e configura o SpriteRenderer
        SpriteRenderer sr = pontinho.AddComponent<SpriteRenderer>();
        sr.sprite = spritePontinho;
        sr.color = corPontinho;
        sr.sortingOrder = 5; // Garante que o pontinho apareça acima do fundo

        // Programa a destruição do pontinho após alguns instantes
        Destroy(pontinho, tempoVidaPontinho);
    }
}