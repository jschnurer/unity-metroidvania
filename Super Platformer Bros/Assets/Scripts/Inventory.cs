using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public bool StartWithAllEquip = false;
    public ParticleSystem DeathParticleSystem;
    public GameObject RespawnPoint;
    public Vector2 Checkpoint;
    private Player_Move_Prot PlayerScript;
    public GameObject HealthBar;
    private float MaxHealthBarWidth = 0;
    public GameObject EnergyBar;
    private float MaxEnergyBarWidth = 0;

    public int Health = 50;
    public int Energy = 50;
    private float EnergyRecharge = 0f;
    public float EnergyPerSecond;
    public bool IsHurt { get { return Health < MaxHealth; } }

    private int MaxHealth = 50;
    private int MaxEnergy = 50;

    private int HealthPickups = 0;
    private int EnergyPickups = 0;
    public List<EquipmentEnum> Equipment = new List<EquipmentEnum>();
    private int Components = 0;
    public int GetComponentCount() { return Components; }

    private int TotalPickupsInWorld;
    public float GetPickupPercent()
    {
        return (float)(HealthPickups + EnergyPickups) / (float)TotalPickupsInWorld;
    }

    public void GainPickup(Pickup pickup)
    {
        if (pickup.Type == PickupType.Health)
        {
            MaxHealth += 10;
            Health += 10;
            HealthPickups++;
            MaxHealthBarWidth += 20;
        }
        else if (pickup.Type == PickupType.Energy)
        {
            MaxEnergy += 10;
            Energy += 10;
            EnergyPickups++;
            MaxEnergyBarWidth += 20;
            UpdateEnergyBar();
        }
        else if (pickup.Type == PickupType.Equipment)
        {
            Equipment.Add(pickup.Equipment);
        }
        else if (pickup.Type == PickupType.Component)
        {
            Components++;
        }
    }

    public int NumOfEquipment(EquipmentEnum equipment)
    {
        return Equipment.FindAll(x => x == equipment).Count;
    }

    private void Start()
    {
        TotalPickupsInWorld = GameObject.FindGameObjectsWithTag("Pickup").Length;
        PlayerScript = GetComponent<Player_Move_Prot>();
        MaxHealthBarWidth = HealthBar.GetComponent<RectTransform>().sizeDelta.x;
        MaxEnergyBarWidth = EnergyBar.GetComponent<RectTransform>().sizeDelta.x;

        if (StartWithAllEquip)
        {
            Equipment.Add(EquipmentEnum.BlinkDrive);
            Equipment.Add(EquipmentEnum.CloudTread);
            Equipment.Add(EquipmentEnum.PhaseWalk);
        }
    }

    private void UpdateEnergyBar()
    {
        EnergyBar.GetComponent<RectTransform>().sizeDelta = new Vector2(MaxEnergyBarWidth * ((float)Energy / (float)MaxEnergy), 7);
    }

    private void UpdateHealthBar()
    {
        HealthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(MaxHealthBarWidth
            * ((float)Health / (float)MaxHealth), 7);
    }

    public void GainEnergy(int amount)
    {
        if (Energy < MaxEnergy)
        {
            Energy += amount;
            UpdateEnergyBar();
        }
    }

    /// <summary>
    /// Removes energy from the player.
    /// </summary>
    /// <param name="amount">Amount to lose</param>
    /// <returns>Returns true if the user had at least that much energy</returns>
    public bool LoseEnergy(int amount)
    {
        if (Energy < amount)
            return false;

        Energy -= amount;
        UpdateEnergyBar();
        return true;
    }

    public void GainHealth(int amount)
    {
        if (IsHurt)
        {
            Health += amount;
            if (Health > MaxHealth)
                Health = MaxHealth;
            UpdateHealthBar();
        }
    }

    /// <summary>
    /// Subtracts from health.
    /// </summary>
    /// <param name="amount">Amount of health to lose</param>
    /// <returns>Remaining health</returns>
    public int LoseHealth(int amount)
    {
        Health -= amount;
        UpdateHealthBar();

        if (Health <= 0)
        {
            StartCoroutine(Die());
        }

        return Health;
    }

    private void FixedUpdate()
    {
        if (!PlayerScript.IsPhaseWalking() && !PlayerScript.IsChargingBlink())
        {
            EnergyRecharge += ((MaxEnergy * (.025f + .002f * EnergyPickups)) * Time.deltaTime);

            if (EnergyRecharge > 1.0f)
            {
                EnergyRecharge -= 1.0f;
                GainEnergy(1);
            }
        }
    }

    IEnumerator Die()
    {
        Instantiate(DeathParticleSystem, transform.position, transform.rotation);
        PlayerScript.enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;

        yield return new WaitForSeconds(2);

        Health = MaxHealth;
        UpdateHealthBar();
        transform.position = Checkpoint;
        PlayerScript.enabled = true;
        GetComponent<SpriteRenderer>().enabled = true;
        DialogManager.Find().ShowMessage("You have died. You will be returned to your most recent checkpoint"
            + " all of your acquired items. Be more careful this time.");
    }
}
