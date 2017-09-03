// DataDownloader.cs
// Written by Dan W.
//

using UnityEngine;
using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;

public class DataDownloader : Singleton<DataDownloader>
{
    public List<Texture> MapImages
    {
        get
        {
            return _mapImages;
        }
    }

    private string apiKey = "AIzaSyCC6UQRiQPCpUOIWh70ExSM36v60fjWCWM";
    private string apiUrl = "https://maps.googleapis.com/maps/api/staticmap?center=";
    private string location = "9665+Chesapeake+Drive+Suite+401+San+Diego,+CA+92123";
    private List<Texture> _mapImages;

    private void Awake()
    {
        _mapImages = new List<Texture>();
    }
    
    // Pre download all neccessary google map images
    private IEnumerator Start()
    {
        for (int i = 1; i < 18; ++i)
        {
            WWW url = new WWW(apiUrl + location + "&zoom=" + i.ToString() + "&size=384x384&maptype=hybrid&key=" + apiKey);

            yield return url;

            if (string.IsNullOrEmpty(url.error))
            {
                _mapImages.Add(url.texture);
            }

            else
            {
                Debug.Log(url.error);
            }
        }
    }
}