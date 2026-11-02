using System.Collections;
using UnityEngine;

public class SpawnerBrinquedosPraca : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] prefabsBrinquedos;

    [Header("Pontos de Spawn")]
    public Transform[] pontosSpawn;

    [Header("Quantidade")]
    public int quantidadeInicial = 8;

    [Header("Intervalo de Spawn")]
    public float intervaloSpawn = 5f;

    [Header("Spawn Aleatório")]
    [Range(0f, 1f)]
    public float chanceBrinquedoRuim = 0.2f;

    [Header("Animação de Spawn")]
    public float duracaoEntrada = 0.35f;
    public float alturaPulo = 0.65f;
    public float duracaoPulo = 0.4f;
    public float escalaInicial = 0.2f;

    [Header("Flutuação")]
    public bool flutuarDepoisDoSpawn = true;
    public float alturaFlutuacao = 0.08f;
    public float velocidadeFlutuacao = 2.5f;

    private float proximoSpawn;
    private bool spawnInicialRealizado = false;

    private void Start()
    {
        proximoSpawn = Time.time + intervaloSpawn;
    }

    private void Update()
    {
        if (PracaManager.Instance == null)
            return;

        if (!PracaManager.Instance.PodeColetar())
            return;

        if (!spawnInicialRealizado)
        {
            SpawnQuantidadeInicial();
            spawnInicialRealizado = true;

            proximoSpawn = Time.time + intervaloSpawn;
            return;
        }

        if (Time.time >= proximoSpawn)
        {
            SpawnBrinquedo();
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
            SpawnBrinquedo();
        }

        Debug.Log(
            "PRAÇA | Spawn inicial: " + quantidade + " brinquedos."
        );
    }

    private bool ConfiguracaoValida()
    {
        if (prefabsBrinquedos == null ||
            prefabsBrinquedos.Length == 0)
        {
            Debug.LogWarning(
                "SpawnerBrinquedosPraca: configure os prefabs!"
            );
            return false;
        }

        if (pontosSpawn == null || pontosSpawn.Length == 0)
        {
            Debug.LogWarning(
                "SpawnerBrinquedosPraca: configure os pontos de spawn!"
            );
            return false;
        }

        return true;
    }

    private void SpawnBrinquedo()
    {
        if (!ConfiguracaoValida())
            return;

        Transform ponto =
            pontosSpawn[Random.Range(0, pontosSpawn.Length)];

        GameObject prefab =
            prefabsBrinquedos[
                Random.Range(0, prefabsBrinquedos.Length)
            ];

        if (ponto == null || prefab == null)
            return;

        Vector3 posicaoFinal = ponto.position;

        GameObject brinquedo = Instantiate(
            prefab,
            posicaoFinal,
            Quaternion.identity
        );

        BrinquedoPraca componente =
            brinquedo.GetComponent<BrinquedoPraca>();

        if (componente != null)
        {
            componente.brinquedoRuim =
                Random.value < chanceBrinquedoRuim;
        }
        else
        {
            Debug.LogWarning(
                "SpawnerBrinquedosPraca: o prefab " +
                prefab.name +
                " não possui BrinquedoPraca!"
            );
        }

        StartCoroutine(
            AnimarSpawn(brinquedo, posicaoFinal)
        );
    }

    private IEnumerator AnimarSpawn(
        GameObject brinquedo,
        Vector3 posicaoFinal)
    {
        if (brinquedo == null)
            yield break;

        Transform visual = brinquedo.transform;
        Vector3 escalaFinal = visual.localScale;

        visual.localScale = escalaFinal * escalaInicial;

        float tempo = 0f;

        while (tempo < duracaoEntrada)
        {
            if (brinquedo == null)
                yield break;

            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracaoEntrada
            );

            visual.localScale = Vector3.Lerp(
                escalaFinal * escalaInicial,
                escalaFinal,
                Mathf.SmoothStep(0f, 1f, progresso)
            );

            yield return null;
        }

        if (brinquedo == null)
            yield break;

        visual.localScale = escalaFinal;

        tempo = 0f;

        while (tempo < duracaoPulo)
        {
            if (brinquedo == null)
                yield break;

            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracaoPulo
            );

            float altura =
                Mathf.Sin(progresso * Mathf.PI) * alturaPulo;

            visual.position = posicaoFinal
                + Vector3.up * altura;

            yield return null;
        }

        if (brinquedo == null)
            yield break;

        visual.position = posicaoFinal;

        if (flutuarDepoisDoSpawn)
        {
            float faseInicial =
                Random.Range(0f, Mathf.PI * 2f);

            while (brinquedo != null)
            {
                float deslocamento =
                    Mathf.Sin(
                        Time.time * velocidadeFlutuacao
                        + faseInicial
                    ) * alturaFlutuacao;

                visual.position = posicaoFinal
                    + Vector3.up * deslocamento;

                yield return null;
            }
        }
    }
}