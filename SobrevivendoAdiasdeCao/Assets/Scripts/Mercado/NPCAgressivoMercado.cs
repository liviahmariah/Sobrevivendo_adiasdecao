using UnityEngine;

public class NPCAgressivoMercado : MonoBehaviour
{
    [Header("Configuração da agressividade")]
    public bool eAgressivo = false;

    [Header("Identificação visual (Editável no Prefab)")]
    [Tooltip("Arraste aqui o objeto filho do sinal de raiva criado no Prefab para posicioná-lo livremente na Scene.")]
    public GameObject objetoIndicador;

    [Header("Fallback Automático (caso não use objeto filho)")]
    public Sprite imagemAgressividade;
    public Vector3 deslocamentoImagem = new Vector3(0f, 1.2f, 0f);
    public Vector2 tamanhoImagem = new Vector2(0.6f, 0.6f);

    [Header("Dano")]
    [Tooltip("Cada contato causa a perda de uma unidade de energia.")]
    public int danoPorContato = 1;

    private EnergiaSandy energiaSandy;
    private bool contatoAtivo;

    private void Awake()
    {
        InicializarIndicadorVisual();
    }

    // Chamado pelo SpawnNPCMercado após sortear a agressividade.
    public void ConfigurarAgressividade(bool agressivo)
    {
        eAgressivo = agressivo;
        AtualizarIndicadorVisual();
    }

    private void InicializarIndicadorVisual()
    {
        // Se você já criou e posicionou o indicador no Prefab, apenas usa ele!
        if (objetoIndicador != null)
        {
            AtualizarIndicadorVisual();
            return;
        }

        // Caso não tenha arrastado um objeto filho, tenta encontrar um chamado "IndicadorAgressividade"
        Transform filhoExistente = transform.Find("IndicadorAgressividade");
        if (filhoExistente != null)
        {
            objetoIndicador = filhoExistente.gameObject;
            AtualizarIndicadorVisual();
            return;
        }

        // Fallback: cria via código se nenhum objeto filho existir no Prefab
        if (imagemAgressividade == null)
            return;

        objetoIndicador = new GameObject("IndicadorAgressividade");
        objetoIndicador.transform.SetParent(transform, false);
        objetoIndicador.transform.localPosition = deslocamentoImagem;
        objetoIndicador.transform.localScale = new Vector3(tamanhoImagem.x, tamanhoImagem.y, 1f);

        SpriteRenderer indicador = objetoIndicador.AddComponent<SpriteRenderer>();
        indicador.sprite = imagemAgressividade;
        indicador.drawMode = SpriteDrawMode.Simple;
        indicador.sortingOrder = 100;
        indicador.color = Color.white;

        AtualizarIndicadorVisual();
    }

    private void AtualizarIndicadorVisual()
    {
        if (objetoIndicador != null)
            objetoIndicador.SetActive(eAgressivo);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!eAgressivo || contatoAtivo)
            return;

        EnergiaSandy energia = other.GetComponentInParent<EnergiaSandy>();
        if (energia == null)
            return;

        energiaSandy = energia;
        contatoAtivo = true;

        AplicarDano();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (energiaSandy == null)
            return;

        EnergiaSandy energia = other.GetComponentInParent<EnergiaSandy>();
        if (energia == energiaSandy)
        {
            energiaSandy = null;
            contatoAtivo = false;
        }
    }

    private void AplicarDano()
    {
        if (energiaSandy == null)
            return;

        energiaSandy.PerderEnergia();
    }

    private void OnValidate()
    {
        if (objetoIndicador != null)
            objetoIndicador.SetActive(eAgressivo);
    }
}