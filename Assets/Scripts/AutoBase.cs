using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using MapTile = System.Tuple<int, int, int>;

public abstract class AutoBase 
{    
    protected MapTile GetPlayerPosition(GameManager manager) {
        manager.GetMainPlayerPosition(out int map, out int x, out int y);
        return Tuple.Create(map, x, y);
    }
}
