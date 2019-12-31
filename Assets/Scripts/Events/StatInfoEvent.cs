using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatInfoEvent : Event
{
	public override void Run(GameManager manager, string message) {
		string[] tokens = message.Split(',');
		int index = 0;

		TryGetTokenValue(tokens, ref index, out string guildName);
		TryGetTokenValue(tokens, ref index, out string unknown);
		TryGetTokenValue(tokens, ref index, out string className);
		TryGetTokenValue(tokens, ref index, out int level);
		TryGetTokenValue(tokens, ref index, out int maxHp);
		TryGetTokenValue(tokens, ref index, out int maxMp);
		TryGetTokenValue(tokens, ref index, out int maxSp);
		TryGetTokenValue(tokens, ref index, out int curHp);
		TryGetTokenValue(tokens, ref index, out int curMp);
		TryGetTokenValue(tokens, ref index, out int curSp);
		TryGetTokenValue(tokens, ref index, out int statStr);
		TryGetTokenValue(tokens, ref index, out int statSta);
		TryGetTokenValue(tokens, ref index, out int statInt);
		TryGetTokenValue(tokens, ref index, out int statDex);
		TryGetTokenValue(tokens, ref index, out int armor);
		TryGetTokenValue(tokens, ref index, out int resFire);
		TryGetTokenValue(tokens, ref index, out int resWater);
		TryGetTokenValue(tokens, ref index, out int resEarth);
		TryGetTokenValue(tokens, ref index, out int resAir);
		TryGetTokenValue(tokens, ref index, out int resSpirit);
		TryGetTokenValue(tokens, ref index, out int gold);
		manager.SetMainPlayerStatInfo(guildName, unknown, className, level, maxHp, maxMp, maxSp, curHp, curMp, curSp, 
			statStr, statSta, statInt, statDex, armor, resFire, resWater, resEarth, resAir, resSpirit, gold);
	}
}
