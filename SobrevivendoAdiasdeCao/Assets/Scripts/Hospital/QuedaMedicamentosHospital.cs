
using UnityEngine;
using System.Collections;

public class QuedaMedicamentosHospital : MonoBehaviour
{
    [Header("Referências")]
    public Transform[] pontosQueda;
    public GameObject[] prefabsMedicamentos;

    [Header("Intervalo entre quedas")]
    public float intervaloInicial = 2.5f;
    public float intervaloFinal = 0.7f;

    [Header("Velocidade de queda")]
    public float velocidadeInicial = 2f;
    public float velocidadeFinal = 5f;

    [Header("Animação")]
    public float duracaoEntrada = 0.25f;
    public float rotacaoMaxima = 12f;
    public float balancoHorizontal = 0.12f;

    [Header("Limite de medicamentos")]
    public int maxMedicamentosSimultaneos = 8;

    [Header("Chance de medicamento ruim")]
    [Range(0f, 1f)]
    public float chanceMedicamentoRuim = 0.2f;

    private float tempoProximaQueda;
    private int medicamentosAtivos;
    private bool inicializado;

    private void Start()
    {
        tempoProximaQueda = intervaloInicial;
        inicializado = true;
    }

    private void Update()
    {
        if (!inicializado)
            return;

        if (HospitalManager.Instance == null)
            return;

        if (!HospitalManager.Instance.PodeColetar())
            return;

        if (pontosQueda == null || pontosQueda.Length == 0 ||
            prefabsMedicamentos == null || prefabsMedicamentos.Length == 0)
            return;

        if (medicamentosAtivos >= maxMedicamentosSimultaneos)
            return;

        tempoProximaQueda -= Time.deltaTime;

        if (tempoProximaQueda <= 0f)
        {
            CriarMedicamento();
            AtualizarIntervalo();
        }
    }

    private float ObterDificuldade()
    {
        float tempoRestante =
            HospitalManager.Instance.ObterTempoRestante();

        float tempoTotal = Mathf.Max(
            HospitalManager.Instance.tempoMaximo,
            1f
        );

        return 1f - Mathf.Clamp01(tempoRestante / tempoTotal);
    }

    private void AtualizarIntervalo()
    {
        float dificuldade = ObterDificuldade();

        tempoProximaQueda = Mathf.Lerp(
            intervaloInicial,
            intervaloFinal,
            dificuldade
        );
    }

    private void CriarMedicamento()
    {
        Transform ponto = pontosQueda[
            Random.Range(0, pontosQueda.Length)
        ];

        GameObject prefab = prefabsMedicamentos[
            Random.Range(0, prefabsMedicamentos.Length)
        ];

        if (ponto == null || prefab == null)
            return;

        GameObject medicamento = Instantiate(
            prefab,
            ponto.position,
            Quaternion.identity
        );

        MedicamentoColetavel componente =
            medicamento.GetComponent<MedicamentoColetavel>();

        if (componente == null)
        {
            Debug.LogWarning(
                "O prefab não possui MedicamentoColetavel!",
                medicamento
            );

            Destroy(medicamento);
            return;
        }

        componente.medicamentoRuim =
            Random.value < chanceMedicamentoRuim;

        componente.AtualizarAparencia();

        medicamentosAtivos++;

        StartCoroutine(AnimarQueda(medicamento));
    }

    private float ObterVelocidade()
    {
        float dificuldade = ObterDificuldade();

        return Mathf.Lerp(
            velocidadeInicial,
            velocidadeFinal,
            dificuldade
        );
    }

    private IEnumerator AnimarQueda(GameObject medicamento)
    {
        if (medicamento == null)
        {
            medicamentosAtivos--;
            yield break;
        }

        Transform visual = medicamento.transform;

        Vector3 escalaFinal = visual.localScale;

        float velocidade = ObterVelocidade();

        float rotacao = Random.Range(
            -rotacaoMaxima,
            rotacaoMaxima
        );

        float direcao = Random.value < 0.5f ? -1f : 1f;

        visual.localScale = Vector3.zero;

        float tempo = 0f;

        while (tempo < duracaoEntrada)
        {
            if (medicamento == null)
            {
                medicamentosAtivos--;
                yield break;
            }

            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracaoEntrada
            );

            float escala = Mathf.Sin(
                progresso * Mathf.PI * 0.5f
            );

            visual.localScale = escalaFinal * escala;

            yield return null;
        }

        if (medicamento == null)
        {
            medicamentosAtivos--;
            yield break;
        }

        visual.localScale = escalaFinal;

        while (medicamento != null)
        {
            if (HospitalManager.Instance == null ||
                !HospitalManager.Instance.PodeColetar())
                break;

            float deslocamento = Mathf.Sin(
                Time.time * 5f
            ) * balancoHorizontal * direcao;

            visual.position += new Vector3(
                deslocamento * Time.deltaTime,
                -velocidade * Time.deltaTime,
                0f
            );

            visual.Rotate(
                0f,
                0f,
                rotacao * Time.deltaTime
            );

            yield return null;
        }

        if (medicamento != null)
            Destroy(medicamento);

        medicamentosAtivos--;
    }
}
