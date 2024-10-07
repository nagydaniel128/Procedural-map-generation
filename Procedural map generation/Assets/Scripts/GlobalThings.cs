using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GlobalThings
{
    public static class Helper
    {
        public static void ChangeColorSmoothly(ref Color baseColor, float timeLeft, Color targetColor)
        {
            if (timeLeft <= Time.deltaTime)
            {
                // transition complete
                // assign the target color
                baseColor = targetColor;

                // start a new transition
                targetColor = new Color(Random.value, Random.value, Random.value);
                timeLeft = 1.0f;
            }
            else
            {
                // transition in progress
                // calculate interpolated color
                baseColor = Color.Lerp(baseColor, targetColor, Time.deltaTime / timeLeft);

                // update the timer
                timeLeft -= Time.deltaTime;
            }
        }
        public static Vector2 DirectionVector(Vector2 from, Vector2 to)
        {
            return (to - from).normalized;
        }
        public static Color Blend(Color color, Color backColor)
        {
            byte r = (byte)(((color.r * 255f) + (backColor.r * 255f)) / 2);
            byte g = (byte)(((color.g * 255f) + (backColor.g * 255f)) / 2);
            byte b = (byte)(((color.b * 255f) + (backColor.b * 255f)) / 2);

            return new Color(r / 255f, g / 255f, b / 255f);
        }
        public static Color RandomColor()
        {
            float R, G, B;
            R = Random.Range(0, 255);
            G = Random.Range(0, 255);
            B = Random.Range(0, 255);

            return new Color(R / 255, G / 255, B / 255, 255);
        }
        public static Color RandomColor(float minValue)
        {
            float R, G, B;
            R = Random.Range(minValue, 255);
            G = Random.Range(minValue, 255);
            B = Random.Range(minValue, 255);

            return new Color(R / 255, G / 255, B / 255, 255);
        }
        public static Vector3 RandomPointInBounds(Bounds bounds)
        {
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );
        }
        public static IEnumerator Shake(Transform target, float duration, float power, bool goBackToOriginalPos)
        {
            Vector3 originalPos = target.position;

            float elapsed = 0;

            while (elapsed < duration)
            {
                float x = Random.Range(-1, 1) * power;
                float y = Random.Range(-1, 1) * power;

                target.position = new Vector3(originalPos.x + x, originalPos.y + y, target.position.z);

                elapsed += Time.deltaTime;

                yield return null;
            }
            if(goBackToOriginalPos)
                target.position = originalPos;
        }
        public static void ArraySort(ref float[] tomb)
        {
            for (int i = 0; i < tomb.Length; i++)
            {
                var item = tomb[i];
                var currentIndex = i;

                while (currentIndex > 0 && tomb[currentIndex - 1] > item)
                {
                    tomb[currentIndex] = tomb[currentIndex - 1];
                    currentIndex--;
                }

                tomb[currentIndex] = item;
            }
        }
        public static Texture2D mergeTwoTextures(Texture2D texture1, Texture2D texture2)
        {
            Texture2D texture = new Texture2D(texture1.width, texture1.height);

            for (int i = 0; i < texture1.width; i++)
            {
                for (int j = 0; j < texture1.height; j++)
                {
                    Color color = texture1.GetPixel(i, j) * texture2.GetPixel(i, j);
                    texture.SetPixel(i, j, color);
                }
            }

            texture.Apply();
            return texture;
        }
        public static Texture2D layTextureOnTexture(Texture2D texture1, Texture2D texture2, bool accurate = false)
        {
            Texture2D texture = new Texture2D(texture1.width, texture1.height);

            for (int i = 0; i < texture1.width; i++)
            {
                for (int j = 0; j < texture1.height; j++)
                {
                    if (accurate)
                    {
                        if (texture2.GetPixel(i, j).a > texture1.GetPixel(i, j).a)
                            texture.SetPixel(i, j, texture2.GetPixel(i, j));
                        else
                            texture.SetPixel(i, j, texture1.GetPixel(i, j));
                    }
                    else
                    {
                        if (texture2.GetPixel(i, j).a > 0.2f)
                            texture.SetPixel(i, j, texture2.GetPixel(i, j));
                        else
                            texture.SetPixel(i, j, texture1.GetPixel(i, j));
                    }
                }
            }

            texture.Apply();
            return texture;
        }
        public static Color colorNearToColor(Color kolor, float range = 40)
        {
            Color color;

            bool colorsAreGood = false;
            float r = 0, g = 0, b = 0;
            while (!colorsAreGood)
            {
                r = kolor.r * 256 + Random.Range(-range, range);
                g = kolor.g * 256 + Random.Range(-range, range);
                b = kolor.b * 256 + Random.Range(-range, range);

                if (r >= 0 && r <= 255 && g >= 0 && g <= 255 && b >= 0 && b <= 255)
                    colorsAreGood = true;
            }

            color = new Color(r / 255f, g / 255f, b / 255f);

            return color;
        }
        public static Color complementaryToColor(Color kolor)
        {
            Color color;

            float
                r = 255 - kolor.r * 255,
                g = 255 - kolor.g * 255,
                b = 255 - kolor.b * 255;

            color = new Color(r / 255f, g / 255f, b / 255f);

            return color;
        }
        public static Color triad1ToColor(Color kolor)
        {
            Color color;

            float
                r = kolor.g,
                g = kolor.b,
                b = kolor.r;

            color = new Color(r, g, b);

            return color;
        }
        public static Color triad2ToColor(Color kolor)
        {
            Color color;

            float
                r = kolor.b,
                g = kolor.r,
                b = kolor.g;

            color = new Color(r, g, b);

            return color;
        }
        public static Color square1ToColor(Color kolor)
        {
            return complementaryToColor(kolor);
        }
        public static Color square2ToColor(Color kolor)
        {
            float
                r = 255 - ((kolor.r * 255) / 2),
                g = (255 - (kolor.g * 255)) / 2,
                b = 255 - ((kolor.b * 255) / 2);

            return new Color(r / 255f, g / 255f, b / 255f);
        }
        public static Color square3ToColor(Color kolor)
        {
            float
                r = 255 - ((kolor.r * 255) / 2),
                g = 255 - (kolor.g * 255) / 2,
                b = (255 - (kolor.b * 255)) / 2;

            return new Color(r / 255f, g / 255f, b / 255f);
        }

    }
}
