using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public abstract class AutoAction
{
	GameManager m_manager;
	AutoControllerState m_controllerState;

	public AutoAction(GameManager gameManager, AutoControllerState state) {
		m_manager = gameManager;
		m_controllerState = state;
		LoopAction();
	}

	public abstract bool CanUseAction(GameManager gameManager, AutoControllerState state);
	public abstract int UseAction(GameManager gameManager, AutoControllerState state);
	public abstract int DelayAction(GameManager gameManager, AutoControllerState state);

	// Use a cancellation token or m_controllerState.IsEnabled
    private async void LoopAction() {
		int delay;
        while(m_controllerState.IsActive()) {
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
