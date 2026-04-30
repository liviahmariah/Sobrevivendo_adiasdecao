using UnityEngine;

public class PopupSubir : MonoBehaviour
{
    float tempo = 1f;

    void Update()
    {
        transform.position += Vector3.up * 2f * Time.deltaTime;

        tempo -= Time.deltaTime;

        if (tempo <= 0)
        {
            Destroy(gameObject);
        }
    }
}