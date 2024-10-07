using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CONTROLLER is a singleton for getting every important asset for all objects.
/// </summary>
public class CONTROLLER : MonoBehaviour
{
    static CONTROLLER Instance;
    public static CONTROLLER instance
    {
        get
        { 
            if(Instance == null)
            {
                GameObject go = new GameObject();
                go.AddComponent<CONTROLLER>();
            }
            return Instance;
        }
    }
    private void Awake()
    {
        Application.targetFrameRate = 140;
        Instance = this;
    }

    /* 
    0 -> top
    1 -> right
    2 -> bottom
    3 -> left
    4 -> top/left       corner
    5 -> top/right      corner
    6 -> bottom/right  corner
    7 -> bottom/left   corner
    */
    public PlacableObject.PlacableObjectInfo[] placableObjectWaterInfos = new PlacableObject.PlacableObjectInfo[43];




    public List<Biom> bioms = new List<Biom>();

    public Block[,] map;

    public Block.BlockInfo water;   //there is only one water type per map

    public Color mainColor;
    public Color[] colors = new Color[4];
    public Color[] complementaryColors = new Color[4];
    public Color[] triad1Colors = new Color[4];
    public Color[] triad2Colors = new Color[4];
    public Color[] square1Colors = new Color[4];
    public Color[] square2Colors = new Color[4];

    //float counter;
    //private void Update()
    //{
    //    counter += Time.deltaTime;
    //    if (counter > 0.5f)
    //    {
    //        float fps = 1.0f / Time.deltaTime;
    //        GameObject.Find("Fps").gameObject.GetComponent<TextMeshProUGUI>().text = ((int)fps).ToString();
    //        counter = 0;
    //    }
    //}
}
