
using UnityEngine;
using System.Collections.Generic;

public class CaixaComidasUI : MonoBehaviour
{
    public static CaixaComidasUI Instance;

    [Header("Onde os ícones vão aparecer")]
    public RectTransform conteudo;

    [Header("Modelo do ícone")]
    public GameObject itemComidaPrefab;

    [Header("Configuração da pilha")]
    public float margem = 10f;

    [Header("Variação dos itens")]
    public float variacaoRotacao = 12f;

    private class EntradaComida
    {
        public string idComida;
        public ItemComidaUI item;
    }

    private readonly List<EntradaComida> itens =
        new List<EntradaComida>();

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

    public void AdicionarComida(string idComida, Sprite imagemComida)
    {
        if (itemComidaPrefab == null)
        {
            Debug.LogError(
                "CaixaComidasUI: ItemComidaPrefab não configurado!"
            );
            return;
        }

        if (conteudo == null)
        {
            Debug.LogError(
                "CaixaComidasUI: Conteudo não configurado!"
            );
            return;
        }

        GameObject novoObjeto = Instantiate(
            itemComidaPrefab,
            conteudo
        );

        ItemComidaUI item =
            novoObjeto.GetComponent<ItemComidaUI>();

        if (item == null)
        {
            Debug.LogError(
                "CaixaComidasUI: ItemComidaUI não encontrado no prefab!"
            );

            Destroy(novoObjeto);
            return;
        }

        RectTransform rect =
            novoObjeto.GetComponent<RectTransform>();

        if (rect != null)
        {
            float largura = conteudo.rect.width;
            float altura = conteudo.rect.height;

            float metadeLargura = rect.rect.width / 2f;
            float metadeAltura = rect.rect.height / 2f;

            float xMin =
                -largura / 2f + metadeLargura + margem;

            float xMax =
                largura / 2f - metadeLargura - margem;

            float yMin =
                -altura / 2f + metadeAltura + margem;

            float yMax =
                altura / 2f - metadeAltura - margem;

            // Evita erro caso a caixa seja pequena.
            if (xMin > xMax)
            {
                xMin = 0f;
                xMax = 0f;
            }

            if (yMin > yMax)
            {
                yMin = 0f;
                yMax = 0f;
            }

            rect.anchoredPosition = new Vector2(
                Random.Range(xMin, xMax),
                Random.Range(yMin, yMax)
            );

            float rotacao = Random.Range(
                -variacaoRotacao,
                variacaoRotacao
            );

            rect.localRotation =
                Quaternion.Euler(0f, 0f, rotacao);
        }

        item.ColocarImagem(imagemComida);

        // Cada coleta gera uma entrada, mesmo com ID repetido.
        EntradaComida entrada = new EntradaComida();
        entrada.idComida = idComida;
        entrada.item = item;

        itens.Add(entrada);

        Debug.Log(
            "CAIXA | Comida adicionada. Total de ícones: "
            + itens.Count
        );
    }

    public bool RemoverUmaComida()
    {
        if (itens.Count == 0)
        {
            Debug.Log(
                "CAIXA | Não há comidas para remover."
            );

            return false;
        }

        // Remove o último ícone adicionado.
        int ultimoIndice = itens.Count - 1;

        EntradaComida entrada = itens[ultimoIndice];

        itens.RemoveAt(ultimoIndice);

        if (entrada.item != null)
        {
            Destroy(entrada.item.gameObject);
        }

        Debug.Log(
            "CAIXA | Uma comida removida. Total de ícones: "
            + itens.Count
        );

        return true;
    }

    public bool PossuiComida(string idComida)
    {
        foreach (EntradaComida entrada in itens)
        {
            if (entrada.idComida == idComida)
            {
                return true;
            }
        }

        return false;
    }

    public int ObterQuantidadeComidas()
    {
        return itens.Count;
    }
}
