
using UnityEngine;

public class SpawnRecursosMercado : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] prefabsComida;

    [Header("Pontos de Spawn")]
    public Transform[] pontosSpawn;

    [Header("Quantidade")]
    public int quantidadeInicial = 8;

    [Header("Intervalo de Spawn")]
    public float intervaloSpawn = 5f;

    [Header("Spawn Aleatório")]
    [Range(0f, 1f)]
    public float chanceComidaRuim = 0.2f;

    private float proximoSpawn;
    private bool spawnInicialRealizado = false;

    private void Start()
    {
        proximoSpawn = Time.time + intervaloSpawn;
    }

    private void Update()
    {
        // Só gera recursos durante a coleta do Mercado.
        if (MercadoManager.Instance == null)
            return;

        if (!MercadoManager.Instance.PodeColetar())
            return;

        // Gera a quantidade inicial uma única vez.
        if (!spawnInicialRealizado)
        {
            SpawnQuantidadeInicial();
            spawnInicialRealizado = true;

            proximoSpawn = Time.time + intervaloSpawn;
            return;
        }

        // Gera recursos periodicamente.
        if (Time.time >= proximoSpawn)
        {
            SpawnRecurso();
            proximoSpawn = Time.time + intervaloSpawn;
        }
    }

    private void SpawnQuantidadeInicial()
    {
        if (!ConfiguracaoValida())
            return;

        int quantidade = quantidadeInicial;

        if (CommunityManager.instance != null)
        {
            float multiplicador =
                CommunityManager.instance.MultiplicadorSpawnRecursos();

            quantidade = Mathf.Max(
                0,
                Mathf.RoundToInt(quantidadeInicial * multiplicador)
            );
        }

        for (int i = 0; i < quantidade; i++)
        {
            SpawnRecurso();
        }

        Debug.Log(
            "MERCADO | Spawn inicial: " + quantidade + " recursos."
        );
    }

    private bool ConfiguracaoValida()
    {
        if (prefabsComida == null || prefabsComida.Length == 0)
        {
            Debug.LogWarning(
                "SpawnRecursosMercado: configure os prefabs de comida!"
            );
            return false;
        }

        if (pontosSpawn == null || pontosSpawn.Length == 0)
        {
            Debug.LogWarning(
                "SpawnRecursosMercado: configure os pontos de spawn!"
            );
            return false;
        }

        return true;
    }

    private void SpawnRecurso()
    {
        if (!ConfiguracaoValida())
            return;

        Transform ponto =
            pontosSpawn[Random.Range(0, pontosSpawn.Length)];

        GameObject prefab =
            prefabsComida[Random.Range(0, prefabsComida.Length)];

        if (ponto == null || prefab == null)
            return;

        GameObject recurso = Instantiate(
            prefab,
            ponto.position,
            Quaternion.identity
        );

        RecursoColetavel componente =
            recurso.GetComponent<RecursoColetavel>();

        if (componente != null)
        {
            componente.tipoRecurso =
                RecursoColetavel.TipoRecurso.Comida;

            componente.recursoRuim =
                Random.value < chanceComidaRuim;

            componente.AtualizarAparencia();
        }
        else
        {
            Debug.LogWarning(
                "SpawnRecursosMercado: o prefab " +
                prefab.name +
                " não possui RecursoColetavel!"
            );
        }
    }
}
