using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HostButton : MonoBehaviour {

private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(FindObjectOfType<BNetworkManager>().StartHosting);
    }
}
