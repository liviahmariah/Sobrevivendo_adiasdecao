using System.Collections.Generic;
using UnityEngine;

public class SpawnNPCPraca : MonoBehaviour
{
    [Header("Prefabs de NPCs da Praça")]
    public GameObject[] prefabsNPC;

    [Header("Entradas e saídas")]
    public Transform entradaEsquerda;
    public Transform saidaDireita;
    public Transform entradaDireita;
    public Transform saidaEsquerda;

    [Header("Intervalo")]
    public float intervaloMinimo = 3f;
    public float intervaloMaximo = 7f;

    [Header("Limite")]
    public int maximoNPCsAtivos = 4;

    [Header("Agressividade aleatória")]
    [Range(0f, 100f)]
    public float chanceNPCagressivo = 10f;

    private float proximoSpawn;

    private readonly List<GameObject> npcsAtivos =
        new List<GameObject>();

    private void Start()
    {
        AgendarSpawn();
    }

    private void Update()
    {
        npcsAtivos.RemoveAll(npc => npc == null);

        if (PracaManager.Instance == null ||
            !PracaManager.Instance.PodeColetar())
        {
            return;
        }

        if (Time.time < proximoSpawn)
            return;

        if (npcsAtivos.Count >= maximoNPCsAtivos)
            return;

        SpawnarNPC();
        AgendarSpawn();
    }

    private void SpawnarNPC()
    {
        if (prefabsNPC == null || prefabsNPC.Length == 0)
            return;

        bool esquerdaParaDireita = Random.value < 0.5f;

        Transform entrada = esquerdaParaDireita
            ? entradaEsquerda
            : entradaDireita;

        Transform saida = esquerdaParaDireita
            ? saidaDireita
            : saidaEsquerda;

        if (entrada == null || saida == null)
        {
            Debug.LogError(
                "SpawnNPCPraca: entradas/saídas ausentes."
            );
            return;
        }

        GameObject prefab =
            prefabsNPC[Random.Range(0, prefabsNPC.Length)];

        if (prefab == null)
            return;

        GameObject npc = Instantiate(
            prefab,
            entrada.position,
            Quaternion.identity
        );

        npc.name = prefab.name + "_Praca";

        bool seraAgressivo =
            Random.Range(0f, 100f) < chanceNPCagressivo;

        NPCAgressivoMercado agressividade =
            npc.GetComponent<NPCAgressivoMercado>();

        if (agressividade == null)
        {
            agressividade =
                npc.GetComponentInChildren<NPCAgressivoMercado>();
        }

        if (agressividade != null)
        {
            agressividade.ConfigurarAgressividade(seraAgressivo);
        }

        NPCMovimentoMercado movimento =
            npc.GetComponent<NPCMovimentoMercado>();

        if (movimento == null)
        {
            movimento =
                npc.GetComponentInChildren<NPCMovimentoMercado>();
        }

        if (movimento == null)
        {
            Debug.LogError(
                "O prefab " + prefab.name +
                " não tem NPCMovimentoMercado."
            );

            Destroy(npc);
            return;
        }

        Vector2 direcao = esquerdaParaDireita
            ? Vector2.right
            : Vector2.left;

        movimento.Configurar(direcao, saida);

        npcsAtivos.Add(npc);
    }

    private void AgendarSpawn()
    {
        float minimo = Mathf.Max(0.1f, intervaloMinimo);
        float maximo = Mathf.Max(minimo, intervaloMaximo);

        proximoSpawn =
            Time.time + Random.Range(minimo, maximo);
    }
}