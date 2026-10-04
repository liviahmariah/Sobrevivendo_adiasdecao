using UnityEngine;
using System.Collections.Generic;

public class MercadoManager : MonoBehaviour
{
    public static MercadoManager Instance;

    [Header("Comida conseguida nesta fase")]
    public float comidaConseguida = 0f;

    private HashSet<string> tiposDeComidaConseguidos =
        new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("FASE MERCADO INICIADA!");

        if (DayManager.Instance != null)
        {
            Debug.Log(
                "Tempo restante do dia: " +
                DayManager.Instance.ObterTempoRestante() +
                "s"
            );
        }
    }

    public void AdicionarComida(float valor)
    {
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

        // Avisa a caixa visual.
        if (CaixaComidasUI.Instance != null)
        {
            CaixaComidasUI.Instance.AdicionarComida(
                idComida,
                imagemComida
            );
        }
        else
        {
            Debug.LogWarning(
                "MercadoManager: CaixaComidasUI não encontrada!"
            );
        }

        Debug.Log(
            "MERCADO | NOVO TIPO DE COMIDA: " +
            idComida
        );

        return true;
    }

    public bool JaConseguiuComida(string idComida)
    {
        return tiposDeComidaConseguidos.Contains(idComida);
    }

    public float ObterComidaConseguida()
    {
        return comidaConseguida;
    }

    public void FinalizarMercado()
    {
        Debug.Log(
            "Mercado finalizado | Comida conseguida: " +
            comidaConseguida
        );

        if (CommunityManager.instance != null)
        {
            CommunityManager.instance.AlterarAlimentacao(
                comidaConseguida
            );

            Debug.Log(
                "Alimentação da comunidade aumentou em: " +
                comidaConseguida
            );
        }
        else
        {
            Debug.LogWarning(
                "MercadoManager: CommunityManager não encontrado!"
            );
        }

        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.VoltarParaMapa();
        }
    }
}