using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int itensColetados = 0;
    public int meta = 10;

    public TextMeshProUGUI textoUI;

    void Awake()
    {
        instance = this;
    }

    public void Coletar(int valor)
    {
        itensColetados += valor;

        textoUI.text = "Itens: " + itensColetados + "/" + meta;

        if (itensColetados >= meta)
        {
            Debug.Log("MISSÃO COMPLETA!");
        }
    }
}