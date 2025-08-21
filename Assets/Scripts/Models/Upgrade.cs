using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Upgrade
{
    public string name;
    public float price;
    public Action<TrainController> apply; // fonction à exécuter pour appliquer l'upgrade

    public Upgrade(string name, float price, Action<TrainController> apply)
    {
        this.name = name;
        this.price = price;
        this.apply = apply;
    }
}