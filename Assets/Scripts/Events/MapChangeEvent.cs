using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapChangeEvent : Event
{
	public override void Run(GameManager manager, string message) {
		string[] tokens = message.Split(',');
		int index = 0;

		TryGetTokenValue(tokens, ref index, out int mapId);
		TryGetTokenValue(tokens, ref index, out string version);
		TryGetTokenValue(tokens, ref index, out string mapName);
		manager.StopHandlingMessages();
		manager.LoadScene("GameWorld", mapName, () => {
			manager.LoadMap(mapId);
			manager.SendDoneLoadingMap(mapId);
			manager.ContinueHandlingMessages();
		});
	}
}
