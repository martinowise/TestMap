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

    RectTransform contentRect; //schenllerer zugriff auf content.GetComponent<RectTransform>()

    public float maxScale; //dichtester zustand beim zoomen
    public float minScale; //entferntester zustand beim zoomen
    public float zoomSpeed; //geschwindigkeit des zooms

    public GameObject testPlanet1;
    public GameObject testPlanet2;
    public GameObject testPlanet3;

    public bool clampParallaxX; //toggle für unterschiedliche modi in ParallaxScrollingScript
    public bool clampParallaxY; //toggle für unterschiedliche modi in ParallaxScrollingScript

    bool limitActive; //toggle für LimitMap
    float myMaxScale; //zwischenspeichern der original maxScale für ClearLimit
    float myMinScale; //zwischenspeichern der original minScale für ClearLimit

    Vector3 sLimit; //schnittstelle zwischen LimitMap und Update (anfang limit)
    Vector3 eLimit; //schnittstelle zwischen LimitMap und Update (ende limit)

    //----------------------------------------------------------------------------------------------------------------------//

    private void Start() {
        contentRect = content.GetComponent<RectTransform>();
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
            //beschränkt horizontal + kollisionstoggle an für x
            if (contentRect.anchoredPosition.x > sLimit.x * -1 * contentRect.localScale.x)
            {
                contentRect.anchoredPosition = new Vector2 (sLimit.x * -1 * contentRect.localScale.x, contentRect.anchoredPosition.y);
                clampParallaxX = true;
            }

            if (contentRect.anchoredPosition.x < eLimit.x * -1 * contentRect.localScale.x)
            {
                contentRect.anchoredPosition = new Vector2(eLimit.x * -1 * contentRect.localScale.x, contentRect.anchoredPosition.y);
                clampParallaxX = true;
            }

            //beschränkt vertikal + kollisionstoggle an für y
            if (contentRect.anchoredPosition.y < sLimit.y * -1 * contentRect.localScale.y)
            {
                contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, sLimit.y * -1 * contentRect.localScale.x);
                clampParallaxY = true;
            }

            if (contentRect.anchoredPosition.y > eLimit.y * -1 * contentRect.localScale.y)
            {
                contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, eLimit.y * -1 * contentRect.localScale.x);
                clampParallaxY = true;
            }

            //setzt kollisionstoggle zurück für x
            if(contentRect.anchoredPosition.x < sLimit.x * -1 * contentRect.localScale.x && contentRect.anchoredPosition.x > eLimit.x * -1 * contentRect.localScale.x +.01f)
            { clampParallaxX = false; }

            //setzt kollisionstoggle zurück für y
            if (contentRect.anchoredPosition.y > sLimit.y * -1 * contentRect.localScale.y && contentRect.anchoredPosition.y < eLimit.y * -1 * contentRect.localScale.y - .01f)
            { clampParallaxY = false; }
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
        Vector2 startPos = contentRect.anchoredPosition; //position des contents im viewport
        Vector2 endPos = -1 * obj.GetComponent<RectTransform>().localPosition * contentRect.localScale.x; //negierte lage des objekts mit richtiger skalierung für content

        StartCoroutine(Move(startPos, endPos, 1.0f)); //default ist 1 sekunde, kann man auch eine public variable zuweisen 
    }

    //----------------------------------------------------------------------------------------------------------------------//
    // limit test
    int ccc = 1;
    public void LimitMapForTesting()
    {
        // gelöst über vektoren. wichtig: die image-dummies in der szene sind nicht ausgangspunkt für koordinaten! (gizmos verwenden?)
        if (ccc == 1)
        {
            LimitMap(new Vector2(0, 0), new Vector2(400, -200));
        }
        if (ccc == 2)
        {
            LimitMap(new Vector2(0, 0), new Vector2(800, -400));
        }
        if (ccc == 3)
        {
            LimitMap(new Vector2(0, 0), new Vector2(1200, -600));
        }
        if (ccc == 4)
        {
            LimitMap(new Vector2(0, 0), new Vector2(1600, -800));
        }
        ccc++;
        if (ccc > 4) ccc = 1;
        // LimitMap(int minXinPercentOfMap, int maxXInPercentOfMap, int minYinPercentOfMap, int  maxYInPercentOfMap)
    }

    public void LimitMap(Vector2 sPos, Vector2 ePos) {
        RectTransform rect1 = content.GetComponent<RectTransform>(); //content recttransform
        RectTransform rect2 = viewport.GetComponent<RectTransform>(); //viewport recttransform

        float dist = Vector2.Distance(sPos, ePos); //diagonale zwischen den beiden vektoren
        float myDist = dist; //wird zwischengespeichert
        float scale = 0; //wert, der später zoomen begrenzt

        float myWidth = Mathf.Abs(sPos.x) + Mathf.Abs(ePos.x); //breite des aufgespannten rechtecks zwischen vektoren
        float myHeight = Mathf.Abs(sPos.y) + Mathf.Abs(ePos.y); //höhe des aufgespannten rechtecks zwischen vektoren

        float referenceWidth = rect2.rect.width; //referenzrechteck hat viewportbreite
        float referenceHeight = rect2.rect.width * myHeight / myWidth; //referenzrechteck hat selbes breite:höhe verhältnis wie vektor rechteck
        float referenceDist = Mathf.Sqrt(Mathf.Pow(referenceWidth, 2) + Mathf.Pow(referenceHeight, 2)); //diagonale des referenzrechtecks

        //verkleinern
        if (referenceDist < dist)
        {
            for (int i = 0; referenceDist < dist; i++)
            {
                if (1 - 0.1f * i >= myMinScale)
                {
                    dist = myDist;
                    scale = 1 - .1f * i;

                    dist *= scale;
                    minScale = scale - .05f; //-.05f = fix, damit man auch wirklich auf die gewünschte skalierung kommt
                }
                else { break; } //skaliert niemals unter myMinScale
            }
            //rect1.localScale = new Vector2(scale, scale); //alt
        }
        //vergrößern
        else
        {
            for (int i = 0; referenceDist > dist; i++)
            {
                if (1 + 0.1f * i <= myMaxScale)
                {
                    dist = myDist;
                    scale = 1 + .1f * i;

                    dist *= scale;
                    minScale = scale - .05f; //-.05f = fix, damit man auch wirklich auf die gewünschte skalierung kommt
                }
                else { break; } //skaliert niemals über myMaxScale
            }
            //rect1.localScale = new Vector2(scale, scale); //alt
        }

        Vector2 pos = sPos + ePos;
        pos = new Vector2(pos[0] / 2, pos[1] / 2); //durchschnittliche position der vektoren ist mittelpunkt des rechtecks
        //rect1.anchoredPosition = -pos * rect1.localScale.x; //setzte content auf richtige position //alt

        sLimit = sPos; //schnittstelle zwischen LimitMap und Update (anfang limit)
        eLimit = ePos; //schnittstelle zwischen LimitMap und Update (ende limit)
        limitActive = true; //toggle für bedingung in update

        StartCoroutine(MoveScale(pos, new Vector2(scale, scale), 1f)); //animation, constructor sind zielposition, zielskalierung und dauer der animation (default: 1s)
    }

    public void ClearLimit() {
        limitActive = false;
        maxScale = myMaxScale;
        minScale = myMinScale;
    }

    //----------------------------------------------------------------------------------------------------------------------//
    // animation

    IEnumerator Move(Vector2 sPos, Vector2 ePos, float seconds) {
        float t = 0f;
        while (t <= 1f)
        {
            t += Time.deltaTime / seconds;
            contentRect.anchoredPosition = Vector2.Lerp(sPos, ePos, Mathf.SmoothStep(0f, 1f, t));
            yield return new WaitForFixedUpdate();
        }
    }

    
    IEnumerator MoveScale(Vector2 pos, Vector2 scale, float seconds) {
        float t = 0f;
        while (t <= 1f)
        {
            t += Time.deltaTime / seconds;
            contentRect.anchoredPosition = Vector2.Lerp(contentRect.anchoredPosition, -pos * contentRect.localScale.x, Mathf.SmoothStep(0f, 1f, t));
            contentRect.localScale = Vector2.Lerp(contentRect.localScale, scale, Mathf.SmoothStep(0f, 1f, t));
            yield return new WaitForFixedUpdate();
        }
    }
    

}
