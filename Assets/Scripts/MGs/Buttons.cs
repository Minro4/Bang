using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buttons : NormalMiniGame
{

    List<GameObject> buttonsInScene;
    List<GameObject> buttonList = new List<GameObject>();
    public GameObject buttonHolderPrefab;
    GameObject buttonHolder;
    int nextButtonToPress = 0;

    public override void LoadObjects()
    {
        nextButtonToPress = 0;
        buttonList.Clear();
        buttonHolder = Instantiate(buttonHolderPrefab, Vector3.zero, Quaternion.identity);      
        SetButtons();
    }
    public override void UnloadObjects()
    {
        Destroy(buttonHolder);
    }
    public void SetButtons()
    {
        buttonsInScene = TransformExtensions.FindObjectsWithTag(buttonHolder.transform, "Button");
        int iMax = buttonsInScene.Count;
        for (int i = 0; i < iMax; i++)
        {
            int rng = Random.Range(0, buttonsInScene.Count - 1);
            buttonList.Add(buttonsInScene[rng]);
            buttonsInScene[rng].GetComponentInChildren<Text>().text = (i + 1).ToString();
            //buttonsInScene[rng].SetActive(true);
            buttonsInScene.RemoveAt(rng);
        }

    }
    public override void ResetGame()
    {
        buttonList.Clear();
        SetButtons();
        foreach (GameObject go in buttonList)
        {
            go.SetActive(true);
        }
    }
    public override void ClearMiniGame()
    {
        for (int i = nextButtonToPress; i < buttonList.Count; i++)
        {
            buttonList[i].SetActive(false);
        }
        buttonHolder.SetActive(false);
    }
    public override void StartMiniGame()
    {
        buttonHolder.SetActive(true);
    }

    public void ButtonPressed(GameObject button)            //Need to set buttons in scene to do this function!
    {
        if (button == buttonList[nextButtonToPress])
        {
            button.SetActive(false);
            nextButtonToPress += 1;
            if (nextButtonToPress == buttonList.Count)
            {
                DuelManager.instance.MiniGameFinished();
             //   race = StartCoroutine(Race());

            }
        }
        else
        {
            Debug.Log("Wrong Button");
        }
    }
    public override void SetPlayerOb(int index)
    {
    }

    //List<GameObject> ArrayToList(GameObject[] array)
    //{
    //    List<GameObject> l = new List<GameObject>();
    //    foreach( GameObject a in array)
    //    {
    //        l.Add(a);
    //    }
    //    return l;
    //} 
}
