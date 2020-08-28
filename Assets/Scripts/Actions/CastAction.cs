using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using MapTile = System.Tuple<int, int, int>;

public class CastAction : AutoAction
{
    SlotUI m_slot;
    int m_aether, m_playerId;
    string m_type;
    MapTile m_target;

	public CastAction(GameManager gameManager, AutoControllerState state, CancellationToken token, SlotUI slot, int aether, string type) : base(gameManager, state, token) {
        m_slot = slot;
        m_aether = aether;
        m_type = type;
	}

    int GetPercent(int current, int max) {
        if(max > 0) {
            return (int) ((float) current/max * 100);
        }
        return 0;
    }

	protected override bool CanUseAction(GameManager gameManager, AutoControllerState state) {
        m_target = null;
        if(!String.IsNullOrEmpty(m_type)) {
            gameManager.GetMainPlayerHP(out int curHp, out int maxHp);
            gameManager.GetMainPlayerMP(out int curMp, out int maxMp);
            int hpPercent = GetPercent(curHp, maxHp);
            int mpPercent = GetPercent(curMp, maxMp);
            if(m_type.Equals("Healing")) {
                int targetPlayerId = 0;
                if(hpPercent < 90) {
                    targetPlayerId = gameManager.GetMainPlayerId();
                }
                else {
                    if(mpPercent > 20) {
                        foreach(PartyPlayerUI partyPlayer in gameManager.GetAllPartyPlayers()) {
                            if(partyPlayer.GetHPBar() < 90) {
                                targetPlayerId = partyPlayer.GetPlayerId();
                                break;
                            }
                        } 
                    }
                }
                if(targetPlayerId > 0) {
                    m_playerId = targetPlayerId;
                    return true;
                }
            }
            else if(m_type.Equals("Sacrifice")) {
                if(hpPercent > 50) {
                    int targetPlayerId = 0;
                    if(mpPercent < 50) {
                        targetPlayerId = gameManager.GetMainPlayerId();
                    }
                    else {
                        foreach(PartyPlayerUI partyPlayer in gameManager.GetAllPartyPlayers()) {
                            if(partyPlayer.GetMPBar() < 90) {
                                targetPlayerId = partyPlayer.GetPlayerId();
                                break;
                            }
                        }
                    }
                    if(targetPlayerId > 0) {
                        m_playerId = targetPlayerId;
                        return true;
                    }
                }
            }
            else if(m_type.Equals("Regeneration")) {
                m_playerId = gameManager.GetMainPlayerId();
                return true;
            }
            else if(m_type.Equals("Spirit Strike")) {
                if(hpPercent > 90) {
                    m_playerId = gameManager.GetMainPlayerId();
                    MapTile start = GetPlayerPosition(gameManager);
                    foreach(PlayerManager player in gameManager.GetAllPlayerManagers()) {
                        if(player.IsPlayerMob()) {
                            MapTile goal = GetPlayerPosition(gameManager, player);
                            int simpleDistance = PathManager.DistanceHeuristic(start, goal);
                            if(simpleDistance <= 1) {
                                m_target = goal;
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

	protected override int UseAction(GameManager gameManager, AutoControllerState state) {  // Todo create data object to keep track of slot / playerId
        if(m_target != null) {
            int x = m_target.Item2, y = m_target.Item3;
            gameManager.SetMainPlayerFacingDirection(x, y);
        }
        CastSpell(gameManager, m_slot, m_playerId);
        return m_aether;
    }

	protected override int DelayAction(GameManager gameManager, AutoControllerState state) {
        return 50;
    }

    void CastSpell(GameManager gameManager, SlotUI slot, int playerId) {
        int index = slot.GetSlotIndex();
        gameManager.SendCast(index, playerId);
    }

}
