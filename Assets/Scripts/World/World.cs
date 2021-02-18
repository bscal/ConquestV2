﻿using Conquest;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conquest
{
    public class World
    {
        public WorldSettings settings;

        public readonly Layout layout;
        public readonly Vector2Int size;
        public readonly Dictionary<Hex, TileObject> tileData;
        public readonly List<Plate> plates;

        public int numOfHexes;
        public int pixelW;
        public int pixelH;

        public int plateCounter;
        public int cells = 4;
        public WorldTemp worldTemp;
        public WindManager windManager;

        public int Equator { get { return size.y / 2; } }
        public int NorthPole { get { return size.y; } }
        public int SouthPole { get { return 0; } }

        public World(WorldSettings settings)
        {
            this.settings = settings;
            numOfHexes = settings.width * settings.height;
            pixelW = (int)(settings.width * (Mathf.Sqrt(3) * 8.0f));
            pixelH = (int)(settings.height * (16.0f * .75f));

            // Layout size.x is set to 9.1. Why? Work around for hex widths to line up properly. 
            // Couldnt not figure out the reason why
            layout = new Layout(Layout.pointy, new Point(9.1, 8.0), new Point(0, 0));
            size = new Vector2Int(settings.width, settings.height);
            tileData = new Dictionary<Hex, TileObject>();
            plates = new List<Plate>(settings.plates + 1);
            worldTemp = new WorldTemp(WorldTemp.EARTH_TEMP, GameManager.Singleton.Generator);
            windManager = new WindManager(WindManager.PROGRADE_SPIN, WindManager.CellLayout.EARTH, this);
        }

        public void UpdateValues()
        {
            float avgWorldTemp = 0;
            foreach (var pair in tileData)
            {
                avgWorldTemp = pair.Value.hexData.temp;
            }
            worldTemp.AvgTemp = avgWorldTemp / tileData.Count;
        }

        public bool ContainsHex(Hex hex)
        {
            return tileData.ContainsKey(hex.GetKey());
        }

        public bool TryGetHexData(Hex hex, out TileObject obj)
        {
            return GetHexData(hex, settings.wrapWorld, out obj);
        }

        public bool GetHexData(Hex hex, bool wrapIfNull, out TileObject obj)
        {
            if (wrapIfNull && !tileData.ContainsKey(hex) && !HexUtils.HexOutOfBounds(size, hex, wrapIfNull))
                hex = HexUtils.WrapOffset(hex, size.x);

            return tileData.TryGetValue(hex, out obj);
        }

        public TileObject GetWrappedHex(Hex hex)
        {
            return tileData[HexUtils.WrapOffset(hex, size.x)];
        }

        public Plate GetPlateByID(int id)
        {
            return plates.Find(plate => plate.id == id); 
        }

        public List<Plate> GetPlates() => plates;

        public int AddPlate(Plate plate)
        {
            plates.Add(plate);
            plate.id = plateCounter++;
            return plate.id;
        }

        public TileObject[] HexArrayToTileObjectArray(Hex[] hexes)
        {
            TileObject[] objs = new TileObject[hexes.Length];
            for (int i = 0; i < hexes.Length; i++)
            {
                Hex h = hexes[i];
                bool contains = TryGetHexData(h, out TileObject obj);
                if (contains)
                    objs[i] = obj;
                else
                    objs[i] = null;
            }
            return objs;
        }
    }
}
