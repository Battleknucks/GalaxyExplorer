// MapController.cs
// Written by Dan W.
//

using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;

public class MainUI : Singleton<MainUI>
{
    [SerializeField]
    private Canvas _canvas;

    [SerializeField]
    private MapController _mapController;

    [SerializeField]
    private Text _mainMessage;

    [SerializeField]
    private Image _progressBarBackdrop;

    [SerializeField]
    private Image _progressBarFill;

    private Transform _thisTransform;
    private Transform _canvasHolder;
    private List<string> _hackingMessages = new List<string>() { "IP Address detected - 172.168.250.64...", "Brute force attempted...", "Attempting to access usr/bin...", "IPSec interface comprimised...", "UDP Ports comprimised.." };
    private bool _isHacking;
    private float _hackDuration;
    private bool _messageIsDisplaying;
    private bool _introComplete;
    private bool _solarMessageDisplayed;
    private bool _earthMessageDisplayed;
    private bool _canvasInExploreMode;

    private class _messageData
    {
        public string Message;
        public bool Delay;

        public _messageData (string msg, bool delay = false)
        {
            Message = msg;
            Delay = delay;
        }
    }

    private void Awake()
    {
        _thisTransform = GetComponent<Transform>();
        _canvasHolder = _canvas.transform.parent;
        _mainMessage.text = string.Empty;
        _isHacking = false;
        _messageIsDisplaying = false;
        _introComplete = false;
        _solarMessageDisplayed = false;
        _earthMessageDisplayed = false;
        _mapController.Kill();
        _progressBarBackdrop.enabled = false;
        _progressBarFill.enabled = false;
        _canvasInExploreMode = false;
    }

    private void Update()
    {
        if (_isHacking || !_introComplete)
        {
            return;
        }

        if (ViewLoader.Instance != null && !_messageIsDisplaying)
        {
            switch (ViewLoader.Instance.CurrentView)
            {
                default:
                case (""):
                    break;

                case ("SolarSystemView"):
                    if (!_solarMessageDisplayed)
                    {
                        _solarMessageDisplayed = true;
                        StartCoroutine("DisplayMessage", new _messageData("The attack is occuring on earth!", true));
                    }
                    break;

                case ("EarthView"):
                    if (!_earthMessageDisplayed)
                    {
                        _earthMessageDisplayed = true;
                        StartCoroutine("DisplayMessage", new _messageData("The attack appears to be in the San Diego area of the United States!", true));
                    }
                    break;
            }
        }
    }

    public void GoBack()
    {
        if(!_canvasInExploreMode)
        {
            GameBoard.Instance.CleanupGame();
            SetCanvasToExplorer();
        }
    }

    public void Error(string message)
    {
        StartCoroutine("DisplayMessage", new _messageData(message));
    }

    public void DoIntro()
    {
        SetCanvasToExplorer();
        StartCoroutine("DisplayMessage", new _messageData("There is a cyber attack in progress in the Sol System..."));
        _introComplete = true;
    }

    public void SetCanvasToMiniGame(Transform holder)
    {
        _canvas.transform.SetParent(holder);
        _canvas.transform.localScale = Vector3.one * 0.0066F;
        _canvas.transform.localPosition = Vector3.zero;
        _canvas.transform.localEulerAngles = new Vector3(0, 180, 0);
        _canvas.renderMode = RenderMode.WorldSpace;
        _canvas.worldCamera = Camera.main;
        _canvasInExploreMode = false;
        _mapController.Init();
    }

    public void SetCanvasToExplorer()
    {
        StopAllCoroutines();
        _isHacking = false;
        _progressBarBackdrop.enabled = false;
        _progressBarFill.enabled = false;
        _earthMessageDisplayed = false;
        _canvas.transform.SetParent(_canvasHolder);
        _canvas.transform.localScale = Vector3.one * 0.00075F;
        _canvas.transform.localPosition = Vector3.zero;
        _canvas.transform.localEulerAngles = new Vector3(0, 180, 0);
        _canvas.renderMode = RenderMode.WorldSpace;
        _canvas.worldCamera = Camera.main;
        _canvasInExploreMode = true;
        _mapController.Kill();
    }

    public void StartLocating()
    {
        StartCoroutine("DisplayMessage", new _messageData("Locating targeted system..."));
    }

    public void StartInstructions()
    {
        StartCoroutine("DisplayMessage", new _messageData("System located... Circadence Inc. is under attack! Stop the hacker!"));
    }

    public void StartHacking(float duration)
    {
        if(_canvasInExploreMode)
        {
            return;
        }

        _isHacking = true;
        _hackDuration = duration;
        StartCoroutine("HackRoutine");
        StartCoroutine("HackBar");
    }

    private IEnumerator HackRoutine()
    {
        while (_isHacking)
        {
            yield return StartCoroutine("DisplayMessage", new _messageData(_hackingMessages[Random.Range(0, _hackingMessages.Count)]));
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator HackBar()
    {
        _progressBarBackdrop.enabled = true;
        _progressBarFill.fillAmount = 0.0F;
        _progressBarFill.enabled = true;

        while (_isHacking && _progressBarFill.fillAmount < 1.0F)
        {
            _progressBarFill.fillAmount += Time.unscaledDeltaTime / _hackDuration;
            yield return null;
        }
    }

    private IEnumerator DisplayMessage(_messageData data)
    {
        int i = 0;
        _mainMessage.text = string.Empty;

        if (data.Delay)
        {
            yield return new WaitForSeconds(2.5F);
        }

        while (i < data.Message.Length)
        {
            yield return new WaitForSeconds(Time.unscaledDeltaTime);
            _mainMessage.text += data.Message[i];
            ++i;
        }
    }
}