using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSmoke : MonoBehaviour {
    public int ComponentsRequiredToFix;
    private Inventory inv;

	void Start () {
        inv = GameObject.Find("Player").GetComponent<Inventory>();
	}
	
	void Update () {
        if (inv.GetComponentCount() >= ComponentsRequiredToFix)
            Destroy(gameObject);
	}
}
