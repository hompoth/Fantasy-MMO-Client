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
	static LoginManager m_loginManager;
    static NameFormat m_nameFormat;
    static HealthManaFormat m_healthManaFormat;
	static ArrayList m_gameManagers;
	static int m_gameManagerIndex, m_uniqueId;

    void Awake() {
		if(m_instance == null) {
			m_instance = this;
			m_gameManagers = new ArrayList();
			DontDestroyOnLoad(gameObject);
		}
		else {
			Destroy(gameObject);
		}
    }

	public static void QuitGame() {
		if(m_gameManagers.Count > 0) {
			GameManager manager = GetGameManager(m_gameManagerIndex);
			ToggleScene(manager, "GameWorld");
		}
		else {
        	Application.Quit();
		}
	}

	public static void Disconnect(GameManager manager) {
		bool isActive = IsActiveGameManager(manager);
		manager.HideGameManager();
		manager.DeleteWorldObjects();
        manager.DisableAutoController();
		RemoveGameManager(manager);
		if(isActive) {
			if(m_gameManagers.Count > 0) {
				QuitGame();
			}
			else {
				if (SceneManager.GetActiveScene().name.Equals("LoginScreen")) {
					DisplayLoginMessage("Unable to connect to the server.");
				}
				else {
					DisplayLoginMessage("Disconnected from the server.");
				}
			}
		}
	}

	private static void DisplayLoginMessage(string message) {
		ToggleScene(null, "LoginScreen", () => {
			ConnectionIssue(message);
		});
	}

	public static void ConnectionIssue(string message) {
		(GetLoginManager())?.ConnectionIssue(message);
	}

	private static LoginManager GetLoginManager() {
		if(m_loginManager != null) {
			return m_loginManager;
		}
		else {
			m_loginManager = GameObject.FindWithTag("LoginManager")?.GetComponent<LoginManager>();
			return m_loginManager;
		}
	}

	public static bool IsActiveGameManager(GameManager manager) {
		GameManager currentGameManager = GetGameManager(m_gameManagerIndex);
		string sceneName = SceneManager.GetActiveScene().name;
		if(currentGameManager == null) {
			return true;
		}
		if(sceneName.Equals("LoginScreen")) {
			return true;
		}
		return manager != null && manager.Equals(currentGameManager);
	}

	static async void ToggleScene(GameManager manager, string sceneName, Action action = null) {
		if(sceneName.Equals("LoginScreen")) {
			manager?.HideGameManager();
		}
		if(sceneName.Equals("GameWorld")) {
        	Cursor.visible = false;
		}
		else {
        	Cursor.visible = true;
		}
		await InitiateLoadingScreen();
		await UpdateLoadingScreen(manager, sceneName);
		if(sceneName.Equals("GameWorld")) {
			manager?.ShowGameManager();
		}
		if (action != null) {
			action();
		}
	}

	public static void LoadScene(GameManager manager, string sceneName, string mapName = "", Action action = null) {
		if(IsActiveGameManager(manager)) {
			LoadSceneWithLoadingScreen(manager, sceneName, mapName, action);
		}
		else {
			LoadSceneSilently(manager, sceneName, mapName, action);
		}
	}
	
	static async void LoadSceneSilently(GameManager manager, string sceneName, string mapName, Action action) {
		string previousSceneName = SceneManager.GetActiveScene().name;
		if (!(previousSceneName.Equals("LoginScreen") && sceneName.Equals("LoginScreen"))) {
			if(manager != null) {
				manager.HideGameManager();
				manager.DeleteWorldObjects();
				await WaitForGameManager(manager);
			}
		}
		if (action != null) {
			action();
		}
	}

	static async void LoadSceneWithLoadingScreen(GameManager manager, string sceneName, string mapName = "", Action action = null) {
		string previousSceneName = SceneManager.GetActiveScene().name;
		if (!(previousSceneName.Equals("LoginScreen") && sceneName.Equals("LoginScreen"))) {
			BeforeLoadScene(manager, previousSceneName, sceneName);
			await InitiateLoadingScreen();
			await UpdateLoadingScreen(manager, sceneName, mapName);
			AfterLoadScene(manager, previousSceneName, sceneName);
		}
		if (action != null) {
			action();
		}
	}

	static async Task InitiateLoadingScreen() {
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LoadingScreen");
		asyncLoad.allowSceneActivation = true;
		while (!asyncLoad.isDone) {
        	await Task.Yield();
		}
	}

	static async Task UpdateLoadingScreen(GameManager manager, string sceneName, string mapName = "") {
		float progress = 0f;
		Slider slider = GameObject.FindWithTag("ProgressBar").GetComponent<Slider>();
		TextMeshProUGUI text = GameObject.FindWithTag("LoadingMapName").GetComponent<TextMeshProUGUI>();
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
		text.text = mapName;
		asyncLoad.allowSceneActivation = false;
		while (progress < 1f && (manager == null || !manager.IsDoneSending())) {
			progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
			slider.value = progress;
        	await Task.Yield();
		}
		asyncLoad.allowSceneActivation = true;
		while (!asyncLoad.isDone) {
        	await Task.Yield();
		}
	}

	static async Task WaitForGameManager(GameManager manager) {
		while(manager != null && !manager.IsDoneSending()) {
        	await Task.Yield();
		}
	}

	static void BeforeLoadScene(GameManager manager, string previousSceneName, string sceneName) {
		if(sceneName.Equals("GameWorld")) {
        	Cursor.visible = false;
		}
		else {
        	Cursor.visible = true;
		}
		if(manager != null) {
			if(previousSceneName.Equals("LoginScreen") && sceneName.Equals("GameWorld")) {
				AddGameManager(manager);
			}
			else {
				manager.HideGameManager();
				manager.DeleteWorldObjects();
			}
		}
	}

	static void AfterLoadScene(GameManager manager, string previousSceneName, string sceneName) {
		if(manager != null) {
			if(!previousSceneName.Equals("GameWorld") && sceneName.Equals("GameWorld")) {
				manager.LoadWindowPreferences();
			}
		}
	}

	static void SetGameManagerPosition(GameManager manager, int index = default(int)) {
        if(index == default(int)) {
            index = ++m_uniqueId;
        }
		manager.GetMainPlayerPosition(out int _, out int x, out int y);
		manager.transform.position = new Vector3(index * 1000, 1000, 0);
		manager.SetMainPlayerPosition(x, y);
	}

	public static void AddGameManager(GameManager manager) {
		m_gameManagers.Add(manager);
		LoadPlayerPreferences(manager);
		m_gameManagerIndex = m_gameManagers.Count - 1;
		GameObject gameObject = GameObject.FindWithTag("ClientManager");
		if(gameObject != null) {
			SetGameManagerPosition(manager);
			manager.transform.SetParent(gameObject.transform);
		}
	}

	public static void RemoveGameManager(GameManager managerToRemove) {
		int removedIndex = m_gameManagers.IndexOf(managerToRemove);
        SetGameManagerPosition(managerToRemove, -1);   // Move away to prevent collisions
		m_gameManagers.Remove(managerToRemove);
		Destroy(managerToRemove.gameObject, 10);
		m_gameManagerIndex = m_gameManagers.Count - 1;
	}

	public static void NextGameManager() {
		string currentSceneName = SceneManager.GetActiveScene().name;
		if(currentSceneName.Equals("GameWorld")) {
			if(m_gameManagers.Count > 1) {
				HideGameManager();
				m_gameManagerIndex = (m_gameManagerIndex + 1) % m_gameManagers.Count;
				ShowGameManager();
			}
		}
	}

	public static void LoadLoginScene() {
		GameManager manager = GetGameManager(m_gameManagerIndex);
		ToggleScene(manager, "LoginScreen");
	}

    public static GameManager GetCurrentGameManager() {
        GameManager currentGameManager = GetGameManager(m_gameManagerIndex);
        return currentGameManager;
    }

	static void HideGameManager(GameManager manager = default(GameManager)) {
		if(manager == default(GameManager)) {
			manager = GetGameManager(m_gameManagerIndex);
		}
		if(manager != null) {
			manager.HideGameManager();
		}
	}

	static void ShowGameManager(GameManager manager = default(GameManager)) {
		if(manager == default(GameManager)) {
			manager = GetGameManager(m_gameManagerIndex);
		}
		if(manager != null) {
			manager.ShowGameManager();
		}
	}

	static GameManager GetGameManager(int index) {
		if(m_gameManagers != null && m_gameManagers.Count > 0 && index >= 0 && index < m_gameManagers.Count) {
			return m_gameManagers[index] as GameManager;
		}
		return null;
	}

    static void LoadPlayerPreferences(GameManager manager) {
        m_nameFormat = UserPrefs.GetNameFormat();
        m_healthManaFormat = UserPrefs.GetHealthManaFormat();
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
