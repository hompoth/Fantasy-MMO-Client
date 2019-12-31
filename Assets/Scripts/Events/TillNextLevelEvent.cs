using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TillNextLevelEvent : Event
{
	public override void Run(GameManager manager, string message) {
		string[] tokens = message.Split(',');
		int index = 0;

		TryGetTokenValue(tokens, ref index, out int percent);
		TryGetTokenValue(tokens, ref index, out int experience);
		TryGetTokenValue(tokens, ref index, out int experienceTillNextLevel);
		manager.SetMainPlayerExperience(percent, experience, experienceTillNextLevel);
	}
}
