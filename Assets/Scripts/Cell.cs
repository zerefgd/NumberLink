using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [HideInInspector]
    public int Number
    {
        get
        {
            return number;
        }
        set
        {
            number = value;
            _numberText.text = number.ToString();
            if (number == 0)
            {
                _cellSprite.color = _solvedColor;
                _numberText.gameObject.SetActive(false);
            }
            else if (number < 0)
            {
                _cellSprite.color = _inCorrectColor;
                _numberText.gameObject.SetActive(false);
            }
            else
            {
                _cellSprite.color = _defaultColor;
                _numberText.gameObject.SetActive(true);
            }
        }

    }

    [HideInInspector] public int Row;
    [HideInInspector] public int Column;

    [SerializeField] private TMP_Text _numberText;
    [SerializeField] private SpriteRenderer _cellSprite;

    [SerializeField] private GameObject _right1;
    [SerializeField] private GameObject _right2;
    [SerializeField] private GameObject _top1;
    [SerializeField] private GameObject _top2;
    [SerializeField] private GameObject _left1;
    [SerializeField] private GameObject _left2;
    [SerializeField] private GameObject _bottom1;
    [SerializeField] private GameObject _bottom2;

    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _solvedColor;
    [SerializeField] private Color _inCorrectColor;

    private int number;
    private Dictionary<int, Dictionary<int, GameObject>> edges;
    private Dictionary<int, int> edgeCounts;
    private Dictionary<int, Cell> connectedCell;

    private const int RIGHT = 0;
    private const int TOP = 1;
    private const int LEFT = 2;
    private const int BOTTOM = 3;

    public void Init(int row, int col, int num)
    {
        Number = num;
        Row = row;
        Column = col;

        edgeCounts = new Dictionary<int, int>();
        edgeCounts[RIGHT] = 0;
        edgeCounts[LEFT] = 0;
        edgeCounts[TOP] = 0;
        edgeCounts[BOTTOM] = 0;

        connectedCell = new Dictionary<int, Cell>();
        connectedCell[LEFT] = null;
        connectedCell[TOP] = null;
        connectedCell[BOTTOM] = null;
        connectedCell[RIGHT] = null;

        edges = new Dictionary<int, Dictionary<int, GameObject>>();
        edges[RIGHT] = new Dictionary<int, GameObject>();
        edges[RIGHT][1] = _right1;
        edges[RIGHT][2] = _right2;

        edges[TOP] = new Dictionary<int, GameObject>();
        edges[TOP][1] = _top1;
        edges[TOP][2] = _top2;

        edges[LEFT] = new Dictionary<int, GameObject>();
        edges[LEFT][1] = _left1;
        edges[LEFT][2] = _left2;

        edges[BOTTOM] = new Dictionary<int, GameObject>();
        edges[BOTTOM][1] = _bottom1;
        edges[BOTTOM][2] = _bottom2;
    }

    public void Init()
    {
        for (int i = 0; i < 4; i++)
        {
            connectedCell[i] = GameManager.Instance.GetAdjacentCell(Row, Column, i);
            if (connectedCell[i] == null) continue;

            var singleEdge = edges[i][1].GetComponentInChildren<SpriteRenderer>();
            var doubleEdges = edges[i][2].GetComponentsInChildren<SpriteRenderer>();
            Vector2Int edgeOffset = new Vector2Int(
                connectedCell[i].Row - Row, connectedCell[i].Column - Column
                );
            float edgeSize = Mathf.Abs(edgeOffset.x) > Mathf.Abs(edgeOffset.y) ?
                Mathf.Abs(edgeOffset.x) : Mathf.Abs(edgeOffset.y);
            edgeSize *= GameManager.Instance.EdgeSize;
            ChangeSpriteSize(singleEdge, edgeSize);
            foreach (var item in doubleEdges)
            {
                ChangeSpriteSize(item, edgeSize);
            }
        }

        _right1.SetActive(false);
        _right2.SetActive(false);
        _bottom1.SetActive(false);
        _bottom2.SetActive(false);
        _left1.SetActive(false);
        _left2.SetActive(false);
        _top1.SetActive(false);
        _top2.SetActive(false);
    }

    public void AddEdge(int direction)
    {
        if (connectedCell[direction] == null) return;

        if (edgeCounts[direction] == 2)
        {
            RemoveEdge(direction);
            RemoveEdge(direction);
            return;
        }

        edgeCounts[direction]++;
        Number--;
        edges[direction][1].SetActive(false);
        edges[direction][2].SetActive(false);
        edges[direction][edgeCounts[direction]].SetActive(true);
    }

    public void RemoveEdge(int direction)
    {
        if (connectedCell[direction] == null ||
            edgeCounts[direction] == 0
            ) return;

        edgeCounts[direction]--;
        Number++;
        edges[direction][1].SetActive(false);
        edges[direction][2].SetActive(false);
        if (edgeCounts[direction] != 0)
        {
            edges[direction][edgeCounts[direction]].SetActive(true);
        }
    }

    public void RemoveAllEdges()
    {
        for (int i = 0; i < 4; i++)
        {
            RemoveEdge(i);
            RemoveEdge(i);
        }
    }

    private void ChangeSpriteSize(SpriteRenderer sprite, float size)
    {
        sprite.size = new Vector2(sprite.size.x, size);
    }

    public bool IsValidCell(Cell cell,int direction)
    {
        return connectedCell[direction] == cell;
    }
}
