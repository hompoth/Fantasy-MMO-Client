﻿using System;
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

    protected async Task WalkToTile(GameManager gameManager, PathManager pathManager, AutoControllerState state, MapTile start, MapTile goal, PlayerManager player, int distanceFromGoal = 0) {
        LinkedList<MapTile> walkPath = null;
        if(await pathManager.TryGetWalkPath(gameManager, start, goal, player, value => walkPath = value)) {
            if(walkPath.Count > 1 && PathManager.DistanceHeuristic(start, goal) > distanceFromGoal) {
                MapTile nextTile = walkPath.ElementAt(1);
                Vector3 direction = new Vector3(nextTile.Item2 - start.Item2, -(nextTile.Item3 - start.Item3), 0);
                gameManager.HandlePlayerPosition(direction, false);
                Vector3 previous = default(Vector3);
                foreach(MapTile tile in walkPath) {
                    Vector3 current = gameManager.WorldPosition(tile.Item2, tile.Item3, true) + Vector3.forward;
                    if(!previous.Equals(default(Vector3))) {
                        Debug.DrawLine(previous, current, Color.red, 1f, false);
                    }
                    previous = current;
                }
            }
        }
    }

    // TODO Consider simply returning WalkToTile if samearea is true.
    protected async Task MoveToMapTile(GameManager gameManager, PathManager pathManager, AutoControllerState state, MapTile goal, PlayerManager player, int distanceFromGoal = 0) {
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
                    await WalkToTile(gameManager, pathManager, state, start, targetTile, player);
                }
            }
        }
    }
}
