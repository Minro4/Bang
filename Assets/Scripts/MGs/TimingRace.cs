using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using EZCameraShake;

public class TimingRace : NormalMiniGame
{
    public GameObject mapPrefab;
    public GameObject playerPrefab;
    int indexPlayerOb;
    GameObject map;
    Coroutine timing;
    public float startingXPosition;
    public float raceEndPos;

    // public float maxHeightToRegisterTouch;
    public float cameraOffsetX;
    bool jumpRegistered = false;
    //public float xForceMult;
    public float timeInterval;
    //public float jumpHeight;
    public float timeWorstYouCanDo;
    public float floorHeight;
    public float gravityForce;
    public float xVelocityMulti;
    public float baseXVelocity;
    float yVelocity = 0;
    float xVelocity = 0;
    float yVelocityOnJump;
    Slider powerSlider;
    TimingMapReference tmr;

    Animator textAnim;

    float registeredTime;


    private void Start()
    {
        yVelocityOnJump = -gravityForce * timeInterval / 2;
    }
    public override void StartMiniGame()
    {
        DisplayRace(true);
        timing = StartCoroutine(Timing());
    }
    public override void ClearMiniGame()
    {
        if (timing != null)
        {
            StopCoroutine(timing);
            timing = null;
        }
        DisplayRace(false);
        StopAllCoroutines();
    }
    public override void LoadObjects()
    {
        map = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        tmr = map.GetComponent<TimingMapReference>();
        textAnim = tmr.feedBackText.GetComponent<Animator>();
        GameObject finishLine = tmr.finishLine;//TransformExtensions.FindObjectWithTag(map.transform, "FinishLine");
        finishLine.transform.position = new Vector3(raceEndPos, floorHeight, 0);
        indexPlayerOb = (DuelManager.instance.player.playerForMG.Count);
        powerSlider = tmr.powerSlider;
        powerSlider.value = 0;
        foreach (Player p in DuelManager.instance.players)
        {
            p.playerForMG.Add(Instantiate(playerPrefab, Vector3.zero, Quaternion.identity));
            p.playerForMG[indexPlayerOb].GetComponent<MiniGamePlayer>().miniGame = this;
            p.playerForMG[indexPlayerOb].GetComponent<SpriteRenderer>().color = p.color;
            p.playerForMG[indexPlayerOb].transform.parent = p.transform;
            p.playerForMG[indexPlayerOb].transform.position = new Vector3(startingXPosition, floorHeight, 0);
        }        
    }
    public override void UnloadObjects()
    {
        Destroy(map);
        //int index = -1;   //LE CODE POUR QUE CA RELOAD PAS LES CHOSES QUI SONT DEJA LOAD MAIS AUSSI CHECKER LES UNLOAD DES MINIGAME POUR ENLEVER LES COMMENTAIRES
        //for (int i = 0; i < DuelManager.instance.player.playerForMG.Count; i++)
        //{
        //    if (DuelManager.instance.player.playerForMG[i].GetComponent<MiniGamePlayer>().miniGame = this)
        //    {
        //        index = i;
        //        return;
        //    }
        //}
        //if (index == -1)
        //{
        //    Debug.Log("Rien a unload");
        //    return;
        //}
        ////DeleteTheThings
        //foreach (Player p in DuelManager.instance.players)
        //{
        //    Destroy(p.playerForMG[index]);
        //    p.playerForMG.RemoveAt(index);


        //}
    }
    public override void ResetGame()
    {
        RepeatingBG[] rpg = map.GetComponentsInChildren<RepeatingBG>();
        rpg[0].transform.position = new Vector3(0, 0, 0);
        rpg[1].transform.position = new Vector3(14.5f, 0, 0);

        tmr.finishLine.transform.position = new Vector3(raceEndPos, floorHeight, 0);
        DuelManager.instance.player.playerForMG[indexPlayerOb].transform.position = new Vector3(startingXPosition, floorHeight, 0);
    }
    public void DisplayRace(bool t)
    {
        foreach (Player p in DuelManager.instance.players)
        {
            p.playerForMG[indexPlayerOb].GetComponent<SpriteRenderer>().enabled = t;
            p.playerForMG[indexPlayerOb].GetComponent<NetworkTransform>().enabled = t;
        }
        map.SetActive(t);
    }
    IEnumerator Timing()
    {
        Transform camHolder = GameObject.FindGameObjectWithTag("CamHolder").transform;
        xVelocity = 0;
        yVelocity = 0;
        float timeSinceLastTouch = timeInterval + timeWorstYouCanDo;
        Transform playerTransform = DuelManager.instance.player.playerForMG[indexPlayerOb].transform;
        playerTransform.position = new Vector3(startingXPosition, floorHeight, 0);
        while (playerTransform.position.x < raceEndPos)
        {
            timeSinceLastTouch += Time.deltaTime;
            if (jumpRegistered)
            {
                timeSinceLastTouch = ScreenTouch(timeSinceLastTouch, playerTransform);
            }
            else
            {
                foreach (Touch touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        timeSinceLastTouch = ScreenTouch(timeSinceLastTouch, playerTransform);
                    }
                }
                if (Input.GetMouseButtonDown(0))
                {
                    timeSinceLastTouch = ScreenTouch(timeSinceLastTouch, playerTransform);
                }
            }
            SliderUpdate(timeSinceLastTouch);
            JumpUpdate(playerTransform);
            UpdateCamera(camHolder, playerTransform);
            yield return null;
        }
        camHolder.position = Vector3.zero;
        DisplayRace(false);
        DuelManager.instance.MiniGameFinished();
    }
    float ScreenTouch(float time, Transform playerTransform)
    {
        if (playerTransform.position.y == floorHeight)
        {
            if (jumpRegistered)
            {
                Jump(registeredTime, playerTransform);
                jumpRegistered = false;
            }
            else
            {
                Jump(time, playerTransform);
                registeredTime = time;
            }
            time = 0;
        }
        else if((time >= (timeInterval - timeWorstYouCanDo) && !jumpRegistered)) //Register time
        {
            registeredTime = time;
            jumpRegistered = true;
        }
        return time;
    }
    void Jump(float time,Transform playerTransform)
    {
            playerTransform.position += new Vector3(0, 0.01f, 0);
            float mult = GetMult(time);
            xVelocity = baseXVelocity + (xVelocityMulti * mult * mult * mult * mult * mult);
            yVelocity = yVelocityOnJump;
            ShowJumpFeedBack(mult);
        CameraShaker.Instance.ShakeOnce(mult * 5, 0.5f, 0.05f, 0.5f); 
        
    }
    float GetMult(float time)
    {
        float mult = Mathf.Abs(timeInterval - time);
        if (mult > timeWorstYouCanDo)
        {
            mult = timeWorstYouCanDo;
        }
        return (timeWorstYouCanDo - mult) / timeWorstYouCanDo;
    }
    void JumpUpdate(Transform playerTransform)
    {
        if (playerTransform.position.y > floorHeight)
        {
            yVelocity += gravityForce * Time.deltaTime;
            playerTransform.position += new Vector3(xVelocity, yVelocity, 0) * Time.deltaTime;
        }
        else
        {
            playerTransform.position = new Vector3(playerTransform.position.x, floorHeight, 0);
            yVelocity = 0;
            xVelocity = 0;
        }
    }
    void SliderUpdate(float time)
    {
        if (jumpRegistered || time < timeInterval - timeWorstYouCanDo)
        {
            time = registeredTime;
        }
        float mult = GetMult(time);
        powerSlider.value = 1 - mult*mult;
    }
    void SliderUpdate2(float time)
    {
        if (jumpRegistered || time < timeInterval - timeWorstYouCanDo)
        {
            time = registeredTime;
        }
        if (time < timeInterval)
        {
            powerSlider.value = 1 - GetMult(time)/2;
        }
        else
        {
            powerSlider.value = (GetMult(time))/2;
        }
    }
    void UpdateCamera(Transform camHolder, Transform playerTransform)
    {
        camHolder.position = new Vector3(playerTransform.position.x + cameraOffsetX, 0, 0);
    }

    void ShowJumpFeedBack(float quality)
    {
        float[] interval = { 0.75f, 0.85f, 0.9f, 0.98f,1 };
        int qi = 0;
        string message;
        for (int i =0; i< interval.Length; i++)
        {
            if (quality < interval[i]) {
            qi = i;
                break;

        }
        }

        switch(qi)
        {
            case 0: message = "Okay"; break;
            case 1: message = "Good"; break;
            case 2: message = "Great"; break;
            case 3: message = "Incredible"; break;
            case 4: message = "Perfect"; break;
            default: message = "Okay"; break;
        }
        textAnim.SetTrigger("TextAppear");
        tmr.feedBackText.text = message;
    }

    public override void SetPlayerOb(int index)
    {
        indexPlayerOb = index;
    }
    //void Jump(float height)
    //{
    //    float xforce = (maxHeightToRegisterTouch-height)*xForceMult;
    //    Vector2 force = new Vector2(xforce, yForce);
    //    rb.AddForce(force);
    //    jumpRegistered = false; //changer ca parce que si tu click 2 quand tes en bas sa va register 2 fois
    //}

}
