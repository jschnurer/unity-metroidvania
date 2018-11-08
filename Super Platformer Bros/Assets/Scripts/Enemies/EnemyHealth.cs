using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public GameObject deathSoundObject;
    public GameObject deathParticleSystem;
    public int Health;
    public Color HurtColor;
    SpriteRenderer spriteRenderer;
    Coroutine flashDamageCoroutine;
    /// <summary>
    /// Chance to drop anything.
    /// </summary>
    public float dropChance;
    /// <summary>
    /// % chance that the drop will be big.
    /// </summary>
    public float bigDropChance;
    /// <summary>
    /// % chance the drop will be health instead of energy.
    /// </summary>
    public float healthChance;
    public GameObject HealthSmall;
    public GameObject HealthBig;
    public GameObject EnergySmall;
    public GameObject EnergyBig;

    // Use this for initialization
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Shot")
        {
            Health -= collision.gameObject.GetComponent<Shot>().Damage;

            Destroy(collision.gameObject);

            if (Health <= 0)
            {
                Die();
                return;
            }

            if (flashDamageCoroutine != null)
            {
                StopCoroutine(flashDamageCoroutine);
            }

            flashDamageCoroutine = StartCoroutine(FlashDamage());
        }
    }

    void Die()
    {
        Instantiate(deathSoundObject, transform.position, transform.rotation);
        Instantiate(deathParticleSystem, transform.position, transform.rotation);
        SpawnDrop();
        Destroy(gameObject);
    }

    void SpawnDrop()
    {
        if (Random.Range(0, 100) <= dropChance + (GameObject.Find("Player").GetComponent<Inventory>().IsHurt ? 20 : 0))
        {
            bool isBig = Random.Range(0, 100) <= bigDropChance;
            GameObject prefab = null;
            if (Random.Range(0, 100) <= healthChance)
                prefab = isBig ? HealthBig : HealthSmall;
            else
                prefab = isBig ? EnergyBig : EnergySmall;

            if (prefab != null)
                Instantiate(prefab, transform.position, Quaternion.identity);
        }
    }

    IEnumerator FlashDamage()
    {
        spriteRenderer.color = HurtColor;
        yield return new WaitForSeconds(.15f);
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }
}
