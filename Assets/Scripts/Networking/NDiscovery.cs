using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts;
using System;

public class NDiscovery : NetworkDiscovery {

    private float timeout = 3f;
    private Dictionary<LanConnectionInfo, float> lanAddresses = new Dictionary<LanConnectionInfo, float>();

    public static NDiscovery instance;

    void Start()
    {
        instance = this;

        base.Initialize();
        base.StartAsClient();
        StartCoroutine(CleanupExpiredEntries());

    }
    public void StartBroadcast()
    {
        StopBroadcast();
        base.Initialize();
        base.StartAsServer();
        Debug.Log("Start Broadcast");
    }
    public void RestartAsClient()
    {
        base.StopBroadcast();
        base.Initialize();
        base.StartAsClient();
        Debug.Log("Restart as Client");
    }
    public override void OnReceivedBroadcast(string fromAddress, string data)
    {

        base.OnReceivedBroadcast(fromAddress, data);

        LanConnectionInfo info = new LanConnectionInfo(fromAddress, data);

        if (!lanAddresses.ContainsKey(info))
        {
            lanAddresses.Add(info, Time.time + timeout);
            UpdateMatchInfos();
        }
        else
        {
            lanAddresses[info] = Time.time + timeout;
        }
    }

    internal void StopDiscovery()
    {
        base.StopBroadcast();
    }

    private IEnumerator CleanupExpiredEntries()
    {
        while (true)
        {
            bool changed = false;
            var keys = lanAddresses.Keys;
            foreach (var key in keys)
            {
                if (lanAddresses[key] <= Time.time)
                {
                    lanAddresses.Remove(key);
                    changed = true;
                }
            }
            if (changed)
                UpdateMatchInfos();

            yield return new WaitForSeconds(timeout);
        }
    }
    private void UpdateMatchInfos()
    {
        List<LanConnectionInfo> keyList = new List<LanConnectionInfo>(this.lanAddresses.Keys);
        AvailableMatchesList.HandleNewMatchList(keyList);
    }
}

//public struct LanConnectionInfo
//{
//    public string ipAddress;
//    public int port;
//    public string name;

//    public LanConnectionInfo(string fromAddress, string data)
//    {
//        ipAddress = fromAddress.Substring(fromAddress.LastIndexOf(":") + 1, fromAddress.Length - (fromAddress.LastIndexOf(":") + 1));
//        string portText = data.Substring(data.LastIndexOf(":") + 1, data.Length - (data.LastIndexOf(":") + 1));
//        int.TryParse(portText, out port);
//        name = "local";
//    }
//}
