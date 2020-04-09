using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using MapTile = System.Tuple<int, int, int>;
using WarpDevice = System.Tuple<string, int>;

public abstract class AutoTask
{
	public abstract Task<bool> IsActive(GameManager gameManager, PathManager pathManager, AutoControllerState state);
	public abstract Task Move(GameManager gameManager, PathManager pathManager, AutoControllerState state);

	protected bool IsSurrounded(GameManager gameManager, AutoControllerState state) {
		return state.IsSurrounded(gameManager);
	}
    
    protected MapTile GetPlayerPosition(GameManager manager) {
        manager.GetMainPlayerPosition(out int map, out int x, out int y);
        return Tuple.Create(map, x, y);
    }

    protected async Task WalkToTile(GameManager gameManager, PathManager pathManager, AutoControllerState state, MapTile start, MapTile goal, int distanceFromGoal = 0) {
        LinkedList<MapTile> walkPath = await pathManager.GetWalkPath(gameManager, start, goal);
        if(walkPath.Count > distanceFromGoal + 1) {
            MapTile nextTile = walkPath.ElementAt(1);
            Vector3 direction = new Vector3(nextTile.Item2 - start.Item2, -(nextTile.Item3 - start.Item3), 0);
            gameManager.HandlePlayerPosition(direction, false);
        }
    }

    protected async Task MoveToMapTile(GameManager gameManager, PathManager pathManager, AutoControllerState state, MapTile goal, int distanceFromGoal = 0) {
        MapTile start = GetPlayerPosition(gameManager);
        Tuple<LinkedList<MapTile>, WarpDevice, int> mapPathInfo = await pathManager.GetMapPath(gameManager, start, goal);
        if(mapPathInfo != null) {
            LinkedList<MapTile> mapPath = mapPathInfo.Item1;
            WarpDevice warpDevice = mapPathInfo.Item2;
            int pathDistance = mapPathInfo.Item3;
            bool sameArea = await pathManager.IsSameArea(gameManager, start, goal);
            if(!sameArea || pathDistance > distanceFromGoal) {
                if(warpDevice != default(WarpDevice)) {
                    // Use spell/item
                }
                else if(mapPath.Count > 1) {
                    MapTile targetTile = mapPath.ElementAt(1);
                    await WalkToTile(gameManager, pathManager, state, start, targetTile);
                }
            }
        }
    }
}
