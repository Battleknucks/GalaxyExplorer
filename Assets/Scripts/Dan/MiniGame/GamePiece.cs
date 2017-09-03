// GamePiece.cs
// Written by Dan W.
//

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR.WSA.Input;
using System.Collections;

public class GamePiece : GazeSelectionTarget
{
    [SerializeField]
    private Image _hiddenPiece;

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
        _selected = false;
        _inGaze = false;
        _isAnimating = false;
        _hiddenPiece.enabled = false;
    }

    // Set the ID that corresponds to the image on this piece
    public void SetID (int newID)
    {
        _id = newID;
    }

    // Set the sprite for this piece
    public void SetHiddenImage (Sprite sprite)
    {
        _hiddenPiece.sprite = sprite;
    }

    #region PieceInteraction

    // Turn this piece on and move it onto the board
    public void Activate (Vector3 boardPosition)
    {
        _thisTransform.localScale = Vector3.one;
        gameObject.SetActive(true);

        if (!_isAnimating)
        {
            StartCoroutine(MoveToBoardRoutine(boardPosition));
        }
    }

    // Turn this piece off
    public void Deactivate (bool instant = false)
    {
        gameObject.SetActive(false);
    }

    // User has selected this piece
    public void SelectPiece ()
    {
        GameBoard.Instance.RevealPiece(this);
    }

    // User/game has deselected this piece
    public void DeselectPiece (bool recycle = false)
    {
        _selected = false;
        GameBoard.Instance.ResetPiece(this, recycle);
    }

    // Show the piece image to the user
    public void RevealPiece ()
    {
        StartCoroutine(SelectRoutine());
    }

    // Hide the piece image from the user
    public void ObscurePiece ()
    {
        _hiddenPiece.enabled = false;
        DeselectPiece();
    }

    // This piece was part of a successfull match, 'collect' it
    public IEnumerator CollectPiece ()
    {
        _isAnimating = true;
        _hiddenPiece.enabled = false;
        float i = 0.0F;

        while (i < 1.0F)
        {
            yield return null;
            i += Time.unscaledDeltaTime / _revealDuration;
            _thisTransform.localScale = Vector3.Lerp(_thisTransform.localScale, Vector3.zero, i);
        }

        yield return new WaitForEndOfFrame();
        _isAnimating = false;
        DeselectPiece(true);
    }

    #endregion

    #region Animation

    // Fly the piece onto it's spot on the board
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

    // Show the piece image to the user, and animate graphic
    private IEnumerator SelectRoutine ()
    {
        _isAnimating = true;
        yield return new WaitForEndOfFrame();
        float i = 0.0F;

        while (i < 1.0F)
        {
            yield return null;
            i += Time.unscaledDeltaTime / (_revealDuration / 2);
            _thisTransform.localScale = Vector3.Lerp(_thisTransform.localScale, Vector3.one * 1.25F, i);
        }

        i = 0.0F;
        _hiddenPiece.enabled = true;

        while (i < 1.0F)
        {
            yield return null;
            i += Time.unscaledDeltaTime / (_revealDuration / 2);
            _thisTransform.localScale = Vector3.Lerp(_thisTransform.localScale, Vector3.one, i);
        }

        _isAnimating = false;
        _selected = true;
    }

    // Hide the piece image from the user, animate the graphic
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