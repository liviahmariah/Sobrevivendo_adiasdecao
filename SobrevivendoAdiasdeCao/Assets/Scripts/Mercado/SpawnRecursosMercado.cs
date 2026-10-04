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

    private void Start()
    {
        SpawnQuantidadeInicial();

        proximoSpawn = Time.time + intervaloSpawn;
    }

    private void Update()
    {
        if (Time.time >= proximoSpawn)
        {
            SpawnRecurso();

            proximoSpawn = Time.time + intervaloSpawn;
        }
    }

    private void SpawnQuantidadeInicial()
    {
        if (prefabsComida == null || prefabsComida.Length == 0)
        {
            Debug.LogWarning(
                "SpawnRecursosMercado: Nenhum prefab de comida foi configurado!"
            );

            return;
        }

        if (pontosSpawn == null || pontosSpawn.Length == 0)
        {
            Debug.LogWarning(
                "SpawnRecursosMercado: Nenhum ponto de spawn foi configurado!"
            );

            return;
        }

        int quantidade = quantidadeInicial;

        if (CommunityManager.instance != null)
        {
            float multiplicador =
                CommunityManager.instance.MultiplicadorSpawnRecursos();

            quantidade = Mathf.RoundToInt(
                quantidadeInicial * multiplicador
            );
        }

        for (int i = 0; i < quantidade; i++)
        {
            SpawnRecurso();
        }
    }

    private void SpawnRecurso()
    {
        if (prefabsComida == null || prefabsComida.Length == 0)
            return;

        if (pontosSpawn == null || pontosSpawn.Length == 0)
            return;

        Transform ponto =
            pontosSpawn[Random.Range(0, pontosSpawn.Length)];

        GameObject prefab =
            prefabsComida[Random.Range(0, prefabsComida.Length)];

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
        }
    }
}