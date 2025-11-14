using UnityEngine;

// Define a classe para segurar os dados de cada item de compra
// O atributo [System.Serializable] permite que a Unity exiba essa classe no Inspector
[System.Serializable]
public class WeaponBuyData
{
    [Header("Item")]
    public string weaponName; // Nome da arma para exibição na UI
    public Sprite weaponIcon; // Ícone da arma
    public GameObject weaponPrefab; // O Prefab da arma a ser instanciada (o GameObject com o script AssaultRifle/Pistol/etc.)

    [Header("Custos")]
    public int weaponCost; // Custo para comprar a arma
    public int ammoCost; // Custo para recarga da munição
    public int ammoRefillAmount; // Quantidade de munição de reserva a ser adicionada
}