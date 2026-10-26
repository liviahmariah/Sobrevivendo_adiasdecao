using System.Collections;
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
        if (MercadoManager.Instance == null)
            return;

        if (!MercadoManager.Instance.PodeColetar())
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

        Vector3 posicaoFinal = ponto.position;

        GameObject recurso = Instantiate(
            prefab,
            posicaoFinal,
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

        StartCoroutine(AnimarSpawn(recurso, posicaoFinal));
    }

    private IEnumerator AnimarSpawn(
        GameObject recurso,
        Vector3 posicaoFinal)
    {
        if (recurso == null)
            yield break;

        Transform visual = recurso.transform;

        Vector3 escalaFinal = visual.localScale;

        // Começa pequeno, como se estivesse aparecendo magicamente.
        visual.localScale = escalaFinal * escalaInicial;

        float tempo = 0f;

        while (tempo < duracaoEntrada)
        {
            if (recurso == null)
                yield break;

            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracaoEntrada
            );

            // EaseOutBack: cria uma pequena sensação de elasticidade.
            float t = 1f - progresso;
            float elasticidade =
                1f + 2.2f * Mathf.Pow(t, 3f)
                + 1.2f * Mathf.Pow(t, 2f) * -1f;

            visual.localScale = Vector3.LerpUnclamped(
                escalaFinal * escalaInicial,
                escalaFinal,
                Mathf.Clamp01(progresso)
            );

            // Mantém a entrada suave e termina no tamanho original.
            visual.localScale = Vector3.Lerp(
                escalaFinal * escalaInicial,
                escalaFinal,
                Mathf.SmoothStep(0f, 1f, progresso)
            );

            yield return null;
        }

        if (recurso == null)
            yield break;

        visual.localScale = escalaFinal;

        // Pulo curto de aterrissagem.
        tempo = 0f;

        while (tempo < duracaoPulo)
        {
            if (recurso == null)
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

        if (recurso == null)
            yield break;

        visual.position = posicaoFinal;

        // Flutuação contínua depois que o recurso aparece.
        if (flutuarDepoisDoSpawn)
        {
            float faseInicial = Random.Range(0f, Mathf.PI * 2f);

            while (recurso != null)
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
