using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Upgrade
{
    public string Name;
    public float Price;
    public Action<TrainController> Apply; // fonction à exécuter pour appliquer l'upgrade

    public Upgrade(string name, float price, Action<TrainController> apply)
    {
        Name = name;
        Price = price;
        Apply = apply;
    }
}