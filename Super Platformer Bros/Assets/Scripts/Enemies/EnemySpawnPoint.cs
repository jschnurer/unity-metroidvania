using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour {
    public GameObject EnemyPrefab;
    public bool FacingRight = true;

    public void Start()
    {
        Destroy(GetComponent<SpriteRenderer>());
    }

    public GameObject SpawnEnemy()
    {
        var clone = Instantiate(EnemyPrefab, transform.position, transform.rotation);

        if (!FacingRight)
        {
            clone.transform.localScale = new Vector3(
                clone.transform.localScale.x * -1,
                clone.transform.localScale.y);
        }

        return clone;
    }
}
