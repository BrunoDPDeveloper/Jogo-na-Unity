using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
    /// <summary>
    /// Estende a classe List para permitir o embaralhamento usando o algoritmo Fisher-Yates.
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        // Usa System.Random para melhor aleatoriedade fora do loop de jogo principal
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            // Escolhe um índice aleatório k entre 0 e n
            int k = rng.Next(n + 1);

            // Troca o elemento atual (list[n]) com o elemento aleatório (list[k])
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}