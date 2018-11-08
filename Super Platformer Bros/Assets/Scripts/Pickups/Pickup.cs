using UnityEngine;

[System.Serializable]
public class Pickup
{
    public PickupType Type;
    public EquipmentEnum Equipment;
    [TextArea(3, 10)]
    public string PickupText;
}

public enum PickupType
{
    Equipment,
    Health,
    Energy,
    Component
}