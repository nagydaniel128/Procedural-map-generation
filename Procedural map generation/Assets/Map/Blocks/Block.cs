using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D collider;

    public Transform placableObjectPlace;
    public PlacableObject innerPlacableObject;
    public BlockInfo info;

    public bool litUp;

    public int x, y;
    public int biomNumber = 0;


    private void Awake()
    {
        //getting references
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<BoxCollider2D>();

        placableObjectPlace = transform.GetChild(0);
        innerPlacableObject = transform.GetChild(0).GetComponent<PlacableObject>();
        innerPlacableObject.owner = this;

        spriteRenderer.color = new Color(200f / 255f, 200f / 255f, 200f / 255f);
    }

    /// <summary>
    /// Setting the block to a type by a BlockInfo.
    /// </summary>
    /// <param name="Type">The BlockInfo contains the data needed to transform the Block.</param>
    public void SetBlockType(BlockInfo Type)
    {
        info = Type;

        spriteRenderer.sprite = info.sprite;
        spriteRenderer.color = info.color;
        gameObject.layer = info.isTrigger ? 6 : 4;
        collider.isTrigger = info.isTrigger;
    }

    public Color colorAfterMapGeneration;

    /// <summary>
    /// Stores data about Block types.
    /// </summary>
    public class BlockInfo
    {
        public Sprite sprite;
        public Color color;
        public bool isTrigger; //isTrigger means that if its true, you can walk on it, else you can't

        public BlockInfo() { }
        public BlockInfo(Sprite Spritee, Color Colorr, bool IsTrigger)
        {
            sprite = Spritee;
            color = Colorr;
            isTrigger = IsTrigger;
        }
    }

}
