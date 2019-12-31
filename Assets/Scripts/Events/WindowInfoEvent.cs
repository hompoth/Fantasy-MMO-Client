using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowInfoEvent : Event
{
	public override void Run(GameManager manager, string message) {
		string[] tokens = SplitWindowInfoMessage(message);
		int index = 0;
		TryGetTokenValue(tokens, ref index, out int windowId);
		TryGetTokenValue(tokens, ref index, out int windowLine);
		TryGetTokenValue(tokens, ref index, out string description);
		TryGetTokenValue(tokens, ref index, out int itemAmount, 1);
		TryGetTokenValue(tokens, ref index, out int itemId);
		TryGetTokenValue(tokens, ref index, out int itemSlotId);
		TryGetTokenValue(tokens, ref index, out Color itemSlotColor);
		manager.UpdateWindowLine(windowId, windowLine, description, itemAmount, itemId, itemSlotId, itemSlotColor);
	}

	string[] SplitWindowInfoMessage(string message) {
		int commaArrayLength = 2;
		char[] delimiter = {','};
		string[] splitByComma = message.Split(delimiter, 3);
		if(splitByComma.Length > commaArrayLength) {
			string[] splitByPipe = splitByComma[commaArrayLength].Split('|');
			string[] splitMessages = new string[commaArrayLength + splitByPipe.Length];
			Array.Copy(splitByComma, splitMessages, commaArrayLength);
			Array.Copy(splitByPipe, 0, splitMessages, commaArrayLength, splitByPipe.Length);
			return splitMessages;
		}
		return splitByComma;
	}
}
