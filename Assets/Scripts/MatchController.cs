using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    public static event Action noMatch;
    public static event Action checkMatchAllBoard;

    public delegate void TileTransform (Transform tileTransform);
    public static event TileTransform MoveTile;
    
    public delegate void TileTransformList (List<Transform> tilesTransformList);
    public static event TileTransformList DestroyTilesList;

    private Transform[,] _cellsTransformArray;

    private void OnEnable()
    {
        TileController.CheckSwipe += CheckSwipe;
        BoardController.CheckTile += CheckTIle;
        BoardController.BoardGenerateComplete += GetBoardInformation;
    }
    
    private void OnDisable()
    {
        TileController.CheckSwipe -= CheckSwipe;
        BoardController.CheckTile -= CheckTIle;
        BoardController.BoardGenerateComplete -= GetBoardInformation;
    }
    
    private void GetBoardInformation(Transform[,] cellsTransformArray)
    {
        _cellsTransformArray = new Transform[cellsTransformArray.Length, cellsTransformArray.Length];
        _cellsTransformArray = cellsTransformArray;
    }
    
    private void CheckSwipe(int[] firstCellIndex, int firstTileIndex, Vector2 direction)
    {
        int[] secondCellIndex = new int[2];
        Array.Copy(firstCellIndex, secondCellIndex,firstCellIndex.Length);

        if (direction == Vector2.right)
            secondCellIndex[0]++;
        else if (direction == Vector2.left)
            secondCellIndex[0]--;
        else if (direction == Vector2.up)
            secondCellIndex[1]--;
        else if (direction == Vector2.down)
            secondCellIndex[1]++;
        
        Transform firstTile = _cellsTransformArray[firstCellIndex[0], firstCellIndex[1]].GetChild(0);
        Transform secondCell = _cellsTransformArray[secondCellIndex[0], secondCellIndex[1]];
        
        if (secondCell.childCount != 0)
        {
            Transform secondTile = secondCell.GetChild(0);
            firstTile.SetParent(_cellsTransformArray[secondCellIndex[0], secondCellIndex[1]]);
            secondTile.SetParent(_cellsTransformArray[firstCellIndex[0], firstCellIndex[1]]);
            
            List<Transform> matchTilesList = CheckMatch(secondCellIndex, firstTileIndex);
            if (matchTilesList.Count != 0)
            {
                matchTilesList.Add(firstTile);
                DestroyMatchTiles(matchTilesList);
            
                MoveTile?.Invoke(secondTile);
                checkMatchAllBoard?.Invoke();
            }
            else
            {
                firstTile.SetParent(_cellsTransformArray[firstCellIndex[0], firstCellIndex[1]]);
                secondTile.SetParent(_cellsTransformArray[secondCellIndex[0], secondCellIndex[1]]);
            
                MoveTile?.Invoke(firstTile);
            }
        }
        else
        {
            firstTile.SetParent(_cellsTransformArray[firstCellIndex[0], firstCellIndex[1]]);
            MoveTile?.Invoke(firstTile);
        }
    }

    private void CheckTIle(int[] cellIndex, int tileIndex, Transform tile)
    {
        List<Transform> matchTilesList = CheckMatch(cellIndex, tileIndex);
        
        if (matchTilesList.Count != 0)
        {
            matchTilesList.Add(tile);
            DestroyMatchTiles(matchTilesList);
        }
        else
        {
            noMatch?.Invoke();   
        }
    }

    private List<Transform> CheckMatch(int[] cellIndex, int tileIndex)
    {
        List<Transform> matchTilesList = new List<Transform>();
        
        int right1Index = 0, right2Index = 0, left1Index = 0, left2Index = 0;
        int down1Index = 0, down2Index = 0, up1Index = 0, up2Index = 0;
        
        if (cellIndex[0] != 9)
        {
            Transform right1Cell = _cellsTransformArray[cellIndex[0] + 1, cellIndex[1]];
            if (right1Cell.childCount != 0)
            {
                right1Index = right1Cell.GetChild(0).GetComponent<TileController>().GetTileIndex();

                if (cellIndex[0] < 8 && right1Index == tileIndex)
                {
                    Transform right2Cell = _cellsTransformArray[cellIndex[0] + 2, cellIndex[1]];
                    if (right2Cell.childCount != 0)
                    {
                        right2Index = right2Cell.GetChild(0).GetComponent<TileController>().GetTileIndex();    
                    }
                }    
            }
        }

        if (cellIndex[0] != 0)
        {
            Transform left1Cell = _cellsTransformArray[cellIndex[0] - 1, cellIndex[1]];
            if (left1Cell.childCount != 0)
            {
                left1Index = left1Cell.GetChild(0).GetComponent<TileController>().GetTileIndex();

                if (cellIndex[0] > 1 && left1Index == tileIndex)
                {
                    Transform left2Cell = _cellsTransformArray[cellIndex[0] - 2, cellIndex[1]];
                    if (left2Cell.childCount != 0)
                    {
                        left2Index = left2Cell.GetChild(0).GetComponent<TileController>().GetTileIndex();
                    }
                }  
            }
        }

        if (cellIndex[1] != 9)
        {
            Transform down1Cell = _cellsTransformArray[cellIndex[0], cellIndex[1] + 1];
            if (down1Cell.childCount != 0)
            {
                down1Index = down1Cell.GetChild(0).GetComponent<TileController>().GetTileIndex();

                if (cellIndex[1] < 8 && down1Index == tileIndex)
                {
                    Transform down2Cell = _cellsTransformArray[cellIndex[0], cellIndex[1] + 2];
                    if (down2Cell.childCount != 0)
                    {
                        down2Index = down2Cell.GetChild(0).GetComponent<TileController>().GetTileIndex();
                    }
                }
            }
        }
        
        if (cellIndex[1] != 0)
        {
            Transform up1Cell = _cellsTransformArray[cellIndex[0], cellIndex[1] - 1];
            if (up1Cell.childCount != 0)
            {
                up1Index = up1Cell.GetChild(0).GetComponent<TileController>().GetTileIndex();

                if (cellIndex[1] > 1 && up1Index == tileIndex)
                {
                    Transform up2Cell = _cellsTransformArray[cellIndex[0], cellIndex[1] - 2];
                    if (up2Cell.childCount != 0)
                    {
                        up2Index = up2Cell.GetChild(0).GetComponent<TileController>().GetTileIndex();
                    }
                } 
            }
        }

        if (right1Index == tileIndex && left1Index == tileIndex)
        {
            matchTilesList.Add(_cellsTransformArray[cellIndex[0] + 1, cellIndex[1]].GetChild(0));
            matchTilesList.Add(_cellsTransformArray[cellIndex[0] - 1, cellIndex[1]].GetChild(0));
        }
        
        if (down1Index == tileIndex && up1Index == tileIndex)
        {
            matchTilesList.Add(_cellsTransformArray[cellIndex[0], cellIndex[1] + 1].GetChild(0));
            matchTilesList.Add(_cellsTransformArray[cellIndex[0], cellIndex[1] - 1].GetChild(0));
        }
        
        if (right1Index == tileIndex && right2Index == tileIndex)
        {
            matchTilesList.Add(_cellsTransformArray[cellIndex[0] + 1, cellIndex[1]].GetChild(0));
            matchTilesList.Add(_cellsTransformArray[cellIndex[0] + 2, cellIndex[1]].GetChild(0));
        }
        
        if (left1Index == tileIndex && left2Index == tileIndex)
        {
            matchTilesList.Add(_cellsTransformArray[cellIndex[0] - 1, cellIndex[1]].GetChild(0));
            matchTilesList.Add(_cellsTransformArray[cellIndex[0] - 2, cellIndex[1]].GetChild(0));
        }
        
        if (down1Index == tileIndex && down2Index == tileIndex)
        {
            matchTilesList.Add(_cellsTransformArray[cellIndex[0], cellIndex[1] + 1].GetChild(0));
            matchTilesList.Add(_cellsTransformArray[cellIndex[0], cellIndex[1] + 2].GetChild(0));
        }
        
        if (up1Index == tileIndex && up2Index == tileIndex)
        {
            matchTilesList.Add(_cellsTransformArray[cellIndex[0], cellIndex[1] - 1].GetChild(0));
            matchTilesList.Add(_cellsTransformArray[cellIndex[0], cellIndex[1] - 2].GetChild(0));
        }

        return matchTilesList;
    }

    private void DestroyMatchTiles(List<Transform> matchTilesList)
    {
        matchTilesList = matchTilesList.Distinct().ToList();
        DestroyTilesList?.Invoke(matchTilesList);
    }

}

