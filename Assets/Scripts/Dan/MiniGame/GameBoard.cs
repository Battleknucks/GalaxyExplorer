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
    private Transform _boardHolder;

    private Transform _thisTransform;
    private List<GamePiece> _piecePool;
    private List<GamePiece> _piecesOnBoard;
    private List<GamePiece> _revealedPieces;
    private bool _runningMatch;
    private List<Vector2> _boards = new List<Vector2>() { new Vector2(3, 4), new Vector2(4, 3), new Vector2(4, 4), new Vector2(4, 5), new Vector2(5, 4) };
   
    private static System.Random _rng = new System.Random();

    private void Awake()
    {
        _thisTransform = GetComponent<Transform>();
        _runningMatch = false;
        SetupPool();
    }

    #region Game

    public void Init()
    {
        StartCoroutine(BeginGame());
    }

    public IEnumerator BeginGame()
    {
        yield return StartCoroutine(SetupBoard());
        yield return StartCoroutine(RevealAllPieces());
    }

    private IEnumerator CheckForMatchRoutine ()
    {
        _runningMatch = true;
        bool match = false;
        yield return new WaitForSeconds(1);
       
        match = _revealedPieces[0].ID == _revealedPieces[1].ID;

        if (!match)
        {
            while (_revealedPieces.Count > 0)
            {
                _revealedPieces[0].ObscurePiece();
            }
        }

        if (match)
        {
            while (_revealedPieces.Count > 0)
            {
                _revealedPieces[0].ResetPiece();
            }

            yield return new WaitForSeconds(1);
        }

        _revealedPieces.Clear();
        _runningMatch = false;
    }

    #endregion

    #region Board

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
            StartCoroutine(CheckForMatchRoutine());
        }
    }

    public void ResetPiece (GamePiece pieceToRemove, bool recycle = false)
    {
        _revealedPieces.Remove(pieceToRemove);

        if(recycle)
        {
            ReturnPieceToPool(pieceToRemove);
        }
    }

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

    private IEnumerator SetupBoard ()
    {
        _piecesOnBoard = new List<GamePiece>();
        _revealedPieces = new List<GamePiece>();
        Vector2 rand = _boards[Random.Range(0, _boards.Count)];
        int boardWidth = (int)rand.x;
        int boardHeight = (int)rand.y;
        int totalPairs = (boardWidth * boardHeight) / 2;
        List<int> nums = new List<int>();
        int counter = 0;

        for(int i = 0; i < totalPairs; ++i)
        {
            nums.Add(i);
            nums.Add(i);
        }

        Shuffle<int>(nums);

        yield return new WaitForSeconds(1);

        for(int x = 0; x < boardWidth; ++x)
        {
            for(int y = 0; y < boardHeight; ++y)
            {
                GamePiece piece = GetPieceFromPool();
                piece.CachedTransform.SetParent(_boardHolder);
                piece.CachedTransform.localEulerAngles = new Vector3(45, 0, 0);
                piece.CachedTransform.localScale = Vector3.one;
                piece.SetID(nums[counter]);
                ++counter;
                _piecesOnBoard.Add(piece);
                piece.Activate(new Vector3(x, y, 0));
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