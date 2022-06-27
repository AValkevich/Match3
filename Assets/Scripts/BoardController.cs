using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardController : MonoBehaviour
{
    public delegate void Tile (int[] cellIndex, int tileIndex, Transform tile);
    public static event Tile CheckTile;

    public delegate void BoardGenerate(Transform[,] cellsTransformArray);
    public static event BoardGenerate BoardGenerateComplete;
    
    [SerializeField] private Transform[] tiles;
    [SerializeField] private Transform boardFolder;
    [SerializeField] private Transform backgroundFolder;
    [SerializeField] private Transform cell;

    private bool _isMatchFound, _isMatchInProgress, _isDestroyInProgress, _isNeedCheckMatchAllBoard;
    private List<Transform> _moveTilesList = new List<Transform>();
    private List<Transform> _destroyTilesTransformList = new List<Transform>();
    private List<Transform> _emptyCellsTransformList = new List<Transform>();
    private Transform[,] _cellsTransformArray;
    private int _lastSpawnTileIndex;
    
    private void OnEnable()
    {
        MatchController.MoveTile += MoveTile;
        MatchController.DestroyTilesList += DestroyTilesList;
        MatchController.noMatch += NoMatch;
        MatchController.checkMatchAllBoard += CheckMatchAllBoard;
    }
    
    private void OnDisable()
    {
        MatchController.MoveTile -= MoveTile;
        MatchController.DestroyTilesList -= DestroyTilesList;
        MatchController.noMatch -= NoMatch;
        MatchController.checkMatchAllBoard -= CheckMatchAllBoard;
    }

    private void Start()
    {
        _cellsTransformArray = new Transform[10, 10];
        FirstBoardGenerate();
    }
    
    private void FirstBoardGenerate()
    {
       
        int[,] tilesIndexArray = new int[10, 10];
        
        for (int row = 0; row < 10; row++)
        {
            for (int column = 0; column < 10; column++)
            {
                Vector2 spawnPosition = new Vector2(column * 100, row * -100f);

                int tileIndex = Random.Range(1, tiles.Length);
                if (row != 0 || column != 0)
                {
                    while (true)
                    {
                        if (row > 1)
                            if (tilesIndexArray[column, row - 1] == tileIndex && tilesIndexArray[column, row - 2] == tileIndex)
                            {
                                tileIndex = Random.Range(1, tiles.Length);
                                continue;
                            }

                        if (column > 1)
                            if (tilesIndexArray[column - 1, row] == tileIndex && tilesIndexArray[column - 2, row] == tileIndex)
                            {
                                tileIndex = Random.Range(1, tiles.Length);
                                continue;
                            }

                        break;
                    }
                }

                Transform cellBackground = Instantiate(tiles[0], backgroundFolder, false);
                cellBackground.localPosition = spawnPosition + new Vector2(50, -50);

                Transform cellClone = Instantiate(cell, boardFolder, false).transform;
                cellClone.name = $"{column} | {row} : {tileIndex}";
                cellClone.localPosition = spawnPosition + new Vector2(60, -60);
                cellClone.GetComponent<CellController>().SetCellIndex(new []{column,row});
                _cellsTransformArray[column, row] = cellClone;
                
                tilesIndexArray[column, row] = tileIndex;
                TileSpawn(tileIndex, _cellsTransformArray[column, row], column, row);
            }
        }
        BoardGenerateComplete?.Invoke(_cellsTransformArray);
    }

    private void TileSpawn(int tileIndex, Transform parent, int column, int row)
    {
       Transform tile = Instantiate(tiles[tileIndex], parent, false);
       tile.GetComponent<TileController>().SetTileIndex(tileIndex);
       parent.name = $"{column} | {row} : {tileIndex}";
    }

    private void MoveTile(Transform tileTransform)
    {
        StartCoroutine(MoveTileCoroutine(tileTransform));
    }
    
    private IEnumerator MoveTileCoroutine(Transform tileTransform)
    {
        _moveTilesList.Add(tileTransform);
        
        yield return new WaitForSecondsRealtime(0.1f);
        tileTransform.parent.SetAsLastSibling();
        
        Vector3 startPosition = tileTransform.localPosition;
        
        float progress = 0;
        while (progress < 1)
        {
            tileTransform.localPosition = Vector3.Lerp(startPosition, Vector3.zero, progress += Time.deltaTime * 3);
            yield return new WaitForFixedUpdate();
        }

        tileTransform.localPosition = Vector3.zero;
        _moveTilesList.Remove(tileTransform);
    }

    private void DestroyTilesList(List<Transform> destroyTilesTransformList)
    {
        _destroyTilesTransformList = _destroyTilesTransformList.Union(destroyTilesTransformList).ToList();
        _isMatchInProgress = false;
    }
    
    private void DestroyTiles()
    {
        List<Transform> destroyTilesTransformList = new List<Transform>(_destroyTilesTransformList);
        foreach (var tileTransform in destroyTilesTransformList)
        {
            _emptyCellsTransformList.Add(tileTransform.parent);
            Destroy(tileTransform.gameObject);
            _destroyTilesTransformList.Remove(tileTransform);
        }
    }
    
    private void CheckEmptyCellList()
    {
        List<Transform> emptyCellsTransformList = new List<Transform>(_emptyCellsTransformList);
        foreach (var emptyCellTransform in emptyCellsTransformList)
        {
            int[] cellIndex = emptyCellTransform.GetComponent<CellController>().GetCellIndex();
            CheckEmptyCell(cellIndex);
            _emptyCellsTransformList.Remove(emptyCellTransform);
        }
    }
    
    private void CheckEmptyCell(int[] cellIndex)
    {
        if(cellIndex[1]>0 && cellIndex[1]<10)
        {
            Transform checkCell = _cellsTransformArray[cellIndex[0], cellIndex[1]];
            Transform upperCell = _cellsTransformArray[cellIndex[0], cellIndex[1] - 1]; 

            if ((checkCell.childCount == 0) && (upperCell.childCount != 0))
            {
                {
                    Transform tileTransform = upperCell.GetChild(0);
                    tileTransform.SetParent(checkCell);
                    
                    MoveTile(tileTransform);

                    int[] upperCellIndex = {cellIndex[0], cellIndex[1] - 1};
                    CheckEmptyCell(upperCellIndex);
                }
            }
            
            if(cellIndex[1]<9)
            {
                Transform lowerCell = _cellsTransformArray[cellIndex[0], cellIndex[1] + 1];
                if ((checkCell.childCount != 0) && (lowerCell.childCount == 0))
                {
                    int[] lowerCellIndex = {cellIndex[0], cellIndex[1] + 1};
                    CheckEmptyCell(lowerCellIndex);
                }
                else if ((checkCell.childCount == 0) && (lowerCell.childCount == 0))
                {
                    int[] upperCellIndex = {cellIndex[0], cellIndex[1] - 1};
                    CheckEmptyCell(upperCellIndex);
                }
            }
        }
        else if(cellIndex[1] == 0)
        {
            Transform checkCell = _cellsTransformArray[cellIndex[0], 0];
            if (checkCell.childCount == 0)
            {
                int tileIndex;
                while (true)
                {
                    tileIndex = Random.Range(1, tiles.Length);
                    if (tileIndex != _lastSpawnTileIndex)
                    {
                        _lastSpawnTileIndex = tileIndex;
                        break;
                    }    
                }
                
                TileSpawn(tileIndex, checkCell, cellIndex[0],0);
                int[] lowerCellIndex = {cellIndex[0], 1};
                CheckEmptyCell(lowerCellIndex);
            } 
        }
    }

    private void CheckMatchAllBoard()
    {
        StartCoroutine(CheckMatchAllBoardCoroutine());
    }

    private IEnumerator CheckMatchAllBoardCoroutine()
    {
        while (true)
        {
            while (_moveTilesList.Count != 0)
            {
                yield return new WaitForFixedUpdate();  
            }
            
            for (int row = 0; row < 10; row++)
            {
                for (int column = 0; column < 10; column++)
                {
                    _isMatchInProgress = true;

                    int[] cellIndex = {column, row};
                    Transform cellTransform = _cellsTransformArray[column, row];
                
                    if (cellTransform.childCount != 0)
                    {
                        int tileIndex = cellTransform.GetChild(0).GetComponent<TileController>().GetTileIndex();

                        CheckTile?.Invoke(cellIndex, tileIndex, cellTransform.GetChild(0));
                        
                        while (_isMatchInProgress)
                        {
                            yield return new WaitForFixedUpdate();  
                        }
                    }
                }
            }
            
            if (_destroyTilesTransformList.Count != 0)
            {
                DestroyTiles();
                
                yield return new WaitForFixedUpdate();
                
                CheckEmptyCellList();
            }
            else
            {
                break;
            }
        }
    }

    private void NoMatch()
    {
        _isMatchInProgress = false;
    }
}
