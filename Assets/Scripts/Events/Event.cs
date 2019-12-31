using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Event
{
	public abstract void Run(GameManager manager, string message);

	public bool TryGetTokenValue(string[] tokens, ref int index, out string value, string failureValue = "") {
		value = failureValue;
		if (0 <= index && index < tokens.Length) {
			value = tokens[index++];
			return true;
		}
		index++;
		return false;
	}

	public bool TryGetTokenValue(string[] tokens, ref int index, out int value, int failureValue = 0) {
		value = failureValue;
		if(TryGetTokenValue(tokens, ref index, out string stringValue)) {
			if(Int32.TryParse(stringValue, out int intValue)) {
				value = intValue;
			}
			return true;
		}
		return false;
	}

	public bool TryGetTokenValue(string[] tokens, ref int index, out bool value, bool failureValue = false) {
		value = failureValue;
		if(TryGetTokenValue(tokens, ref index, out int intValue)) {
			if(intValue == 1) {
				value = true;
			}
			else {
				value = false;
			}
			return true;
		}
		return false;
	}

	public bool TryGetTokenValue(string[] tokens, ref int index, out Color value, Color failureValue = default(Color)) {
		if(failureValue == default(Color)) {
			failureValue = Color.clear;
		}
		value = failureValue;
		bool success = TryGetTokenValue(tokens, ref index, out string colorString);
		if(colorString.Equals("*")) {
			value = Color.clear;
		}
		else {
			index--;
			success = TryGetTokenValue(tokens, ref index, out int colorR) && success;
			success = TryGetTokenValue(tokens, ref index, out int colorG) && success;
			success = TryGetTokenValue(tokens, ref index, out int colorB) && success;
			success = TryGetTokenValue(tokens, ref index, out int colorA) && success;
			value = new Color(colorR/255f, colorG/255f, colorB/255f, colorA/255f);
		}
		return success;
	}

	public bool TryGetTokenValue<T>(string[] tokens, ref int index, out T value, T failureValue = default(T)) where T : Enum {
		value = failureValue;
		if(TryGetTokenValue(tokens, ref index, out int enumValue)) {
			if(EnumHelper.TryGetValueName<T>(enumValue, out value)) {
				return true;
			}
		}
		return false;
	}
}
