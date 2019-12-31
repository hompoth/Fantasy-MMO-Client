using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateHealthManaEvent : Event
{
	public override void Run(GameManager manager, string message) {
		string[] tokens = message.Split(',');
		int index = 0;

		TryGetTokenValue(tokens, ref index, out int hpMax);
		TryGetTokenValue(tokens, ref index, out int mpMax);
		TryGetTokenValue(tokens, ref index, out int spMax);
		TryGetTokenValue(tokens, ref index, out int hp);
		TryGetTokenValue(tokens, ref index, out int mp);
		TryGetTokenValue(tokens, ref index, out int sp);
		TryGetTokenValue(tokens, ref index, out int hpBar);
		TryGetTokenValue(tokens, ref index, out int mpBar);
		manager.SetMainPlayerHPMPSP(hpMax, mpMax, spMax, hp, mp, sp, hpBar, mpBar);
	}
}
