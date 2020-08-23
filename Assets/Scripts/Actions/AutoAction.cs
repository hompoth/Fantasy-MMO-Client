using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public abstract class AutoAction : AutoBase
{
	GameManager m_manager;
	AutoControllerState m_controllerState;

	public AutoAction(GameManager gameManager, AutoControllerState state, CancellationToken token) {
		m_manager = gameManager;
		m_controllerState = state;
		LoopAction(token);
	}

	protected abstract bool CanUseAction(GameManager gameManager, AutoControllerState state);
	protected abstract int UseAction(GameManager gameManager, AutoControllerState state);
	protected abstract int DelayAction(GameManager gameManager, AutoControllerState state);

	// Use a cancellation token or m_controllerState.IsEnabled
    protected async void LoopAction(CancellationToken token) {
		int delay;
        while(m_controllerState.IsActive() && !token.IsCancellationRequested) {
            if(CanUseAction(m_manager, m_controllerState)) {
				delay = UseAction(m_manager, m_controllerState);
			}
			else {
				delay = DelayAction(m_manager, m_controllerState);
			}
            await Task.Delay(delay);
        }
    }
}
