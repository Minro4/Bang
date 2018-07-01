using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MiniGame : MonoBehaviour {


    public abstract void StartMiniGame();
    public abstract void ClearMiniGame();
    public abstract void LoadObjects();
    public abstract void UnloadObjects();


}
