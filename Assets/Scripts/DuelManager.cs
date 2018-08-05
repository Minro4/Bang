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
    public GameObject restartNumber;

    public Color restartGreen;
    public Color restartRed;

    //  public Color playerColor;
    // public Color otherPlayerColor;

   // Coroutine udUpdate;

    bool doingServerSetup;
    public bool groupPlay = false;

    [HideInInspector] public List<Player> livingPlayers = new List<Player>();             //g

    [HideInInspector] public List<Player> players;
    [HideInInspector] public Player player;

    [HideInInspector] public List<MiniGame> miniGames = new List<MiniGame>();
  //  Shoot shootMG;
    [HideInInspector] public int[] miniGameIndex = null;

    const float timeWaitForServer = 0.1f;

    bool setupDone = false;

    public int playerWhoWantsToRestart = 0;

    void Start () {
        instance = this;
        instance.setupDone = false;
        Invoke("GetPlayersMG", 1f);
    }
    //private void Update()
    //{
    //    Debug.Log("players " + instance.players.Count);
    //    Debug.Log("living players " + instance.livingPlayers.Count);
    //}
    void GetPlayersMG()
    {
        instance.players = GetPlayers();
        GetMiniGameList();
        Setup();
    }
    public void Setup()
    {
       
        ResetVar();

        if (instance.players.Count >= minimumGroupPlayer)                    //Setup for Group
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
        StartCoroutine(SendUDUpdate());
        if (isServer)                               //gameStart
        {
            ServerDuel.instance.FStart();
        }
    }
    List<Player> GetPlayers()
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
        return pl;
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
                        instance.player.compassValueCenter = averagedCompass;
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
            else if(SystemInfo.deviceType == DeviceType.Desktop)
            {
                instance.player.CmdSetCompassValue(120);
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
        SortPlayersById();
        yield return new WaitForSeconds(1);
        endText.text = "";
        SetupGameStart();

    }
    
    public void WantsToRestartGame(GameObject button)
    {
        button.SetActive(false);
        instance.player.CmdRestart(true);
       // UpdatePlayerRestartCount();
    }
    public void UpdatePlayerRestartCount(bool t)
    {     
        if (t)
        {
            instance.playerWhoWantsToRestart += 1;
        }
            restartNumber.GetComponentInChildren<Text>().text = instance.playerWhoWantsToRestart + "/" + instance.players.Count;
      //  restartNumber.GetComponent<Image>().color = instance.restartGreen;
        
    }
    public void RestartGame(bool recalibrate)
    {
        
        if (recalibrate)
        {
            Setup();
        }
        else
        {
            ResetVar();
            SetupGameStart();
        }
    }
    void ResetVar()
    {      
        foreach (Player p in instance.players)
        {
            p.ResetPlayer();
        }
        instance.livingPlayers.Clear();
        instance.livingPlayers.AddRange(instance.players);
       
        StopUpdates();
       // udUpdate = StartCoroutine(SendUDUpdate());
        //compassUpdate = StartCoroutine(SendCompassUpdate());
        instance.restartButton.SetActive(false);
        instance.restartNumber.SetActive(false);
        instance.playerWhoWantsToRestart = 0;
    }
    void SortPlayersById()
    {
        instance.players.Sort(delegate (Player x, Player y)
        {
            return x.id.CompareTo(y.id);
        });
        instance.livingPlayers.Sort(delegate (Player x, Player y)
        {
            return x.id.CompareTo(y.id);
        });
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
        StopAllCoroutines();
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
        Vibrate();
        StartNextMG();
    }
    public void MiniGameFinished()
    {
        instance.player.currentMGIndex += 1;
        StartNextMG();    
    }
    public void MiniGameShootFinished()
    {
        instance.player.currentMGIndex -= 1;
        StartNextMG();
    }
    void StartNextMG()
    {
        //    Debug.Log("Start Game " + player.currentMGIndex + " Index: " + miniGameIndex[player.currentMGIndex]);
            miniGames[miniGameIndex[player.currentMGIndex]].StartMiniGame();
    }

    IEnumerator LoadObjectsForMG()
    {
        while(!setupDone)
        {
            yield return null;
        }
        #region best way
        //LE CODE POUR QUE CA RELOAD PAS LES CHOSES QUI SONT DEJA LOAD MAIS AUSSI CHECKER LES UNLOAD DES MINIGAME POUR ENLEVER LES COMMENTAIRES
        //List<int> mgToLoad = new List<int>();
        //List<int> mgToUnload = new List<int>();
        //List<int> mgToSort = new List<int>();


        //foreach (int index in loaded)
        //{
        //    mgToUnload.Add(index);
        //}
        //foreach (int i in instance.miniGameIndex)
        //{
        //    bool isLoaded = false;
        //    foreach(int j in loaded)
        //    {
        //        if (i == j)
        //        {
        //            mgToUnload.Remove(j);
        //            mgToSort.Add(j);
        //            isLoaded = true;
        //            continue;
        //        }
        //    }
        //    if (!isLoaded)
        //    {
        //        mgToLoad.Add(i);
        //    }
        //}
        //foreach (int i in mgToUnload)
        //{
        //    instance.miniGames[i].UnloadObjects();
        //}
        //foreach(int i in mgToSort)
        //{
        //    for (int j = 0; j < DuelManager.instance.player.playerForMG.Count; j++)
        //    {
        //        if (DuelManager.instance.player.playerForMG[j].GetComponent<MiniGamePlayer>().miniGame = instance.miniGames[i])
        //        {
        //            instance.miniGames[i].SetPlayerOb(j);
        //        }
        //    }
        //    DuelManager.instance.miniGames[i].ResetGame();
        //}
        //foreach (int i in mgToLoad)
        //{
        //    instance.miniGames[i].LoadObjects();
        //}

        #endregion
        for (int i = 0; i < instance.miniGameIndex.Length; i++)
        {
            miniGames[instance.miniGameIndex[i]].LoadObjects();
        }

    }
    void UnloadObjects()
    {
        for (int i = 0; i < instance.miniGameIndex.Length; i++)
        {
            miniGames[instance.miniGameIndex[i]].UnloadObjects();
        }
        foreach (Player p in DuelManager.instance.players)
        {
            foreach (GameObject go in p.playerForMG)
            {
                Destroy(go);
            }
            p.playerForMG.Clear();
        }

    }
    public MiniGame GetMiniGame(int index)
    {
        return DuelManager.instance.miniGames[DuelManager.instance.miniGameIndex[index]];
    }

    public static void Vibrate()
    {
       // Handheld.Vibrate();
    }












  


    [ClientRpc]
    public void RpcStartSetup()
    {
        Setup();
    }

    [ClientRpc]
    public void RpcStartMG()
    {
        StopUpdates();
        StartCoroutine(StartPlayingMiniGames());
    }
    [ClientRpc]
    public void RpcDisplayEnd()
    {
        if (!instance.player.hasWon)
        {
            player.DisplayEndResult(false);
        }       
    }
    //[ClientRpc]
    //public void RpcDisplayEndGroup(string winner)
    //{
    //        player.DisplayEndResultGroup(player.hasWon, winner);
         
    //}
    [ClientRpc]
    public void RpcRestart(bool recalibrate)
    {
        RestartGame(recalibrate);
    }
    [ClientRpc]
    public void RpcServerSetup(bool w)
    {
        instance.doingServerSetup = w;
    }
    [ClientRpc]
    public void RpcHasDied(int victim,int shooter)
    {
        //if (victim >= instance.livingPlayers.Count|| shooter >= instance.livingPlayers.Count)
        //{       
        //    return;
        //}
        if (instance.player.id == victim)
        {
            instance.player.DisplayYouDiedGroup(instance.players[shooter].name);
        }
        else
        {
            Debug.Log(victim + " has been eliminated by " + shooter);
            //Debug.Log(instance.livingPlayers[victim].name + " has been eliminated by " + instance.livingPlayers[shooter].name);
        }
        instance.livingPlayers.Remove(instance.players[victim]);
        if (instance.livingPlayers.Count == 1)
        {
           // instance.livingPlayers[0].hasWon = true;
            instance.player.DisplayEndResultGroup(instance.player == instance.livingPlayers[0], instance.livingPlayers[0].name);
        }
    }
    [ClientRpc]
    public void RpcMiniGameList(int[] mgL)
    {
        UnloadObjects();
        instance.miniGameIndex = mgL;
        StartCoroutine(LoadObjectsForMG());
    }
    [ClientRpc]
    public void RpcUpdatePlayerRestartCount(bool t)
    {
        UpdatePlayerRestartCount(t);
    }
    [ClientRpc]
    public void RpcRemovePlayer(int id)
    {
        Debug.Log("Jle Fait");
        instance.players.RemoveAt(id);
    }
    //[ClientRpc]
    //public void RcpSetIndex(int i)
    //{
    //    index = i;
    //}


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