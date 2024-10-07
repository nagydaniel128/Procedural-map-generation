using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalThings;

public class MapGenerator : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    public bool randomSeed = true;
    public string seed = "0";
    float[,] noiseMap;
    float[,] noiseMapForBiomes;

    const int MINBIOMNUMBER = 2, MAXBIOMNUMBER = 2;

    const float blockSize = 3;

    public GameObject tile;

    public Texture2D[] texturesForBlocks;
    public Texture2D[] texturesForWaters;

    public Texture2D[] texturesForStones_main;
    public Texture2D[] texturesForStones_addition;

    public Texture2D[] texturesForBushes_main;
    public Texture2D[] texturesForBushes_decoration;

    public Texture2D[] texturesForPlacableObjectWaterPrefab;    //placable object water is the shore on blocks

    public Texture2D defaultTexture;


    /// <summary>
    /// Gets the biom number of a block's coordinate.
    /// </summary>
    /// <param name="x">X coordinate of the Block.</param>
    /// <param name="y">Y coordinate of the Block.</param>
    /// <returns></returns>
    int biomnumber(int x, int y)
    {
        int biomNumber = 0;
        switch (CONTROLLER.instance.bioms.Count)
        {
            case 2:
                if (noiseMapForBiomes[x, y] < 0.5f)
                    biomNumber = 0;
                else biomNumber = 1;

                break;
            case 3:
                if (noiseMapForBiomes[x, y] < 0.33f)
                    biomNumber = 0;
                else if (noiseMapForBiomes[x, y] < 0.66f)
                    biomNumber = 1;
                else biomNumber = 2;

                break;
        }
        return biomNumber;
    }

    /// <summary>
    /// Returns if a PlacableObject can be placed on a Block.
    /// </summary>
    /// <param name="xCoordinate">X coordinate of the Block.</param>
    /// <param name="yCoordinate">Y coordinate of the Block.</param>
    /// <returns></returns>
    bool canPlacePlacableObjectThere(int xCoordinate, int yCoordinate)
    {

        if (CONTROLLER.instance.map[xCoordinate, yCoordinate].innerPlacableObject.isSomethingInside || !CONTROLLER.instance.map[xCoordinate, yCoordinate].info.isTrigger)
        {
            return false;
        }
        return true;
    }
    /// <summary>
    /// Returns if an area is free for PlacableObjects to place on Blocks.
    /// </summary>
    /// <param name="xCoordinate">X coordinate.</param>
    /// <param name="yCoordinate">Y coordinate.</param>
    /// <param name="size">Size of the area around the coordinates.</param>
    /// <returns></returns>
    bool canPlacePlacableObjectsInAreaAroundCoordinate(int xCoordinate, int yCoordinate, int size)
    {

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (Mathf.Abs(size / 2 - x) + Mathf.Abs(size / 2 - y) < size / 2 + 1)
                    if (CONTROLLER.instance.map[x + xCoordinate - size / 2, y + yCoordinate - size / 2].innerPlacableObject.isSomethingInside || !CONTROLLER.instance.map[x + xCoordinate - size / 2, y + yCoordinate - size / 2].info.isTrigger)
                    {
                        return false;
                    }
            }
        }
        return true;
    }

    private void Start()
    {
        if (!randomSeed)
            Random.seed = seed.GetHashCode();

        StartCoroutine(LoadMap());
    }

    int loadProgress;
    IEnumerator LoadMap()
    {
        yield return new WaitForSeconds(0.1f);
        int howManyTexturesOnTheTexture;

        #region colors
        //getting the main color
        CONTROLLER.instance.mainColor = new Color(Random.Range(100f, 255f) / 255f, Random.Range(100f, 255f) / 255f, Random.Range(100f, 255f) / 255f);
        //colors similar to main color
        for (int i = 0; i < CONTROLLER.instance.colors.Length; i++)
            CONTROLLER.instance.colors[i] = Helper.colorNearToColor(CONTROLLER.instance.mainColor);

        //complementary colors
        for (int i = 0; i < CONTROLLER.instance.complementaryColors.Length; i++)
            CONTROLLER.instance.complementaryColors[i] = Helper.complementaryToColor(CONTROLLER.instance.colors[i]);

        //triad colors
        for (int i = 0; i < CONTROLLER.instance.triad1Colors.Length; i++)
            CONTROLLER.instance.triad1Colors[i] = Helper.triad1ToColor(CONTROLLER.instance.colors[i]);
        for (int i = 0; i < CONTROLLER.instance.triad2Colors.Length; i++)
            CONTROLLER.instance.triad2Colors[i] = Helper.triad2ToColor(CONTROLLER.instance.colors[i]);

        //square colors
        for (int i = 0; i < CONTROLLER.instance.square1Colors.Length; i++)
            CONTROLLER.instance.square1Colors[i] = Helper.square2ToColor(CONTROLLER.instance.colors[i]);
        for (int i = 0; i < CONTROLLER.instance.square2Colors.Length; i++)
            CONTROLLER.instance.square2Colors[i] = Helper.square3ToColor(CONTROLLER.instance.colors[i]);

        #endregion

        #region water
        //generating water texture
        CONTROLLER.instance.water = new Block.BlockInfo();
        howManyTexturesOnTheTexture = Random.Range(2, 4);       //number of textures to mix
        Texture2D textureOfNewWater = new Texture2D(128, 128);      //new empty texture

        textureOfNewWater = Helper.mergeTwoTextures(defaultTexture, defaultTexture);
        for (int j = 0; j < howManyTexturesOnTheTexture; j++)       //mixing the textures on the empty one
            textureOfNewWater = Helper.mergeTwoTextures(textureOfNewWater, texturesForWaters[Random.Range(0, texturesForWaters.Length)]);
        Sprite asd = Sprite.Create(textureOfNewWater, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 128);      //making the texture to a sprite (y)

        //setting the sprite of the water
        CONTROLLER.instance.water.sprite = asd;

        CONTROLLER.instance.water.color = CONTROLLER.instance.mainColor;

        //setting if the block is walkable
        CONTROLLER.instance.water.isTrigger = false;




        //waterslides
        for (int i = 0; i < CONTROLLER.instance.placableObjectWaterInfos.Length; i++)
        {
            Texture2D textureOfWaterPrefab = new Texture2D(128, 128);      //new empty texture
            textureOfWaterPrefab = Helper.mergeTwoTextures(CONTROLLER.instance.water.sprite.texture, texturesForPlacableObjectWaterPrefab[i]);
            Sprite spriteOfWaterPrefab = Sprite.Create(textureOfWaterPrefab, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 128);      //making the texture to a sprite (y)
            CONTROLLER.instance.placableObjectWaterInfos[i] = new PlacableObject.PlacableObjectInfo(spriteOfWaterPrefab);
            CONTROLLER.instance.placableObjectWaterInfos[i].color = CONTROLLER.instance.mainColor;
        }
        #endregion

        int numberOfBioms = Random.Range(MINBIOMNUMBER, MAXBIOMNUMBER + 1);
        //numberOfBioms = 3;
        for (int biomNumber = 0; biomNumber < numberOfBioms; biomNumber++)
        {
            //new instance of biom
            CONTROLLER.instance.bioms.Add(new Biom());

            #region blocks
            //generating block textures
            CONTROLLER.instance.bioms[biomNumber].blocks = new Block.BlockInfo[2];
            for (int i = 0; i < CONTROLLER.instance.bioms[biomNumber].blocks.Length; i++)
            {
                howManyTexturesOnTheTexture = Random.Range(2, 4);       //number of textures to mix
                Texture2D textureOfNewBlock = new Texture2D(128, 128);      //new empty texture

                textureOfNewBlock = Helper.mergeTwoTextures(defaultTexture, defaultTexture);
                for (int j = 0; j < howManyTexturesOnTheTexture; j++)       //mixing the textures on the empty one
                    textureOfNewBlock = Helper.mergeTwoTextures(textureOfNewBlock, texturesForBlocks[Random.Range(0, texturesForBlocks.Length)]);
                asd = Sprite.Create(textureOfNewBlock, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 128);      //making the texture to a sprite (y)

                CONTROLLER.instance.bioms[biomNumber].blocks[i] = new Block.BlockInfo();      //making new object of class
                CONTROLLER.instance.bioms[biomNumber].blocks[i].sprite = asd;                 //setting the sprite of the block
            }
            for (int i = 0; i < CONTROLLER.instance.bioms[biomNumber].blocks.Length; i++)
            {
                Color color = new Color();
                switch (numberOfBioms)
                {
                    case 1:
                        //complementary color
                        color = CONTROLLER.instance.complementaryColors[Random.Range(0, CONTROLLER.instance.complementaryColors.Length)];
                        break;
                    case 2:
                        //triad color
                        if (biomNumber == 0)
                            color = CONTROLLER.instance.triad1Colors[Random.Range(0, CONTROLLER.instance.triad1Colors.Length)];
                        else
                            color = CONTROLLER.instance.triad2Colors[Random.Range(0, CONTROLLER.instance.triad2Colors.Length)];
                        break;
                    case 3:
                        //square color
                        if (biomNumber == 0)
                            color = CONTROLLER.instance.complementaryColors[Random.Range(0, CONTROLLER.instance.complementaryColors.Length)];
                        else if (biomNumber == 1)
                            color = CONTROLLER.instance.square1Colors[Random.Range(0, CONTROLLER.instance.square1Colors.Length)];
                        else
                            color = CONTROLLER.instance.square2Colors[Random.Range(0, CONTROLLER.instance.square2Colors.Length)];
                        break;
                }
                CONTROLLER.instance.bioms[biomNumber].blocks[i].color = color;

                //setting if the block is walkable
                CONTROLLER.instance.bioms[biomNumber].blocks[i].isTrigger = true;
            }
            #endregion

            #region placable objects
            //generate 1-4 random placable object to the biom
            for (int i = 0; i < Random.Range(1, 4); i++)
            {
                PlacableObject.PlacableObjectInfo placableObjectInfo = new PlacableObject.PlacableObjectInfo();

                switch (Random.Range(0, 2))
                {
                    case 0:     //stones
                                //cant walk through
                        placableObjectInfo = new PlacableObject.PlacableObjectInfo(texturesForStones_main, texturesForStones_addition);
                        placableObjectInfo.isTrigger = false;
                        placableObjectInfo.color = Helper.RandomColor(100);
                        break;
                    case 1:     //bushes
                        placableObjectInfo = new PlacableObject.PlacableObjectInfo(texturesForBushes_main, texturesForBushes_decoration);
                        placableObjectInfo.isTrigger = Random.Range(0, 2) == 1 ? true : false;           //random true/false
                        placableObjectInfo.color = Helper.RandomColor(100);
                        placableObjectInfo.secondaryColor = Helper.RandomColor(100);
                        break;
                }
                CONTROLLER.instance.bioms[biomNumber].placableObjects.Add(placableObjectInfo);
            }

            #endregion
        }

        //noisemap
        noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);
        noiseMapForBiomes = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale * 1.5f, false);

        CONTROLLER.instance.map = new Block[mapWidth, mapHeight];

        #region generating blocks
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                //making gameobject
                GameObject a = Instantiate(tile, new Vector3((-mapWidth / 2 + x) * blockSize, (-mapHeight / 2 + y) * blockSize), transform.rotation);

                //setting location
                a.GetComponent<Block>().x = x;
                a.GetComponent<Block>().y = mapHeight - y - 1;

                a.GetComponent<Block>().biomNumber = biomnumber(x, y);

                //setting type by the noise
                if (noiseMap[x, y] < 0.3f)
                    a.GetComponent<Block>().SetBlockType(CONTROLLER.instance.water);
                else if (noiseMap[x, y] < 0.4f)
                    a.GetComponent<Block>().SetBlockType(CONTROLLER.instance.bioms[biomnumber(x, y)].blocks[0]);
                else
                    a.GetComponent<Block>().SetBlockType(CONTROLLER.instance.bioms[biomnumber(x, y)].blocks[1]);

                CONTROLLER.instance.map[x, mapHeight - y - 1] = a.GetComponent<Block>();
                a.transform.parent = GameObject.Find("Map").transform;
            }
        }
        #endregion

        #region blending neighbour blocks so they dont differ too much
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                //if it is water then skip
                if (!CONTROLLER.instance.map[x, y].info.isTrigger)
                    continue;

                Color colorOfBlock = CONTROLLER.instance.map[x, y].info.color;
                Block block = CONTROLLER.instance.map[x, y];
                List<int> biomsInvolved = new List<int>();

                //above
                if (block.biomNumber != CONTROLLER.instance.map[x, y - 1].biomNumber && CONTROLLER.instance.map[x, y - 1].info.isTrigger && !biomsInvolved.Contains(CONTROLLER.instance.map[x, y - 1].biomNumber))
                {
                    colorOfBlock = Helper.Blend(colorOfBlock, CONTROLLER.instance.map[x, y - 1].spriteRenderer.color);
                    biomsInvolved.Add(CONTROLLER.instance.map[x, y - 1].biomNumber);
                }
                //to its right
                if (block.biomNumber != CONTROLLER.instance.map[x + 1, y].biomNumber && CONTROLLER.instance.map[x + 1, y].info.isTrigger && !biomsInvolved.Contains(CONTROLLER.instance.map[x + 1, y].biomNumber))
                {
                    colorOfBlock = Helper.Blend(colorOfBlock, CONTROLLER.instance.map[x + 1, y].spriteRenderer.color);
                    biomsInvolved.Add(CONTROLLER.instance.map[x + 1, y].biomNumber);
                }
                //below
                if (block.biomNumber != CONTROLLER.instance.map[x, y + 1].biomNumber && CONTROLLER.instance.map[x, y + 1].info.isTrigger && !biomsInvolved.Contains(CONTROLLER.instance.map[x, y + 1].biomNumber))
                {
                    colorOfBlock = Helper.Blend(colorOfBlock, CONTROLLER.instance.map[x, y + 1].spriteRenderer.color);
                    biomsInvolved.Add(CONTROLLER.instance.map[x, y + 1].biomNumber);
                }
                //to its left
                if (block.biomNumber != CONTROLLER.instance.map[x - 1, y].biomNumber && CONTROLLER.instance.map[x - 1, y].info.isTrigger && !biomsInvolved.Contains(CONTROLLER.instance.map[x - 1, y].biomNumber))
                {
                    colorOfBlock = Helper.Blend(colorOfBlock, CONTROLLER.instance.map[x - 1, y].spriteRenderer.color);
                    biomsInvolved.Add(CONTROLLER.instance.map[x - 1, y].biomNumber);
                }

                CONTROLLER.instance.map[x, y].colorAfterMapGeneration = colorOfBlock;
            }
        }
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (CONTROLLER.instance.map[x, y].info.isTrigger)
                    CONTROLLER.instance.map[x, y].spriteRenderer.color = CONTROLLER.instance.map[x, y].colorAfterMapGeneration;
            }
        }
        #endregion

        #region shores
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (CONTROLLER.instance.map[x, y].info.isTrigger)
                {
                    //top
                    if
                        (!CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&        //bottom
                        CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[0]);
                    //right
                    else if
                        (CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[1]);
                    //bottom
                    else if
                        (CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[2]);
                    //left
                    else if
                        (CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[3]);
                    //top/left
                    else if
                        (!CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[4]);
                    //top/right
                    else if
                        (CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[5]);
                    //bottom/right
                    else if
                        (CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[6]);
                    //bottom/left
                    else if
                        (CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[7]);
                    //top és right
                    else if
                        (!CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[8]);
                    //right és bottom
                    else if
                        (CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[9]);
                    //bottom és left
                    else if
                        (CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[10]);
                    //left és top
                    else if
                        (!CONTROLLER.instance.map[x, y - 1].info.isTrigger &&        //top
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[11]);
                    //top és right és bottom
                    else if
                        (!CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[12]);
                    //right és bottom és left
                    else if
                        (CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[13]);
                    //bottom és left és top
                    else if
                        (!CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[14]);
                    //left és top és right
                    else if
                        (!CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[15]);
                    //körbe
                    else if
                        (!CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[16]);
                    //top és right és bottom/left
                    else if
                        (!CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[17]);
                    //right és bottom és top/left
                    else if
                        (!CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[18]);
                    //bottom és left és top/right
                    else if
                        (CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[19]);
                    //top/left
                    else if
                        (!CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[20]);
                    //top és bottom
                    else if
                        (!CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[21]);
                    //left és right
                    else if
                        (CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[22]);
                    //top/left és top/right
                    else if
                        (!CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[23]);
                    //top/right és bottom/right
                    else if
                        (CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[24]);
                    //bottom/left és bottom/right
                    else if
                        (CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[25]);
                    //top/left és bottom/left
                    else if
                        (!CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[26]);
                    //top/left és top/right és bottom right
                    else if
                        (!CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[27]);
                    //top/right és bottom/right és bottom/left
                    else if
                        (CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[28]);
                    //top/left és bottom/right és bottom/left
                    else if
                        (!CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[29]);
                    //összes sarok
                    else if
                        (!CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[30]);
                    //top és bottom/right
                    else if
                        (!CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[31]);
                    //top és bottom/left
                    else if
                        (!CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[32]);
                    //top és bottom/left és bottom/right
                    else if
                        (!CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[33]);
                    //right és bottom/left
                    else if
                        (CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[34]);
                    //right és top/left
                    else if
                        (!CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[35]);
                    //right és top/left és bottom/left
                    else if
                        (!CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y + 1].info.isTrigger &&     //bottom left
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[36]);
                    //bottom és top/left
                    else if
                        (!CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[37]);
                    //bottom és top/right
                    else if
                        (CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[38]);
                    //bottom és top/right és top/left
                    else if
                        (!CONTROLLER.instance.map[x - 1, y - 1].info.isTrigger &&    //top left
                        CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[39]);
                    //left és top/right
                    else if
                        (CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[40]);
                    //left és bottom/right
                    else if
                        (CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[41]);
                    //left és bottom/right és top/right
                    else if
                        (CONTROLLER.instance.map[x, y - 1].info.isTrigger &&         //top
                        !CONTROLLER.instance.map[x + 1, y - 1].info.isTrigger &&     //top right
                        CONTROLLER.instance.map[x + 1, y].info.isTrigger &&         //right
                        !CONTROLLER.instance.map[x + 1, y + 1].info.isTrigger &&     //right bottom
                        CONTROLLER.instance.map[x, y + 1].info.isTrigger &&         //bottom
                        !CONTROLLER.instance.map[x - 1, y].info.isTrigger            //left
                        )
                        CONTROLLER.instance.map[x, y].innerPlacableObject.SetPlacableObjectType(CONTROLLER.instance.placableObjectWaterInfos[42]);
                }
            }
        }
        #endregion

        #region placable objects

        //1 placable object group generation (if can generate) per 100 blocks
        for (int i = 0; i < mapHeight * mapWidth / 100; i++)
        {
            //finding a possible place on the map for the placable object group (group is a place where are multiple placable objects of the same type exist)
            int x, y;
            while (true)
            {
                x = Random.Range(0, mapWidth);
                y = Random.Range(0, mapHeight);
                if (CONTROLLER.instance.map[x, y].innerPlacableObject.isSomethingInside == false && CONTROLLER.instance.map[x, y].info.isTrigger == true)
                    break;
            }

            //check if the picked random size (of the placable object group) can be placed around the picked coordinate
            int randomSize = Random.Range(4, 8);
            if (canPlacePlacableObjectsInAreaAroundCoordinate(x, y, randomSize))
            {
                //getting a placable object info based on the coordinate's biom
                PlacableObject.PlacableObjectInfo info = CONTROLLER.instance.bioms[biomnumber(x, y)].placableObjects[Random.Range(0, CONTROLLER.instance.bioms[biomnumber(x, y)].placableObjects.Count)];
                for (int j = x; j < x + randomSize; j++)
                {
                    for (int k = y; k < y + randomSize; k++)
                    {
                        //if loop is inside placing bounds
                        if (Mathf.Abs(randomSize / 2 - (j - x)) + Mathf.Abs(randomSize / 2 - (k - y)) < randomSize / 2 + 1)
                        {
                            if (canPlacePlacableObjectThere(j, k))   //if can move on block (=not water)
                                if (Random.Range(0, 100) <= 50)
                                {
                                    CONTROLLER.instance.map[j, k].innerPlacableObject.enabled = true;
                                    CONTROLLER.instance.map[j, k].innerPlacableObject.SetPlacableObjectType(info);
                                }
                        }
                        else
                        {
                            //else there is a 15% chance to place outside of bounds to randomize the look of the group a bit
                            if (Random.Range(0, 100) < 15)
                                if (canPlacePlacableObjectThere(j, k))   //if can move on block (=not water)
                                    if (Random.Range(0, 100) <= 50)
                                    {
                                        CONTROLLER.instance.map[j, k].innerPlacableObject.enabled = true;
                                        CONTROLLER.instance.map[j, k].innerPlacableObject.SetPlacableObjectType(info);
                                    }
                        }
                    }
                }
            }
        }
        #endregion
    }

}
