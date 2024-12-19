using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JanKenShopJSON
{
    public List<Discount> discount;

    public static JanKenShopJSON Instance;

    /// <summary>
    /// Establecimiento de la instancia principal
    /// </summary>
    /// <param name="instance"></param>
    public static void SetInstance(JanKenShopJSON instance)
    {
        Instance = instance;
    }

    /// <summary>
    /// Conversion de un string en formato JSON a la clase JanKenShopJSON.
    /// </summary>
    /// <param name="jsonString"></param>
    /// <returns></returns>
    public static JanKenShopJSON CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<JanKenShopJSON>(jsonString);
    }

    /// <summary>
    /// Obtencion del descuento actual para cierto articulo de la tienda
    /// </summary>
    /// <param name="itemID"></param>
    /// <returns></returns>
    public static int GetDiscount(string itemID)
    {
        if (Instance == null) return 0;

        // Revision en los descuentos
        Discount itemDiscount = Instance.discount.Find(discount => discount.item == itemID);

        if (itemDiscount != null) return itemDiscount.amount;

        return 0;
    }

}

[System.Serializable]
public class Discount
{
    public string item;
    public int amount;
}