using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using MapTile = System.Tuple<int, int, int>;

public class AttackAction : AutoAction
{
    MapTile m_target;

	public AttackAction(GameManager gameManager, AutoControllerState state, CancellationToken token) : base(gameManager, state, token) {}

	protected override bool CanUseAction(GameManager gameManager, AutoControllerState state) {
        MapTile start = GetPlayerPosition(gameManager);
        int map = start.Item1;
        foreach(PlayerManager player in gameManager.GetAllPlayerManagers()) {
            if(player.IsPlayerMob()) {
                player.GetPlayerPosition(gameManager, out int x, out int y);
                MapTile goal = Tuple.Create(map, x, y);
                int simpleDistance = PathManager.DistanceHeuristic(start, goal);
                if(simpleDistance <= 1) {
                    m_target = goal;
                    return true;
                }
            }
        }
        return false;
    }

	protected override int UseAction(GameManager gameManager, AutoControllerState state) {
        int x = m_target.Item2, y = m_target.Item3;
        gameManager.SetMainPlayerFacingDirection(x, y);
        gameManager.HandlePlayerAttack();
        return gameManager.GetMainPlayerAttackSpeed() + 50;
    }

	protected override int DelayAction(GameManager gameManager, AutoControllerState state) {
        return 500;
    }
}
