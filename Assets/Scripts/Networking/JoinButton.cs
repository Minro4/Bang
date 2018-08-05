using UnityEngine;
using TMPro;
using UnityEngine.Networking.Match;
using Prototype.NetworkLobby;

public class JoinButton : MonoBehaviour {

    private TextMeshProUGUI buttonText;
    private LanConnectionInfo match;
    private void Awake()
    {
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }
    public void Initialize(LanConnectionInfo match, Transform panelTransform)
    {
        this.match = match;
        buttonText.text = match.name;
        transform.SetParent(panelTransform);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
    }

    public void JoinMatch()
    {
        FindObjectOfType<LobbyMainMenu>().OnClickJoin(match);
    }
}
