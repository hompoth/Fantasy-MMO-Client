using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapTile = System.Tuple<int, int, int>;
using WarpDevice = System.Tuple<string, int>;

public abstract class AutoTask
{
	public abstract bool IsActive(GameManager gameManager, AutoControllerState state);
	public abstract void Move(GameManager gameManager, AutoControllerState state);

	protected bool IsSurrounded(GameManager gameManager, AutoControllerState state) {
		return state.IsSurrounded(gameManager);
	}

    protected void MoveToTile(GameManager gameManager, AutoControllerState state, MapTile goal) {
        MapTile start = getCurrentLocation(gameManager);
        LinkedList<MapTile> mapPath = state.GetMapPath(gameManager, start, goal, out WarpDevice warpDevice);
        if(warpDevice != default(WarpDevice)) {
            // Use spell/item
        }
        else if(mapPath.Count > 1) {
            mapPath.RemoveFirst();
            MapTile targetTile = mapPath.First();
            LinkedList<MapTile> walkPath = state.GetWalkPath(gameManager, start, targetTile);
            if(walkPath.Count > 1) {
                walkPath.RemoveFirst();
                MapTile nextTile = walkPath.First();
                Vector3 direction = new Vector3(goal.Item2 - start.Item2, -(goal.Item3 - start.Item3), 0);
                gameManager.HandlePlayerPosition(direction, false);
            }
        }
    }

    private MapTile getCurrentLocation(GameManager gameManager) {
        gameManager.GetMainPlayerPosition(out int map, out int x, out int y);
        return Tuple.Create(map, x, y);
    }
}
