// TemporaryEffect.cs
using UnityEngine;

public class TemporaryEffect : MonoBehaviour
{
    [SerializeField] private float duration = 0.2f;

    void Start()
    {
        // Destrói este objeto após o tempo definido
        Destroy(gameObject, duration);
    }
}
