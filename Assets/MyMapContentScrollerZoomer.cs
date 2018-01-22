using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MyMapContentScrollerZoomer : MonoBehaviour
{

    public GameObject container;  // its a panel to scale
    public GameObject mapScrollRect;  // ists the scroll rect container
    public GameObject viewPort;  // ists the scroll rect container
    public float maxScale;
    public float minScale;
    public float zoomSpeed;
    public float resetDuration;
    public GameObject debugPrinter;

    public GameObject testPlanet1;
    public GameObject testPlanet2;
    public GameObject testPlanet3;
    
    private void Start()
    {
        
    }

    void Update()
    {
        // Mausbedienung

        Zoom(Input.GetAxis("Mouse ScrollWheel")); // zoom mit fixpunkt mitte des screens

        //    If there are two touches on the device...
        if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
        {
            mapScrollRect.GetComponent<ScrollRect>().enabled = false;
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

            //  if (currentScale.x != maxScale || currentScale.x != minScale)
            {
                Vector2 middle = new Vector2((touchZeroPrevPos.x + touchOnePrevPos.x) / 2, (touchZeroPrevPos.y + touchOnePrevPos.y) / 2);
                Vector2 pointOnMap = GetMapCoordinates(container, middle);
                Zoom(-1 * deltaMagnitudeDiff * zoomSpeed, pointOnMap);
            }
        }

        if ((Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended) || (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Ended && Input.GetTouch(1).phase == TouchPhase.Ended))
        {
            mapScrollRect.GetComponent<ScrollRect>().enabled = true;
        }
        //if (Input.GetMouseButton(0))
        //{
        //    Debug.Log(Input.mousePosition);
        //    // die mitte des screens in weltcoodinaten soll nach zoom wieder dort sein, bzw.  bei touch der mittelpunkt der geste..
        //    var rect1 = viewPort.GetComponent<RectTransform>();
        //     // dieser punkt soll nachher wieder in der mitte des screens liegen!
        //    Vector2 fixPoint = new Vector2(rect1.rect.width/2, rect1.rect.height / 2);
        //    Vector2 v4 = GetMapCoordinates(container, fixPoint);

        //}

    }

    private Vector2 GetMapCoordinates(GameObject map, Vector2 v2)
    {
        // tod: wahrschenlich richtig, aber ich weiss es nicht genau..
        Vector2 localCursor;
        var rect1 = map.GetComponent<RectTransform>();
        // var pos1 = dat.position;


        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect1, v2,
            null, out localCursor))
            localCursor = new Vector2(0, 0);

        float px = localCursor.x / rect1.rect.width;
        float py = localCursor.y / rect1.rect.height;


        return localCursor;

    }
    
    void Zoom(float increment)
    {
        // todo.. das stimmt wohl nicht ganz..
        // middle of screen as fix point..
        var rect1 = viewPort.GetComponent<RectTransform>(); // screen
        // keine ahnung, ob das wirklich stimmt..
        Vector2 centerOfScreen = new Vector2(rect1.rect.left + (rect1.rect.width / 2), rect1.rect.bottom + (rect1.rect.height / 2));
        Vector2 centerOfScreenOnMap = GetMapCoordinates(container, centerOfScreen);

        if (debugPrinter != null)
            debugPrinter.GetComponent<Text>().text = centerOfScreenOnMap.ToString("0") + "," + centerOfScreenOnMap.ToString("0");

        Zoom(increment, centerOfScreenOnMap);
    }
    
    void Zoom(float increment, Vector2 fixpointOnMap)
    {
        // todo.. das stimmt wohl nicht ganz..

        Vector3 currentScale = container.GetComponent<RectTransform>().localScale;
        float oldScale = currentScale.x;


        currentScale.x += increment;
        currentScale.y += increment;
        if (currentScale.x >= maxScale || currentScale.x <= minScale)
        {
            return;
        }


        //https://stackoverflow.com/questions/2916081/zoom-in-on-a-point-using-scale-and-translate

        float newScale = currentScale.x;

        float dX = fixpointOnMap.x * (newScale - oldScale);
        float dY = fixpointOnMap.y * (newScale - oldScale);

        //if (debugPrinter != null)
        //    debugPrinter.GetComponent<Text>().text = dX.ToString("0") + "->" + dY.ToString("0");

        Vector3 p = container.GetComponent<RectTransform>().position;
        Vector3 newPosition = new Vector3(p.x - dX, p.y - dY, p.z);

        // das hier kann sicherlich in eine coroutine, und schrittweise in kleinen änderungen, sonst nicht smooth..
        // also zb 5 schritte 
        container.GetComponent<RectTransform>().localScale = currentScale;
        container.GetComponent<RectTransform>().position = newPosition;


    }

    public void ZoomPlus()
    {
        Zoom(0.1f);
    }

    public void ZoomMinus()
    {
        Zoom(-0.1f);
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

    int ccc = 1;
    public void LimitMapForTesting()
    {
        // todo.. auch hier ist noch nicht klar, in welchen coodinaten das sein soll..
        if (ccc == 1)
        {
            LimitMap(40, 40, 60, 60); return;
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
    
    public void CenterObject(GameObject obj)
    {

        // todo..
        // put this object in the center of the screen..
        // so .. wie machen wir das?
        Vector2 pos = obj.GetComponent<RectTransform>().localPosition; // ist bereits auf der map..

        var rect1 = viewPort.GetComponent<RectTransform>(); // screen
        // keine ahnung, ob das wirklich stimmt..
        Vector2 centerOfScreen = new Vector2(rect1.rect.left + (rect1.rect.width / 2), rect1.rect.bottom + (rect1.rect.height / 2));
        Vector2 centerOfScreenOnMap = GetMapCoordinates(container, centerOfScreen);

        Vector2 dif = pos - centerOfScreenOnMap;

        Vector3 p = container.GetComponent<RectTransform>().position;
        Vector3 newPosition = new Vector3(p.x - dif.x, p.y - dif.y, p.z);

        container.GetComponent<RectTransform>().position = newPosition;


    }

}
