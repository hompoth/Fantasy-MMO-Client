using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AutoTask { AttackInSpot, AttackFullMap, AttackAndWander, FollowGroup, FarmGold, BossFarming }

public class AutoController : MonoBehaviour
{
    public static AutoTask m_task;
    static bool m_active = false;

    void Update() {
        if(m_active) {

        }
    }


    public static void SetAutoTask(AutoTask task) {
        switch(m_task) {
            case AutoTask.AttackInSpot:
                //AttackInSpot: Idle
                break;
            case AutoTask.AttackFullMap:
                //AttackFullMap: Regroup, AttackMob, PickUp, CycleMaps
                break;
            case AutoTask.AttackAndWander:
                //AttackAndWander: Regroup, AttackMob, PickUp, Wander
                break;
            case AutoTask.FollowGroup:
                //FollowGroup: Regroup, FollowGroup, Idle (Minita)
                break;
            case AutoTask.FarmGold:
                //FarmGold: SellItems, PickUp, AttackMob, Wander
                break;
            case AutoTask.BossFarming:
                //BossFarming: Regroup, KillBoss (for each boss), PickUp, CycleMaps
                break;
        }
    }

    //----How it works.
    // AutoTask can be updated at any time. When changed, a new set of tasks will be chosen.
    // Each second task priority will be re-calculated.
    // Per update, use any actions that are off cooldown and have a target (and meet other requirements like hp/mp)
    // Per tile movement, get the task movement-priority.
    //  If mobs, look for mob within priority range
    //  If drops, look for drop within priority range
    //  If players, look for player within priority range
    //  If location, move to location waypoint

        //Example
            //FarmGold, AttackInSpot, If near players, run to waypoint

            //----TASKS
            //
            //AttackInSpot: Actions without movement
            //AttackFullMap: Generate waypoints covering map. Go to each in efficient order.
            //AttackAndWander: Move towards a random point.
            //FollowGroup: Follow group. If no group, do a different task.
            //FarmGold: Sell inventory. If inventory isn't full, do a different task.
            //BossFarming: Kill a boss. If no bosses are alive, do a different task

            //----SPECIAL SUB-TASKS
            //
            //If near players, run to waypoint
            //If in Minita, run to waypoint

    //----TASKS
    //Task priority function: Inventory full, Bosses alive, Group in range, Near players, Surrounded, In Minita, Always active. 
    //                        Classes similar to events can be created.
    //Movement: Create a list of points to cycle through. Classes similar to events can be created.
    //Movement-priority: A priority list of movement actions. (mobs, players, drops, location)
    //Actions: A list of actions to perform when the cycle time elapsed

    //----MOVEMENT
    //
    //Move to location
    //Move to map (move to location based on warp waypoints)
    //Move to player/mob (closest location within distance. Distance can be valid attack points)
    //Move to drop

    //----ACTIONS
    //Spells:
    //Cycle time.
    //Target pattern:
    //  Cycle Party
    //  Party Leader
    //  Party members of a certain class
    //  Party members low on hp/mp
    //  Near most enemies
    //  Furthest from most enemies (furthest and single)
    //  Closest to a plus, line, spray
    //Source. Target/Self
    //Spell hit pattern. (single, plus, spray, line)
    //
    //Items:
    //Pickup or ignore
    //Sell when inventory is full
    //Destroy when encountered
    //
    //Misc:
    //Buy hp/mp
    //Type something when players in range
    //Emote when players in range


    //Send actions to a thread to not be restricted by update's frame-based speed

    //Task
    //Movement: new Mob(dist), new Player(dist), new Drop(dist), new Wander(new Point[]{point1, point2, etc})
    // movement-list.FirstActiveOrDefault()?.Move(); - triggered per movement timer
    // Move() will generate the path and then move to the next location

    //Disabled Attacking:
    // Minita
    // Paradise
    // Shops
    // If vendor
    // If player
    // If missed?




    //Move Tasks:
    // KillBoss - Priority based on timer (i.e. if boss alive) - Find player (mob), keep moving towards player until dead
    // SellItems - Priority based on inventory fullness and items available to sell - Find player (vendor), sell to vendor
    // PickUp - Priority based on item drop within range - Move to item drop
    // AttackMob - Priority based on mobs within range - MoveTo
    // FollowGroup - Priority based on if in group - Space out around group. If leader is not on map, go to map.
    // Regroup - Priority based on if group isn't fully present after a given time period or player dies - Search for missing members
    // Avoid - Priority based on non-grouped players nearby - Run to a waypoint
    // Idle - Always active - Move to a location to idle
    // Wander - Always active - Go to a random attainable location. If at that location, generate a new one.
    // CycleMaps - Always active - Use ScanMap function and cycle waypoints of maps

    //Move functions:
    // Find player (mob/player/vendor) - Go to map, MoveTo / ScanMap
    // ScanMap - Generate waypoints such that each spot of the map is visible, then move through them in an efficient manner.

    //Move actions:
    // Player - Attack - Move to player such that it is within your attack range
    // Player - Follow - Move to one or more players and remain spaced out
    // Location - MoveTo - Move to a location on the map
    // Location - MoveToMap - Cache waypoints and move through them

    //Attack Range:
    //FL, SOS, Ranged dist (a circle of given radius)

    //Avoid Location:
    //Mob Range - Applicable to all movement (per mob type)
    //Tile - To avoid a boss room for example

    //Note - If point can't be moved to or a timer passes, skip it. I.e move to next waypoint
    //       Also find way to ignore mob range at times

    public static void ToggleActive() {
        if(m_active) {
            Disable();
        }
        else {
            Enable();
        }
    }

    public static void Enable() {
        m_active = true;
    }

    public static void Disable() {
        m_active = false;
    }
}
