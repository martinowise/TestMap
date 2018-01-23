using System;
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

    float myMaxScale;
    float myMinScale;
    bool limitActive;
    Vector3 sLimit;
    Vector3 eLimit;

    private void Start() {
        myMaxScale = maxScale;
        myMinScale = minScale;
    }

    void Update()
    {
        // Mausbedienung
        Vector2 middle_m = new Vector2(Input.mousePosition[0], Input.mousePosition[1]);
        Vector2 pointOnMap_m = GetMapCoordinates(content, middle_m);
        Zoom(Input.GetAxis("Mouse ScrollWheel"), pointOnMap_m); //dividiere input für bessere interpolation

        //----------------------------------------------------------------------------------------------------------------------//
        //----------------------------------------------------------------------------------------------------------------------//

        //Touchbedienung
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

        //----------------------------------------------------------------------------------------------------------------------//
        //----------------------------------------------------------------------------------------------------------------------//
        
        //Limit
        if(limitActive)
        {
            //beschränkt horizontal
            if (content.GetComponent<RectTransform>().anchoredPosition.x > sLimit.x * -1 * content.GetComponent<RectTransform>().localScale.x)
            {
                content.GetComponent<RectTransform>().anchoredPosition = new Vector2 (sLimit.x * -1 * content.GetComponent<RectTransform>().localScale.x, content.GetComponent<RectTransform>().anchoredPosition.y);
            }

            if (content.GetComponent<RectTransform>().anchoredPosition.x < eLimit.x * -1 * content.GetComponent<RectTransform>().localScale.x)
            {
                content.GetComponent<RectTransform>().anchoredPosition = new Vector2(eLimit.x * -1 * content.GetComponent<RectTransform>().localScale.x, content.GetComponent<RectTransform>().anchoredPosition.y);
            }

            //beschränkt vertikal
            if (content.GetComponent<RectTransform>().anchoredPosition.y < sLimit.y * -1 * content.GetComponent<RectTransform>().localScale.y)
            {
                content.GetComponent<RectTransform>().anchoredPosition = new Vector2(content.GetComponent<RectTransform>().anchoredPosition.x, sLimit.y * -1 * content.GetComponent<RectTransform>().localScale.x);
            }

            if (content.GetComponent<RectTransform>().anchoredPosition.y > eLimit.y * -1 * content.GetComponent<RectTransform>().localScale.y)
            {
                content.GetComponent<RectTransform>().anchoredPosition = new Vector2(content.GetComponent<RectTransform>().anchoredPosition.x, eLimit.y * -1 * content.GetComponent<RectTransform>().localScale.x);
            }
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

    //----------------------------------------------------------------------------------------------------------------------//
    // center test
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
        content.GetComponent<RectTransform>().anchoredPosition = -pos * content.GetComponent<RectTransform>().localScale.x; //geankerte position des contents bewegt sich obj-koordinaten entgegen
    }

    //----------------------------------------------------------------------------------------------------------------------//
    // limit test
    int ccc = 1;
    public void LimitMapForTesting()
    {
        // gelöst über vektoren. wichtig: die image-dummies in der szene sind nicht ausgangspunkt für koordinaten! (gizmos verwenden?)
        if (ccc == 1)
        {
            LimitMap(new Vector3(0, 0, 0), new Vector3(300, -200, 0));
        }
        if (ccc == 2)
        {
            LimitMap(new Vector3(0, 0, 0), new Vector3(600, -400, 0));
        }
        if (ccc == 3)
        {
            LimitMap(new Vector3(0, 0, 0), new Vector3(900, -600, 0));
        }
        if (ccc == 4)
        {
            LimitMap(new Vector3(0, 0, 0), new Vector3(1200, -800, 0));
        }
        ccc++;
        if (ccc > 4) ccc = 1;
        // LimitMap(int minXinPercentOfMap, int maxXInPercentOfMap, int minYinPercentOfMap, int  maxYInPercentOfMap)
    }

    /* alt
    public void LimitMap(float minXinPercentOfMap, float maxXInPercentOfMap, float minYinPercentOfMap, float maxYInPercentOfMap)
    {
        // todo.. auch hier ist noch nicht klar, in welchen coodinaten das sein soll..

        // LimitMap(int minXinPercentOfMap, int maxXInPercentOfMap, int minYinPercentOfMap, int  maxYInPercentOfMap)

    }
    */

    public void LimitMap(Vector3 sPos, Vector3 ePos) {
        //die position wird berechnet aus dem durchschnitt zweier vektoren, vektoren treffen sich in der mitte (funktioniert nur für quadrate weil: Mathf.sqrt(2))
        RectTransform rect1 = content.GetComponent<RectTransform>();
        RectTransform rect2 = viewport.GetComponent<RectTransform>();
        Vector3 pos = sPos + ePos;

        float dist = Vector3.Distance(sPos, ePos);
        float myDist = dist;
        float scale = 0;

        //verkleinern
        if (rect2.rect.width * Mathf.Sqrt(2) < dist)
        {
            for (int i = 0; rect2.rect.width * Mathf.Sqrt(2) < dist; i++)
            {
                if (1 - 0.1f * i >= myMinScale)
                {
                    dist = myDist;
                    scale = 1 - .1f * i;

                    dist = dist * scale;
                    rect1.localScale = new Vector3(scale, scale, scale);
                    minScale = scale - .05f; //fix, damit man auch wirklich auf die gewünschte skalierung kommt
                }
                else { break; }
            }
        }
        //vergrößern
        else
        {
            for (int i = 0; rect2.rect.width * Mathf.Sqrt(2) > dist; i++)
            {
                if (1 + 0.1f * i <= myMaxScale)
                {
                    dist = myDist;
                    scale = 1 + .1f * i;

                    dist = dist * scale;
                    rect1.localScale = new Vector3(scale, scale, scale);
                    minScale = scale - .05f; //fix, damit man auch wirklich auf die gewünschte skalierung kommt
                }
                else { break; }
            }
        }

        pos = new Vector3(pos[0] / 2, pos[1] / 2, 0);
        rect1.anchoredPosition = -pos * rect1.localScale.x;

        sLimit = sPos;
        eLimit = ePos;
        limitActive = true;
    }

    public void ClearLimit() {
        limitActive = false;
        maxScale = myMaxScale;
        minScale = myMinScale;
    }

    /* alt
    public void ClearLimit()
    {
        LimitMap(0, 0, 100f, 100f);

    }
    */
}
