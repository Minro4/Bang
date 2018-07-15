using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerDuel : NetworkBehaviour
{
    public static ServerDuel instance;

    public bool testingMG;
    public int testedMGIndex;

    public float timeBeforeStartMin;
    public float timeBeforeStartMax;
    Coroutine phase2;

    bool showMG = false;
    int consistent = 0;                                         //g
    const int consistentMin = 5;                                //g


    public int numberOfMGBeforeShoot;

    [HideInInspector]public bool needToCalibrate = false;


    void Start()
    {
        instance = this;
        //StartCoroutine(WaitingForSetup());
    }
    //private void Update()
    //{
    //    Debug.Log(DuelManager.instance.livingPlayers.Count);
    //}
    public IEnumerator CompassSetup()                       //g
    {
        consistent = 0;
       
        List<Player> newPlayerOrder = new List<Player>();
        foreach(Player p in DuelManager.instance.players)
        {
            newPlayerOrder.Add(p);
        }
        // List<Player> playerOrder = new List<Player>();
        while (!PlacePlayers( newPlayerOrder, consistentMin))
        {
            yield return new WaitForSeconds(0.2f);
        }      
        DuelManager.instance.RpcServerSetup(false);
    }

    bool PlacePlayers(List<Player> newPlayerOrder, int consistentMin)       //g
    {
       
         foreach (Player p in DuelManager.instance.players)
         {
                if (p.compassValueCenter == -1)
                {
                    return false;
                }
         }
        newPlayerOrder.Sort(delegate (Player x, Player y)
        {
            return x.compassValueCenter.CompareTo(y.compassValueCenter);
        });
       DuelManager.instance.endText.text = "";

        if (compareArrays(newPlayerOrder.ToArray(), DuelManager.instance.players.ToArray()))
        {
            consistent += 1;
            DuelManager.instance.endText.text += "\n Consistent: " + consistent;
            if (consistent >= consistentMin)
            {
                for (int i = 0; i < DuelManager.instance.players.Count; i++)
                {
                    DuelManager.instance.players[i].id = i;

                }
                return true;
            }
        }
        else
        {
            DuelManager.instance.endText.text += "\n Consistent Reset";
           // DuelManager.instance.players = newPlayerOrder; //fucked
            DuelManager.instance.players.Clear();
            DuelManager.instance.players.AddRange(newPlayerOrder);
           consistent = 0;
        }
        return false;
    }

    public void FStart()
    {
        DuelManager.instance.RpcMiniGameList(GenerateRandomMG());
        StartCoroutine(DuelPhase1());
    }

    public void Restart()
    {
        showMG = false;
        DuelManager.instance.playerWhoWantsToRestart = 0;
        DuelManager.instance.RpcRestart(needToCalibrate);
        needToCalibrate = false;

    }
    //IEnumerator WaitingForSetup()
    //{
    //    while (!CheckSetup())
    //    {
    //        yield return null;
    //    }
    //    StartCoroutine(DuelPhase1());
    //}
    IEnumerator DuelPhase1() //Everyone places phone down
    {
     //   Debug.Log("Phase1");
        while (!allDeviceUD())
        {
            yield return null;
        }
        phase2 = StartCoroutine(DuelPhase2());
    }
    IEnumerator DuelPhase2() //waiting for time to start
    {
//        Debug.Log("Phase2");
        Coroutine timeUp = null;

        timeUp = StartCoroutine(ServerTimeUpdate());
        
        while (!showMG)
        {
            if (!allDeviceUD())
            {
                StartCoroutine(DuelPhase1());
                if (isServer)
                {
                    StopCoroutine(timeUp);
                }
                StopCoroutine(phase2);
            }
            yield return null;
        }
        GamePhase();
    }
    IEnumerator ServerTimeUpdate()
    {
        float time = 0;
        float timeBeforeStart = GenerateRandomTime();

        if (testingMG)
        {
            timeBeforeStart = 0;
        }

        while (time < timeBeforeStart)
        {
            time += Time.deltaTime;
            yield return null;
        }
        showMG = true;

    }
    void GamePhase() //need to press buttons   
    {
     //   Debug.Log("Phase3");
        DuelManager.instance.RpcStartMG();
        DuelManager.instance.RpcStopUpdate();        
        //while (!CheckWin())
        //{
        //    yield return null;
        //}
        //DuelManager.instance.RpcDisplayEnd();
        //StartCoroutine(RestartPhase());
    }
    public void SomeoneHasWonDuel()
    {
        DuelManager.instance.RpcDisplayEnd();
    }
    //public void SomeoneHasWonGroup(string winner) 
    //{
    //    DuelManager.instance.RpcDisplayEndGroup(winner);
    //}
 
   
    //bool CheckSetup()
    //{
    //    foreach (Player p in DuelManager.instance.players)
    //    {
    //        if (!p.setupDone)
    //        {
    //            return false;
    //        }
    //    }
    //    return true;
    //}
    //bool CheckRestart()
    //{
    //    int wtr = 0;
    //    foreach(Player p in DuelManager.instance.players)
    //    {
    //        if (p.wantsToRestart)
    //        {
    //            wtr += 1;
    //        }
    //    }
    //    //RpcDisplay de number 
    //    return (wtr == 2);
    //}
    bool allDeviceUD()
    {
        foreach (Player p in DuelManager.instance.players)
        {
           // p.CheckIsUD();
            if (!p.isUD)
            {
                return false;
            }
        }
        return true;
    }
   //public bool CheckWin()
   // {
   //     foreach (Player p in DuelManager.instance.players)
   //     {
   //         if (p.hasWon)
   //         {
   //             return true;
   //         }
   //     }
   //     return false;
   // }

    float GenerateRandomTime()
    {
        return Random.Range(timeBeforeStartMin, timeBeforeStartMax);
    }

    int[] GenerateRandomMG()
    {

        List<int> selection = new List<int>();

        if (testingMG)
        {
            selection.Add(testedMGIndex);
            return selection.ToArray();
        }



        int numberOfMG = DuelManager.instance.miniGames.Count-1;

        List<int> candidates = new List<int>();
        for (int i = 0; i < numberOfMG; i++)
        {
            candidates.Add(i);
        }

       
      
        for (int i =0; i< numberOfMGBeforeShoot;i++)
        {
            int index = Random.Range(0,candidates.Count);
            selection.Add( candidates[index]);
            candidates.RemoveAt(index);
        }
        selection.Add(DuelManager.instance.miniGames.Count - 1);
        return selection.ToArray();
    }
    bool compareArrays(Player[] a1, Player[] a2)            //g
    {
        if (a1.Length != a2.Length)
        {
            return false;
        }
        for (int i = 0; i < a1.Length; i++)
        {
            if(a1[i] != a2[i])
            {
                return false;
            }
        }
        return true;
    }


}
