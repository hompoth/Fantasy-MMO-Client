using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PartyWindowUI : WindowUI
{   
	string SLASH = Path.DirectorySeparatorChar.ToString();

    public PartyPlayerUI[] partyPlayers;
    Dictionary<int, int> playerIndexMapping;

    new void Awake() {
        base.Awake();
        partyPlayers = new PartyPlayerUI[10];
        playerIndexMapping = new Dictionary<int, int>();
    }

    public void UpdatePartyIndex(int index, int playerId, string name, int level, string className) {
        index--;
        if(index >= 0 && index < 10) {
            if(playerId == 0 || string.IsNullOrEmpty(name)) {
                if(partyPlayers[index] != null) {
                    int oldPlayerId = partyPlayers[index].GetPlayerId();
                    playerIndexMapping.Remove(oldPlayerId);
                    partyPlayers[index].gameObject.SetActive(false);
                }
            }
            else if(partyPlayers[index] != null) {
                int oldPlayerId = partyPlayers[index].GetPlayerId();
                playerIndexMapping.Remove(oldPlayerId);
                ResetPartyPlayer(index, playerId, name);
            }
            else {
                PartyPlayerUI partyPlayerObject = Resources.Load<PartyPlayerUI>("Prefabs" + SLASH + "PartyPlayerUI");
                if (partyPlayerObject != null) {
                    PartyPlayerUI partyPlayer = Instantiate(partyPlayerObject, PartyPosition(index), Quaternion.identity);
                    partyPlayer.gameObject.transform.SetParent(gameObject.transform);
                    partyPlayers[index] = partyPlayer;
                    ResetPartyPlayer(index, playerId, name);
                }
            }
        }
    }

    private void ResetPartyPlayer(int index, int playerId, string name) {
        GameObject partyObject = partyPlayers[index].gameObject;
        if(!partyObject.activeSelf) {
            partyObject.SetActive(true);
        }
        partyPlayers[index].SetText(name);
        partyPlayers[index].SetHPBar(100);
        partyPlayers[index].SetMPBar(0);
        partyPlayers[index].SetPlayerId(playerId);
        playerIndexMapping[playerId] = index;
    }

    private Vector3 PartyPosition(int index) {
        Vector3 partyWindowPosition = gameObject.transform.position;
        return partyWindowPosition + new Vector3(91f / 32f, -21f / 32f + index * -14f / 32f, 0);
    }

    public void SetHPBar(int playerId, int hpPercent) {
        if((playerId != 0) && playerIndexMapping.TryGetValue(playerId, out int index)) {
            partyPlayers[index].SetHPBar(hpPercent);
        }
    }

    public void SetMPBar(int playerId, int mpPercent) {
        if((playerId != 0) && playerIndexMapping.TryGetValue(playerId, out int index)) {
            partyPlayers[index].SetMPBar(mpPercent);
        }
    }

    public bool IsPlayerInParty(int playerId) {
        return playerIndexMapping.ContainsKey(playerId);
    }
}
