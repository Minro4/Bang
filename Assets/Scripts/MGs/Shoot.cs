using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MiniGame {

    Coroutine shoot;
    Coroutine compassReading;
    bool finishedShooting = false;

    public override void StartMiniGame()
    {
        finishedShooting = false;
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
        if (shoot != compassReading)
        {
            StopCoroutine(compassReading);
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
        StartCompassReading();
        while (!(Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight || Input.GetKey(KeyCode.Z)))
        {
            yield return null;
        }       
        while (!(Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown || Input.GetKey(KeyCode.X)))
        {
            yield return null;
        }
        finishedShooting = true;
        Handheld.Vibrate();
    }
    public void StartCompassReading()
    {
        compassReading = StartCoroutine(CompassReading());
    }
    private IEnumerator CompassReading()
    {
        float averagedCompass = -1;
        Vector3 lfAcceleration = Vector3.zero;
        const float maxAngleSpeed = 500;

        //int averagedTime = 0;
        //const int averagedTimeMin = 10;
        //float compassValue;
        //float time = 0;
        //const float timeMin = 0.25f;

        do
        {
            Vector3 acceleration = Input.acceleration;
            if (averagedCompass == -1)
            {               
                averagedCompass = Input.compass.magneticHeading;
            }
            else
            {
                if (acceleration != lfAcceleration)
                {
                    float angleSpeed = Vector3.Angle(acceleration, lfAcceleration) / Time.deltaTime;
                    Debug.Log(angleSpeed);
                    angleSpeed = Mathf.Clamp01(angleSpeed / maxAngleSpeed);
                    float weight = 1 - angleSpeed;

                    averagedCompass = AverageOut(averagedCompass, Input.compass.magneticHeading, weight);
                    // averagedCompass = (averagedCompass*2 + Input.compass.magneticHeading) / 3;
                }
            }

            lfAcceleration = acceleration;
            yield return null;
        } while (!finishedShooting);
            FindTheTarget(DuelManager.instance.player, averagedCompass);

        //while (!finishedShooting)           vien qui marche pas super bien quand tu bouge
        //{
        //    time += Time.deltaTime;
        //    averagedTime += 1;
        //    compassValue = Input.compass.magneticHeading;
        //    averagedCompass = ((averagedCompass * (averagedTime-1)) + compassValue) / (averagedTime);


        //    yield return null;
        //}
        //FindTheTarget(DuelManager.instance.player, averagedCompass);
    }
    public void FindTheTarget(Player shooter, float compass)            //g
    {
        string temp;
        int idTarget;
        int numberOfPlayers = DuelManager.instance.livingPlayers.Count;
        //if (numberOfPlayers <= 1)
        //{
        //    Debug.Log("WTF");
        //    //shooter.hasWon = true;
        //   // ServerDuel.instance.SomeoneHasWonGroup(shooter.name);
        //    return;
        //}
        if ((compass - shooter.compassValueCenter + 360) % 360 > 180)      //Changer le millieu parce que le milleu va pas toujours etre ca quand le moitier du monde est omrt pi que ya gros du monde qui joue
        {
            int index = mod((shooter.GetLPIndex() - 1),numberOfPlayers);
            idTarget = DuelManager.instance.livingPlayers[index].id;
            temp = "Left";
        }
        else
        {
            int index = (shooter.GetLPIndex() + 1) % (numberOfPlayers);
            idTarget = DuelManager.instance.livingPlayers[index].id;           
            temp = "Right";
        }
        DuelManager.instance.endText.text = "Compass: " + compass + " Center: " + DuelManager.instance.player.compassValueCenter + " Shot: " + temp;       
        if (DuelManager.instance.livingPlayers.Count > 2)
        {
            DuelManager.instance.MiniGameShootFinished();
        }
        DuelManager.instance.player.CmdGroupShoot(idTarget, shooter.id);
    }

    float AverageOut(float average,float newValue,float weight)
    {
        return ((average + newValue*weight) / (weight + 1));
    }
    public override void SetPlayerOb(int index)
    {
    }

    public override void ResetGame()
    {
    }

    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}
