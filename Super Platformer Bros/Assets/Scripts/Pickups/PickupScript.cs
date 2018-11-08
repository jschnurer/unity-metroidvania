using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupScript : MonoBehaviour
{
    public Pickup Item;
    public GameObject PickupSound;
    bool pickedUp;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !pickedUp)
        {
            pickedUp = true;
            
            var inv = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
            inv.GainPickup(Item);
            string message = Item.PickupText;

            if (Item.Type == PickupType.Component)
            {
                int numComps = inv.GetComponentCount();
                if (numComps < 5)
                {
                    message = "Component acquired. Only " + (5 - numComps) + " remain.";
                }
                else
                {
                    message += "Final component acquired! Return to the ship!";
                }
            }

            Instantiate(PickupSound, transform.position, transform.rotation);
            DialogManager.Find().ShowMessage(message);
            Destroy(gameObject);
        }
    }
}
