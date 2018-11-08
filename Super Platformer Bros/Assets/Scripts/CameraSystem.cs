using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    private GameObject player;
    public bool doClamping = false;
    private float vertExtent = 0;
    private float horzExtent = 0;
    private List<ClampBounds> clampBounds = new List<ClampBounds>();
    public string CurrentRoom;
    new private Camera camera;

    public bool IsInCamera(Vector3 point)
    {
        var vpPoint = camera.WorldToViewportPoint(point);
        return vpPoint.x >= 0
            && vpPoint.x <= 1
            && vpPoint.y >= 0
            && vpPoint.y <= 1;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        camera = GetComponent<Camera>();
        vertExtent = camera.orthographicSize;
        horzExtent = vertExtent * Screen.width / Screen.height;
    }

    void LateUpdate()
    {
        float x = 0;
        float y = 0;

        if (doClamping)
        {
            var bounds = clampBounds[clampBounds.Count - 1];
            x = Mathf.Clamp(player.transform.position.x, bounds.XMin, bounds.XMax);
            y = Mathf.Clamp(player.transform.position.y, bounds.YMin, bounds.YMax);
        }
        else
        {
            x = player.transform.position.x;
            y = player.transform.position.y;
        }
        gameObject.transform.position = new Vector3(x, y, gameObject.transform.position.z);
    }

    public void AddBounds(string name, CameraBounds camBounds, float minx, float maxx, float miny, float maxy)
    {
        clampBounds.Add(new ClampBounds
        {
            Name = name,
            XMin = minx + horzExtent,
            YMin = miny + vertExtent,
            XMax = maxx - horzExtent,
            YMax = maxy - vertExtent,
            CameraBounds = camBounds
        });
        CurrentRoom = name;
    }

    public void RemoveBounds(string name)
    {
        clampBounds.Remove(clampBounds.Find(x => x.Name == name));
        if (clampBounds.Count > 0)
        {
            var bounds = clampBounds[clampBounds.Count - 1];
            if (GameObject.Find("EnemyManager").GetComponent<EnemyManager>().GetEnemyCount() == 0)
                bounds.CameraBounds.SpawnEnemies();
        }
    }
}