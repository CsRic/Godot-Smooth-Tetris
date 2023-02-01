using Godot;
using System;
public struct SetPosition
{
    public int row;
    public int column;
    public int rotationType;
}
public class AIControl
{
    public int BestRow = 0, BestColumn = 0, BestRotation = 0;
    int[,] map;
    int rowsize, columnsize;
    char nowType, nextType;
    public float assessSituiation(int[,]map,int rowsize,int columnsize)
    {//simplist assessment. The more gap under a block in a column, the more return value;
        int i, j;
        float value;
        bool underOneBlock = false;
        bool inBlock = false;
        int totalGap = 0;
        value = 0;
        for (i = 0; i < columnsize;i++)
        {
            inBlock = false;
            underOneBlock = false;
            totalGap = 0;
            for (j = 0; j < rowsize;j++)
            {
                if(map[j,i]==1)
                {
                    inBlock = true;
                    underOneBlock = true;
                }
                if(map[j,i]==0)
                {
                    if(inBlock)
                    {
                        inBlock = false;
                        value += 1.0f;
                    }
                    if(underOneBlock)
                    {
                        totalGap += 1;
                    }
                }
            }
            value += totalGap * totalGap / 20f;
        }
        return value;
    }
    private void SearchSolution()
    {
        var nowTetromino = new Tetromino();
        nowTetromino.BuildTetromino(nowType);
        var nextTetromino = new Tetromino();
        nextTetromino.BuildTetromino(nextType);
        var m = new MainView();

        for (int x1 = 0; x1 < 4;x1++)
        {
            
        }
    }
    public SetPosition GiveInstruct()
    {
        SetPosition a = new SetPosition();
        a.column = BestColumn;
        a.row = BestRow;
        a.rotationType = BestRotation;
        return a;
    }
}
