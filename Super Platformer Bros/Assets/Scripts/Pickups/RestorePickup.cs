using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestorePickup : MonoBehaviour {
    public int Health;
    public int Energy;
    public float LifetimeSeconds;
    public GameObject SoundPrefab;

    public void Start()
    {
        StartCoroutine(Expire());
    }

    IEnumerator Expire()
    {
        yield return new WaitForSeconds(LifetimeSeconds);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var inv = collision.gameObject.GetComponent<Inventory>();
        if (Health > 0)
            inv.GainHealth(Health);
        else if(Energy > 0)
            inv.GainEnergy(Energy);

        Instantiate(SoundPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
