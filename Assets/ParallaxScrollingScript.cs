using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParallaxScrollingScript : MonoBehaviour {

    public GameObject content;
    public GameObject layer;
    public float speed; //geschwindigkeit des scrollings
    public int tile; //anzahl der wiederholungen (auflösung)

    RectTransform contentRect;
    RectTransform layerRect;

    float lastX; //letzte dynamische x-position
    float lastY; //letzte dynamische y-position

    void Start() {
        contentRect = content.GetComponent<RectTransform>();
        layerRect = layer.GetComponent<RectTransform>();
        speed /= 10000; //speed muss drastisch dividiert werden, speed von 1 entspricht dem doppeln der textur
    }

    void Update () { //offset abhängig von anchoredPosition des contents (negieren für richtige richtung), tiles wie angegeben

        //wenn man an die ränder links und rechts kommt, wird horizontales scrolling deaktiviert, vertikals scrolling läuft wie gehabt
        if (content.GetComponent<MyMapContentScrollerZoomer>().clampParallaxX && !content.GetComponent<MyMapContentScrollerZoomer>().clampParallaxY)
        {
            lastY = contentRect.anchoredPosition.y * -speed;
            layerRect.GetComponent<RawImage>().uvRect = new Rect(lastX, lastY, tile, tile);
        }

        //wenn man an die ränder oben und unten kommt, wird vertikales scrolling deaktiviert, horizontales scrolling läuft wie gehabt
        if (content.GetComponent<MyMapContentScrollerZoomer>().clampParallaxY && !content.GetComponent<MyMapContentScrollerZoomer>().clampParallaxX)
        {
            lastX = contentRect.anchoredPosition.x * -speed;
            layerRect.GetComponent<RawImage>().uvRect = new Rect(lastX, lastY, tile, tile);
        }

        //wenn man eine ecke kommt, wird horizontales und vertikales scrolling deaktiviert
        if (content.GetComponent<MyMapContentScrollerZoomer>().clampParallaxX && content.GetComponent<MyMapContentScrollerZoomer>().clampParallaxY)
        {
            layerRect.GetComponent<RawImage>().uvRect = new Rect(lastX, lastY, tile, tile);
        }

        //wenn man sich an keinem rand befindet, scrollt man in x und y
        if (!content.GetComponent<MyMapContentScrollerZoomer>().clampParallaxX && !content.GetComponent<MyMapContentScrollerZoomer>().clampParallaxY)
        {
            lastX = contentRect.anchoredPosition.x * -speed;
            lastY = contentRect.anchoredPosition.y * -speed;
            layerRect.GetComponent<RawImage>().uvRect = new Rect(lastX, lastY, tile, tile);
        }
    }
}
