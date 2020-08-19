using System;

[Obsolete("HexMovableData is deprecated, please use HexData instead.", true)]
public class HexMovableData
{
    public float height;
    public int plateId;
    public int oldPlateId;
    public Hex movedToHex;
    public bool formingMoutain;
    public bool isOcean;
    public bool isCoast;

}
