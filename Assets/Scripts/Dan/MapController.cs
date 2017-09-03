// MapController.cs
// Written by Dan W.
//

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MapController : MonoBehaviour
{
    private RectTransform _thisTransform;
    private RawImage _thisRawImage;

    private string apiKey = "AIzaSyCC6UQRiQPCpUOIWh70ExSM36v60fjWCWM";
    private string apiUrl = "https://maps.googleapis.com/maps/api/staticmap?center=";
    private string location = "9665 Chesapeake Drive Suite 401 San Diego, CA 92123";
    private List<Texture> _mapImages;
    private List<string> _zoomSize = new List<string>() { "5", "8", "11", "14", "17"};

    private void Awake()
    {
        _thisTransform = GetComponent<RectTransform>();
        _thisRawImage = GetComponent<RawImage>();
        _mapImages = new List<Texture>();
    }

    private IEnumerator Start()
    {
        for (int i = 0; i < _zoomSize.Count; ++i)
        {
            WWW url = new WWW(apiUrl + location + "&zoom=" + _zoomSize[i] + "&size=384x384&maptype=hybrid&key=" + apiKey);

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

        yield return new WaitForEndOfFrame();
        _thisRawImage.material.SetTexture("_Texture1", _mapImages[0]);
        yield return new WaitForSeconds(0.5F);
        float timer = 0.0F;

        for (int i = 1; i < _mapImages.Count; ++i)
        {
            _thisRawImage.material.SetTexture("_Texture1", _mapImages[i - 1]);
            _thisRawImage.material.SetTexture("_Texture2", _mapImages[i]);
            timer = 0.0F;

            while (timer < 1.0F)
            {
                timer += Time.unscaledDeltaTime / 0.5F;
                _thisRawImage.material.SetFloat("_Fade", timer);
                yield return null;
            }

            yield return new WaitForSeconds(1);
        }

        yield return new WaitForEndOfFrame();

        timer = 0.0F;

        while(timer < 1.0F)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
            _thisTransform.anchoredPosition = Vector2.Lerp(_thisTransform.anchoredPosition, new Vector2(384, 320), timer);
            _thisTransform.sizeDelta = Vector2.Lerp(_thisTransform.sizeDelta, new Vector2(128, 128), timer);
        }

        yield return new WaitForEndOfFrame();

        if (GameBoard.Instance != null)
        {
            GameBoard.Instance.Init();
        }
    }
}