using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class CastAction : AutoAction
{
    SlotUI m_slot;
    int m_aether, m_playerId;
    string m_type;

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
                        for(int index = 1; index <= 10; ++index) {
                            PartyPlayerUI partyPlayer = gameManager.GetPartyPlayer(index);
                            if(partyPlayer != null) {
                                if(partyPlayer.GetHPBar() < 90) {
                                    targetPlayerId = partyPlayer.GetPlayerId();
                                    break;
                                }
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
                        for(int index = 1; index <= 10; ++index) {
                            PartyPlayerUI partyPlayer = gameManager.GetPartyPlayer(index);
                            if(partyPlayer != null) {
                                if(partyPlayer.GetMPBar() < 90) {
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
            }
            else if(m_type.Equals("Regeneration")) {
                m_playerId = gameManager.GetMainPlayerId();
                return true;
            }
        }
        return false;
    }

	protected override int UseAction(GameManager gameManager, AutoControllerState state) {  // Todo create data object to keep track of slot / playerId
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
