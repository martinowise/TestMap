﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MyMapContentScrollerZoomer : MonoBehaviour
{

    public GameObject scrollRect;  //scrollrect, parent of viewport
    public GameObject viewport;  // viewport, parent of content
    public GameObject content;  // content

    public float maxScale;
    public float minScale;
    public float zoomSpeed;

    public GameObject testPlanet1;
    public GameObject testPlanet2;
    public GameObject testPlanet3;

    void Update()
    {
        // Mausbedienung
        Vector2 middle_m = new Vector2(Input.mousePosition[0], Input.mousePosition[1]);
        Vector2 pointOnMap_m = GetMapCoordinates(content, middle_m);
        Zoom(Input.GetAxis("Mouse ScrollWheel"), pointOnMap_m); //dividiere input für bessere interpolation

        //----------------------------------------------------------------------------------------------------------------------//
        //----------------------------------------------------------------------------------------------------------------------//

        // If there are two touches on the device...
        if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
        {
            scrollRect.GetComponent<ScrollRect>().enabled = false;

            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
            
            Vector2 middle_t = new Vector2((touchZeroPrevPos.x + touchOnePrevPos.x) / 2, (touchZeroPrevPos.y + touchOnePrevPos.y) / 2);
            Vector2 pointOnMap_t = GetMapCoordinates(content, middle_t);

            Zoom(-1 * deltaMagnitudeDiff * zoomSpeed, pointOnMap_t);
        }

        //damit zoom mit touch nicht zittert wird scrollrect deaktiviert bis die finger den bildschirm verlassen
        if ((Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended) || (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Ended && Input.GetTouch(1).phase == TouchPhase.Ended))
        {
            scrollRect.GetComponent<ScrollRect>().enabled = true;
        }
    }
    
    private Vector2 GetMapCoordinates(GameObject content, Vector2 zoomPos)
        {
        Vector2 localCursor;
        RectTransform rect1 = content.GetComponent<RectTransform>();

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect1, zoomPos, null, out localCursor))
        { localCursor = new Vector2(0, 0); }
        
        return localCursor;
    }

    void Zoom(float increment, Vector2 fixpointOnMap)
    {
        Vector3 currentScale = content.GetComponent<RectTransform>().localScale;
        float oldScale = currentScale.x;

        currentScale.x += increment;
        currentScale.y += increment;

        if (currentScale.x >= maxScale || currentScale.x <= minScale)
        { return; }

        //https://stackoverflow.com/questions/2916081/zoom-in-on-a-point-using-scale-and-translate

        float newScale = currentScale.x;

        float dX = fixpointOnMap.x * (newScale - oldScale);
        float dY = fixpointOnMap.y * (newScale - oldScale);

        Vector3 p = content.GetComponent<RectTransform>().position;
        Vector3 newPosition = new Vector3(p.x - dX, p.y - dY, p.z);

        // das hier kann sicherlich in eine coroutine, und schrittweise in kleinen änderungen, sonst nicht smooth..
        // also zb 5 schritte 
        content.GetComponent<RectTransform>().localScale = currentScale;
        content.GetComponent<RectTransform>().position = newPosition;
    }


    // zum testen..
    int cc = 0;
    public void CenterObjectForTesting()
    {
        cc++;
        // put this object in the center of the screen..
        GameObject o = testPlanet1;
        if (cc == 2) o = testPlanet2;
        if (cc == 3) o = testPlanet3;

        if (cc > 3) cc = 1;
        CenterObject(o);
    }

    public void CenterObject(GameObject obj) {
        Vector2 pos = obj.GetComponent<RectTransform>().localPosition; // position des objekts
        content.GetComponent<RectTransform>().anchoredPosition = -pos; //geankerte position des contents bewegt sich obj-koordinaten entgegen
    }

    //----------------------------------------------------------------------------------------------------------------------//

    public int koorSize;
    float sectorSize;

    int ccc = 1;
    public void LimitMapForTesting()
    {
        // todo.. auch hier ist noch nicht klar, in welchen coodinaten das sein soll...
        if (ccc == 1)
        {
            LimitMap(40,40,60,60); return;
        }
        if (ccc == 2)
        {
            LimitMap(30, 30, 70, 70); return;
        }

        if (ccc == 3)
        {
            LimitMap(10, 10, 90, 90); return;
        }
        if (ccc == 3)
        {
            ClearLimit(); return;
        }
        ccc++;
        if (ccc > 4) ccc = 1;
        // LimitMap(int minXinPercentOfMap, int maxXInPercentOfMap, int minYinPercentOfMap, int  maxYInPercentOfMap)
    }

    public void LimitMap(float minXinPercentOfMap, float maxXInPercentOfMap, float minYinPercentOfMap, float maxYInPercentOfMap)
    {
        // todo.. auch hier ist noch nicht klar, in welchen coodinaten das sein soll..

        // LimitMap(int minXinPercentOfMap, int maxXInPercentOfMap, int minYinPercentOfMap, int  maxYInPercentOfMap)

    }

    public void ClearLimit()
    {
        LimitMap(0, 0, 100f, 100f);

    }

}
