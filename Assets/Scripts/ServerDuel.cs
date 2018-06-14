using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerDuel : NetworkBehaviour
{
    public static ServerDuel instance;

    public float timeBeforeStartMin;
    public float timeBeforeStartMax;
    Coroutine phase2;

    bool showMG = false;
    int consistent = 0;
    const int consistentMin = 50000;

    void Start()
    {
        instance = this;
        //StartCoroutine(WaitingForSetup());
    }
    public IEnumerator CompassSetup()
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
    bool PlacePlayers(List<Player> newPlayerOrder, int consistentMin)
    {
       
         foreach (Player p in DuelManager.instance.players)
         {
                if (p.compassValue == -1)
                {
                    return false;
                }
         }
        newPlayerOrder.Sort(delegate (Player x, Player y)
        {
            return x.compassValue.CompareTo(y.compassValue);
        });
       // DuelManager.instance.endText.text = "";

        if (compareArrays(newPlayerOrder.ToArray(), DuelManager.instance.players))
        {
            consistent += 1;
            //DuelManager.instance.endText.text += "\n Consistent: " + consistent;
            if (consistent >= consistentMin)
            {
                return true;
            }
        }
        else
        {
            DuelManager.instance.endText.text += "\n Consistent Reset";
            DuelManager.instance.players = newPlayerOrder.ToArray();
            consistent = 0;
        }
        return false;
    }
    public void FStart()
    {
        StartCoroutine(DuelPhase1());
    }
    public void Restart()
    {
        showMG = false;
        DuelManager.instance.RpcRestart();
        StartCoroutine(DuelPhase1());
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
        Debug.Log("Phase1");
        while (!allDeviceUD())
        {
            yield return null;
        }
        phase2 = StartCoroutine(DuelPhase2());
    }
    IEnumerator DuelPhase2() //waiting for time to start
    {
        Debug.Log("Phase2");
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
        StartCoroutine(GamePhase());
    }
    IEnumerator ServerTimeUpdate()
    {
        float time = 0;
        float timeBeforeStart = GenerateRandomTime();
        while (time < timeBeforeStart)
        {
            time += Time.deltaTime;
            yield return null;
        }
        showMG = true;

    }
    IEnumerator GamePhase() //need to press buttons   
    {
        Debug.Log("Phase3");
        DuelManager.instance.RpcStartMG();
        DuelManager.instance.RpcStopUpdate();        
        while (!CheckWin())
        {
            yield return null;
        }
        DuelManager.instance.RpcDisplayEnd();
        StartCoroutine(RestartPhase());
    }
    IEnumerator RestartPhase()
    {
        Debug.Log("RestartPhase");
        while (!CheckRestart())
        {
            yield return null;
        }
        Restart();
    }
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
    bool CheckRestart()
    {
        int wtr = 0;
        foreach(Player p in DuelManager.instance.players)
        {
            if (p.wantsToRestart)
            {
                wtr += 1;
            }
        }
        //RpcDisplay de number 
        return (wtr == 2);
    }
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
   public bool CheckWin()
    {
        foreach (Player p in DuelManager.instance.players)
        {
            if (p.hasWon)
            {
                return true;
            }
        }
        return false;
    }

    float GenerateRandomTime()
    {
        return Random.Range(timeBeforeStartMin, timeBeforeStartMax);
    }
    bool compareArrays(Player[] a1, Player[] a2)
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
