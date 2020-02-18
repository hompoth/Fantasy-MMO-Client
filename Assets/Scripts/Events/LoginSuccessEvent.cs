using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginSuccessEvent : Event
{
	public override void Run(GameManager manager, string message) {
		manager.SendLoginContinued();
	}
}
