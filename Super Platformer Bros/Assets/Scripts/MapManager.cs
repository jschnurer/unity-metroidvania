using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    GameObject[,] map = new GameObject[20, 6];

    bool pausing = false;
    bool paused = false;
    public GameObject mapBGObj;
    public GameObject mapObj;
    public GameObject compass;
    public GameObject prefab_MapRoomHider;

    private RectTransform compassRect;

    public void Start()
    {
        compassRect = compass.GetComponent<RectTransform>();

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y])
                    continue;

                var hider = Instantiate(prefab_MapRoomHider, mapObj.transform, false);
                var rect = hider.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(x * rect.rect.width, (y + 1) * -rect.rect.height);
                map[x, y] = hider;
            }
        }
    }

    public void Update()
    {
        if (pausing)
        {
            pausing = false;
            paused = true;
            return;
        }

        if (paused && Input.GetButtonUp("Start"))
        {
            HideMap();
        }
    }

    public void ShowMap()
    {
        pausing = true;
        Time.timeScale = 0;
        GameObject.Find("Player").GetComponent<Player_Move_Prot>().InControl = false;

        mapObj.SetActive(true);
        mapBGObj.SetActive(true);
    }

    public void HideMap()
    {
        StartCoroutine(ReenableControls());
    }

    private IEnumerator ReenableControls()
    {
        yield return new WaitForEndOfFrame();
        paused = false;
        mapObj.SetActive(false);
        mapBGObj.SetActive(false);
        Time.timeScale = 1;
        GameObject.Find("Player").GetComponent<Player_Move_Prot>().InControl = true;
    }

    public void RecordPlayerPosition(Vector2 position)
    {
        int mapX = (int)position.x / 16;
        int mapY = (int)position.y / -17;

        if (mapX < 0 || mapX > map.GetLength(0))
            return;

        if (mapY < 0 || mapY > map.GetLength(1))
            return;

        // Remove the square hiding the map
        Destroy(map[mapX, mapY]);

        compassRect.anchoredPosition = new Vector2(mapX * compassRect.rect.width,
            (mapY + 1) * -compassRect.rect.height);
    }
}
