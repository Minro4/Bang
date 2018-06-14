using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {

    List<GameObject> buttonsInScene;
    List<GameObject> buttonList = new List<GameObject>();
    int nextButtonToPress = 0;
    public SpriteRenderer sprite;

    [SyncVar]
    public bool isUD = false;
    [SyncVar]
    public bool hasWon = false;
    [SyncVar]
    public bool wantsToRestart = false;
    [SyncVar]
    public float compassValue = -1;

    Vector3 initialPos;

    Coroutine shoot;
    Coroutine race;

    const float timeWaitForServer = 0.1f;


    public void InitializePlayer()
    {
        initialPos = transform.position;
        if (isLocalPlayer)
        {
            sprite.color = DuelManager.instance.playerColor;
            DuelManager.instance.player = this;
        }
        else
        {
            sprite.color = DuelManager.instance.otherPlayerColor;
        }
    }
    public void ResetPlayer()
    {
        //CmdWin(false);
        //CmdUpdateUD(false);
        wantsToRestart = false;
        hasWon = false;
        isUD = false;
        sprite.enabled = false;
        nextButtonToPress = 0;
        buttonList.Clear();
        transform.position = initialPos;
        shoot = null;
        race = null;
    }
    public bool DeviceIsUD()
    {
        return Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown || Input.GetKey(KeyCode.Space);
    }
    public IEnumerator StartMG()
    {
       if (isServer)
        {
            yield return new WaitForSeconds(timeWaitForServer);
        }
        Handheld.Vibrate();
        SetButtons();
    }

    public void SetButtons()
    {
        buttonsInScene = TransformExtensions.FindObjectsWithTag(DuelManager.instance.canvasDuel.transform, "Button");
        int iMax = buttonsInScene.Count;
        for (int i = 0; i < iMax; i++)
        {
            int rng = Random.Range(0, buttonsInScene.Count - 1);
            buttonList.Add(buttonsInScene[rng]);
            buttonsInScene[rng].GetComponentInChildren<Text>().text = (i + 1).ToString();
            buttonsInScene[rng].SetActive(true);
            buttonsInScene.RemoveAt(rng);
        }

    }
    public void ButtonPressed(GameObject button)
    {
        if (button == buttonList[nextButtonToPress])
        {
            button.SetActive(false);
            nextButtonToPress += 1;
            if (nextButtonToPress == buttonList.Count)
            {
                race = StartCoroutine(Race());

            }
        }
        else
        {
            Debug.Log("Wrong Button");
        }
    }
    IEnumerator Race()
    {
        DisplayRace(true);
        while (transform.position.y < DuelManager.instance.raceEndPos)
        {
            foreach (Touch touch in Input.touches)
            {
              if(  touch.phase == TouchPhase.Began)
                {
                    transform.position += Vector3.up / 3.5f;
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                transform.position += Vector3.up / 3.5f;
            }
            yield return null;
        }
        DisplayRace(false);
        shoot = StartCoroutine(Shoot());
    }
    public void DisplayRace(bool t)
    {
        foreach (Player p in DuelManager.instance.players)
        {
            p.sprite.enabled = t;
            p.GetComponent<NetworkTransform>().enabled = t;
        }
        DuelManager.instance.map.SetActive(t);
    }
    IEnumerator Shoot()
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
                DuelManager.instance.map.SetActive(false);
                CmdUpdateUD(false);
                DuelManager.instance.endText.text = "Eille";
                //show le caca
            }
        }
    }
    public void DisplayEndResult(bool won)
    {
        ClearMiniGame();
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
    void ClearMiniGame()
    {
        if (race != null)
        {
            StopCoroutine(race);
            DuelManager.instance.map.SetActive(false);
        }
        if (shoot != null)
        {
            StopCoroutine(shoot);
        }
        for(int i = nextButtonToPress; i < buttonList.Count; i++)
        {
            buttonList[i].SetActive(false);
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
        compassValue = c;
    }
}
