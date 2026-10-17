
using UnityEngine;

public class ConfetesVitoria : MonoBehaviour
{
    private ParticleSystem particulas;

    void Awake()
    {
        particulas = GetComponent<ParticleSystem>();

        // Impede os confetes de aparecerem no início
        particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void MostrarConfetes()
    {
        particulas.Play();
    }

    public void PararConfetes()
    {
        particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
