using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MiniGame {

    Coroutine shoot;

    public override void StartMiniGame()
    {
        if (DuelManager.instance.groupPlay)
        {
            shoot = StartCoroutine(ShootGroup());
        }
        else
        {
            shoot = StartCoroutine(ShootDuel());
        }
    }
    public override void ClearMiniGame()
    {
        if (shoot != null)
        {
            StopCoroutine(shoot);
            shoot = null;
        }
    }
    public override void LoadObjects()
    {

    }
    public override void UnloadObjects()
    {

    }

    IEnumerator ShootDuel()
    {
        DuelManager.instance.endText.text = "Shoot!";
        while (!(Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight || Input.GetKey(KeyCode.Z)))
        {
            yield return null;
        }
        while (!(Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown || Input.GetKey(KeyCode.X)))
        {
            yield return null;
        }
        Handheld.Vibrate();

        DuelManager.instance.player.ShootSomeone();
    }

    IEnumerator ShootGroup()
    {
        DuelManager.instance.endText.text = "Shoot!";
        while (!(Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight || Input.GetKey(KeyCode.Z)))
        {
            yield return null;
        }
        DuelManager.instance.StartCompassReading();
        while (!(Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown || Input.GetKey(KeyCode.X)))
        {
            yield return null;
        }
        DuelManager.instance.player.finishedShooting = true;
        Handheld.Vibrate();
    }
    public override void SetPlayerOb(int index)
    {
    }

    public override void ResetGame()
    {
    }
}
