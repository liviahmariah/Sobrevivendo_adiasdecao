using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPC : MonoBehaviour
{
    [Header("Item e Visual na Mão")]
    public GameObject itemPrefab;
    [Tooltip("Ponto exato da mão do NPC de onde o item vai sair.")]
    public Transform pontoMao;
    [Tooltip("Objeto visual do item segurado na mão (desaparece ao soltar).")]
    public GameObject visualItemNaMao;

    [Header("Configuração do Salto do Recurso")]
    [Tooltip("Velocidade horizontal do salto (use valores positivos para direita ou negativos para esquerda, ou deixe aleatório).")]
    public float forcaHorizontal = 3.2f;
    public bool direcaoAleatoria = true;
    [Tooltip("Força vertical do pulo do item.")]
    public float forcaVertical = 7.5f;

    [Header("Trilha de Pontinhos (Trajetória)")]
    [Tooltip("Opcional: prefab de um pontinho branco/amarelo. Se deixar vazio, o script cria bolinhas automaticamente!")]
    public GameObject prefabPontinho;
    public int quantidadePontinhos = 12;
    [Tooltip("Intervalo de tempo (em segundos) entre cada pontinho na trajetória.")]
    public float espacamentoTempoPontos = 0.06f;
    public Color corPontinhos = new Color(1f, 1f, 1f, 0.85f);
    public float tamanhoInicialPonto = 0.16f;
    public float tempoParaSumirTrilha = 1.2f;

    private bool jaDropou = false;
    private readonly List<GameObject> pontinhosAtivos = new List<GameObject>();

    private void Start()
    {
        ConfigurarItemNaMao();
    }

    private void ConfigurarItemNaMao()
    {
        // Se você não criou um objeto visual manualmente, o script cria um na mão usando o sprite do itemPrefab
        if (visualItemNaMao == null && itemPrefab != null)
        {
            SpriteRenderer srPrefab = itemPrefab.GetComponentInChildren<SpriteRenderer>(true);
            if (srPrefab != null && srPrefab.sprite != null)
            {
                Transform pai = pontoMao != null ? pontoMao : transform;
                visualItemNaMao = new GameObject("VisualItemNaMao");
                visualItemNaMao.transform.SetParent(pai);
                visualItemNaMao.transform.localPosition = pontoMao != null ? Vector3.zero : new Vector3(0.4f, 0.2f, 0f);
                visualItemNaMao.transform.localScale = itemPrefab.transform.localScale;

                SpriteRenderer srMao = visualItemNaMao.AddComponent<SpriteRenderer>();
                srMao.sprite = srPrefab.sprite;
                srMao.sortingLayerID = srPrefab.sortingLayerID;
                srMao.sortingOrder = srPrefab.sortingOrder + 1;
            }
        }

        if (visualItemNaMao != null)
        {
            visualItemNaMao.SetActive(true);
        }
    }

    public void DropItem()
    {
        if (jaDropou || itemPrefab == null)
            return;

        jaDropou = true;

        // 1. Esconde o item visual que estava na mão do NPC
        if (visualItemNaMao != null)
        {
            visualItemNaMao.SetActive(false);
        }

        // 2. Define a posição exata de saída (da mão ou um pouco acima do NPC)
        Vector3 spawnPos = pontoMao != null
            ? pontoMao.position
            : transform.position + new Vector3(0f, 0.8f, 0f);

        // 3. Instancia o item real
        GameObject item = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();

        // Calcula a direção do salto
        float lado = direcaoAleatoria ? (Random.value < 0.5f ? -1f : 1f) : 1f;
        Vector2 velocidadeInicial = new Vector2(Mathf.Abs(forcaHorizontal) * lado, forcaVertical);

        float escalaGravidade = 1f;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            escalaGravidade = rb.gravityScale;

            // Aplica a velocidade direta para garantir um arco limpo e previsível
            rb.linearVelocity = velocidadeInicial;
        }

        // 4. Desenha a trilha de pontinhos seguindo a física real do salto
        DesenharTrilhaTrajetoria(spawnPos, velocidadeInicial, escalaGravidade);
    }

    private void DesenharTrilhaTrajetoria(Vector2 origem, Vector2 velocidadeInicial, float gravityScale)
    {
        LimparPontinhos();

        Vector2 gravidadeEfetiva = Physics2D.gravity * gravityScale;

        for (int i = 1; i <= quantidadePontinhos; i++)
        {
            float t = i * espacamentoTempoPontos;

            // Equação física do movimento parabólico: P(t) = P0 + V0*t + 0.5 * g * t^2
            Vector2 posicaoPonto = origem + (velocidadeInicial * t) + (0.5f * gravidadeEfetiva * t * t);

            GameObject ponto = CriarObjetoPontinho(posicaoPonto, i);
            pontinhosAtivos.Add(ponto);
        }

        StartCoroutine(ApagarTrilhaGradualmente());
    }

    private GameObject CriarObjetoPontinho(Vector2 posicao, int indice)
    {
        GameObject ponto;

        if (prefabPontinho != null)
        {
            ponto = Instantiate(prefabPontinho, posicao, Quaternion.identity);
        }
        else
        {
            // Gera uma textura de círculo suave em tempo real (não precisa importar sprite extra!)
            ponto = new GameObject("PontoTrajetoria_" + indice);
            ponto.transform.position = posicao;

            SpriteRenderer sr = ponto.AddComponent<SpriteRenderer>();
            sr.sprite = GerarSpriteCirculo();
            sr.color = corPontinhos;
            sr.sortingOrder = 15;
        }

        // Faz os pontinhos diminuírem levemente conforme vão longe (efeito visual de rastro)
        float fatorEscala = Mathf.Lerp(1f, 0.45f, (float)indice / quantidadePontinhos);
        ponto.transform.localScale = Vector3.one * (tamanhoInicialPonto * fatorEscala);

        return ponto;
    }

    private IEnumerator ApagarTrilhaGradualmente()
    {
        yield return new WaitForSeconds(tempoParaSumirTrilha);

        // Faz os pontinhos sumirem um por um na ordem do salto
        for (int i = 0; i < pontinhosAtivos.Count; i++)
        {
            if (pontinhosAtivos[i] != null)
            {
                Destroy(pontinhosAtivos[i]);
            }
            yield return new WaitForSeconds(0.03f);
        }

        pontinhosAtivos.Clear();
    }

    private void LimparPontinhos()
    {
        for (int i = 0; i < pontinhosAtivos.Count; i++)
        {
            if (pontinhosAtivos[i] != null)
                Destroy(pontinhosAtivos[i]);
        }
        pontinhosAtivos.Clear();
    }

    // Cria um sprite circular em memória caso nenhum prefab de ponto seja atribuído
    private static Sprite spriteCirculoCache;
    private static Sprite GerarSpriteCirculo()
    {
        if (spriteCirculoCache != null)
            return spriteCirculoCache;

        int resolucao = 32;
        Texture2D tex = new Texture2D(resolucao, resolucao, TextureFormat.RGBA32, false);
        float centro = (resolucao - 1) * 0.5f;
        float raio = resolucao * 0.45f;

        for (int y = 0; y < resolucao; y++)
        {
            for (int x = 0; x < resolucao; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centro, centro));
                float alpha = Mathf.Clamp01(1f - ((dist - (raio - 1.5f)) / 1.5f));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        spriteCirculoCache = Sprite.Create(tex, new Rect(0, 0, resolucao, resolucao), new Vector2(0.5f, 0.5f), 32f);
        return spriteCirculoCache;
    }
}