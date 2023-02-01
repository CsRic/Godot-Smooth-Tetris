using Godot;
using System;

public static class GrandMap
{
    public static int RowSize = 27;
    public static int ColumnSize = 12;
    public static int[,] Map = new int[RowSize + 5, ColumnSize + 5];
}
