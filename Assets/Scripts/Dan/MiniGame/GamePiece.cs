// GamePiece.cs
// Written by Dan W.
//

using UnityEngine;
using UnityEngine.VR.WSA.Input;
using System.Collections;

public class GamePiece : GazeSelectionTarget
{
    [SerializeField]
    private SpriteRenderer _hiddenPiece;

    public bool InUse
    {
        get
        {
            return _inUse;
        }

        set
        {
            _inUse = value;
        }
    }

    public Transform CachedTransform
    {
        get
        {
            return _thisTransform;
        }
    }

    public int ID
    {
        get
        {
            return _id;
        }
    }

    private Transform _thisTransform;
    private Material _thisMaterial;
    private bool _inUse;
    private bool _selected;
    private bool _inGaze;
    private int _id;
    private bool _isAnimating;
    private float _moveToBoardDuration = 1.5F;
    private float _revealDuration = 0.5F;

    private void Awake()
    {
        _thisTransform = GetComponent<Transform>();
        _thisMaterial = GetComponentInChildren<Renderer>().material;
        _selected = false;
        _inGaze = false;
        _isAnimating = false;
        _hiddenPiece.enabled = false;
    }

    public void SetID (int newID)
    {
        _id = newID;
    }

    #region PieceInteraction

    public void Activate (Vector3 boardPosition)
    {
        gameObject.SetActive(true);

        if (!_isAnimating)
        {
            StartCoroutine(MoveToBoardRoutine(boardPosition));
        }
    }

    public void Deactivate (bool instant = false)
    {
        gameObject.SetActive(false);
    }

    public void SelectPiece ()
    {
        GameBoard.Instance.RevealPiece(this);
    }

    public void DeselectPiece (bool recycle = false)
    {
        _selected = false;
        GameBoard.Instance.ResetPiece(this, recycle);
    }

    public void RevealPiece ()
    {
        StartCoroutine(SelectRoutine());
    }

    public void ObscurePiece ()
    {
        _hiddenPiece.enabled = false;
        _thisMaterial.SetFloat("_Pulse", 0.0F);
        DeselectPiece();
    }

    public void ResetPiece ()
    {
        _hiddenPiece.enabled = false;
        _thisMaterial.SetFloat("_Pulse", 0.0F);
        DeselectPiece(true);
    }

    #endregion

    #region Animation

    private IEnumerator MoveToBoardRoutine (Vector3 destination)
    {
        _isAnimating = true;
        float i = 0.0F;
  
        while (i < 1.0F)
        {
            yield return null;
            i += Time.unscaledDeltaTime / _moveToBoardDuration;
            _thisTransform.localPosition = Vector3.Lerp(_thisTransform.localPosition, destination, i);
        }

        _isAnimating = false;
    }

    private IEnumerator SelectRoutine ()
    {
        _isAnimating = true;
        float i = 0.0F;

        while (i < 1.0F)
        {
            yield return null;
            i += Time.unscaledDeltaTime / (_revealDuration / 2);
            _thisTransform.localScale = Vector3.Lerp(_thisTransform.localScale, Vector3.one * 1.25F, i);
        }

        i = 0.0F;
        _hiddenPiece.enabled = true;
        _thisMaterial.SetFloat("_Pulse", 1.0F);

        while (i < 1.0F)
        {
            yield return null;
            i += Time.unscaledDeltaTime / (_revealDuration / 2);
            _thisTransform.localScale = Vector3.Lerp(_thisTransform.localScale, Vector3.one, i);
        }

        _isAnimating = false;
        _selected = true;
    }

    private IEnumerator DeSelectRoutine()
    {
        _isAnimating = true;
        float i = 0.0F;

        while (i < 1.0F)
        {
            yield return null;
            i += Time.unscaledDeltaTime / (_revealDuration / 2);
            _thisTransform.localScale = Vector3.Lerp(_thisTransform.localScale, Vector3.one, i);
        }

        _isAnimating = false;
    }

    #endregion

    #region HoloLensInteraction

    public override void OnGazeSelect()
    {
        _inGaze = true;
    }

    public override void OnGazeDeselect()
    {
        _inGaze = false;
    }

    public override bool OnTapped(InteractionSourceKind source, int tapCount, Ray ray)
    {
        if(_inGaze && !_isAnimating)
        {
            if(!_selected)
            {
                SelectPiece();
            }

            else if (_selected)
            {
                DeselectPiece();
            }
        }

        return _inGaze;
    }

    #endregion
}