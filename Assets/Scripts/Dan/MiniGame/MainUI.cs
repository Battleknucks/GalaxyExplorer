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
    public Transform CanvasTransform
    {
        get
        {
            return _canvas.transform;
        }
    }

    [SerializeField]
    private Canvas _canvas;

    [SerializeField]
    private GameObject _mapController;

    [SerializeField]
    private Text _mainMessage;

    [SerializeField]
    private Image _progressBarBackdrop;

    [SerializeField]
    private Image _progressBarFill;

    private Transform _thisTransform;
    private List<string> _hackingMessages = new List<string>() { "IP Address detected - 172.168.250.64...", "Brute force attempted...", "Attempting to access usr/bin...", "IPSec interface comprimised...", "UDP Ports comprimised.." };
    private bool _isHacking;
    private float _hackDuration;
    private bool _messageIsDisplaying;
    private bool _introComplete;
    private bool _solarMessageDisplayed;
    private bool _earthMessageDisplayed;

    private void Awake()
    {
        _thisTransform = GetComponent<Transform>();
        _mainMessage.text = string.Empty;
        _isHacking = false;
        _messageIsDisplaying = false;
        _introComplete = false;
        _solarMessageDisplayed = false;
        _earthMessageDisplayed = false;
        _mapController.SetActive(false);
        _progressBarBackdrop.enabled = false;
        _progressBarFill.enabled = false;
    }

    private void Update()
    {
        if(_isHacking || !_introComplete)
        {
            return;
        }

        if(ViewLoader.Instance != null && !_messageIsDisplaying)
        {
            switch(ViewLoader.Instance.CurrentView)
            {
                default:
                case (""):
                    break;

                case("SolarSystemView"):
                    if (!_solarMessageDisplayed)
                    {
                        _solarMessageDisplayed = true;
                        StartCoroutine(DisplayMessage("The attack is occuring on earth!", true));
                    }
                    break;

                case ("EarthView"):
                    if (!_earthMessageDisplayed)
                    {
                        _earthMessageDisplayed = true;
                        StartCoroutine(DisplayMessage("The attack appears to be in the San Diego area of the United States!", true));
                    }
                    break;
            }
        }
    }

    public void Error (string message)
    {
        StartCoroutine(DisplayMessage(message));
    }

    public void DoIntro ()
    {
        _canvas.worldCamera = Camera.main;
        StartCoroutine(DisplayMessage("There is a cyber attack in progress in the Sol System..."));
        _introComplete = true;
    }

    public void SetupCanvas ()
    {
        _canvas.transform.localScale = Vector3.one * 0.0066F;
        _canvas.transform.localPosition = Vector3.zero;
        _canvas.transform.localEulerAngles = new Vector3(0, 180, 0);
        _canvas.renderMode = RenderMode.WorldSpace;
        _canvas.worldCamera = Camera.main;
        _mapController.SetActive(true);
    }

    public void StartLocating ()
    {
        StartCoroutine(DisplayMessage("Locating targeted system..."));
    }

    public void StartInstructions ()
    {
        StartCoroutine(DisplayMessage("System located... Circadence Inc. is under attack! Stop the hacker!"));
    }

    public void StartHacking(float duration)
    {
        _isHacking = true;
        _hackDuration = duration;
        StartCoroutine(HackRoutine());
        StartCoroutine(HackBar());
    }
     
    private IEnumerator HackRoutine ()
    {
        while (_isHacking)
        {
            yield return StartCoroutine(DisplayMessage(_hackingMessages[Random.Range(0, _hackingMessages.Count)]));
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator HackBar ()
    {
        _progressBarBackdrop.enabled = true;
        _progressBarFill.fillAmount = 0.0F;
        _progressBarFill.enabled = true;

        while(_progressBarFill.fillAmount < 1.0F)
        {
            _progressBarFill.fillAmount += Time.unscaledDeltaTime / _hackDuration;
            yield return null;
        }
    }

    private IEnumerator DisplayMessage (string message, bool delay = false)
    {
        int i = 0;
        _mainMessage.text = string.Empty;

        if(delay)
        {
            yield return new WaitForSeconds(2.5F);
        }

        while(i < message.Length)
        {
            yield return new WaitForSeconds(Time.unscaledDeltaTime);
            _mainMessage.text += message[i];
            ++i;
        }
    }
}