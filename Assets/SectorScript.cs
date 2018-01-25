using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class SectorScript : MonoBehaviour {

    public GameObject content; //content in scrollrect
    RectTransform contentRect;

    public GameObject linePrefab; //objekt mit UILineRenderer script
    List<GameObject> myLines = new List<GameObject>(); //liste aller erstellten linien
    List<Vector2> mySectos = new List<Vector2>(); //liste aller sektor-vektoren

    public float sectorSize; //größer eines sektors, bestenfalls ein teiler der contentbreite
    public float lineThickness; //stärke der grid-linien
    float offset; //zentriert den mittelpunkt der ausgangszelle 

	void Start () {
        contentRect = content.GetComponent<RectTransform>();
        offset = sectorSize / 2;

        CreateLines();
        CreateSectorVectors();

        Debug.Log(Sector(1, 1));
        Debug.Log(Sector(-1, -1));
    }
	
	void Update () {
        //breite der linien skaliert umgekehrt zur skalierung des contents
        foreach(GameObject obj in myLines)
        {
            obj.GetComponent<UILineRenderer>().LineThickness = lineThickness / contentRect.localScale.x;
        }
    }

    //erstellt horizontale und vertikale linien mit offset
    void CreateLines() {

        //horizontale linien
        for (int i = 0; i * sectorSize <= contentRect.rect.height; i++)
        {
            GameObject temp = Instantiate(linePrefab) as GameObject;

            temp.GetComponent<UILineRenderer>().m_points[0] = new Vector2(0 - contentRect.rect.width / 2, i * sectorSize - contentRect.rect.height / 2 - offset);
            temp.GetComponent<UILineRenderer>().m_points[1] = new Vector2(0 + contentRect.rect.width / 2, i * sectorSize - contentRect.rect.height / 2 - offset);

            temp.GetComponent<RectTransform>().SetParent(contentRect, false);
            myLines.Add(temp);
        }

        //vertikale linien
        for (int i = 0; i * sectorSize <= contentRect.rect.width; i++)
        {
            GameObject temp = Instantiate(linePrefab) as GameObject;

            temp.GetComponent<UILineRenderer>().m_points[0] = new Vector2(i * sectorSize - contentRect.rect.width / 2 - offset, 0 + contentRect.rect.height / 2);
            temp.GetComponent<UILineRenderer>().m_points[1] = new Vector2(i * sectorSize - contentRect.rect.width / 2 - offset, 0 - contentRect.rect.height / 2);

            temp.GetComponent<RectTransform>().SetParent(contentRect, false);
            myLines.Add(temp);
        }
    }

    //erstellt sektor koordinaten
    void CreateSectorVectors() {
        float ratio = contentRect.rect.width / 2 / sectorSize;
        for (float x = -ratio; x <= ratio; x++)
        {
            for (float y = -ratio; y <= ratio; y++)
            {
                Vector2 temp = new Vector2(x * sectorSize, y * sectorSize);
                mySectos.Add(temp);
            }
        }
    }

    //kovertiert sektor-index zu vector2 koordinaten
    Vector2 Sector(int x, int y) {
        Vector2 sector = new Vector2(x * sectorSize, -y * sectorSize);
        for(int i = 0; i < mySectos.Count; i++)
        {
            if (mySectos[i] == sector)
            {
                return sector;
            }
        }
        return Vector2.zero;
    }
}
