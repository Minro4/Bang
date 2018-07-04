using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swipe : MonoBehaviour {

    public bool tap, swipe;
    Vector2 startTouch, swipeDelta;

    private void Update()
    {
        tap = swipe = false;


        #region Mobile Inputs
        if (Input.touchCount > 0)
        {
            if (Input.touchCount[0].phase == TouchPhase.Began)
            {
                tap = true;
                startTouch = Input.touches[0].position;
            }
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
        }

            #endregion




            #region Standalone Inputs
            if (Input.GetMouseButtonDown(0))
        {
            tap = true;
            startTouch = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Reset();
        }

#endregion 
    }
    private void Reset()
    {
        startTouch = swipeDelta = Vector2.zero;
    }
}
