using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleBackgroundManager : MonoBehaviour
{
    private Image image;

    [Serializable]
    public struct AreaColors
    {
        public int ColorR1Min;
        public int ColorR1Max;
        public int ColorG1Min;
        public int ColorG1Max;
        public int ColorB1Min;
        public int ColorB1Max;
        public int ColorR2Min;
        public int ColorR2Max;
        public int ColorG2Min;
        public int ColorG2Max;
        public int ColorB2Min;
        public int ColorB2Max;
    }

    public List<AreaColors> areaColors;

    /*
    public int ColorR1Min = 0;
    public int ColorR1Max = 255;
    public int ColorG1Min = 0;
    public int ColorG1Max = 255;
    public int ColorB1Min = 0;
    public int ColorB1Max = 255;
    public int ColorR2Min = 0;
    public int ColorR2Max = 255;
    public int ColorG2Min = 0;
    public int ColorG2Max = 255;
    public int ColorB2Min = 0;
    public int ColorB2Max = 255;
    */

    public List<Texture> patterns;

    // Update is called once per frame
    void Awake()
    {
        image = gameObject.GetComponent<Image>();
        int area = Player.instance.roomLevel;
        Debug.Log($"area : {area}");
        Debug.Log($"AreaColors count : {areaColors.Count}");
        Debug.Log($"test couleurs : {areaColors[area].ColorR1Min}");

        int index1, index2;
        index1 = UnityEngine.Random.Range(0, patterns.Count);
        do
        {
            index2 = UnityEngine.Random.Range(0, patterns.Count);
        } while (index2 == index1);

        image.material.SetTexture("_MainTex", patterns[index1]);
        image.material.SetTexture("_BlendTex", patterns[index2]);
        image.material.SetFloat("_TextureSize", UnityEngine.Random.Range(100, 1100));
        image.material.SetFloat("_AmplitudeX", UnityEngine.Random.Range(0, 80));
        image.material.SetFloat("_CompressionX", UnityEngine.Random.Range(0f, 1f));
        image.material.SetFloat("_AmplitudeY", UnityEngine.Random.Range(0, 80));
        image.material.SetFloat("_CompressionY", UnityEngine.Random.Range(0f, 1f));
        int distortX = UnityEngine.Random.Range(0, 5);
        image.material.SetInt("_DistortX", distortX);
        image.material.SetInt("_DistortY", UnityEngine.Random.Range(distortX == 0 ? 1 : 0, 5));
        image.material.SetColor("_MainTintColor", new Color32((byte)UnityEngine.Random.Range(areaColors[area].ColorR1Min, areaColors[area].ColorR1Max), (byte)UnityEngine.Random.Range(areaColors[area].ColorG1Min, areaColors[area].ColorG1Max), (byte)UnityEngine.Random.Range(areaColors[area].ColorB1Min, areaColors[area].ColorB1Max), 255));
        image.material.SetColor("_BlendTintColor", new Color32((byte)UnityEngine.Random.Range(areaColors[area].ColorR2Min, areaColors[area].ColorR2Max), (byte)UnityEngine.Random.Range(areaColors[area].ColorG2Min, areaColors[area].ColorG2Max), (byte)UnityEngine.Random.Range(areaColors[area].ColorB2Min, areaColors[area].ColorB2Max), 255));
    }
}