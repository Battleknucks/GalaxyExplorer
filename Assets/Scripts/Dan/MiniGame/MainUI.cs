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

    [SerializeField]
    private GameObject _robotObj;

    private Transform _thisTransform;
    private Transform _canvasHolder;
    // Random 'hacking' messages
    private List<string> _hackingMessages = new List<string>() { "IP Address detected - 172.168.250.64...", "Brute force attempted...", "Attempting to access usr/bin...", "IPSec interface comprimised...", "UDP Ports comprimised.." };
    private bool _isHacking;
    private float _hackDuration;
    private bool _messageIsDisplaying;
    private bool _introComplete;
    private bool _solarMessageDisplayed;
    private bool _earthMessageDisplayed;
    private bool _canvasInExploreMode;
    private Animator _robotAnimator;

    // Custom data to pass to a coroutine, since they can only accept 1 parameter when invoked by name
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
        _robotAnimator = _robotObj.GetComponent<Animator>();
        _canvasHolder = _canvas.transform.parent;
        _mainMessage.text = string.Empty;
        _isHacking = false;
        _messageIsDisplaying = false;
        _introComplete = false;
        _solarMessageDisplayed = false;
        _earthMessageDisplayed = false;
        _progressBarBackdrop.enabled = false;
        _progressBarFill.enabled = false;
        _canvasInExploreMode = false;
    }

    private void Start()
    {
        _mapController.Kill();
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

    // The user pressed the back button on the tool bar
    public void GoBack()
    {
        if(!_canvasInExploreMode)
        {
            GameBoard.Instance.CleanupGame();
            SetCanvasToExplorer();
        }
    }

    // Used to debug on device
    public void Error(string message)
    {
        StartCoroutine("DisplayMessage", new _messageData(message));
    }

    // Begin informing the user there is a cyber security threat
    public void DoIntro()
    {
        SetCanvasToExplorer();
        StartCoroutine("DisplayMessage", new _messageData("There is a cyber attack in progress in the Sol System..."));
        _introComplete = true;
    }

    // Put the UI in mini game mode
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

    // Put the UI in 'explore' mode for navigating the galaxy
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

    // Inform the user we are locating the affected system
    public void StartLocating()
    {
        StartCoroutine("DisplayMessage", new _messageData("Locating targeted system..."));
    }

    // Inform the user they must stop the hacker
    public void StartInstructions()
    {
        StartCoroutine("DisplayMessage", new _messageData("System located... Circadence Inc. is under attack! Stop the hacker!"));
    }

    // Start the 'countdown' proccess of the 'hacker' getting into the system
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

    // User has won the mini game
    public void Win ()
    {
        StopAllCoroutines();
        _robotObj.SetActive(true);
        _progressBarBackdrop.enabled = false;
        _progressBarFill.enabled = false;
        StartCoroutine("DisplayMessage", new _messageData("Congradulations, you successfully stopped the hacker."));
    }

    // User has lost the mini game
    public void Loose ()
    {
        StopAllCoroutines();
        _robotObj.SetActive(true);
        _progressBarBackdrop.enabled = false;
        _progressBarFill.enabled = false;
        StartCoroutine("DisplayMessage", new _messageData("The hacker successfully breached the system. You have failed.."));
    }

    // Display random messages about the hacker
    private IEnumerator HackRoutine()
    {
        while (_isHacking)
        {
            yield return StartCoroutine("DisplayMessage", new _messageData(_hackingMessages[Random.Range(0, _hackingMessages.Count)]));
            yield return new WaitForSeconds(1);
        }
    }

    // Update the progress bar
    private IEnumerator HackBar()
    {
        _robotObj.SetActive(false);
        _progressBarBackdrop.enabled = true;
        _progressBarFill.fillAmount = 0.0F;
        _progressBarFill.enabled = true;

        while (_isHacking && _progressBarFill.fillAmount < 1.0F)
        {
            _progressBarFill.fillAmount += Time.unscaledDeltaTime / _hackDuration;
            yield return null;
        }

        // We have reached the end of the timer
        GameBoard.Instance.HackCompleted();
    }

    // Displays a message in the UI marquee
    private IEnumerator DisplayMessage(_messageData data)
    {
        int i = 0;
        _mainMessage.text = string.Empty;

        if (data.Delay)
        {
            yield return new WaitForSeconds(2.5F);
        }

        _robotAnimator.SetTrigger("Talk");

        while (i < data.Message.Length)
        {
            yield return new WaitForSeconds(Time.unscaledDeltaTime);
            _mainMessage.text += data.Message[i];
            ++i;
        }
    }
}