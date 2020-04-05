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
	public override bool IsActive(GameManager gameManager, PathManager pathManager, AutoControllerState state) {
        // If in group &&
        // -- If a member of your group has been gone for 5 seconds or more (customizable)
        // -- -- If leader go to waypoint, otherwise go to leader. Leader is the first gamemanager in list

        // Rather than check if surrounded, check if move failed recently.
        // This will be stored in state and will be referenced in all other tags. 
        // For instance, if you can't get to the goal, try killing mobs for 15seconds? or until the path is available. 
		return !IsSurrounded(gameManager, state);
	}
    
	public override async Task Move(GameManager gameManager, PathManager pathManager, AutoControllerState state) {
        MapTile goal = GetTargetLocation(gameManager);
        if(goal != null) {
            int distanceFromTarget = pathManager.GetDistanceToPlayer();
            await MoveToTile(gameManager, pathManager, state, goal, distanceFromTarget);
        }
	}

    private MapTile GetTargetLocation(GameManager gameManager) {
        GameManager currentGameManager = ClientManager.GetCurrentGameManager();
        if(currentGameManager == null) {
            return null;
        }
        if(!gameManager.Equals(currentGameManager)) {
            currentGameManager.GetMainPlayerPosition(out int map, out int x, out int y);
            return Tuple.Create(map, x, y);
        }
        else {
            // TODO go to waypoint instead
            gameManager.GetMainPlayerPosition(out int map, out int x, out int y);
            return Tuple.Create(map, x, y);
        }
    }
}
