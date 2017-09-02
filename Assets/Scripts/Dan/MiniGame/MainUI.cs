
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [SerializeField]
    private Image _progressBarFill;

	void Update ()
    {
        if (_progressBarFill.fillAmount < 1.0F)
        {
            _progressBarFill.fillAmount += Time.deltaTime;
        }
	}
}