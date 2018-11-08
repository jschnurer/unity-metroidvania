using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyManager : MonoBehaviour {
    private class EnemyInRoom
    {
        public string RoomName;
        public GameObject Enemy;
    }


    List<EnemyInRoom> enemies = new List<EnemyInRoom>();
    public int GetEnemyCount() { return enemies.Count; }

    public void TrackEnemy(string roomName, GameObject enemy)
    {
        enemies.Add(new EnemyInRoom
        {
            RoomName = roomName,
            Enemy = enemy
        });
    }

    public void ClearEnemies(string roomName)
    {
        var roomEnemies = enemies.Where(x => x.RoomName == roomName).ToList();
        foreach (var enemy in roomEnemies)
        {
            Destroy(enemy.Enemy);
            enemies.Remove(enemy);
        }
    }
}
