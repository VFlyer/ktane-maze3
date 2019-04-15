using System;

class MapNode {
    
    public const int up = 0;
    public const int down = 1;
    public const int left = 2;
    public const int right = 3;

    public int id;

    public int[] path;
    public bool[] turn;

    public MapNode(int id, int[] path, bool[] turn)
    {
        this.id = id;
        this.path = path;
        this.turn = turn;
    }

    public int getPath(int coord)
    {
        return path[coord];
    }

    public bool turns(int coord)
    {
        return turn[coord];
    }
}