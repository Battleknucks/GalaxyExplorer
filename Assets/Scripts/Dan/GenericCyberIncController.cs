// GenericCyberIncController.cs
// Written by Dan W.
//

using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Unity;
using System.Collections;

public class GenericCyberIncController : Singleton<GenericCyberIncController>
{
    [SerializeField]
    private Text _text;

    private Canvas _canvas;
    private string _textStr = "San Diego, California - USA";
    private float _revealIncriment = 3.0F;
    private bool _revealing;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.worldCamera = Camera.main;
        _text.text = string.Empty;
        _text.enabled = false;
        _revealing = false;
    }

    public void Activate()
    {
        if (!_revealing)
        {
            StartCoroutine(TextReveal());
        }
    }

    // Type out the text on the POI floating over earth
    private IEnumerator TextReveal ()
    {
        _revealing = true;
        yield return new WaitForSeconds(1);
        _text.enabled = true;

        for(int i = 0; i < _textStr.Length; ++i)
        { 
            yield return new WaitForSeconds(Time.deltaTime / _revealIncriment);
            _text.text += _textStr[i];
        }

        _revealing = false;
    }
}