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

    void Start() {
        contentRect = content.GetComponent<RectTransform>();
        layerRect = layer.GetComponent<RectTransform>();
        speed /= 10000; //speed muss drastisch dividiert werden, speed von 1 entspricht dem doppeln der textur
    }

    void Update () {
        //offset abhängig von anchoredposition des contents, tiles wie angegeben
        layerRect.GetComponent<RawImage>().uvRect = new Rect(contentRect.anchoredPosition.x * -speed, contentRect.anchoredPosition.y * -speed, tile, tile);
	}
}
