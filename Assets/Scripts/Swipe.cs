using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swipe : MonoBehaviour
{

    public bool tap;
    bool isDragging = false;
    Vector2 startTouch, swipeDelta;

    public Vector2 wheelCenter;

    private void Update()
    {
        tap = false;


        #region Mobile Inputs
        if (Input.touchCount > 0)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                tap = true;
                isDragging = true;
                startTouch = Input.touches[0].position;
            }
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
            {
                isDragging = false;
                Reset();
            }
        }

        #endregion
        #region Standalone Inputs
        if (Input.GetMouseButtonDown(0))
        {
            tap = true;
            isDragging = true;
            startTouch = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Reset();
            isDragging = false;
        }

        #endregion

        #region Manage Dragging
        if (isDragging)
        {
            Vector2 endTouch = Vector2.zero;
            swipeDelta = Vector2.zero;

            if (Input.touchCount > 0)
            {
                endTouch = Input.touches[0].position;
                swipeDelta = Input.touches[0].position - startTouch;
            }
            else if (Input.GetMouseButton(0))
            {
                endTouch = (Vector2)Input.mousePosition;
            }
            swipeDelta = endTouch - startTouch;

            if (swipeDelta.magnitude > 5)
            {
                Vector2 wToStartTouch = startTouch - wheelCenter;
                Vector2 wToEndTouch = endTouch - wheelCenter;
                bool clockwise = false;

                Vector2 perpendiculaire = new Vector2(-wToStartTouch.y, wToStartTouch.x);
                Vector2 projection = Vector3.Project(swipeDelta, perpendiculaire);

                if (Vector2.SignedAngle(wToStartTouch, wToEndTouch) > 0)
                {
                    Debug.Log(Vector2.SignedAngle(wToStartTouch, wToEndTouch));
                    clockwise = true;
                }
                RotateWheel(projection.magnitude, clockwise);
                startTouch = Input.touches[0].position;
            }
            
        }
#endregion
    }
    private void Reset()
    {
        startTouch = swipeDelta = Vector2.zero;
    }
    void RotateWheel(float amount, bool clockwise)
    {

    }
}
