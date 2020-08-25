using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MapTile = System.Tuple<int, int, int>;
using WarpDevice = System.Tuple<string, int>;

public class RegroupTask : AutoTask
{
    int REGROUP_REQUIRED_DISTANCE = 12;

	public override async Task<bool> IsActive(GameManager gameManager, PathManager pathManager, AutoControllerState state) {
        GameManager closestGameManager = ClientManager.GetCurrentGameManager();
        int minDistance = Int32.MaxValue;
        bool regroupRequired = false;
        bool allWithinRange = true;
        MapTile start = GetPlayerPosition(gameManager);
        foreach(GameManager manager in ClientManager.GetAllGameManagers()) {
            if(!manager.Equals(gameManager)) {
                MapTile goal = GetPlayerPosition(manager);
                Tuple<LinkedList<MapTile>, WarpDevice, int> mapPathInfo = await pathManager.GetMapPath(gameManager, start, goal);
                int distance = mapPathInfo.Item3;
                bool sameArea = await pathManager.IsSameArea(gameManager, start, goal);
                if(distance > REGROUP_REQUIRED_DISTANCE) {
                    if(distance < minDistance) {
                        minDistance = distance;
                        closestGameManager = manager;
                    }
                    allWithinRange = false;
                }
                if(distance > REGROUP_REQUIRED_DISTANCE || !sameArea) {
                    regroupRequired = true;
                }
            }
        }
        if(closestGameManager != null) {
            PlayerManager player = closestGameManager.GetMainPlayerManager();
            state.SetTargetTile(GetPlayerPosition(closestGameManager, player));
            state.SetTarget(player);
        }
        if(regroupRequired && !allWithinRange) {
            MapTile safePoint = pathManager.GetSafePoint();
            Tuple<LinkedList<MapTile>, WarpDevice, int> mapPathInfo = await pathManager.GetMapPath(gameManager, start, safePoint);
            int distance = mapPathInfo.Item3;
            if(distance < minDistance) {
                state.SetTargetTile(safePoint);
                state.SetTarget(null);
            }
        }
        return regroupRequired;
	}
    
	public override async Task Move(GameManager gameManager, PathManager pathManager, AutoControllerState state) {
        MapTile goal = state.GetTargetTile();
        PlayerManager player = state.GetTarget();
        int distance = (player == null ? 3 : 0);
        if(goal != null) {
            await MoveToMapTile(gameManager, pathManager, state, goal, player, distance);
        }
	}
}
