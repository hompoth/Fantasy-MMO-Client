﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAppearanceEvent : Event
{
	public override void Run(GameManager manager, string message) {
		string[] tokens = message.Split(',');
		int index = 0;

		TryGetTokenValue(tokens, ref index, out int playerId);
		TryGetTokenValue(tokens, ref index, out int bodyId);
		TryGetTokenValue(tokens, ref index, out int poseId);
		TryGetTokenValue(tokens, ref index, out int hairId);
		TryGetTokenValue(tokens, ref index, out int chestId);
		TryGetTokenValue(tokens, ref index, out Color chestColor);
		TryGetTokenValue(tokens, ref index, out int helmId);
		TryGetTokenValue(tokens, ref index, out Color helmColor);
		TryGetTokenValue(tokens, ref index, out int pantsId);
		TryGetTokenValue(tokens, ref index, out Color pantsColor);
		TryGetTokenValue(tokens, ref index, out int shoesId);
		TryGetTokenValue(tokens, ref index, out Color shoesColor);
		TryGetTokenValue(tokens, ref index, out int shieldId);
		TryGetTokenValue(tokens, ref index, out Color shieldColor);
		TryGetTokenValue(tokens, ref index, out int weaponId);
		TryGetTokenValue(tokens, ref index, out Color weaponColor);
		TryGetTokenValue(tokens, ref index, out Color hairColor);
		TryGetTokenValue(tokens, ref index, out bool invis);
		TryGetTokenValue(tokens, ref index, out int faceId);
		manager.UpdatePlayerAppearance(playerId, bodyId, poseId, hairId, hairColor, chestId, chestColor, helmId, helmColor, pantsId, 
			pantsColor, shoesId, shoesColor, shieldId, shieldColor, weaponId, weaponColor, invis, faceId);
	}
}