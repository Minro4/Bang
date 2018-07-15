using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {


    // [HideInInspector] public SpriteRenderer sprite;

    [HideInInspector]public List<GameObject> playerForMG = new List<GameObject>();
    //public bool finishedShooting = false;
    [SyncVar]
    public int id;
    public string name = "a";
    public Color color;

    [SyncVar]
    public bool isUD = false;

    //public bool hasWon = false;
    //[SyncVar]
    //public bool wantsToRestart = false;
    public float compassValueCenter = -1;
    [SyncVar]
    public float wheelXPosition;

    Vector3 initialPos;
    [HideInInspector]public int currentMGIndex = 0;

    //Mini Game Related Vars
    [SyncVar]
    public int tapRaceNbr;

    public bool hasWon = false; //juste pour duel







    public void InitializePlayer()
    {
        initialPos = transform.position;
        if (isLocalPlayer)
        {
          //  sprite.color = DuelManager.instance.playerColor;
            DuelManager.instance.player = this;
        }
        //else
        //{
        //    sprite.color = DuelManager.instance.otherPlayerColor;
        //}
    }
    public void ResetPlayer()
    {
        //CmdWin(false);
        //CmdUpdateUD(false);
       // wantsToRestart = false;
        //hasWon = false;
        isUD = false;
     //   finishedShooting = false;
        transform.position = initialPos;
        currentMGIndex = 0;
        tapRaceNbr = 0;
        wheelXPosition = 0;
        hasWon = false;
        
}
    public bool DeviceIsUD()
    {
        return Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown || Input.GetKey(KeyCode.Space);
    }
  
  
    //public void GroupShoot(float averagedCompass)
    //{
    //    CmdWin(true);
    //    DisplayEndResult(true);
    //}
    public void ShootSomeone()
    {
        CmdWin();
        DisplayEndResult(true);
    }

    public void CheckIsUD()   {
        if (isLocalPlayer)
        {
            if (Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown || Input.GetKey(KeyCode.Space))
            {
                CmdUpdateUD(true);
                DuelManager.instance.endText.text = "";
            }
            else
            {
                CmdUpdateUD(false);
                DuelManager.instance.endText.text = "Eille";
                //show le caca
            }
        }
    }
    public void DisplayEndResult(bool won)
    {
        DuelManager.instance.GetMiniGame(currentMGIndex).ClearMiniGame();
        DuelManager.instance.restartButton.SetActive(true);
        DuelManager.instance.restartNumber.SetActive(true);
        DuelManager.instance.UpdatePlayerRestartCount(false);
        if (won)
        {
            DuelManager.instance.endText.text = "You Won!!!";
        }
        else
        {
            Handheld.Vibrate();
            DuelManager.instance.endText.text = "Game Over";
        }
    }
    public void DisplayYouDiedGroup(string killer)
    {
        DuelManager.instance.GetMiniGame(currentMGIndex).ClearMiniGame();
        DuelManager.instance.endText.text = "You were eliminated by " + killer;
    }
    public void DisplayEndResultGroup(bool won, string winner)
    {
        DuelManager.instance.GetMiniGame(currentMGIndex).ClearMiniGame();
        DuelManager.instance.restartButton.SetActive(true);
        DuelManager.instance.restartNumber.SetActive(true);
        DuelManager.instance.UpdatePlayerRestartCount(false);
        if (won)
        {
            DuelManager.instance.endText.text = "You Won!!!";
        }
        else
        {
            Handheld.Vibrate();
            DuelManager.instance.endText.text = winner + " won!";
        }
    }

    public int GetLPIndex()
    {
        for(int i =0; i < DuelManager.instance.livingPlayers.Count;i++)
        {
            if (DuelManager.instance.livingPlayers[i] == this)
            {
                return i;
            }
        }
        return 0;
    }

    [Command]
    void CmdUpdateUD(bool ud)
    {
        isUD = ud;
    }
    [Command]
    void CmdWin()
    {
        hasWon = true;
        DuelManager.instance.player.DisplayEndResult(true);
        ServerDuel.instance.SomeoneHasWonDuel();
    }
    [Command]
    public void CmdRestart(bool playerWantsToRestart)
    {
        if (playerWantsToRestart)
        {
           // DuelManager.instance.playerWhoWantsToRestart += 1;
            if (DuelManager.instance.playerWhoWantsToRestart+1 == DuelManager.instance.players.Count)
            {
                ServerDuel.instance.Restart();
                return;
            }
        }
        else
        {
            DuelManager.instance.RpcRemovePlayer(id);
            ServerDuel.instance.needToCalibrate = true;          
         //   DuelManager.instance.players.RemoveAt(index);
        }
        DuelManager.instance.RpcUpdatePlayerRestartCount(playerWantsToRestart);
    }
    [Command]
    public void CmdStartCompassSetup()
    {
       if (isServer)
        {
            StartCoroutine(ServerDuel.instance.CompassSetup());
        }
    }
    [Command]
    public void CmdSetCompassValue(float c)
    {
        compassValueCenter = c;
    }
    [Command]
    public void CmdGroupShoot(int indexTarget, int indexShooter)
    {
        DuelManager.instance.RpcHasDied(indexTarget, indexShooter);
        //ServerDuel.instance.FindTheTarget(this, c);
    }
    [Command]
    public void CmdUpdateTapRaceNbr(int n)
    {
        tapRaceNbr = n;
    }
    [Command]
    public void CmdUpdateWheelPos(float p)
    {
        wheelXPosition = p;
    }
   
}
