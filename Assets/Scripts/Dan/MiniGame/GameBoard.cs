// GameBoard.cs
// Written by Dan W.
//

using UnityEngine;
using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;

public class GameBoard : Singleton<GameBoard>
{
    [SerializeField]
    private GameObject _gamePiecePrefab;

    [SerializeField]
    private Transform _uiHolder;

    [SerializeField]
    private AudioClip _music;

    [SerializeField]
    private Sprite[] _hiddenPieceSprites;

    private AudioSource _thisAudioSource;
    private Transform _thisTransform;
    private Transform _boardHolder;
    private List<GamePiece> _piecePool;
    private List<GamePiece> _piecesOnBoard;
    private List<GamePiece> _revealedPieces;
    private bool _runningMatch;
    private List<Vector2> _boards = new List<Vector2>() { new Vector2(3, 4), new Vector2(4, 3), new Vector2(4, 4), new Vector2(4, 5), new Vector2(5, 4) };
    private List<float> _boardDurations = new List<float>() { 60.0F, 60.0F, 70.0F, 75.0F, 75.0F};
    private float _currentBoardDuration;

    private static System.Random _rng = new System.Random();

    private void Awake()
    {
        _thisTransform = GetComponent<Transform>();
        _thisAudioSource = GetComponent<AudioSource>();
        _runningMatch = false;
        SetupPool();
        MainUI.Instance.SetCanvasToMiniGame(_uiHolder);
    }

    #region Game

    // We are loaded into the mini game scene, and are ready to begin a game
    public void Init()
    {
        _boardHolder = GameObject.Find("BoardHolder").transform;
        StartCoroutine(BeginGame());
    }

    // Start playing the music
    public void StartAudio ()
    {
        _thisAudioSource.clip = _music;
        _thisAudioSource.loop = true;
        _thisAudioSource.Play();
    }

    // Start a new game
    public IEnumerator BeginGame()
    {
        yield return StartCoroutine(SetupBoard());
        yield return StartCoroutine(RevealAllPieces());
        MainUI.Instance.StartHacking(_currentBoardDuration);
    }

    // Cleanup the board and it's pieces
    public void CleanupGame()
    {
        StopAllCoroutines();

        for(int i = 0; i < _piecesOnBoard.Count; ++i)
        {
            ReturnPieceToPool(_piecesOnBoard[i]);
        }

        _piecesOnBoard.Clear();
        _thisAudioSource.Stop();
    }

    // The timer has run out
    public void HackCompleted ()
    {
        if(_piecesOnBoard.Count > 0)
        {
            LooseGame();
        }
    }

    // Check if the pieces the user has selected make a match
    private IEnumerator CheckForMatchRoutine ()
    {
        _runningMatch = true;
        bool match = false;
        yield return new WaitForSeconds(1);
       
        match = _revealedPieces[0].ID == _revealedPieces[1].ID;

        // No match, put the pieces back down on the board
        if (!match)
        {
            while (_revealedPieces.Count > 0)
            {
                _revealedPieces[0].ObscurePiece();
            }
        }

        // Match, remove pieces from the board
        if (match)
        {
            while (_revealedPieces.Count > 0)
            {
                yield return StartCoroutine(_revealedPieces[0].CollectPiece());
            }
        }

        _revealedPieces.Clear();
        yield return new WaitForEndOfFrame();

        // There are no more pieces on the board
        if(_piecesOnBoard.Count < 1)
        {
            WinGame();
        }

        _runningMatch = false;
    }

    // User has won
    private void WinGame ()
    {
        MainUI.Instance.Win();
        CleanupGame();
    }

    // User has lost
    private void LooseGame ()
    {
        MainUI.Instance.Loose();
        CleanupGame();
    }

    #endregion

    #region Board

    // Show the piece to the user
    public void RevealPiece (GamePiece pieceToReveal)
    {
        if(_revealedPieces.Contains(pieceToReveal) || _runningMatch)
        {
            return;
        }

        _revealedPieces.Add(pieceToReveal);
        pieceToReveal.RevealPiece();

        if (_revealedPieces.Count == 2)
        {
            StartCoroutine("CheckForMatchRoutine");
        }
    }

    // Resets the piece to default and returns it to the pool
    public void ResetPiece (GamePiece pieceToRemove, bool recycle = false)
    {
        _revealedPieces.Remove(pieceToRemove);

        if(recycle)
        {
            if(_piecesOnBoard.Contains(pieceToRemove))
            {
                _piecesOnBoard.Remove(pieceToRemove);
            }

            ReturnPieceToPool(pieceToRemove);
        }
    }

    // Show the user all the pieces at the begining of the game
    private IEnumerator RevealAllPieces ()
    {
        for(int i = 0; i < _piecesOnBoard.Count; ++i)
        {
            _piecesOnBoard[i].RevealPiece();
        }

        yield return new WaitForSeconds(2);

        for (int i = 0; i < _piecesOnBoard.Count; ++i)
        {
            _piecesOnBoard[i].ObscurePiece();
        }
    }

    // Setup the game board
    private IEnumerator SetupBoard ()
    {
        _piecesOnBoard = new List<GamePiece>();
        _revealedPieces = new List<GamePiece>();
        int rand = Random.Range(0, _boards.Count);
        Vector2 randVec = _boards[rand];
        _currentBoardDuration = _boardDurations[rand];
        int boardWidth = (int)randVec.x;
        int boardHeight = (int)randVec.y;
        int totalPairs = (boardWidth * boardHeight) / 2;
        List<int> nums = new List<int>();
        int counter = 0;

        // Ensure there are the exact number of pieces to make matches
        // 2 of each type
        for(int i = 0; i < totalPairs; ++i)
        {
            nums.Add(i);
            nums.Add(i);
        }

        Shuffle<int>(nums);

        yield return new WaitForSeconds(1);
        Vector2 startingPosition = new Vector2(((boardWidth * 128) / 2) * -1, ((boardHeight * 128) / 2) * -1);

        // Layout the board pieces
        for(int x = 0; x < boardWidth; ++x)
        {
            for(int y = 0; y < boardHeight; ++y)
            {
                Vector3 position = new Vector3(startingPosition.x + (x * 128), startingPosition.y + (y * 128), 0);
                GamePiece piece = GetPieceFromPool();
                piece.CachedTransform.SetParent(_boardHolder);
                piece.CachedTransform.localEulerAngles = new Vector3(45, 0, 0);
                piece.CachedTransform.localScale = Vector3.one;
                piece.SetID(nums[counter]);
                piece.SetHiddenImage(_hiddenPieceSprites[nums[counter]]);
                ++counter;
                _piecesOnBoard.Add(piece);
                piece.Activate(position);
                yield return new WaitForSeconds(0.2F);
            }
        }
    }

    #endregion

    #region Pooling

    private GamePiece GetPieceFromPool ()
    {
        GamePiece result = null;
        bool found = false;

        for(int i = 0; i < _piecePool.Count; ++i)
        {
            if(!_piecePool[i].InUse)
            {
                result = _piecePool[i];
                result.InUse = true;
                found = true;
                break;
            }
        }

        if(!found)
        {
            result = AddPieceToPool();
            result.InUse = true;
        }

        return result;
    }

    private GamePiece AddPieceToPool()
    {
        GamePiece result = null;
        GameObject obj = Instantiate(_gamePiecePrefab, Vector3.zero, Quaternion.identity) as GameObject;
        obj.transform.SetParent(_thisTransform);
        result = obj.GetComponent<GamePiece>();
        _piecePool.Add(result);

        return result;
    }

    private void ReturnPieceToPool(GamePiece piece)
    {
        piece.InUse = false;
        piece.CachedTransform.SetParent(_thisTransform);
        piece.Deactivate();
    }

    // Pre populate the pool
    private void SetupPool ()
    {
        _piecePool = new List<GamePiece>();

        for(int i = 0; i < 50; ++i)
        {
            GamePiece piece = AddPieceToPool();
            piece.Deactivate(true);
        }
    }

    #endregion

    #region Utility

    // Randomizes a list
    public static void Shuffle<T>(List<T> list)
    {
        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = _rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    #endregion
}