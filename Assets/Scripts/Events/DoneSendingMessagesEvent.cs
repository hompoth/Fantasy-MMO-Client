﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoneSendingMessagesEvent : Event
{
	public override void Run(GameManager manager, string message) {
		manager.DoneSendingMessages();
	}
}