using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    EnemyManager enemyManager;

    private void Start()
    {
        enemyManager = GameObject.Find("EnemyManager")
                    .GetComponent<EnemyManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var bounds = GetComponent<BoxCollider2D>().bounds;
        GameObject.Find("Main Camera")
            .GetComponent<CameraSystem>()
            .AddBounds(gameObject.name,
                this,
                bounds.min.x,
                bounds.max.x,
                bounds.min.y,
                bounds.max.y);

        enemyManager.ClearEnemies(gameObject.name);
        SpawnEnemies();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject.Find("Main Camera")
            .GetComponent<CameraSystem>()
            .RemoveBounds(gameObject.name);

        // Save checkpoint
        collision.GetComponentInParent<Inventory>()
            .Checkpoint = collision.transform.position;

        enemyManager.ClearEnemies(gameObject.name);
    }

    public void SpawnEnemies()
    {
        foreach (Transform spawner in transform)
        {
            enemyManager.TrackEnemy(gameObject.name,
                spawner
                    .gameObject
                    .GetComponent<EnemySpawnPoint>()
                    .SpawnEnemy());
        }
    }
}
