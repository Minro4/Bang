using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using Assets.Scripts;
using UnityEngine.UI;

public class BNetworkManager : NetworkLobbyManager {

    public BNetworkDiscovery discovery;
    private float nextRefreshTime = 0;
    const float refreshDelay = 2f;

    static public BNetworkManager instance;

    [Space]
    [Header("UI Reference")]
    public RectTransform mainMenuPanel;
    public RectTransform lobbyPanel;

    protected RectTransform currentPanel;

    public Button backButton;

    public Text statusInfo;
    public Text hostInfo;

    //Client numPlayers from NetworkManager is always 0, so we count (throught connect/destroy in LobbyPlayer) the number
    //of players, so that even client know how many player there is.
    [HideInInspector]
    public int _playerNumber = 0;
    void Start()
    {
        instance = this;
        currentPanel = mainMenuPanel;

        backButton.gameObject.SetActive(false);
        //GetComponent<Canvas>().enabled = true;

        DontDestroyOnLoad(gameObject);
    }
    public void StartHosting()
    {
        ChangeTo(lobbyPanel);
        Debug.Log("StartHosting");
        NetworkServer.Reset();
        base.StartHost();
    }
    public override void OnStartHost()
    {
        Debug.Log("OnStartHosting");
        discovery.StartBroadcast();
     
    }
    private void Update()
    {
        if (Time.time >= nextRefreshTime)
            RefreshMatches();
    }

    private void RefreshMatches()
    {
        nextRefreshTime = Time.time + refreshDelay;

    }

    public override void OnStopClient()
    {
        discovery.StopBroadcast();
    }

    private void OnMatchCreated(bool success, string extendedInfo, MatchInfo responseData)
    {
        base.StartHost(responseData);
    }

    internal void JoinMatch(LanConnectionInfo match)
    {
        discovery.StopDiscovery();
        if (BNetworkManager.singleton.client == null)
        {
            Debug.Log("StartClient");
            ChangeTo(lobbyPanel);
            BNetworkManager.singleton.networkAddress = match.ipAddress;
            BNetworkManager.singleton.StartClient();
        }
    }

    public void ChangeTo(RectTransform newPanel)
    {
        if (currentPanel != null)
        {
            currentPanel.gameObject.SetActive(false);
        }
        if (newPanel != null)
        {
            newPanel.gameObject.SetActive(true);
        }
        currentPanel = newPanel;

        if (currentPanel != mainMenuPanel)
        {
            backButton.gameObject.SetActive(true);
        }
        else
        {
            backButton.gameObject.SetActive(false);
            BNetworkManager.singleton.StopClient();          
            discovery.StartClient();          
        }
    }

    public void BackButton()
    {
        if (currentPanel == lobbyPanel)
        {         
            ChangeTo(mainMenuPanel);
        }
    }
    
    #region matchmaking

    //private float nextRefreshTime = 0;
    //const float refreshDelay = 2f;

    //public void StartHosting()
    //{
    //    StartMatchMaker();
    //    matchMaker.CreateMatch("Bob", 6, true, "", "", "", 0, 0, OnMatchCreated); ;
    //}


    //private void OnMatchCreated(bool success, string extendedInfo, MatchInfo responseData)
    //{
    //    base.StartHost(responseData);
    //}

    //private void Update()
    //{
    //    if (Time.time >= nextRefreshTime)
    //        RefreshMatches();
    //}

    //private void RefreshMatches()
    //{
    //    nextRefreshTime = Time.time + refreshDelay;
    //    if (matchMaker == null)
    //    {
    //        StartMatchMaker();
    //    }
    //    matchMaker.ListMatches(0, 10, "", true, 0, 0, HandleListMatchesComplete);
    //}

    //internal void JoinMatch(MatchInfoSnapshot match)
    //{
    //    if (matchMaker == null)
    //    {
    //        StartMatchMaker();
    //    }
    //    matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, HandleJoinedMatch);
    //}

    //private void HandleJoinedMatch(bool success, string extendedInfo, MatchInfo responseData)
    //{
    //    StartClient(responseData);
    //}

    //private void HandleListMatchesComplete(bool success, string extendedInfo, List<MatchInfoSnapshot> responseData)
    //{
    //    AvailableMatchesList.HandleNewMatchList(responseData);
    //}
    #endregion
}
