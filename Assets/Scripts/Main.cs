using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class Main : MonoBehaviour
{

    private string key = "Assets/Textures/kenan.png";

    public RawImage img;


    private void Start()
    {
        Addressables.LoadAssetAsync<Texture2D>(key).Completed+=(handle)=>
        {
            Texture2D tex = handle.Result;
            img.texture = tex;
            img.GetComponent<RectTransform>().sizeDelta = new Vector2 (tex.width, tex.height);
        };


    }










}
