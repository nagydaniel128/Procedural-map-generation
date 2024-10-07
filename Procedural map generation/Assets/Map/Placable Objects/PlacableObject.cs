using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalThings;

public class PlacableObject : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D collider;
    public Block owner;

    public PlacableObjectInfo info;


    public bool isSomethingInside;

    /// <summary>
    /// Sets the Placable object type (the object inside of the block).
    /// </summary>
    /// <param name="Info">The type of the Placable object to set in.</param>
    public void SetPlacableObjectType(PlacableObjectInfo Info)
    {
        info = Info;

        spriteRenderer.enabled = true;
        spriteRenderer.sprite = info.secondaryColor == Color.black ? info.getSprite() : info.getSpriteWithDifferentDecorColor(info.secondaryColor);
        spriteRenderer.color = Info.color;

        isSomethingInside = true;
        enabled = true;
    }



    /// <summary>
    /// Stores data about PlacableObject types.
    /// </summary>
    public class PlacableObjectInfo
    {
        public Sprite sprite;
        public Color color, secondaryColor = Color.black;
        public bool isTrigger = false;

        public Texture2D[] mainTextures;
        public Texture2D[] decorationTextures;

        public PlacableObjectInfo() { }
        public PlacableObjectInfo(Sprite sprite)
        {
            this.sprite = sprite;
        }
        public PlacableObjectInfo(Texture2D[] mainTextures, Texture2D[] decorationTextures)
        {
            this.mainTextures = mainTextures;
            this.decorationTextures = decorationTextures;
        }

        /// <summary>
        /// Generates a sprite for the PlacableObject (by the PlacableObjectInfo).
        /// </summary>
        /// <returns></returns>
        public Sprite getSprite()
        {
            if (mainTextures != null || decorationTextures != null)
            {
                //texture
                int howManyTexturesOnTheTexture = Random.Range(2, 4);       //number of textures to mix
                Texture2D texture = new Texture2D(128, 128);                //new empty texture

                Texture2D mainTexture;
                if (mainTextures != null)
                    mainTexture = mainTextures[Random.Range(0, mainTextures.Length)];
                else
                    mainTexture = decorationTextures[Random.Range(0, decorationTextures.Length)];

                texture = Helper.mergeTwoTextures(mainTexture, mainTexture);
                for (int j = 0; j < howManyTexturesOnTheTexture; j++)       //mixing the textures on the empty one
                    texture = Helper.layTextureOnTexture(texture, decorationTextures[Random.Range(0, decorationTextures.Length)]);
                Sprite generatedSprite = Sprite.Create(texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 128);      //making the texture to a sprite (y)

                return generatedSprite;
            }
            else
            {
                return sprite;
            }
        }

        /// <summary>
        /// Generates a sprite for the PlacableObject but with a different color for the decoration (by the PlacableObjectInfo).
        /// </summary>
        /// <param name="decorColor">The color for the decoration of the PlacableObject.</param>
        /// <returns></returns>
        public Sprite getSpriteWithDifferentDecorColor(Color decorColor)
        {
            //texture
            int howManyTexturesOnTheTexture = Random.Range(0, 3);       //number of textures to mix
            Texture2D texture = new Texture2D(128, 128);                //new empty texture

            Texture2D mainTexture = mainTextures[Random.Range(0, mainTextures.Length)];
            texture = Helper.mergeTwoTextures(mainTexture, mainTexture);    //this is only necessary because that way i can get a copy of the maintexture

            //burning random color on bush and apply it
            for (int x = 0; x < 128; x++)
                for (int y = 0; y < 128; y++)
                    texture.SetPixel(x, y, texture.GetPixel(x, y) * color);
            texture.Apply();

            for (int j = 0; j < howManyTexturesOnTheTexture; j++)       //mixing the textures on the empty one
            {
                //decor random color and texture
                Texture2D defDecorationTexture = decorationTextures[Random.Range(0, decorationTextures.Length)];
                Texture2D decorationTexture = Helper.mergeTwoTextures(defDecorationTexture, defDecorationTexture);

                //burning the color on the texture and apply it
                for (int x = 0; x < 128; x++)
                    for (int y = 0; y < 128; y++)
                        decorationTexture.SetPixel(x, y, decorationTexture.GetPixel(x, y) * decorColor);
                decorationTexture.Apply();

                //lay the decor on the bush
                texture = Helper.layTextureOnTexture(texture, decorationTexture);
            }
            Sprite generatedSprite = Sprite.Create(texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 128);      //making the texture to a sprite

            return generatedSprite;
        }
    }

    /// <summary>
    /// Turns off everything about the PlacableObject.
    /// </summary>
    public void DestroyPlacableObject()
    {
        isSomethingInside = false;
        spriteRenderer.enabled = false;
        transform.localPosition = new Vector3();
        enabled = false;
    }
}
