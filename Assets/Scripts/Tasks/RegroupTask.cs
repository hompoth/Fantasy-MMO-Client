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
    float REGROUP_TIMEOUT = 5f;

	public override async Task<bool> IsActive(GameManager gameManager, PathManager pathManager, AutoControllerState state) {
        bool regroupRequired = false;
            if(Time.time > state.GetRegroupPointExpireTime()) {
                regroupRequired = await UpdateRegroupPoint(gameManager, pathManager, state);
            }
            else {
                MapTile start = GetPlayerPosition(gameManager);
                MapTile goal = state.GetRegroupPoint();
                bool sameArea = await pathManager.IsSameArea(gameManager, start, goal);
                if(!sameArea) {
                    regroupRequired = true;
                }
                else {
                    Tuple<LinkedList<MapTile>, WarpDevice, int> mapPathInfo = await pathManager.GetMapPath(gameManager, start, goal);
                    int distance = mapPathInfo.Item3;
                    if(distance <= REGROUP_REQUIRED_DISTANCE) {
                        regroupRequired = await UpdateRegroupPoint(gameManager, pathManager, state);
                    }
                    else {
                        regroupRequired = true;
                    }
                }
            }
        return regroupRequired;
	}
    
	public override async Task Move(GameManager gameManager, PathManager pathManager, AutoControllerState state) {
        MapTile goal = state.GetRegroupPoint();
        if(goal != null) {
            await MoveToMapTile(gameManager, pathManager, state, goal);
        }
	}

    private async Task<bool> UpdateRegroupPoint(GameManager gameManager, PathManager pathManager, AutoControllerState state) {
        GameManager closestPlayer = ClientManager.GetCurrentGameManager();
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
                        closestPlayer = manager;
                    }
                    allWithinRange = false;
                }
                if(distance > REGROUP_REQUIRED_DISTANCE || !sameArea) {
                    regroupRequired = true;
                }
            }
        }
        state.SetRegroupPoint(closestPlayer);
        if(regroupRequired && !allWithinRange) {
            MapTile safePoint = pathManager.GetSafePoint();
            Tuple<LinkedList<MapTile>, WarpDevice, int> mapPathInfo = await pathManager.GetMapPath(gameManager, start, safePoint);
            int distance = mapPathInfo.Item3;
            if(distance < minDistance) {
                state.SetRegroupPoint(safePoint);
            }
        }
        state.SetRegroupPointExpireTime(Time.time + REGROUP_TIMEOUT);
        return regroupRequired;
    }
}
