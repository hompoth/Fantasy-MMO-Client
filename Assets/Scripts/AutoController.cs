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
                //Movement: None
                break;
            case AutoTask.AttackFullMap:
                //Movement: Follow path around map
                //Move towards mobs and items
                break;
            case AutoTask.AttackAndWander:
                //Movement: Move towards a random point
                //Move towards mobs and items
                break;
            case AutoTask.FollowGroup:
                //Movement: Follow group while avoiding mobs. Otherwise defer to another task
                break;
            case AutoTask.FarmGold:
                //Movement: If inventory full, move towards/search for sell vendor. Otherwise defer to another task
                break;
            case AutoTask.BossFarming:
                //Movement: Move towards next boss in list. 
                //If there, kill it and reset timer. 
                //If not there, reset timer. 
                //Otherwise defer to another task
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
    //  Farthest/Closest enemy
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

    public static void ToggleActive() {
        m_active = !m_active;
    }

    public static void Disable() {
        m_active = false;
    }
}
