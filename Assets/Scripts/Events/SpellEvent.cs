﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellEvent : Event
{
	public override void Run(GameManager manager, string message) {
		string[] tokens = message.Split(',');
		int index = 0;

		TryGetTokenValue(tokens, ref index, out int x);
		TryGetTokenValue(tokens, ref index, out int y);
		TryGetTokenValue(tokens, ref index, out int animationId);
		manager.LoadSpell(x, y, animationId);
	}
}
