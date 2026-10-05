using UnityEngine;

public class ConfeteVitoria : MonoBehaviour
{
    [Header("Quantidade")]
    [SerializeField] private int quantidadePorLado = 30;

    [Header("Altura do lançamento")]
    [SerializeField] private float alturaLancamento = -3.5f;

    [Header("Força")]
    [SerializeField] private float forcaHorizontal = 4f;
    [SerializeField] private float forcaVerticalMin = 5f;
    [SerializeField] private float forcaVerticalMax = 8f;

    [Header("Confete")]
    [SerializeField] private float tamanhoMin = 0.12f;
    [SerializeField] private float tamanhoMax = 0.22f;

    [Header("Tempo de vida")]
    [SerializeField] private float tempoVidaMin = 3f;
    [SerializeField] private float tempoVidaMax = 5f;

    [Header("Gravidade")]
    [SerializeField] private float gravidade = 1.5f;

    [Header("Distância das laterais")]
    [SerializeField] private float distanciaLateral = 1f;

    [Header("Cores")]
    [SerializeField]
    private Color[] cores =
    {
        new Color(1f, 0.2f, 0.2f),   // Vermelho
        new Color(1f, 0.85f, 0.1f),  // Amarelo
        new Color(0.2f, 0.6f, 1f),   // Azul
        new Color(0.2f, 0.9f, 0.4f), // Verde
        new Color(1f, 0.3f, 0.7f),   // Rosa
        new Color(1f, 0.5f, 0.1f)    // Laranja
    };

    private Camera cameraPrincipal;

    private void Start()
    {
        cameraPrincipal = Camera.main;

        if (cameraPrincipal == null)
        {
            Debug.LogError("ConfeteVitoria: nenhuma Main Camera foi encontrada!");
            return;
        }

        CriarConfetes();
    }

    private void CriarConfetes()
    {
        for (int i = 0; i < quantidadePorLado; i++)
        {
            CriarConfete(-1);
            CriarConfete(1);
        }
    }

    private void CriarConfete(int lado)
    {
        GameObject confete = new GameObject("Confete");

        confete.transform.SetParent(transform);

        // Descobre os limites da câmera
        float alturaCamera = cameraPrincipal.orthographicSize;
        float larguraCamera =
            alturaCamera * cameraPrincipal.aspect;

        // Posição inicial
        float posicaoX;

        if (lado < 0)
        {
            // Lado esquerdo
            posicaoX = -larguraCamera - distanciaLateral;
        }
        else
        {
            // Lado direito
            posicaoX = larguraCamera + distanciaLateral;
        }

        float posicaoY =
            cameraPrincipal.transform.position.y +
            alturaLancamento +
            Random.Range(-1f, 1f);

        confete.transform.position = new Vector3(
            cameraPrincipal.transform.position.x + posicaoX,
            posicaoY,
            0f
        );

        // Sprite
        SpriteRenderer spriteRenderer =
            confete.AddComponent<SpriteRenderer>();

        spriteRenderer.sprite = CriarSpritePixel();

        // Cor aleatória
        spriteRenderer.color =
            cores[Random.Range(0, cores.Length)];

        // Sempre na frente da tela de vitória
        spriteRenderer.sortingOrder = 1000;

        // Tamanho
        float tamanho =
            Random.Range(tamanhoMin, tamanhoMax);

        confete.transform.localScale = new Vector3(
            tamanho,
            tamanho * Random.Range(0.7f, 1.4f),
            1f
        );

        // Física
        Rigidbody2D rb =
            confete.AddComponent<Rigidbody2D>();

        rb.gravityScale = gravidade;

        float velocidadeX =
            Random.Range(
                forcaHorizontal * 0.7f,
                forcaHorizontal * 1.3f
            );

        // Esquerda → direita
        // Direita → esquerda
        velocidadeX *= -lado;

        float velocidadeY =
            Random.Range(
                forcaVerticalMin,
                forcaVerticalMax
            );

        rb.linearVelocity = new Vector2(
            velocidadeX,
            velocidadeY
        );

        // Rotação
        rb.angularVelocity =
            Random.Range(-600f, 600f);

        // Destruição
        Destroy(
            confete,
            Random.Range(
                tempoVidaMin,
                tempoVidaMax
            )
        );
    }

    private Sprite CriarSpritePixel()
    {
        // Sprite 8x8 para ficar realmente visível
        Texture2D textura =
            new Texture2D(8, 8);

        textura.filterMode =
            FilterMode.Point;

        textura.wrapMode =
            TextureWrapMode.Clamp;

        // Preenche todos os pixels
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                textura.SetPixel(
                    x,
                    y,
                    Color.white
                );
            }
        }

        textura.Apply();

        Sprite sprite =
            Sprite.Create(
                textura,
                new Rect(0, 0, 8, 8),
                new Vector2(0.5f, 0.5f),
                32f
            );

        return sprite;
    }
}