using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TimingRace : NormalMiniGame
{
    public GameObject mapPrefab;
    public GameObject playerPrefab;
    int indexPlayerOb;
    GameObject map;
    Coroutine timing;
    public float startingXPosition;
    public float raceEndPosMin;
    public float raceEndPosMax;
    float raceEndPos;

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
    }
    public override void LoadObjects()
    {
        raceEndPos = Random.Range(raceEndPosMin, raceEndPosMax);
        map = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        GameObject finishLine = TransformExtensions.FindObjectWithTag(map.transform, "FinishLine");
        finishLine.transform.position = new Vector3(raceEndPos, floorHeight, 0);
        indexPlayerOb = (DuelManager.instance.player.playerForMG.Count);

        foreach (Player p in DuelManager.instance.players)
        {
            p.playerForMG.Add(Instantiate(playerPrefab, Vector3.zero, Quaternion.identity));
            p.playerForMG[indexPlayerOb].GetComponent<SpriteRenderer>().color = p.color;
            p.playerForMG[indexPlayerOb].transform.parent = p.transform;
            p.playerForMG[indexPlayerOb].transform.position = new Vector3(startingXPosition, floorHeight, 0);
        }        
    }
    public override void UnloadObjects()
    {
        Destroy(map);
        foreach (Player p in DuelManager.instance.players)
        {
            Destroy(p.playerForMG[indexPlayerOb]);
            p.playerForMG.RemoveAt(indexPlayerOb);
        }
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
        Camera cam = Camera.main;
        xVelocity = 0;
        yVelocity = 0;
        float timeSinceLastTouch = 0;
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
            JumpUpdate(playerTransform);
            UpdateCamera(cam, playerTransform);
            yield return null;
        }
        Debug.Log(raceEndPos + "   " + playerTransform.position);
        cam.transform.position = Vector3.zero;
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
        float mult = Mathf.Abs(timeInterval - time);
        if (mult > timeWorstYouCanDo)  {
            mult = timeWorstYouCanDo;
        }
        mult = (timeWorstYouCanDo - mult)/timeWorstYouCanDo;
        xVelocity = baseXVelocity + (xVelocityMulti * mult * mult * mult);
        yVelocity = yVelocityOnJump;
        ShowJumpFeedBack(mult);
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
    void UpdateCamera(Camera cam, Transform playerTransform)
    {
        cam.transform.position = new Vector3(playerTransform.position.x + cameraOffsetX, 0, 0);
    }

    void ShowJumpFeedBack(float quality)
    {
        float[] interval = { 0.35f, 0.7f, 0.85f, 0.95f,1 };
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
            case 0: message = "Ok"; break;
            case 1: message = "Good"; break;
            case 2: message = "Great"; break;
            case 3: message = "Incredible"; break;
            case 4: message = "Perfect"; break;
            default: message = "Ok"; break;
        }
        Debug.Log(message);
    }
    //void Jump(float height)
    //{
    //    float xforce = (maxHeightToRegisterTouch-height)*xForceMult;
    //    Vector2 force = new Vector2(xforce, yForce);
    //    rb.AddForce(force);
    //    jumpRegistered = false; //changer ca parce que si tu click 2 quand tes en bas sa va register 2 fois
    //}

}
