using UnityEngine;
using System.Collections.Generic;

public class MercadoManager : FaseManager
{
    public static MercadoManager Instance;

    [Header("Comida conseguida nesta fase")]
    public float comidaConseguida = 0f;

    [Header("Penalidade por ser pega")]
    public float perdaFelicidadeAoSerPega = 10f;

    private HashSet<string> tiposDeComidaConseguidos =
        new HashSet<string>();

    protected override void Awake()
    {
        base.Awake();

        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected override void Start()
    {
        // Configuração específica do Mercado
        tipoFase = TipoFase.Mercado;

        comidaConseguida = 0f;

        tiposDeComidaConseguidos.Clear();

        base.Start();
    }

    // ==========================================
    // TEMPO ESPECÍFICO DO MERCADO
    // ==========================================

    protected override float CalcularTempoDaFase()
    {
        if (CommunityManager.instance == null)
        {
            Debug.LogWarning(
                "MercadoManager: CommunityManager não encontrado!"
            );

            return tempoMaximo;
        }

        float alimentacao =
            CommunityManager.instance.alimentacao;

        alimentacao =
            Mathf.Clamp(alimentacao, 0f, 100f);

        float percentual =
            alimentacao / 100f;

        float tempo =
            Mathf.Lerp(
                tempoMinimo,
                tempoMaximo,
                percentual
            );

        return Mathf.Clamp(
            tempo,
            tempoMinimo,
            tempoMaximo
        );
    }

    // ==========================================
    // COMIDA
    // ==========================================

    public void AdicionarComida(float valor)
    {
        if (faseFinalizada)
            return;

        if (!coletaAtiva)
            return;

        comidaConseguida += valor;

        if (comidaConseguida < 0f)
        {
            comidaConseguida = 0f;
        }

        Debug.Log(
            "MERCADO | Comida conseguida: " +
            comidaConseguida
        );
    }

    public bool RegistrarTipoComida(
        string idComida,
        Sprite imagemComida)
    {
        if (!coletaAtiva)
            return false;

        if (string.IsNullOrEmpty(idComida))
        {
            Debug.LogWarning(
                "MercadoManager: ID da comida está vazio!"
            );

            return false;
        }

        if (tiposDeComidaConseguidos.Contains(idComida))
        {
            return false;
        }

        tiposDeComidaConseguidos.Add(idComida);

        if (CaixaComidasUI.Instance != null)
        {
            CaixaComidasUI.Instance.AdicionarComida(
                idComida,
                imagemComida
            );
        }

        return true;
    }

    public bool JaConseguiuComida(string idComida)
    {
        return tiposDeComidaConseguidos.Contains(
            idComida
        );
    }

    public float ObterComidaConseguida()
    {
        return comidaConseguida;
    }

    // ==========================================
    // VITÓRIA DO MERCADO
    // ==========================================

    protected override void Vitoria()
    {
        if (faseFinalizada)
            return;

        Debug.Log("=================================");
        Debug.Log("MERCADO: VITÓRIA!");
        Debug.Log(
            "Comida entregue: " +
            comidaConseguida
        );
        Debug.Log("=================================");

        // A comida vai para a comunidade.
        if (CommunityManager.instance != null)
        {
            CommunityManager.instance.AlterarAlimentacao(
                comidaConseguida
            );
        }

        base.Vitoria();
    }

    // ==========================================
    // DERROTA DO MERCADO
    // ==========================================

    public override void Derrota()
    {
        if (faseFinalizada)
            return;

        Debug.Log("=================================");
        Debug.Log("MERCADO: DERROTA!");
        Debug.Log(
            "Comida perdida: " +
            comidaConseguida
        );

        // Perde felicidade.
        if (CommunityManager.instance != null)
        {
            CommunityManager.instance.AlterarFelicidade(
                -perdaFelicidadeAoSerPega
            );

            Debug.Log(
                "Felicidade perdida: " +
                perdaFelicidadeAoSerPega
            );
        }

        // Não adicionamos comida.
        // Portanto, a comida coletada é perdida.

        Debug.Log("=================================");

        base.Derrota();
    }
}