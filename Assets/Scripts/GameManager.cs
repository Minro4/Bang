using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public static GameManager instance;
    public enum Mode {Duel,Group };
    public Mode mode = Mode.Duel;

	// Use this for initialization
	void Start () {
        if(instance == null)
        {
            instance = this;
        }
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void StartGameDuel()
    {
        instance.mode = Mode.Duel;
        SceneManager.LoadScene(1);
        //SceneManager.LoadScene("Duel", LoadSceneMode.Single);
    }
    public void StartGameGroup()
    {
        instance.mode = Mode.Group;
        SceneManager.LoadScene(1);
        //SceneManager.LoadScene("Duel", LoadSceneMode.Single);
    }
    public void BackToMenu()
    {
       // if(!DuelManager.instance.isPlaying)
        SceneManager.LoadScene("SceneMenu", LoadSceneMode.Single);
    }
}
