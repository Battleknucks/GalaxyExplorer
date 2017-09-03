// MapController.cs
// Written by Dan W.
//

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MapController : MonoBehaviour
{
    [SerializeField]
    private RawImage _mapRawImage;

    private RectTransform _thisTransform;

    private void Awake()
    {
        _thisTransform = GetComponent<RectTransform>();
    }

    public void Init()
    {
        gameObject.SetActive(true);
        Invoke("Commence", 1.0F);
    }

    public void Kill ()
    {
        StopAllCoroutines();
        _thisTransform.localPosition = Vector3.zero;
        _thisTransform.sizeDelta = new Vector2(512, 512);
        gameObject.SetActive(false);
    }

    private void Commence ()
    {
        StartCoroutine("DoMapRoutine");
    }

    private IEnumerator DoMapRoutine()
    {
        yield return new WaitForEndOfFrame();

        if (DataDownloader.Instance.MapImages == null || DataDownloader.Instance.MapImages.Count < 2)
        {
            Debug.LogError("Failed to download map data...");
            MainUI.Instance.Error("Failed to download map data...");
            yield return null;
        }

        else
        {
            yield return new WaitForSeconds(1);
            MusicManager.Instance.KillBackgroundMusic();
            ToolManager.Instance.HideTools(true);
            MainUI.Instance.StartLocating();
            GameBoard.Instance.StartAudio();
            _mapRawImage.material.SetTexture("_Texture1", DataDownloader.Instance.MapImages[0]);
            yield return new WaitForSeconds(0.5F);
            float timer = 0.0F;

            for (int i = 1; i < DataDownloader.Instance.MapImages.Count; ++i)
            {
                _mapRawImage.material.SetTexture("_Texture1", DataDownloader.Instance.MapImages[i - 1]);
                _mapRawImage.material.SetTexture("_Texture2", DataDownloader.Instance.MapImages[i]);
                timer = 0.0F;

                while (timer < 1.0F)
                {
                    timer += Time.unscaledDeltaTime / 0.5F;
                    _mapRawImage.material.SetFloat("_Fade", timer);
                    yield return null;
                }
            }

            yield return new WaitForSeconds(1);
            MainUI.Instance.StartInstructions();
            timer = 0.0F;

            while (timer < 1.0F)
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
}