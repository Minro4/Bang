using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPress : MonoBehaviour {

public void CallDuelManagerButtonPressed()
    {
        DuelManager.instance.GetComponent<Buttons>().ButtonPressed(this.gameObject);
    }
}
