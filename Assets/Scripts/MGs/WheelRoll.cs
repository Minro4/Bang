using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelRoll : NormalMiniGame {

    public GameObject mapPrefab;
    public GameObject playerPrefab;
    int indexPlayerOb;

    GameObject map;

    Coroutine miniGame;
    public float floorHeight;
    public float startingXPosition;
    public float raceEndPos;
    public float speedPerAmount;
    public float airResistance;
    public float otherResistance;
    float speedX;

    public float cameraOffsetX;
    public float minMovement;

    bool isDragging = false;
    Vector2 startTouch, swipeDelta;

    Vector2 wheelCenterOffset;

    WheelMapReference wmr;
    Animator[] wheels = new Animator[2];
    float[] wheelCirc = new float[2]; //12 frame pour 1 tour


    public override void ClearMiniGame()
    {
        if (miniGame != null)
        {
            StopCoroutine(miniGame);
            miniGame = null;
        }
        DisplayRace(false);
        StopAllCoroutines();
    }

    public override void LoadObjects()
    {
        map = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        wmr = map.GetComponent<WheelMapReference>();
        wheelCenterOffset = wmr.wheelCenterT.position;
       // GameObject finishLine = wmr.finishLine;//TransformExtensions.FindObjectWithTag(map.transform, "FinishLine");
        wmr.finishLine.transform.position = new Vector3(raceEndPos, floorHeight, 0);
        indexPlayerOb = (DuelManager.instance.player.playerForMG.Count);

        foreach (Player p in DuelManager.instance.players)
        {
            p.playerForMG.Add(Instantiate(playerPrefab, Vector3.zero, Quaternion.identity));
            p.playerForMG[indexPlayerOb].GetComponent<MiniGamePlayer>().miniGame = this;
            p.playerForMG[indexPlayerOb].transform.parent = p.transform;
            p.playerForMG[indexPlayerOb].transform.position = new Vector3(startingXPosition, floorHeight, 0);
        }

        GameObject[] wheelOb = TransformExtensions.FindObjectsWithTag(DuelManager.instance.player.playerForMG[indexPlayerOb].transform, "Wheel").ToArray();
        for (int i = 0; i < 2;i++)    {
            wheels[i] = wheelOb[i].GetComponent<Animator>();
            wheelCirc[i] = Mathf.PI * wheelOb[i].transform.lossyScale.x;
        }

    }
    public override void ResetGame()
    {
        RepeatingBG[] rpg = map.GetComponentsInChildren<RepeatingBG>();
        rpg[0].transform.position = new Vector3(0, 0, 0);
        rpg[1].transform.position = new Vector3(14.5f, 0, 0);

        wmr.finishLine.transform.position = new Vector3(raceEndPos, floorHeight, 0);
        DuelManager.instance.player.playerForMG[indexPlayerOb].transform.position = new Vector3(startingXPosition, floorHeight, 0);
    }
    public override void StartMiniGame()
    {
        DisplayRace(true);
        miniGame = StartCoroutine(MiniGame());
    }

    public override void UnloadObjects()
    {
        Destroy(map);
        //Get the index //LE CODE POUR QUE CA RELOAD PAS LES CHOSES QUI SONT DEJA LOAD MAIS AUSSI CHECKER LES UNLOAD DES MINIGAME POUR ENLEVER LES COMMENTAIRES
        //int index = -1;
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
        //            Destroy(p.playerForMG[index]);
        //            p.playerForMG.RemoveAt(index);


        //}
    }

    IEnumerator MiniGame()
    {
        speedX = 0;
        Transform camHolder = GameObject.FindGameObjectWithTag("CamHolder").transform;
        Camera camSec = Camera.main;
        Transform playerTransform = DuelManager.instance.player.playerForMG[indexPlayerOb].transform;
        playerTransform.position = new Vector3(startingXPosition, floorHeight, 0);
        bool tap;
        while (playerTransform.position.x < raceEndPos)
        {
            tap = false;
            #region Mobile Inputs
            if (Input.touchCount > 0)
            {
                if (Input.touches[0].phase == TouchPhase.Began)
                {
                    tap = true;
                    isDragging = true;
                    startTouch = GetCurrentWorldPos(true, camSec);
                }
                else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
                {
                    isDragging = false;
                    Reset();
                }
            }

            #endregion
            #region Standalone Inputs
            if (Input.GetMouseButtonDown(0))
            {
                tap = true;
                isDragging = true;
                startTouch = GetCurrentWorldPos(false, camSec);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                Reset();
                isDragging = false;
            }

            #endregion
            #region Manage Dragging
            if (isDragging)
            {
                Vector2 endTouch = Vector2.zero;
                swipeDelta = Vector2.zero;

                endTouch = GetCurrentWorldPos(Input.touchCount > 0, camSec);
                swipeDelta = endTouch - startTouch;

                if (swipeDelta.magnitude > minMovement)
                {
                    Vector2 wToStartTouch = startTouch - GetWheelCenter(camHolder);
                    Vector2 wToEndTouch = endTouch - GetWheelCenter(camHolder);
                    bool clockwise = true;

                    Vector2 perpendiculaire = new Vector2(-wToStartTouch.y, wToStartTouch.x);
                    Vector2 projection = Vector3.Project(swipeDelta, perpendiculaire);

                    if (Vector2.SignedAngle(wToStartTouch, wToEndTouch) > 0)
                    {
                        //Debug.Log(Vector2.SignedAngle(wToStartTouch, wToEndTouch));
                        clockwise = false;
                    }
                    RotateWheel(projection.magnitude, clockwise);
                    startTouch = GetCurrentWorldPos(Input.touchCount > 0, camSec);
                }

            }
            #endregion

            UpdateSpeed(playerTransform);
            UpdatePlayersX();
            UpdateCamera(camHolder, playerTransform);
            yield return null;
        }
        camHolder.position = Vector3.zero;
        DisplayRace(false);
        DuelManager.instance.MiniGameFinished();
    }
    public void DisplayRace(bool t)
    {
        foreach (Player p in DuelManager.instance.players)
        {
            // p.playerForMG[indexPlayerOb].GetComponent<SpriteRenderer>().enabled = t;
            p.playerForMG[indexPlayerOb].SetActive(t);
        }
        map.SetActive(t);
    }
    void UpdateSpeed(Transform playerTransform)
    { 
        speedX -= (airResistance  * speedX * speedX + otherResistance) * Time.deltaTime;
        if (speedX < 0)
        {
            speedX = 0;
        }
        playerTransform.position += new Vector3(speedX, 0, 0) * Time.deltaTime;
        DuelManager.instance.player.CmdUpdateWheelPos(playerTransform.position.x);
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].speed = speedX / wheelCirc[i];
        }
    }
    void UpdatePlayersX()
    {
        foreach (Player p in DuelManager.instance.livingPlayers)
        {
            if (p != DuelManager.instance.player)
            {
                p.playerForMG[indexPlayerOb].transform.position = new Vector3(p.wheelXPosition, floorHeight, 0);
            }            
        }
    }
    void UpdateCamera(Transform camHolder, Transform playerTransform)
    {
        camHolder.position = new Vector3(playerTransform.position.x + cameraOffsetX, 0, 0);
    }
    private void Reset()
    {
        startTouch = swipeDelta = Vector2.zero;
    }
    void RotateWheel(float amount, bool clockwise)
    {
        int cw = -1;
        if (clockwise)
            cw = 1;
        speedX += speedPerAmount*amount* cw;
    }
    Vector2 GetCurrentWorldPos(bool isTap, Camera cam)
    {
        if (isTap)
        {
           return cam.ScreenToWorldPoint(Input.touches[0].position);
        }
        else
        {
            return cam.ScreenToWorldPoint(Input.mousePosition);
        }
    }
    Vector2 GetWheelCenter(Transform cam)
    {
        return (Vector2)cam.position + wheelCenterOffset;
    }

    public override void SetPlayerOb(int index)
    {
        indexPlayerOb = index;
    }
}
