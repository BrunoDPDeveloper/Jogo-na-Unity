using UnityEngine;
using System;

[Serializable]
public struct WeaponTier
{
    [Tooltip("Nome do Tier (ex: Comum, Incomum)")]
    public string tierName;

    [Tooltip("Cor do nome do Tier (ex: Branco, Verde)")]
    public Color tierColor;

    [Tooltip("Nível de Tier que esta configuração representa (0, 1, 2...)")]
    public int tierIndex;

    [Tooltip("A Chave do PlayerPrefs para o componente (ex: Componente_T1)")]
    public string componentKeyNeeded;

    [Tooltip("A Quantidade de Componentes necessária para este Upgrade")]
    public int componentQuantityNeeded;

    // ⭐ CAMPOS DE MULTIPLICADORES (NOVOS)
    [Header("Weapon Stats Multipliers")]
    [Tooltip("Multiplicador aplicado ao Dano base da arma. (1.0 = 0% de aumento)")]
    public float damageMultiplier;

    [Tooltip("Multiplicador aplicado à Taxa de Tiro base da arma. (1.0 = 0% de aumento)")]
    public float fireRateMultiplier;

    [Tooltip("Valor de bônus direto adicionado ao dano de Headshot (após o multiplicador de dano)")]
    public float bonusHeadshotDamage;
}