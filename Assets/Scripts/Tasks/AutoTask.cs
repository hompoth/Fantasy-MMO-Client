using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using MapTile = System.Tuple<int, int, int>;
using WarpDevice = System.Tuple<string, int>;

public abstract class AutoTask : AutoBase
{
	public abstract Task<bool> IsActive(GameManager gameManager, PathManager pathManager, AutoControllerState state);
	public abstract Task Move(GameManager gameManager, PathManager pathManager, AutoControllerState state);

	protected bool IsSurrounded(GameManager gameManager, AutoControllerState state) {
		return state.IsSurrounded(gameManager);
	}

    protected async Task WalkToTile(GameManager gameManager, PathManager pathManager, AutoControllerState state, MapTile start, MapTile goal, int distanceFromGoal = 0) {
        LinkedList<MapTile> walkPath = null;
        if(await pathManager.TryGetWalkPath(gameManager, start, goal, value => walkPath = value)) {
            
            if(ClientManager.IsActiveGameManager(gameManager)) {    // TODO REMOVE
                string pathString = "Current Path:";
                foreach(MapTile tile in walkPath) {
                    pathString = pathString + " " + tile;  
                }
                Debug.Log(pathString);
            }
            
            
            if(walkPath.Count > distanceFromGoal + 1) {
                MapTile nextTile = walkPath.ElementAt(1);
                Vector3 direction = new Vector3(nextTile.Item2 - start.Item2, -(nextTile.Item3 - start.Item3), 0);
                gameManager.HandlePlayerPosition(direction, false);
            }
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
