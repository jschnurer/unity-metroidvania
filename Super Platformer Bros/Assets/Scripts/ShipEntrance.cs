using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipEntrance : MonoBehaviour
{
    Action QuitGameAction;
    Inventory inv;
    DialogManager dman;

    // Use this for initialization
    void Start()
    {
        inv = GameObject.Find("Player").GetComponent<Inventory>();
        dman = GameObject.Find("DialogManager").GetComponent<DialogManager>();
        QuitGameAction = () =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
            Application.OpenURL("http://google.com");
#else
            Application.Quit();
#endif
        };
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        System.TimeSpan playTime = System.TimeSpan.FromSeconds(Time.realtimeSinceStartup);
        string playTimeStr = System.String.Format("{0:D2}h:{1:D2}m:{2:D2}s",
            playTime.Hours,
            playTime.Minutes,
            playTime.Seconds);

        float perc = inv.GetPickupPercent();
        dman.ShowChoice("You have " + inv.GetComponentCount() + " of 5 components."
                + "\r\n"
                + "You have acquired " + perc.ToString("P0") + " of all items."
                + "\r\n"
                + "Playtime: " + playTimeStr,
            "Continue Playing",
            null,
            "Quit game",
            QuitGameAction);
    }
}
