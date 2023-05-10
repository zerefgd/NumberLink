using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [HideInInspector] public bool hasGameFinished;

    public float EdgeSize => _cellGap + _cellSize;

    [SerializeField] private Cell _cellPrefab;
    [SerializeField] private SpriteRenderer _bgSprite;
    [SerializeField] private SpriteRenderer _highlightSprite;
    [SerializeField] private Vector2 _highlightSize;
    [SerializeField] private LevelData _levelData;
    [SerializeField] private float _cellGap;
    [SerializeField] private float _cellSize;
    [SerializeField] private float _levelGap;

    private int[,] levelGrid;
    private Cell[,] cellGrid;
    private Cell startCell;
    private Vector2 startPos;

    private List<Vector2Int> Directions = new List<Vector2Int>()
    { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

    private void Awake()
    {
        Instance = this;
        hasGameFinished = false;
        _highlightSprite.gameObject.SetActive(false);
        levelGrid = new int[_levelData.row, _levelData.col];
        cellGrid = new Cell[_levelData.row, _levelData.col];
        for (int i = 0; i < _levelData.row; i++)
        {
            for (int j = 0; j < _levelData.col; j++)
            {
                levelGrid[i, j] = _levelData.data[i * _levelData.row + j];
            }
        }

        SpawnLevel();
    }

    private void SpawnLevel()
    {
        float width = (_cellSize + _cellGap) * _levelData.col - _cellGap + _levelGap;
        float height = (_cellSize + _cellGap) * _levelData.row - _cellGap + _levelGap;
        _bgSprite.size = new Vector2(width, height);
        Vector3 bgPos = new Vector3(
            width / 2f - _cellSize / 2f - _levelGap / 2f,
            height / 2f - _cellSize / 2f - _levelGap / 2f,
            0
            );
        _bgSprite.transform.position = bgPos;

        Camera.main.orthographicSize = width * 1.2f;
        Camera.main.transform.position = new Vector3(bgPos.x, bgPos.y, -10f);

        Vector3 startPos = Vector3.zero;
        Vector3 rightOffset = Vector3.right * (_cellSize + _cellGap);
        Vector3 topOffset = Vector3.up * (_cellSize + _cellGap);

        for (int i = 0; i < _levelData.row; i++)
        {
            for (int j = 0; j < _levelData.col; j++)
            {
                Vector3 spawnPos = startPos + j * rightOffset + i * topOffset;
                Cell tempCell = Instantiate(_cellPrefab, spawnPos, Quaternion.identity);
                tempCell.Init(i, j, levelGrid[i, j]);
                cellGrid[i, j] = tempCell;
                if (tempCell.Number == 0)
                {
                    Destroy(tempCell.gameObject);
                    cellGrid[i, j] = null;
                }
            }
        }

        for (int i = 0; i < _levelData.row; i++)
        {
            for (int j = 0; j < _levelData.col; j++)
            {
                if (cellGrid[i, j] != null)
                {
                    cellGrid[i, j].Init();
                }
            }
        }
    }

    private void Update()
    {
        if (hasGameFinished) return;

        if (Input.GetMouseButtonDown(0))
        {
            startCell = null;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            startPos = mousePos2D;
            if (hit && hit.collider.TryGetComponent(out startCell))
            {
                _highlightSprite.gameObject.SetActive(true);
                _highlightSprite.size = _highlightSize;
                _highlightSprite.transform.position = startCell.transform.position;
            }
            else
            {
                hit = Physics2D.Raycast(mousePos, Vector2.left);
                if(hit && hit.collider.TryGetComponent(out startCell))
                {
                    startCell.RemoveEdge(0);
                }
                hit = Physics2D.Raycast(mousePos, Vector2.down);
                if (hit && hit.collider.TryGetComponent(out startCell))
                {
                    startCell.RemoveEdge(1);
                }
                hit = Physics2D.Raycast(mousePos, Vector2.right);
                if (hit && hit.collider.TryGetComponent(out startCell))
                {
                    startCell.RemoveEdge(2);
                }
                hit = Physics2D.Raycast(mousePos, Vector2.up);
                if (hit && hit.collider.TryGetComponent(out startCell))
                {
                    startCell.RemoveEdge(3);
                }
                startCell = null;
            }

        }
        else if (Input.GetMouseButton(0))
        {
            if (startCell == null) return;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            Vector2 offset = mousePos2D - startPos;
            Vector2Int offsetDirection = GetDirection(offset);
            float offsetValue = GetOffset(offset, offsetDirection);
            int directionIndex = GetDirectionIndex(offsetDirection);
            Vector3 angle = new Vector3(0, 0, 90f * (directionIndex - 1));
            _highlightSprite.size = new Vector2(_highlightSize.x, offsetValue);
            _highlightSprite.transform.eulerAngles = angle;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (startCell == null) return;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if(hit && hit.collider.TryGetComponent(out Cell endCell))
            {
                if(endCell == startCell)
                {
                    startCell.RemoveAllEdges();
                    for (int i = 0; i < 4; i++)
                    {
                        var adjacentCell = GetAdjacentCell(startCell.Row, startCell.Column,i);
                        if(adjacentCell != null)
                        {
                            int adjacentDirection = (i + 2) % 4;
                            adjacentCell.RemoveEdge(adjacentDirection);
                            adjacentCell.RemoveEdge(adjacentDirection);
                        }
                    }
                }
                else
                {
                    Vector2 offset = mousePos2D - startPos;
                    Vector2Int offsetDirection = GetDirection(offset);
                    int directionIndex = GetDirectionIndex(offsetDirection);
                    if(startCell.IsValidCell(endCell, directionIndex))
                    {
                        startCell.AddEdge(directionIndex);
                        endCell.AddEdge((directionIndex + 2) % 4);
                    }
                }
            }
            startCell = null;
            _highlightSprite.gameObject.SetActive(false);
            CheckWin();
        }
    }

    private void CheckWin()
    {
        for (int i = 0; i < _levelData.row; i++)
        {
            for (int j = 0; j < _levelData.col; j++)
            {
                if (cellGrid[i, j] != null && cellGrid[i, j].Number != 0) return; ;
            }
        }

        hasGameFinished = true;
    }

    private int GetDirectionIndex(Vector2Int offsetDirection)
    {
        int result = 0;
        if (offsetDirection == Vector2Int.right)
        {
            result = 0;
        }
        if (offsetDirection == Vector2Int.left)
        {
            result = 2;
        }
        if (offsetDirection == Vector2Int.up)
        {
            result = 1;
        }
        if (offsetDirection == Vector2Int.down)
        {
            result = 3;
        }
        return result;
    }

    private float GetOffset(Vector2 offset, Vector2Int offsetDirection)
    {
        float result = 0;
        if (offsetDirection == Vector2Int.left || offsetDirection == Vector2Int.right)
        {
            result = Mathf.Abs(offset.x);
        }
        if (offsetDirection == Vector2Int.up || offsetDirection == Vector2Int.down)
        {
            result = Mathf.Abs(offset.y);
        }
        return result;
    }

    private Vector2Int GetDirection(Vector2 offset)
    {
        Vector2Int result = Vector2Int.zero;

        if (Mathf.Abs(offset.y) > Mathf.Abs(offset.x) && offset.y > 0)
        {
            result = Vector2Int.up;
        }
        if (Mathf.Abs(offset.y) > Mathf.Abs(offset.x) && offset.y < 0)
        {
            result = Vector2Int.down;
        }
        if (Mathf.Abs(offset.y) < Mathf.Abs(offset.x) && offset.x > 0)
        {
            result = Vector2Int.right;
        }
        if (Mathf.Abs(offset.y) < Mathf.Abs(offset.x) && offset.x < 0)
        {
            result = Vector2Int.left;
        }

        return result;
    }

    public Cell GetAdjacentCell(int row, int col, int direction)
    {
        Vector2Int currentDirection = Directions[direction];
        Vector2Int startPos = new Vector2Int(row, col);
        Vector2Int checkPos = startPos + currentDirection;
        while (IsValid(checkPos) && cellGrid[checkPos.x, checkPos.y] == null)
        {
            checkPos += currentDirection;
        }
        return IsValid(checkPos) ? cellGrid[checkPos.x, checkPos.y] : null;
    }

    public bool IsValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < _levelData.row && pos.y < _levelData.col;
    }
}

[Serializable]
public struct LevelData
{
    public int row, col;
    public List<int> data;
}
