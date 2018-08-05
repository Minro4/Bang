using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using UnityEngine.Networking.Match;
using System;

public class MatchList : MonoBehaviour {

    [SerializeField]
    private JoinButton joinButtonPrefab;
	void OnEnable () {
        AvailableMatchesList.OnAvailableMatchesChanged += AvailableMatchesList_OnAvailableMatchesChanged;
	}

    private void AvailableMatchesList_OnAvailableMatchesChanged(List<LanConnectionInfo> matches)
    {
        ClearExistingButtons();
        CreateNewJoinGameButtons(matches);
    }
    private void ClearExistingButtons()
    {
        var buttons = GetComponentsInChildren  < JoinButton > ();
        foreach(var button in buttons)
        {
            Destroy(button.gameObject);
        }
    }
    private void CreateNewJoinGameButtons(List<LanConnectionInfo> matches)
    {
       foreach(var match in matches)
        {
            var button = Instantiate(joinButtonPrefab);
            button.Initialize(match, transform);
        }
    }
}
