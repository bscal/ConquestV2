using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class River
{

    public Hex start;
    public Hex end;

    public int startPoint;
    public int endPoint;

    public int riverType;
    public int riverFlow;

}

public class HexPoint
{

    public const int SE         = 0;
    public const int NE         = 1;
    public const int N          = 2;
    public const int NW         = 3;
    public const int SW         = 4;
    public const int S          = 5;
    public const int CENTER     = 6;

}
