using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BNetworkDiscoveryClient : NetworkDiscovery {

    private float timeout = 3f;
    private Dictionary<LanConnectionInfo, float> lanAddresses = new Dictionary<LanConnectionInfo, float>();
    void Awake()
    {
        base.Initialize();
        base.StartAsClient();
        StartCoroutine(CleanupExpiredEntries());
    }
     
    public void StartBroadcast()
    {
        StopBroadcast();
        base.Initialize();
        base.StartAsServer();
        Debug.Log("startboradcast");
    }
    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        Debug.Log("Receive Broadcast");

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


