using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using MapTile = System.Tuple<int, int, int>;

public class MessageAction : AutoAction
{
    int m_aether = 1000;
    string m_message;
	public MessageAction(GameManager gameManager, AutoControllerState state, CancellationToken token, int aether, string message) : base(gameManager, state, token) {
        m_aether = aether;
        m_message = message;
    }

	protected override bool CanUseAction(GameManager gameManager, AutoControllerState state) {
        return true;
    }

	protected override int UseAction(GameManager gameManager, AutoControllerState state) {  // Todo create data object to keep track of slot / playerId
        gameManager.HandleChatInput(m_message);
        return m_aether;
    }

	protected override int DelayAction(GameManager gameManager, AutoControllerState state) {
        return m_aether;
    }
}
