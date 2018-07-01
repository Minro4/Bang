using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {


    // [HideInInspector] public SpriteRenderer sprite;

    [HideInInspector]public List<GameObject> playerForMG = new List<GameObject>();
    public bool finishedShooting = false;
    public int index;
    public string name = "a";
    public Color color;

    [SyncVar]
    public bool isUD = false;
    [SyncVar]
    public bool hasWon = false;
    [SyncVar]
    public bool wantsToRestart = false;
    [SyncVar]
    public float compassValueCenter = -1;

    Vector3 initialPos;
    [HideInInspector]public int currentMGIndex = 0;

    //Mini Game Related Vars
    [SyncVar]
    public int tapRaceNbr;







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
        wantsToRestart = false;
        //hasWon = false;
        isUD = false;
        playerForMG.Clear();
        finishedShooting = false;
        transform.position = initialPos;
        currentMGIndex = 0;
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
        CmdWin(true);
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

    [Command]
    void CmdUpdateUD(bool ud)
    {
        isUD = ud;
    }
    [Command]
    void CmdWin(bool won)
    {
        if (won && !ServerDuel.instance.CheckWin())
        {
            hasWon = true;
            ServerDuel.instance.SomeoneHasWonDuel();
        }
        else
        {
            hasWon = false;
        }
    }
    [Command]
    public void CmdRestart(bool w)
    {
        wantsToRestart = w;
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
    public void CmdGroupShoot(float c)
    {
        ServerDuel.instance.FindTheTarget(this, c);
    }
    [Command]
    public void CmdUpdateTapRaceNbr(int n)
    {
        tapRaceNbr = n;
    }
}
