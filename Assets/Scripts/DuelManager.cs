using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class DuelManager : NetworkBehaviour {
    public static DuelManager instance;

    public Canvas canvasDuel;
    public Text endText;
    public GameObject restartButton;
    public GameObject map;

    public Color playerColor;
    public Color otherPlayerColor;

    public float raceEndPos;
    Coroutine udUpdate;
    Coroutine compassUpdate;

    bool doingServerSetup;

    [HideInInspector] public Player[] players;
    [HideInInspector] public Player player;

   // bool posSuc;
    //bool epIsRunning;

    void Start () {
        instance = this;
        Invoke("Setup",1f);
    }
    public void Setup()
    {
        instance.players = GetPlayers();
        ResetVar();

        if (instance.players.Length >= 1)                    //Setup for Group
        {         
            StartCoroutine(SetupGroup());
        }
        else
        {
            SetupGameStart();
        }

        
    }
    public void SetupGameStart()
    {
        udUpdate = StartCoroutine(SendUDUpdate());
        if (isServer)                               //gameStart
        {
            ServerDuel.instance.FStart();
        }
    }
    Player[] GetPlayers()
    {
        GameObject[] go = GameObject.FindGameObjectsWithTag("Player");
        if (go.Length == 0)
        {
            Debug.Log("Pas de joueurs");
            return null;
        }
        List<Player> pl = new List<Player>();

        foreach (GameObject p in go)
        {
            p.GetComponent<Player>().InitializePlayer();
            pl.Add(p.GetComponent<Player>());
        }
        //  ModeSetup();
        return pl.ToArray();
    }
    IEnumerator SetupGroup()
    {
       // StartCoroutine(EnablePosition());
       // while (epIsRunning)
        //{
        //    yield return null;
        //}
        //if (!posSuc)
        //{
        //    Debug.Log("How shit waddup tu peux pas continuer man");
        //    yield break;
        //}

        Input.compass.enabled = true;
        doingServerSetup = true;
        instance.player.CmdStartCompassSetup();

        endText.text = "Point the top of your cell phone towards the center of the group";

        float averagedCompass = -100;
        float compassError = 30;
        int averagedTime = 0;
        const int averagedTimeMin = 10;

        Vector3 tilt;
        Vector2 tiltAngle;
        Vector3 rawVec;
        Vector3 newVec;
        while (doingServerSetup)
        {
            rawVec = Vector3.up;//Input.compass.rawVector;// Vector3.right;//
            tilt = Input.acceleration;
            tiltAngle = TiltToAngle(tilt);
            newVec = Quaternion.Euler(tiltAngle.x, tiltAngle.y, 0) * rawVec;

            float rawAngle = Vector2.Angle(Vector2.up, rawVec);
            float Angle = Vector2.Angle(Vector2.up, newVec);

            endText.fontSize = 60;
            endText.text = " a: " + tilt + "\nT: " + tiltAngle + "\n oV: " + rawAngle + "\n nV: " + Angle;

            if (Input.deviceOrientation == DeviceOrientation.FaceUp)
            {
                float compassValue = Input.compass.trueHeading;
                if (CompareIntervale(compassValue,averagedCompass,compassError))
                {
                    averagedTime += 1;
                    averagedCompass = ((averagedCompass * averagedTime) + compassValue) / (averagedTime + 1);

                    if (averagedTime >= averagedTimeMin)
                    {
                        averagedTime = 0;                      
                        //endText.text = "C: " + averagedCompass;
                        instance.player.CmdSetCompassValue(averagedCompass);
                    }

                }
                else
                {
                    if (instance.player.compassValue != -1)
                    {
                        instance.player.CmdSetCompassValue(-1);
                    }
                    averagedCompass = compassValue;
                    averagedTime = 0;
                }
                
            }
            else
            {
                 //endText.text += "Point the top of your cell phone towards the center of the group";
                if (instance.player.compassValue != -1)
                {
                    instance.player.CmdSetCompassValue(-1);
                }
                averagedTime = 0;
                averagedCompass = -100;
            }
            yield return new WaitForSeconds(0.02f) ;
        }
        endText.text = "Done!";
        yield return new WaitForSeconds(1);
        endText.text = "";
        SetupGameStart();

    }
    //IEnumerator EnablePosition()
    //{
    //    epIsRunning = true;
    //    // First, check if user has location service enabled
    //    if (Input.location.isEnabledByUser)
    //    {
    //        posSuc = true;
    //        epIsRunning = false;
    //        yield break;
    //    }
    //    // Start service before querying location
    //    Input.location.Start();

    //    // Wait until service initializes
    //    int maxWait = 30;
    //    while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
    //    {
    //        endText.text = "StartingLoc" + maxWait;
    //        yield return new WaitForSeconds(0.5f);
    //        maxWait--;
    //    }

    //    // Service didn't initialize in 10 seconds
    //    if (maxWait < 1)
    //    {
    //        endText.text = "StartingLoc Failed";
    //        posSuc = false;
    //        epIsRunning = false;
    //        yield break;
    //    }

    //    // Connection has failed
    //    if (Input.location.status == LocationServiceStatus.Failed)
    //    {
    //        Debug.Log("Unable to determine device location");
    //        posSuc = false;
    //        epIsRunning = false;
    //        yield break;
    //    }
    //    posSuc = true;
    //    epIsRunning = false;
    //}
    public void WantsToRestartGame()
    {
        instance.player.CmdRestart(true);
    }
    public void RestartGame()
    {
        ResetVar();
    }
    void ResetVar()
    {
        map.SetActive(false);
        foreach (Player p in players)
        {
            p.ResetPlayer();
        }
        StopUpdates();
       // udUpdate = StartCoroutine(SendUDUpdate());
        //compassUpdate = StartCoroutine(SendCompassUpdate());
        instance.restartButton.SetActive(false);                                                          
    }
    IEnumerator SendUDUpdate()
    {
        while(true)
        {
            instance.player.CheckIsUD();
            yield return new WaitForSeconds (0.1f);
        }
    }
    //IEnumerator SendCompassUpdate()
    //{
    //    while (true)
    //    {
    //        if (Input.compass.headingAccuracy < 0)
    //        {
    //            t pas précis man
    //         }
    //        else player.CmdSetCompassValue(Input.compass.rawVector);
    //        yield return new WaitForSeconds(0.5f);
    //    }
    //}
    public void StopUpdates()
    {
        if (udUpdate != null)
        {
            StopCoroutine(udUpdate);
        }
        if (compassUpdate != null)
        {
            StopCoroutine(compassUpdate);
        }
    }
    
	//void ModeSetup()
 //   {
 //       switch (GameManager.instance.mode)
 //       {
 //           case GameManager.Mode.Duel: break;
 //           case GameManager.Mode.Group: GroupSetup(); break;
 //       }
 //   }
 //   void GroupSetup()
 //   {
        
 //   }
    
   
   
    public void ButtonPressed(GameObject button)
    {
        instance.player.ButtonPressed(button);
    }
    public void Win()
    {
        Debug.Log("You Win!");
    }
    bool CompareIntervale(float val1, float val2, float incertitude)
    {
        float valB = (val2 - incertitude) % 360;
        float valH = (val2 + incertitude) % 360;

        if (valB > valH)
        {
            if (val1 <= valH || val1 >= valB)
            {
                return true;
            }
        }
        else
        {
            if (val1 <= valH && val1 >= valB)
            {
                return true;
            }
        }
        return false;
       
    }
    Vector2 TiltToAngle(Vector3 tilt)
    {
        float xVal;
        float yVal;

        if (tilt.z < 0)
        {
            xVal = tilt.x * 90;
            yVal = tilt.y * 90;           
        }
        else
        {           
            int signX = (tilt.x > 0) ? 1 : -1;
            xVal = (signX * 180) - (tilt.x * 90);
            yVal = tilt.y * 90;
            //if (tilt.x < 0)
            //{
            //    yVal = tilt.y * 90;
            //}
            //else
            //{
            //    int signY = (tilt.y > 0) ? 1 : -1;
            //    yVal = (signY * 180) - (tilt.y * 90);
            //}          
           
        }
        return new Vector2(yVal, xVal);
    }

    [ClientRpc]
    public void RpcStartSetup()
    {
        Setup();
    }
    [ClientRpc]
    public void RpcStopUpdate()
    {
        StopUpdates();
    }
    [ClientRpc]
    public void RpcStartMG()
    {
        StartCoroutine(player.StartMG());
    }
    [ClientRpc]
    public void RpcDisplayEnd()
    {
        player.DisplayEndResult(instance.player.hasWon);       
    }
    [ClientRpc]
    public void RpcRestart()
    {
        RestartGame();
    }
    [ClientRpc]
    public void RpcServerSetup(bool w)
    {
        doingServerSetup = w;
    }


}
