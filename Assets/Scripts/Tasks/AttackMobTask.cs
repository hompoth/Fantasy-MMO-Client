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
		// TODO Sort players based on DistanceHeuristic first and loop through until the heuristic distance is more than the min actual distance.
        foreach(PlayerManager player in gameManager.GetAllPlayerManagers()) {
			if(player.IsPlayerMob()) {
                MapTile goal = GetPlayerPosition(gameManager, player);
                int simpleDistance = PathManager.DistanceHeuristic(start, goal);
				if(simpleDistance < minDistance) {
					LinkedList<MapTile> walkPath = null;
					if(await pathManager.TryGetWalkPath(gameManager, start, goal, player, value => walkPath = value)) {
						int distance = walkPath.Count;
						if(distance < minDistance) {
							minDistance = distance;
							state.SetTargetTile(goal);
							state.SetTarget(player);
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
        MapTile goal = state.GetTargetTile();
		PlayerManager player = state.GetTarget();
        if(goal != null) {
            await WalkToTile(gameManager, pathManager, state, start, goal, player);
        }
	}
}
