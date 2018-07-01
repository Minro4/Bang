using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class DuelManager : NetworkBehaviour {
    public static DuelManager instance;
    public const int minimumGroupPlayer = 3;
    public Canvas canvasDuel;
    public Text endText;
    public GameObject restartButton;


    //  public Color playerColor;
    // public Color otherPlayerColor;

    Coroutine udUpdate;
    Coroutine compassUpdate;

    bool doingServerSetup;
    public bool groupPlay = false;

    [HideInInspector] public Player[] players;
    [HideInInspector] public Player player;

    [HideInInspector] public List<MiniGame> miniGames = new List<MiniGame>();
  //  Shoot shootMG;
    [HideInInspector] public int[] miniGameIndex;

    const float timeWaitForServer = 0.1f;

    bool setupDone = false;


    void Start () {
        instance = this;
        instance.setupDone = false;
        Invoke("Setup",1f);
    }
    public void Setup()
    {
        instance.players = GetPlayers();
        GetMiniGameList();
        ResetVar();

        if (instance.players.Length >= minimumGroupPlayer)                    //Setup for Group
        {
            instance.groupPlay = true;
            StartCoroutine(SetupGroup());
        }
        else
        {
            instance.groupPlay = false;
            SetupGameStart();
        }
        instance.setupDone = true;
        
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
        for (int i = 0; i < DuelManager.instance.players.Length; i++)
        {
            instance.players[i].index = i;
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
        instance.doingServerSetup = true;
        instance.player.CmdStartCompassSetup();

        endText.text = "Point the top of your cell phone towards the center of the group";

        float averagedCompass = -100;
        float compassError = 60;
        int averagedTime = 0;
        const int averagedTimeMin = 10;

        while (instance.doingServerSetup)
        {
            // float angle = IGuessCestUneFaconDeLeFaire();
            //endText.fontSize = 60;
            //Debug.Log("A: " + angle + "     R: " + Input.compass.magneticHeading);
            //endText.text = " a: " + tiltAngle + "\nT: " + Vector3.ProjectOnPlane(Vector3.up, tilt) + "\n oV: " + rawVec + "\n nV: " + angle;

            if (Input.deviceOrientation == DeviceOrientation.FaceUp)
            {
                float compassValue = Input.compass.magneticHeading;
                if (CompareIntervale(compassValue,averagedCompass,compassError))
                {
                    averagedTime += 1;
                    averagedCompass = ((averagedCompass * averagedTime) + compassValue) / (averagedTime + 1);
                    if (averagedTime >= averagedTimeMin)
                    {
                        averagedTime = 0;                      
                        endText.text = "C: " + averagedCompass;
                        instance.player.CmdSetCompassValue(averagedCompass);
                    }

                }
                else
                {
                    if (instance.player.compassValueCenter != -1)
                    {
                        instance.player.CmdSetCompassValue(-1);
                    }
                    averagedCompass = compassValue;
                    averagedTime = 0;
                }
                
            }
            else
            {
                 endText.text = "Point the top of your cell phone towards the center of the group";
                if (instance.player.compassValueCenter != -1)
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
    
    public void StartCompassReading()
    {
        StartCoroutine(CompassReading());
    }
    private IEnumerator CompassReading()
    {
        float averagedCompass = 0;
        int averagedTime = 0;
        const int averagedTimeMin = 10;
        float compassValue;
        float time = 0;
        const float timeMin = 0.5f;

        while (averagedTime <= averagedTimeMin || time <= timeMin || !player.finishedShooting)
        {
            time += Time.deltaTime;
            averagedTime += 1;
            compassValue = Input.compass.magneticHeading;       
              averagedCompass = ((averagedCompass * averagedTime) + compassValue) / (averagedTime);
          
            
            yield return null;
        }
        instance.player.CmdGroupShoot(averagedCompass);
    }
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

    //Minigame Management
    void GetMiniGameList()
    {
        MiniGame[] mgs = GetComponents<NormalMiniGame>();
        foreach(MiniGame mg in mgs)
        {
            miniGames.Add(mg);
        }
        miniGames.Add(GetComponent<Shoot>());

       // shootMG = GetComponent<Shoot>();
    }
     IEnumerator StartPlayingMiniGames()
    {
        if (isServer)
        {
            yield return new WaitForSeconds(timeWaitForServer);
        }
        Handheld.Vibrate();
        StartNextMG();
    }
    public void MiniGameFinished()
    {
        player.currentMGIndex += 1;
        StartNextMG();
       
    }
    void StartNextMG()
    {
            Debug.Log("Start Game " + player.currentMGIndex + " Index: " + miniGameIndex[player.currentMGIndex]);
            miniGames[miniGameIndex[player.currentMGIndex]].StartMiniGame();
    }
    IEnumerator LoadObjectsForMG()
    {
        while(!setupDone)
        {
            yield return null;
        }
        for (int i =0; i < instance.miniGameIndex.Length;i++ )
        {
            miniGames[instance.miniGameIndex[i]].LoadObjects();
        }
        //foreach(MiniGame Mg in miniGames)
        //{
        //    Mg.LoadObjects();
        //}
    }
    public MiniGame GetMiniGame(int index)
    {
        return DuelManager.instance.miniGames[DuelManager.instance.miniGameIndex[index]];
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
        StartCoroutine(StartPlayingMiniGames());
    }
    [ClientRpc]
    public void RpcDisplayEnd()
    {
       
        player.DisplayEndResult(instance.player.hasWon);       
    }
    [ClientRpc]
    public void RpcDisplayEndGroup(string winner)
    {
        if (groupPlay)
        {
            player.DisplayEndResultGroup(player.hasWon, winner);
         }
    }
    [ClientRpc]
    public void RpcRestart()
    {
        RestartGame();
    }
    [ClientRpc]
    public void RpcServerSetup(bool w)
    {
        instance.doingServerSetup = w;
    }
    [ClientRpc]
    public void RpcHasDied(string victim,string shooter)
    {
        if (player.name == victim)
        {
            player.DisplayYouDiedGroup(shooter);
        }
        else
        {
            Debug.Log(victim + "has been eliminated by " + shooter);
        }
    }
    [ClientRpc]
    public void RpcMiniGameList(int[] mgL)
    {
        instance.miniGameIndex = mgL;
        StartCoroutine(LoadObjectsForMG());
    }
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

//Vector2 TiltToAngle(Vector3 tilt)
//{
//    float xVal;
//    float yVal;

//    if (tilt.z < 0)
//    {
//        xVal = tilt.x * 90;
//        yVal = tilt.y * 90;           
//    }
//    else
//    {           
//        int signX = (tilt.x > 0) ? 1 : -1;
//        xVal = (signX * 180) - (tilt.x * 90);
//        yVal = tilt.y * 90;
//        //if (tilt.x < 0)
//        //{
//        //    yVal = tilt.y * 90;
//        //}
//        //else
//        //{
//        //    int signY = (tilt.y > 0) ? 1 : -1;
//        //    yVal = (signY * 180) - (tilt.y * 90);
//        //}          

//    }
//    return new Vector2(yVal, xVal);
//}
//float AngleEntreVecteurs(Vector3 plane, Vector3 mag)
//{
//    mag = Vector3.ProjectOnPlane(mag, plane);

//    Vector3 foward = Vector3.ProjectOnPlane(Vector3.up, plane);
//    Vector3 right = Vector3.ProjectOnPlane(Vector3.right, plane);

//    float angle = Vector3.Angle(mag, foward);
//    if (Vector3.Dot(mag, right) > 0)
//    {
//        angle = 360 - angle;
//    }
//    return angle;
//}
//float IGuessCestUneFaconDeLeFaire()
//{
//    Vector3 tilt;
//    Vector2 tiltAngle;
//    Vector3 rawVec;
//    Vector3 newVec;
//    Vector3 VecUp3D;
//    //  Vector3 tiltFoward;
//    // Vector3 tiltRight;
//    rawVec = Input.compass.rawVector;// Vector3.right;//
//    tilt = Input.acceleration;

//   // tiltAngle = TiltToAngle(tilt);
//    //newVec = Quaternion.Euler(0, -tiltAngle.y, 0) * rawVec;
//   // VecUp3D = Quaternion.Euler(-tiltAngle.x, tiltAngle.y, 0) * Vector3.up;

//    //float rawAngle = Vector3.Angle(Vector2.up, rawVec);

//    //float angle = Vector3.Angle(VecUp3D, rawVec);

//    // obaEnlever.transform.rotation = Quaternion.Euler(tiltAngle.x, -tiltAngle.y, 0);
//    obaEnlever.transform.forward = tilt;

//    newVec = obaEnlever.transform.forward;

//    obaEnleverMg.transform.up = Vector3.ProjectOnPlane(rawVec, tilt);//new Vector3(rawVec.x,rawVec.y,0);
//    Debug.Log(Vector3.Angle(obaEnleverMg.transform.up, obaEnlever.transform.up));
//    float angle = AngleEntreVecteurs(newVec, rawVec);//Vector3.Angle(VecUp3D, newVec);
//    //  rawVec = new Vector3(rawVec.x, rawVec.y, 0);
//    // Angle = Vector3.Angle(Vector3.up, rawVec);

//    return angle;
//}