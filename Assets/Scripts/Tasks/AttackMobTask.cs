using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MapTile = System.Tuple<int, int, int>;

public class AttackMobTask : AutoTask
{
	public override async Task<bool> IsActive(GameManager gameManager, PathManager pathManager, AutoControllerState state) {
        MapTile start = GetPlayerPosition(gameManager);
        int map = start.Item1, minDistance = Int32.MaxValue;
		bool targetFound = false;
        foreach(PlayerManager player in gameManager.GetAllPlayerManagers()) {
			if(player.IsPlayerMob()) {
				player.GetPlayerPosition(gameManager, out int x, out int y);
                MapTile goal = Tuple.Create(map, x, y);
                int simpleDistance = PathManager.DistanceHeuristic(start, goal);
				if(simpleDistance < minDistance) {
					LinkedList<MapTile> walkPath = null;
					if(await pathManager.TryGetWalkPath(gameManager, start, goal, value => walkPath = value)) {
						int distance = walkPath.Count;
						if(distance < minDistance) {
							minDistance = distance;
							state.SetAttackPoint(goal);
							targetFound = true;
						}
					}
				}
			}
		}
        return targetFound;
	}

	public override async Task Move(GameManager gameManager, PathManager pathManager, AutoControllerState state) {
        MapTile start = GetPlayerPosition(gameManager);
        MapTile goal = state.GetAttackPoint();
        if(goal != null) {
            await WalkToTile(gameManager, pathManager, state, start, goal);
        }
	}
}
