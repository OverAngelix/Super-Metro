using System;

[Serializable]
public class Upgrade
{
    public string name;
    public float price;
    public Action<TrainController> apply;

    public Upgrade(string name, float price, Action<TrainController> apply)
    {
        this.name = name;
        this.price = price;
        this.apply = apply;
    }
}