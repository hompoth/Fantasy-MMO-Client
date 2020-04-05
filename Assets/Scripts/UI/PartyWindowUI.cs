using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PartyWindowUI : WindowUI
{   
	string SLASH = Path.DirectorySeparatorChar.ToString();

    public PartyPlayerUI[] m_partyPlayers;
    Dictionary<int, int> m_playerIndexMapping;

    new void Awake() {
        base.Awake();
        m_partyPlayers = new PartyPlayerUI[10];
        m_playerIndexMapping = new Dictionary<int, int>();
    }

    public PartyPlayerUI GetPartyPlayer(int index) {
        index--;
        if(index >=0 && index < 10) {
            return m_partyPlayers[index];
        }
        return null;
    }

    public void UpdatePartyIndex(int index, int playerId, string name, int level, string className) {
        index--;
        if(index >= 0 && index < 10) {
            if(playerId == 0 || string.IsNullOrEmpty(name)) {
                if(m_partyPlayers[index] != null) {
                    int oldPlayerId = m_partyPlayers[index].GetPlayerId();
                    m_playerIndexMapping.Remove(oldPlayerId);
                    m_partyPlayers[index].gameObject.SetActive(false);
                }
            }
            else if(m_partyPlayers[index] != null) {
                int oldPlayerId = m_partyPlayers[index].GetPlayerId();
                m_playerIndexMapping.Remove(oldPlayerId);
                ResetPartyPlayer(index, playerId, name);
            }
            else {
                PartyPlayerUI partyPlayerObject = Resources.Load<PartyPlayerUI>("Prefabs" + SLASH + "PartyPlayerUI");
                if (partyPlayerObject != null) {
                    PartyPlayerUI partyPlayer = Instantiate(partyPlayerObject, PartyPosition(index), Quaternion.identity);
                    partyPlayer.gameObject.transform.SetParent(gameObject.transform);
                    m_partyPlayers[index] = partyPlayer;
                    ResetPartyPlayer(index, playerId, name);
                }
            }
        }
    }

    private void ResetPartyPlayer(int index, int playerId, string name) {
        GameObject partyObject = m_partyPlayers[index].gameObject;
        if(!partyObject.activeSelf) {
            partyObject.SetActive(true);
        }
        m_partyPlayers[index].SetText(name);
        m_partyPlayers[index].SetHPBar(100);
        m_partyPlayers[index].SetMPBar(0);
        m_partyPlayers[index].SetPlayerId(playerId);
        m_playerIndexMapping[playerId] = index;
    }

    private Vector3 PartyPosition(int index) {
        Vector3 partyWindowPosition = gameObject.transform.position;
        return partyWindowPosition + new Vector3(91f / 32f, -21f / 32f + index * -14f / 32f, 0);
    }

    public void SetHPBar(int playerId, int hpPercent) {
        if((playerId != 0) && m_playerIndexMapping.TryGetValue(playerId, out int index)) {
            m_partyPlayers[index].SetHPBar(hpPercent);
        }
    }

    public void SetMPBar(int playerId, int mpPercent) {
        if((playerId != 0) && m_playerIndexMapping.TryGetValue(playerId, out int index)) {
            m_partyPlayers[index].SetMPBar(mpPercent);
        }
    }

    public bool IsPlayerInParty(int playerId) {
        return m_playerIndexMapping.ContainsKey(playerId);
    }
}
