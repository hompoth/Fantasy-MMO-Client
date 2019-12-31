using UnityEngine;
using TMPro;
public class AlphaValidator : MonoBehaviour {

	void Awake ()
	{
		TMP_InputField input = GetComponent<TMP_InputField>();
		if (input)
		{
			input.onValidateInput = ValidateInput;
		}
	}

	static char ValidateInput (string text, int charIndex, char addedChar)
	{
		return char.IsLetter(addedChar) ? addedChar : '\0';
	}
}