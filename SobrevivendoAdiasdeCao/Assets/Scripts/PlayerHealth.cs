using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Energia")]
    public int energiaMaxima = 5;
    public int energiaAtual;

    [Header("UI")]
    public Slider barraEnergia;

    private PlayerMovement movement;

    void Start()
    {
        energiaAtual = energiaMaxima;

        movement = GetComponent<PlayerMovement>();

        AtualizarUI();
        AtualizarVelocidade();
    }

    public void LevarDano(int dano)
    {
        energiaAtual -= dano;

        // impede energia negativa
        if (energiaAtual < 1)
            energiaAtual = 1;

        AtualizarUI();
        AtualizarVelocidade();

        Debug.Log("Energia atual: " + energiaAtual);
    }

    void AtualizarUI()
    {
        if (barraEnergia != null)
        {
            barraEnergia.maxValue = energiaMaxima;
            barraEnergia.value = energiaAtual;
        }
    }

    void AtualizarVelocidade()
    {
        if (movement == null) return;

        switch (energiaAtual)
        {
            case 5:
                movement.speed = 5f;
                break;

            case 4:
                movement.speed = 4.5f;
                break;

            case 3:
                movement.speed = 4f;
                break;

            case 2:
                movement.speed = 3f;
                break;

            case 1:
                movement.speed = 2f;
                break;
        }
    }
}