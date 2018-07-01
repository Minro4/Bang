using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Networking;

public class TapRace : NormalMiniGame
{

    Coroutine race;
    public GameObject mapPrefab;
    public GameObject mapPrefabGroup;
    GameObject map;
    //public List<GameObject> startingPositions; // startingPositions[0] = pos du joueur
    GameObject[] startingPos;
    public List<GameObject> startingPositionsGroup; // startingPositions[1] = pos du joueur

    public GameObject playerPrefab;
    int indexPlayerOb;
    public float raceEndPos;
    float raceStartPos;
    public int nbrOfTouchToWin;
    float lenghtOfRace;

  
    public override void StartMiniGame()
    {
        DuelManager.instance.player.tapRaceNbr = 0;
        DuelManager.instance.player.CmdUpdateTapRaceNbr(0);
        DisplayRace(true);
        race = StartCoroutine(Race());
    }
    public override void ClearMiniGame()
    {
        if (race != null)
        {
            StopCoroutine(race);
            race = null;
        }
        DisplayRace(false);
    }
    public override void LoadObjects()
    {
     
        if (!DuelManager.instance.groupPlay)
        {
            map = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
            SetStartPos(map);
            indexPlayerOb = (DuelManager.instance.player.playerForMG.Count);

            foreach (Player p in DuelManager.instance.players)
            {
                p.playerForMG.Add(Instantiate(playerPrefab, Vector3.zero, Quaternion.identity));
                p.playerForMG[indexPlayerOb].GetComponent<SpriteRenderer>().color = p.color;
                p.playerForMG[indexPlayerOb].transform.parent = p.transform;
            }
            DuelManager.instance.player.playerForMG[indexPlayerOb].transform.position = startingPos[0].transform.position;
            if (DuelManager.instance.players.Length > 1)                                                                                                                //ENLEVER CA CEST JUSTE POUR TESTER
            {
                int index;
                if (DuelManager.instance.player.index == 0)
                {
                    index = 1;
                }
                else
                {
                    index = 0;
                }
                DuelManager.instance.players[index].playerForMG[indexPlayerOb].transform.position = startingPos[1].transform.position;  //CEST CA QUI BUG
                //DuelManager.instance.players[(DuelManager.instance.player.index + 1) % 2].playerForMG[indexPlayerOb].transform.position = startingPos[1].transform.position;
            }
        }

        else
        {
            map = Instantiate(mapPrefabGroup, Vector3.zero, Quaternion.identity);
            SetStartPos(map);
            indexPlayerOb = (DuelManager.instance.player.playerForMG.Count);

            if (ServerDuel.instance.livingPlayers.Count > 2)           
            {
               for (int i = -1; i <= 1; i++)
                {
                    Player p = ServerDuel.instance.livingPlayers[DuelManager.instance.player.index + i]; // ca ca marche pas parce que tu peux pas sync var la liste
                    p.playerForMG.Add(Instantiate(playerPrefab, Vector3.zero, Quaternion.identity));
                    p.playerForMG[indexPlayerOb].GetComponent<SpriteRenderer>().color = p.color;
                    p.playerForMG[indexPlayerOb].transform.parent = p.transform;
                    DuelManager.instance.player.playerForMG[indexPlayerOb].transform.position = startingPos[i +1].transform.position;
                }
            }
            else         //au cas ou cest fucked up
            {
                foreach (Player p in DuelManager.instance.players)
                {
                    p.playerForMG.Add(Instantiate(playerPrefab, Vector3.zero, Quaternion.identity));
                    p.playerForMG[indexPlayerOb].GetComponent<SpriteRenderer>().color = p.color;
                    p.playerForMG[indexPlayerOb].transform.parent = p.transform;
                   
                }
                DuelManager.instance.player.playerForMG[indexPlayerOb].transform.position = startingPos[0].transform.position;
                DuelManager.instance.players[(DuelManager.instance.player.index + 1) % 2].playerForMG[indexPlayerOb].transform.position = startingPos[1].transform.position;
            }
        }
    }
   
    void SetStartPos(GameObject holder)
    {
        startingPos = TransformExtensions.FindObjectsWithTag(holder.transform, "Spawn").ToArray();
        raceStartPos = startingPos[0].transform.position.y;
        lenghtOfRace = raceEndPos - raceStartPos;
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

    IEnumerator Race()
    {
        Transform playerTransform = DuelManager.instance.player.playerForMG[indexPlayerOb].transform;
        Player player = DuelManager.instance.player;
        while (playerTransform.position.y < raceEndPos)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    player.tapRaceNbr += 1;
                    player.CmdUpdateTapRaceNbr(player.tapRaceNbr);
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                player.tapRaceNbr += 1;
                player.CmdUpdateTapRaceNbr(player.tapRaceNbr);
            }
            UpdatePlayersY();
            yield return null;
        }
        DisplayRace(false);
        DuelManager.instance.MiniGameFinished();
    }
    public void DisplayRace(bool t)
    {
        foreach (Player p in DuelManager.instance.players)
        {
            p.playerForMG[indexPlayerOb].GetComponent<SpriteRenderer>().enabled = t;
           // p.playerForMG[indexPlayerOb].GetComponent<NetworkTransform>().enabled = t;
        }
        map.SetActive(t);
    }
    void UpdatePlayersY()
    {
        foreach(Player p in DuelManager.instance.players)
        {
            p.playerForMG[indexPlayerOb].transform.position = new Vector3(p.playerForMG[indexPlayerOb].transform.position.x, NbrTapToPosY(p.tapRaceNbr), 0);
        }
    }

    float NbrTapToPosY(int nbr)
    {
        return  (lenghtOfRace *nbr / nbrOfTouchToWin) + raceStartPos;
    }
}
