﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MapTile = System.Tuple<int, int, int>;

public class FollowGroupTask : AutoTask
{
	
    public async override Task<bool> IsActive(GameManager gameManager, PathManager pathManager, AutoControllerState state) {
		await Task.Yield();
        GameManager currentManager = ClientManager.GetCurrentGameManager();
        if(!currentManager.Equals(gameManager)) {
            PlayerManager player = currentManager.GetMainPlayerManager();
            state.SetTarget(player);
            state.SetTargetTile(GetPlayerPosition(currentManager, player));
            return true;
        }
        return false;
    }

    /*public async override Task<bool> IsActive(GameManager gameManager, PathManager pathManager, AutoControllerState state) {
        int distanceFromTarget = pathManager.GetDistanceToPlayer();
        MapTile start = GetPlayerPosition(gameManager);
        List<int> playerIdList = new List<int>();
        int map = start.Item1;
        float totalX = 0f, totalY = 0f, followPositionScale = 0f;
        foreach(GameManager manager in ClientManager.GetAllGameManagers()) {
            if(!manager.Equals(gameManager)) {
                int playerId = manager.GetMainPlayerId();
                MapTile goal = GetPlayerPosition(manager);
                if(await pathManager.IsSameArea(gameManager, start, goal)) {
                    int distance = PathManager.DistanceHeuristic(start, goal);
                    float scale = (float) distance / distanceFromTarget * 2;
                    if(!playerIdList.Contains(playerId)) {
                        totalX += goal.Item2 * scale;
                        totalY += goal.Item3 * scale;
                        followPositionScale+=scale;
                        playerIdList.Add(playerId);
                    }
                }
            }
        }
        foreach(PartyPlayerUI partyPlayer in gameManager.GetAllPartyPlayers()) {
            int playerId = partyPlayer.GetPlayerId();
            PlayerManager player = gameManager.GetPlayerManager(playerId);
            if(player != null) {
                MapTile goal = GetPlayerPosition(gameManager, player);
                int distance = PathManager.DistanceHeuristic(start, goal);
                float scale = (float) distance / distanceFromTarget * 2;
                int x = goal.Item2, y = goal.Item3;
                if(!playerIdList.Contains(playerId)) {
                    totalX += x * scale;
                    totalY += y * scale;
                    followPositionScale+=scale;
                    playerIdList.Add(playerId);
                }
            }
        }
        if(playerIdList.Count > 0) {
            int x = (int) Mathf.Round(totalX / followPositionScale);
            int y = (int) Mathf.Round(totalY / followPositionScale);
            state.SetTargetTile(Tuple.Create(map, x, y));
        }
        return playerIdList.Count > 0;
	}*/

	public override async Task Move(GameManager gameManager, PathManager pathManager, AutoControllerState state) {
        MapTile start = GetPlayerPosition(gameManager);
        MapTile goal = state.GetTargetTile();
        if(goal != null) {
            int distanceFromTarget = pathManager.GetDistanceToPlayer();
            await WalkToTile(gameManager, pathManager, state, start, goal, null, distanceFromTarget);
        }
	}
}
