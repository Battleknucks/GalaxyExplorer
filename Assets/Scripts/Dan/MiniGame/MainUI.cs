
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [SerializeField]
    private Canvas _canvas;

    [SerializeField]
    private Image _progressBarFill;

    private void Awake()
    {
        _canvas.worldCamera = Camera.main;
    }

    void Update ()
    {
        if (_progressBarFill.fillAmount < 1.0F)
        {
            _progressBarFill.fillAmount += Time.deltaTime;
        }
	}
}