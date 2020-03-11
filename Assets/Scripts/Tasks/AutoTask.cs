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
	public abstract bool IsActive(GameManager gameManager, AutoControllerState state);
	public abstract Task Move(GameManager gameManager, AutoControllerState state);

	protected bool IsSurrounded(GameManager gameManager, AutoControllerState state) {
		return state.IsSurrounded(gameManager);
	}

    // TODO Check if current map, and if so, check against the loaded map instead
    // TODO Before choosing a tile, make sure it can walk to the target, or think of a way such that the goal will never be a blocked tile (other than players)
    private MapTile GetClosestUnblockedPosition(GameManager manager, MapTile start, MapTile goal) {
        int map = goal.Item1;
        Tilemap tilemap = GameManager.GetTileMap(map);
        bool sameMap = manager.GetMapId() == map;
        if(tilemap != null && IsPositionBlocked(manager, tilemap, sameMap, goal.Item2, goal.Item3)) {
            Queue<MapTile> neighbours = new Queue<MapTile>();
            Queue<MapTile> unblockedTiles = new Queue<MapTile>();
            neighbours.Enqueue(goal);
            while(neighbours.Count() > 0) {
                MapTile current = neighbours.Dequeue();
                int x = current.Item2, y = current.Item3;
                for(int i = -1; i <= 1; i++) {
                    for(int j = -1; j <= 1; j++) {
                        if(i != j && (i == 0 || j == 0)) {
                            MapTile newTile = Tuple.Create(map, x + i, y + j);
                            if(!IsPositionBlocked(manager, tilemap, sameMap, x + i, y + j)) {
                                unblockedTiles.Enqueue(newTile);
                            }
                            else if(unblockedTiles.Count == 0){
                                if(!neighbours.Contains(newTile)) {
                                    neighbours.Enqueue(newTile);
                                }
                            }
                        }
                    }
                }
            }
            int minDistance = Int32.MaxValue;
            MapTile closestTile = goal;
            while(unblockedTiles.Count > 0) {
                MapTile current = unblockedTiles.Dequeue();
                int currentDistance = DistanceHeuristic(start, current);
                if(currentDistance < minDistance) {
                    minDistance = currentDistance;
                    closestTile = current;
                }
            }
            return closestTile;
        }
        return goal;
    }

    private bool IsPositionBlocked(GameManager manager, Tilemap tilemap, bool sameMap, int x, int y) {
        if(sameMap) {
            return manager.IsWorldPositionBlocked(x, y);
        }
        else {
            return GameManager.IsMapPositionBlocked(tilemap, x, y);
        }
    }

	private int DistanceHeuristic(MapTile moveFrom, MapTile moveTo) {
		int xDiff = moveFrom.Item2 - moveTo.Item2;
		int yDiff = moveFrom.Item3 - moveTo.Item3;
		return Mathf.Abs(xDiff) + Mathf.Abs(yDiff);
	}

    protected async Task MoveToTile(GameManager gameManager, AutoControllerState state, MapTile goal, int distanceFromGoal = 0) {
        MapTile start = getCurrentLocation(gameManager);
        goal = GetClosestUnblockedPosition(gameManager, start, goal);
        Tuple<WarpDevice, LinkedList<MapTile>> mapPathInfo = await state.GetMapPath(gameManager, start, goal);
        WarpDevice warpDevice = mapPathInfo.Item1;
        LinkedList<MapTile> mapPath = mapPathInfo.Item2;
        if(DistanceHeuristic(start, goal) >= distanceFromGoal) {
            if(warpDevice != default(WarpDevice)) {
                // Use spell/item
            }
            else if(mapPath.Count > 1) {
                Debug.Log("HasMap --------"+(mapPath.First()));
                mapPath.RemoveFirst();
                MapTile targetTile = mapPath.First();
                Debug.Log(targetTile + " --------");
                LinkedList<MapTile> walkPath = await state.GetWalkPath(gameManager, start, targetTile);
                if(walkPath.Count > 1) {
                    Debug.Log("HasWalk --------"+(walkPath.First()));
                    walkPath.RemoveFirst();
                    Debug.Log(" --------"+(walkPath.First()));
                    MapTile nextTile = walkPath.First();
                    Vector3 direction = new Vector3(nextTile.Item2 - start.Item2, -(nextTile.Item3 - start.Item3), 0);
                    gameManager.HandlePlayerPosition(direction, false);
                }
            }
        }
    }

    private MapTile getCurrentLocation(GameManager gameManager) {
        gameManager.GetMainPlayerPosition(out int map, out int x, out int y);
        return Tuple.Create(map, x, y);
    }
}
