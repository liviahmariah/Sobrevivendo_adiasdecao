using UnityEngine;

public class DetectorPuloSandy : MonoBehaviour
{
    [Header("Referências")]
    public Animator animatorSandy;

    [Header("Configuração")]
    public string parametroChao = "Grounded";

    public bool EstaNoAr()
    {
        if (animatorSandy == null)
            return false;

        return !animatorSandy.GetBool(parametroChao);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(
                EstaNoAr()
                    ? "SANDY ESTÁ NO AR!"
                    : "SANDY ESTÁ NO CHÃO!"
            );
        }
    }
}