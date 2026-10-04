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

    private Dictionary<string, ItemComidaUI> itens =
        new Dictionary<string, ItemComidaUI>();

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

    public void AdicionarComida(
        string idComida,
        Sprite imagemComida)
    {
        // Impede comida do mesmo tipo de aparecer duas vezes
        if (itens.ContainsKey(idComida))
        {
            return;
        }

        if (itemComidaPrefab == null)
        {
            Debug.LogError(
                "CaixaComidasUI: Item Comida Prefab não foi configurado!"
            );
            return;
        }

        if (conteudo == null)
        {
            Debug.LogError(
                "CaixaComidasUI: Conteudo não foi configurado!"
            );
            return;
        }

        // Cria o novo item dentro da caixa
        GameObject novoObjeto = Instantiate(
            itemComidaPrefab,
            conteudo
        );

        ItemComidaUI item =
            novoObjeto.GetComponent<ItemComidaUI>();

        if (item == null)
        {
            Debug.LogError(
                "CaixaComidasUI: ItemComidaUI não foi encontrado no prefab!"
            );

            Destroy(novoObjeto);
            return;
        }

        // Define uma posição aleatória dentro da caixa
        RectTransform rect =
            novoObjeto.GetComponent<RectTransform>();

        if (rect != null)
        {
            float larguraCaixa = conteudo.rect.width;
            float alturaCaixa = conteudo.rect.height;

            float metadeLargura = rect.rect.width / 2f;
            float metadeAltura = rect.rect.height / 2f;

            float xMin =
                -larguraCaixa / 2f +
                metadeLargura +
                margem;

            float xMax =
                larguraCaixa / 2f -
                metadeLargura -
                margem;

            float yMin =
                -alturaCaixa / 2f +
                metadeAltura +
                margem;

            float yMax =
                alturaCaixa / 2f -
                metadeAltura -
                margem;

            float x = Random.Range(xMin, xMax);
            float y = Random.Range(yMin, yMax);

            rect.anchoredPosition =
                new Vector2(x, y);

            // Pequena rotação para parecer comida empilhada
            float rotacao =
                Random.Range(
                    -variacaoRotacao,
                    variacaoRotacao
                );

            rect.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    rotacao
                );
        }

        // Coloca a imagem e inicia a animação
        item.ColocarImagem(imagemComida);

        // Registra o tipo de comida
        itens.Add(idComida, item);

        Debug.Log(
            "CAIXA | Nova comida adicionada: " +
            idComida
        );
    }

    public bool PossuiComida(string idComida)
    {
        return itens.ContainsKey(idComida);
    }
}