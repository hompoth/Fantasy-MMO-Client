using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;

public enum NameFormat {
    Hidden, Visible, VisibleOnHover,
}

public enum HealthManaFormat {
    Hidden, Visible, VisibleOnUpdate,
}
public class ClientManager : MonoBehaviour
{	
	static ClientManager m_instance;
    static NameFormat m_nameFormat;
    static HealthManaFormat m_healthManaFormat;
	static List<GameManager> m_gameManagers;

    void Awake() {
		if(m_instance == null) {
			m_instance = this;
			m_gameManagers = new List<GameManager>();
			DontDestroyOnLoad(gameObject);
		}
		else {
			Destroy(gameObject);
		}
    }

	public static void QuitGame() {
        Application.Quit();
	}

	public static void AddGameManager(GameManager manager) {
		m_gameManagers.Add(manager);
		LoadPlayerPreferences(manager);
	}

    static void LoadPlayerPreferences(GameManager manager) {
        m_nameFormat = UserPrefs.GetNameFormat();
        m_healthManaFormat = UserPrefs.GetHealthManaFormat();
        UserPrefs.Save();
		manager.UpdatePlayerNameFormat();
		manager.UpdatePlayerHealthManaFormat();
    }

    public static NameFormat GetNameFormat() {
        return m_nameFormat;
    }

    public static HealthManaFormat GetHealthManaFormat() {
        return m_healthManaFormat;
    }

    public static void SwitchNameFormat() {
        m_nameFormat = UserPrefs.GetNextNameFormat(m_nameFormat);
        UserPrefs.Save();

		foreach(GameManager manager in m_gameManagers) {
			switch(m_nameFormat) {
				case NameFormat.Hidden:
					manager.AddColorChatMessage(7, "Player names are now disabled.");
					break;
				case NameFormat.VisibleOnHover:
					manager.AddColorChatMessage(7, "Player names are now visible when mouse is over target.");
					break;
				case NameFormat.Visible:
					manager.AddColorChatMessage(7, "Player names are now visible.");
					break;
			}
			manager.UpdatePlayerNameFormat();
		}
    }
    
    public static void SwitchHealthManaFormat() {
        m_healthManaFormat = UserPrefs.GetNextHealthManaFormat(m_healthManaFormat);
        UserPrefs.Save();

		foreach(GameManager manager in m_gameManagers) {
			switch(m_healthManaFormat) {
				case HealthManaFormat.Hidden:
					manager.AddColorChatMessage(7, "Player vitality bars are now hidden.");
					break;
				case HealthManaFormat.VisibleOnUpdate:
					manager.AddColorChatMessage(7, "Player vitality bars are being shown when updated.");
					break;
				case HealthManaFormat.Visible:
					manager.AddColorChatMessage(7, "Player vitality bars are now visible.");
					break;
			}
			manager.UpdatePlayerHealthManaFormat();
		}
    }
}
