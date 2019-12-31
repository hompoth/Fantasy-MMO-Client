using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum InfoType {SpreadOut, Center};

public class DisplayEvent : Event
{
	public override void Run(GameManager manager, string message) {
		string[] tokens = message.Split(',');
		int index = 0;
		Color displayColor = AsperetaTextColor.white;
		InfoType infoType = InfoType.Center;
		
		TryGetTokenValue(tokens, ref index, out int playerId);
		TryGetTokenValue(tokens, ref index, out int displayType);
		TryGetTokenValue(tokens, ref index, out string displayText);
		TryGetTokenValue(tokens, ref index, out string characterName);			
		if(displayType == 1 || displayType == 2 || displayType == 4 || displayType == 5) {
			displayColor = AsperetaTextColor.red;
			infoType = InfoType.SpreadOut;
		}
		else if(displayType == 7 || displayType == 8) {
			displayColor = AsperetaTextColor.green;
			infoType = InfoType.SpreadOut;
		}
		else if(displayType == 11 || displayType == 51) {
			displayText = "ROOTED";
		}
		else if(displayType == 20) {
			displayText = "DODGE";
		}
		else if(displayType == 21) {
			displayText = "MISS";
		}
		else if(displayType == 50 || displayType == 10) {
			displayText = "STUNNED";
		}
		else if(displayType == 60) {
			displayColor = AsperetaTextColor.yellow;
		}
		else if(displayType == 61) {
			displayColor = AsperetaTextColor.red;
		}
		if(infoType == InfoType.SpreadOut) {
			(manager.GetPlayerManager(playerId))?.AddSpreadOutDisplayObject(displayText, displayColor, characterName);
		}
		else if(infoType == InfoType.Center) {
			(manager.GetPlayerManager(playerId))?.AddCenteredDisplayObject(displayText, displayColor, characterName);
		}
	}
}
