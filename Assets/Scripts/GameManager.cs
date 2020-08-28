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
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;

public class GameManager : MonoBehaviour
{	
	public GameObject m_playerController;
	public AutoController m_autoController;
	public PlayerState m_state;
	public GameObject m_worldObjects;
	private TcpClient socketConnection; 
    private Queue<string> messages;
	private string lastMessage;
	private CancellationTokenSource m_tokenSource;
    private Dictionary<string, Event> messageToEvent;
	private bool m_handleMessages = true, m_doneSendingMessages;
	private float m_messageTimer;
	private bool m_chatFilterEnabled = true, m_groupFilterEnabled = true;
	private bool m_guildFilterEnabled = true, m_tellFilterEnabled = true;
	private bool m_soundEnabled = true;
    private int m_mapId = 0;

	const int MESSAGE_TIMER_WAIT_TIME = 15;
	const string FILTER_CHAT = "chat", FILTER_GROUP = "group", FILTER_GUILD = "guild", FILTER_TELL = "tell";
	static string SLASH = Path.DirectorySeparatorChar.ToString();

	#if UNITY_STANDALONE_WIN
		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
		[DllImport("user32.dll")]
		private static extern IntPtr GetActiveWindow();
	#endif
	
	public void MinimizeScreen() {
		#if UNITY_STANDALONE_WIN
			ShowWindow(GetActiveWindow(), 2);
		#endif
	}

    void Awake() {
		messages = new Queue<string>();
		messageToEvent = new Dictionary<string, Event>();
		InitializeEvents();
    }

    void Update() {
		HandleCurrentMessages();
    }

	void HandleCurrentMessages() {
		if(IsConnectedToServer()) {
			if(messages.Count == 0) {
				if(Time.time - m_messageTimer > MESSAGE_TIMER_WAIT_TIME) {
					Debug.Log("Update MessageTimerElapsed");
					m_messageTimer = Time.time;
					Disconnect();
				}
			}
			else {
				m_messageTimer = Time.time;
				while(messages != null && messages.Count > 0 && m_handleMessages) {
					HandleMessage(messages.Dequeue());
				}
			}
		}
	}

	bool IsConnectedToServer() {
		return socketConnection != null && m_handleMessages;
	}

    public void ConnectToServer(string ip, int port, string username, string password) { 
        CancelLogin();
		ListenForData(ip, port, username, password);
	}

	async void ListenForData(string ip, int port, string username, string password) {
		TaskCompletionSource<bool> cancellationCompletionSource = new TaskCompletionSource<bool>();
		CancellationTokenSource connectTimeoutSource = new CancellationTokenSource(MESSAGE_TIMER_WAIT_TIME * 1000);
		m_tokenSource = new CancellationTokenSource();
		try
		{
			using (socketConnection = new TcpClient())
			{
				Task task = socketConnection.ConnectAsync(ip, port);
				CancellationToken connectTimeoutToken = connectTimeoutSource.Token;
				using (connectTimeoutToken.Register(() => cancellationCompletionSource.TrySetResult(true)))
				{
					if (task != await Task.WhenAny(task, cancellationCompletionSource.Task))
					{
						throw new OperationCanceledException(connectTimeoutToken);
					}
					else if(!socketConnection.Connected) {
						throw new SocketException();
					}
					else {
						CancellationToken loginToken = m_tokenSource.Token;
						await Task.Run(() => LoginAndListenForData(loginToken, username, password), loginToken);  
					}
				}
			}
		}    
		catch (SocketException socketException) {             
			Debug.Log("ListenForData Socket exception: " + socketException);   
			Disconnect();
		}
		catch (OperationCanceledException operationCanceledException) {             
			Debug.Log("ListenForData OperationCanceledException: " + operationCanceledException);   
			Disconnect();
		} 	
		catch (InvalidOperationException exception) {
			Debug.Log("ListenForData InvalidOperationException " + exception);
			Disconnect();
		}
	}
    
    private void LoginAndListenForData(CancellationToken token, string username, string password) {
		this.SendLogin(username, password, "ALPHA33,3.5.2");
		Byte[] bytes = new Byte[1024];
		while (true) {
			if(socketConnection != null) {
				using (NetworkStream stream = socketConnection.GetStream()) { 	
					int length; 									
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) {
						if(token.IsCancellationRequested) {
							token.ThrowIfCancellationRequested();
						}					
						byte[] incomingData = new byte[length];			
						Array.Copy(bytes, 0, incomingData, 0, length);
						string incomingDataString = lastMessage + Encoding.ASCII.GetString(incomingData);
						string[] serverMessages = incomingDataString.Split('\u0001');
						int messagesListLength = serverMessages.Length;
						if (!String.IsNullOrEmpty(incomingDataString) && incomingDataString[incomingDataString.Length-1] != '\u0001') {
							messagesListLength--;
							lastMessage = serverMessages[serverMessages.Length - 1];
						}
						else {
							lastMessage = "";
						}
						for(int i = 0; i < messagesListLength; ++i) {
							string message = serverMessages[i];
							if(!String.IsNullOrEmpty(message)){
								messages.Enqueue(message);
							}
						}
					}				
				} 
			}
		}  
	}  	
    	
	public void SendMessageToServer(string message) {
		if (socketConnection == null) {
			return;
		}
		try {
			// Get a stream object for writing.
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite) {
				// Convert string message to byte array.
				byte[] messageAsByteArray = Encoding.ASCII.GetBytes(message + '\u0001');
				// Write byte array to socketConnection stream.
				stream.Write(messageAsByteArray, 0, messageAsByteArray.Length);
			}
		}
		catch (SocketException socketException) {
			Debug.Log("SendMessageToServer Socket exception: " + socketException);
			Disconnect();
		}
		catch (InvalidOperationException exception) {
			Debug.Log("SendMessageToServer InvalidOperationException " + exception);
			Disconnect();
		}
	}

	public bool IsDoneSending() {
		return m_doneSendingMessages;
	}

	public PlayerManager GetPlayerManager(int playerId) {
		PlayerManager playerManager = null;
		GameObject player = GetPlayer(playerId);
		if(player != null) {
			playerManager = player.GetComponent<PlayerManager>();
		}
		return playerManager;
	}

	public List<PlayerManager> GetAllPlayerManagers() {
		GameObject[] gameObjects = GetPlayerObjects();
		return gameObjects.Select(gameObject => gameObject.GetComponent<PlayerManager>()).ToList();
	}

	public void ShowPlayerWindow(int windowId) {
		m_state.ShowWindow(windowId);
	}

	public void UpdatePlayerNameFormat() {
		List<PlayerManager> players = GetAllPlayerManagers();
		foreach(PlayerManager player in players) {
			player?.UpdateNameVisibility();
		}
	}

	public void UpdatePlayerHealthManaFormat() {
		List<PlayerManager> players = GetAllPlayerManagers();
		foreach(PlayerManager player in players) {
			player?.UpdateHealthManaVisibility();
		}
	}

	public void UpdateTradeEnabled(bool enabledForSelf, bool tradeEnabled) {
		m_state.UpdateTradeEnabled(enabledForSelf, tradeEnabled);
	}

	public void UpdateTradeGold(bool goldForSelf, int amount) {
		m_state.UpdateTradeGold(goldForSelf, amount);
	}

	public void UpdateTradeSlot(bool itemForSelf, int slotIndex, string itemName, int amount, int graphicId, int itemSlotId, Color itemSlotColor) {
		m_state.UpdateTradeSlot(itemForSelf, slotIndex, itemName, amount, graphicId, itemSlotId, itemSlotColor);
	}

	public void UpdateItemSlot(int slotIndex, int itemId, string itemName, int amount, int itemSlotId, Color itemSlotColor) {
		m_state.UpdateItemSlot(slotIndex, itemId, itemName, amount, itemSlotId, itemSlotColor);
        m_state.RefreshCommandBar();
		//TODO Consider making more efficient
	}

	public void UpdateSpellSlot(int slotIndex, string spellName, int soundId, int spellId, string spellTarget, int spellSlotId) {
		m_state.UpdateSpellSlot(slotIndex, spellName, soundId, spellId, spellTarget, spellSlotId);
        m_state.RefreshCommandBar();
        m_autoController.Refresh();
		//TODO Consider making more efficient
	}

	public void UpdateBuffSlot(int slotIndex, string spellName, int spellId) {
		m_state.UpdateBuffSlot(slotIndex, spellName, spellId);
	}

	public void UpdateWindowLine(int windowId, int windowLine, string description, int itemAmount, int itemId, int itemSlotId, Color itemSlotColor) {
		m_state.UpdateWindowLine(windowId, windowLine, description, itemAmount, itemId, itemSlotId, itemSlotColor); 
		m_state.RefreshCommandBar();
		//TODO Consider making more efficient
	}

	public void SetMainPlayerFacingDirection(int x, int y) {
		Vector3 worldPosition = WorldPosition(x, y, true);
        PlayerManager player = GetMainPlayerManager();
		if(player != null) {
			player.SetPlayerDirection(worldPosition);
			FacingDirection direction = player.GetPlayerFacingDirection();
			this.SendFace(direction);
		}
	}

	public void SetPlayerPosition(int playerId, int x, int y) {
		Vector3 worldPosition = WorldPosition(x, y, true);
        (GetPlayerManager(playerId))?.SetPlayerPosition(worldPosition, true);
	}

    public void HandlePlayerPosition(Vector3 input, bool fromKeyboard = true) {
        m_state.HandlePlayerMovement(input, fromKeyboard);
    }

    public int GetMapId() {
        return m_mapId;
    }

    public int GetMainPlayerId() {
        return m_state.GetMainPlayerId();
    }

	public void GetMainPlayerPosition(out int map, out int x, out int y) {
        map = m_mapId;
		m_state.GetMainPlayerPosition(out x, out y);
	}

    public string GetMainPlayerClassName() {
        return m_state.GetMainPlayerClassName();
    }

    public SlotUI GetSpellSlot(int index) {
        return m_state.GetWindowSlot(WindowType.SpellsWindow, index);
    }

    public PartyPlayerUI GetPartyPlayer(int index) {
        return m_state.GetPartyPlayer(index);
    }

    public List<PartyPlayerUI> GetAllPartyPlayers() {
        List<PartyPlayerUI> partyPlayerList = new List<PartyPlayerUI>();
        for(int index = 1; index <= 10; ++index) {
            PartyPlayerUI partyPlayer = GetPartyPlayer(index);
            if(partyPlayer != null) {
                partyPlayerList.Add(partyPlayer);
            }
        }
        return partyPlayerList;
    }

	public void SetMainPlayer(int playerId) {
		m_state.SetMainPlayer(playerId, GetPlayer(playerId));
		if(ClientManager.IsActiveGameManager(this)) {
			ShowGameManager();
		}
	}

	public void SetMainPlayerPosition(int x, int y) {
		Vector3 worldPosition = WorldPosition(x, y, true);
		m_state.SetMainPlayerPosition(worldPosition);
	}

	public void SetMainPlayerAttackSpeed(int weaponSpeed) {
		m_state.SetMainPlayerAttackSpeed(weaponSpeed);
	}

	public int GetMainPlayerAttackSpeed() {
		return m_state.GetMainPlayerAttackSpeed();
	}

	public void SetMainPlayerStatInfo(string guildName, string unknown, string className, int level, int maxHp, int maxMp, int maxSp, int curHp, int curMp, int curSp, 
			int statStr, int statSta, int statInt, int statDex, int armor, int resFire, int resWater, int resEarth, int resAir, int resSpirit, int gold) {
        string previousClassName = GetMainPlayerClassName();
		m_state.SetMainPlayerStatInfo(guildName, unknown, className, level, maxHp, maxMp, maxSp, curHp, curMp, curSp, 
			statStr, statSta, statInt, statDex, armor, resFire, resWater, resEarth, resAir, resSpirit, gold);
        if(!String.Equals(previousClassName, className)) {
            m_autoController.Refresh();
        }
	}

	public void SetMainPlayerHPMPSP(int hpMax, int mpMax, int spMax, int hp, int mp, int sp, int hpBar, int mpBar) {
		m_state.SetMainPlayerHPMPSP(hpMax, mpMax, spMax, hp, mp, sp);
		PlayerManager player = GetMainPlayerManager();
		if(player != null) {
			player.SetPlayerHPPercent(hpBar);
			player.SetPlayerMPPercent(mpBar);
		}
	}

    public void GetMainPlayerHP(out int hp, out int hpMax) {
        m_state.GetMainPlayerHealth(out hp, out hpMax);
    }

    public void GetMainPlayerMP(out int mp, out int mpMax) {
        m_state.GetMainPlayerMana(out mp, out mpMax);
    }

    public void GetMainPlayerSP(out int sp, out int spMax) {
        m_state.GetMainPlayerSpirit(out sp, out spMax);
    }

	public void SetMainPlayerExperience(int percent, int experience, int experienceTillNextLevel) {
		m_state.SetMainPlayerExperience(percent, experience, experienceTillNextLevel);
    }

	public void SetMainPlayerCanSeeInvisible(bool canSeeInvisible) {
		List<PlayerManager> playerManagers = GetAllPlayerManagers();
		foreach(PlayerManager player in playerManagers) {
			player.SetMainPlayerCanSeeInvisible(canSeeInvisible);
			player.UpdatePlayerVisibility();
		}
	}

	public PlayerManager GetMainPlayerManager() {
		return m_state.GetMainPlayerManager();
	}

	public bool MainPlayerIsSurrounded() {
		PlayerManager mainPlayer = GetMainPlayerManager();
		if(mainPlayer != null) {
			return mainPlayer.IsSurrounded();
		}
		return false;
	}

    public void Disconnect(){
        CancelLogin();
		ClientManager.Disconnect(this);
    }

	private void CancelLogin() {
		if (m_tokenSource != null) {
            m_tokenSource.Cancel();
            m_tokenSource.Dispose();
			m_tokenSource = null;
        }
		socketConnection = null;
		messages.Clear();
		lastMessage = "";
		ContinueHandlingMessages();
	}

	public void LoadWindowPreferences() {
		WindowType[] windowTypesToLoad = new WindowType[] {
			WindowType.OptionsBar, WindowType.InventoryWindow, WindowType.SpellsWindow, WindowType.CommandBar, 
			WindowType.BuffBar, WindowType.FpsBar, WindowType.HealthBar, WindowType.ManaBar, WindowType.SpiritBar, 
			WindowType.ExperienceBar, WindowType.CharacterWindow, WindowType.ChatWindow, WindowType.PartyWindow, 
			WindowType.DiscardButton
		};
		foreach(WindowType windowType in windowTypesToLoad) {
			int windowId = EnumHelper.GetNameValue<WindowType>(windowType);
			WindowUI window = LoadWindow(windowId, windowType);
			if(window != null && !windowType.Equals(WindowType.InventoryWindow) && !windowType.Equals(WindowType.SpellsWindow) && !windowType.Equals(WindowType.CharacterWindow)) {
				window.gameObject.SetActive(true);
				// TODO Set active based on preferences
			}
		}
	}

	public void ShowGameManager() {
		Input.ResetInputAxes();
		m_playerController.SetActive(true);
		m_state.EnableCamera();
		m_state.ResetCursor();
	}

	public void HideGameManager() {
		m_playerController.SetActive(false);
		m_state.DisableCamera();
	}

    public void DisableAutoController() {
        m_autoController.Disable();
    }

	public void DeleteWorldObjects() {
		foreach (Transform child in m_worldObjects.transform) {
			Destroy(child.gameObject);
		}
	}

	public void LoadMap(int mapId) {
        m_state.Escape();
        m_mapId = mapId;
		Grid prefab = Resources.Load<Grid>("Prefabs" + SLASH + "map"+mapId);
		if (prefab != null) {
			Grid map = Instantiate(prefab, Vector3.zero, Quaternion.identity); 
			map.transform.SetParent(m_worldObjects.transform, false);
		}
	}

	public GameObject GetPlayer(int playerId) {
		string name = playerId.ToString();
		GameObject[] gameObjects = GetPlayerObjects();
		foreach (GameObject gameObject in gameObjects) {
			if(gameObject.name.Equals(name)) {
				return gameObject;
			}
		}
		return null;
	}

	GameObject[] GetPlayerObjects() {
		PlayerManager[] allChildren = gameObject.GetComponentsInChildren<PlayerManager>();
		List<GameObject> playerObjects = new List<GameObject>();
		foreach (PlayerManager child in allChildren) {
			GameObject childObj = child.gameObject;
			if (childObj.tag == "Player" && childObj.transform.parent.Equals(m_worldObjects.transform)) {
				playerObjects.Add(childObj);
			}
		}
		return playerObjects.ToArray();
	}

	public void RemovePlayer(int playerId) {
		(GetPlayerManager(playerId))?.DestroyAllButPlayerUI();
	}

	public WindowUI GetWindowUI(int windowId) {
		if(m_state.TryGetWindowUI(windowId, out WindowUI window)) {
			return window;
		}
		return null;
	}

	public bool WithinSpellRange(GameObject mainPlayer, GameObject player) {
		if(player != null && mainPlayer != null) {
			Vector3 playerPosition = player.transform.position;
			Vector3 mainPlayerPosition = mainPlayer.transform.position;
			Vector3 positionDifference = mainPlayerPosition - playerPosition;
			if(Mathf.Abs(positionDifference.x) <= 14 && Mathf.Abs(positionDifference.y) <= 8) {
				return true;
			}
		}
        return false;
	}

	public bool TryGetTargetPlayerUp(int targetPlayerId, out int playerId) {
		GameObject targetPlayer = GetPlayer(targetPlayerId);
		GameObject player = GetClosestPlayer(targetPlayer, true);
		return Int32.TryParse(player?.name, out playerId);
	}

	public bool TryGetTargetPlayerDown(int targetPlayerId, out int playerId) {
		GameObject targetPlayer = GetPlayer(targetPlayerId);
		GameObject player = GetClosestPlayer(targetPlayer, false);
		return Int32.TryParse(player?.name, out playerId);
	}

	GameObject GetClosestPlayer(GameObject targetedPlayer, bool searchUp) {
		GameObject[] gameObjects = GetPlayerObjects();
		GameObject mainPlayer = GetMainPlayerManager()?.gameObject;
		GameObject closestPlayer = null;
		float minXDistance = Int32.MaxValue;
		float minYDistance = Int32.MaxValue;
		float maxXDistance = 0;
		float maxYDistance = 0;
		int offSetPenalty;
		if(mainPlayer != null) {
			if(targetedPlayer == null) {
				targetedPlayer = mainPlayer;
			}
			Vector3 playerPosition = targetedPlayer.transform.position;
			foreach (GameObject gameObject in gameObjects) {
				Vector3 gameObjectPosition = gameObject.transform.position;
				float xDistance = -gameObjectPosition.x + 100;
				float yDistance = gameObjectPosition.y + 100;
				if(!WithinSpellRange(mainPlayer, gameObject) || gameObject.Equals(targetedPlayer) || gameObject.name.Equals("Destroyed")) {
					continue;
				}
				if(Mathf.Abs(gameObjectPosition.y - playerPosition.y) < 1) {
					if(playerPosition.x > gameObjectPosition.x && searchUp || playerPosition.x < gameObjectPosition.x && !searchUp) {
						offSetPenalty = 0;
					}
					else {
						offSetPenalty = 100;
					}
				}
				else if(playerPosition.y < gameObjectPosition.y && searchUp || playerPosition.y > gameObjectPosition.y && !searchUp) {
					offSetPenalty = 0;
				}
				else {
					offSetPenalty = 100;
				}
				if(searchUp) {
					xDistance+=offSetPenalty;
					yDistance+=offSetPenalty;
				}
				else {
					xDistance-=offSetPenalty;
					yDistance-=offSetPenalty;
				}
				if(searchUp) {
					if(minYDistance > yDistance || minYDistance == yDistance && minXDistance > xDistance) {
						minXDistance = xDistance;
						minYDistance = yDistance;
						closestPlayer = gameObject;
					}
				}
				else {
					if(maxYDistance < yDistance || maxYDistance == yDistance && maxXDistance < xDistance) {
						maxXDistance = xDistance;
						maxYDistance = yDistance;
						closestPlayer = gameObject;
					}
				}
			}
		}
		return closestPlayer;
	}

	public void TargetSpellCast(int playerId) {
		PlayerManager player = GetPlayerManager(playerId);
		player?.SetPlayerTarget(true);
	}

	public void CancelTargetSpellCast(int playerId) {
		PlayerManager player = GetPlayerManager(playerId);
		player?.SetPlayerTarget(false);
	}

	public void HandleTargetSpellCast(int index, int playerId) {
		PlayerManager player = GetPlayerManager(playerId);
		if(player != null) {
			player.SetPlayerTarget(false);
			this.SendCast(index, playerId);
		}
	}

	public void HandlePlayerAttack() {
		m_state.Attack();
	}

	public void LoadPlayer(int playerId, int type, string name, string title, string surname, string guild, int x, int y, int facing, int hpPercent, int bodyId, int poseId, 
			int hairId, Color hairColor, int chestId, Color chestColor, int helmId, Color helmColor, int pantsId, Color pantsColor, int shoesId, Color shoesColor, int shieldId, 
			Color shieldColor, int weaponId, Color weaponColor, bool invis, int faceId) {
		PlayerManager playerObject = Resources.Load<PlayerManager>("Prefabs" + SLASH + "Player");
		if (playerObject != null) {
			PlayerManager player = Instantiate(playerObject, WorldPosition(x, y), Quaternion.identity);
			player.transform.SetParent(m_worldObjects.transform, false);
			player.name = playerId.ToString();
			player.SetPlayerId(playerId);
    		player.SetPlayerHPPercent(hpPercent);
    		player.SetPlayerMPPercent(0);
			player.SetPlayerType(type);
			player.SetPlayerGuild(guild);
			player.SetPlayerTitle(title);
			player.SetPlayerName(name);
			player.SetPlayerSurname(surname);
			player.SetPlayerFacing(facing);
            player.SetPlayerPosition(WorldPosition(x, y, true));
			player.UpdatePlayerAppearance(bodyId, poseId, hairId, hairColor, chestId, chestColor, helmId, helmColor, pantsId, pantsColor, shoesId, shoesColor, 
				shieldId, shieldColor, weaponId, weaponColor, invis, faceId);
			SetPartyPlayerHPMP(playerId, hpPercent, 0);
			if(m_state.IsMainPlayer(playerId)) {	// Likely unnecessary since SetMainPlayer needs a PlayerManager to exist
            	player.SetIsMainPlayer(true);
				m_state.SetMainPlayerName(playerId, player.GetPlayerName());
			}
		}
	}

	public void SetPlayerHPMP(int playerId, int hpPercent, int mpPercent) {
		PlayerManager player = GetPlayerManager(playerId);
		if(player != null) {
			player.SetPlayerHPPercent(hpPercent);
			player.SetPlayerMPPercent(mpPercent);
		}
		SetPartyPlayerHPMP(playerId, hpPercent, mpPercent);
	}

	private void SetPartyPlayerHPMP(int playerId, int hpPercent, int mpPercent) {
		m_state.UpdatePartyPlayerHP(playerId, hpPercent);
		m_state.UpdatePartyPlayerMP(playerId, mpPercent);
	}

	public void UpdatePartyIndex(int index, int playerId, string name, int level, string className) {
		m_state.UpdatePartyIndex(index, playerId, name, level, className);
		(GetPlayerManager(playerId))?.SetPlayerInParty(m_state.IsPlayerInParty(playerId));
	}

	public void UpdatePlayerAppearance(int playerId, int bodyId, int poseId, int hairId, Color hairColor, int chestId, Color chestColor, int helmId, Color helmColor, int pantsId, 
		Color pantsColor, int shoesId, Color shoesColor, int shieldId, Color shieldColor, int weaponId, Color weaponColor, bool invis, int faceId) {
		(GetPlayerManager(playerId))?.UpdatePlayerAppearance(bodyId, poseId, hairId, hairColor, chestId, chestColor, helmId, helmColor, pantsId, pantsColor, shoesId, shoesColor, 
			shieldId, shieldColor, weaponId, weaponColor, invis, faceId);
	}

    public static Tilemap GetTileMap(int mapId) {
        Grid prefab = Resources.Load<Grid>("Prefabs" + SLASH + "map" + mapId);
        if(prefab != null) {
            foreach(Transform childTransform in prefab.transform) {
                if(childTransform.name.Equals("Blocked Tiles")) {
                    return childTransform.GetComponent<Tilemap>();
                }
            }
        }
        return null;
    }

    public static bool IsMapPositionBlocked(Tilemap tilemap, int x, int y) {
        if(x <= 0 || x > 100 || y <= 0 || y > 100) {
            return true;
        }
        Tile tile = tilemap?.GetTile<Tile>(new Vector3Int(x - 51, 51 - y, 0));
        return tile != null;
    }

    public bool IsWorldPositionBlocked(int x, int y) {
        if(x <= 0 || x > 100 || y <= 0 || y > 100) {
            return true;
        }
        Vector3 worldPosition = WorldPosition(x, y, true);
        return IsPositionBlocked(worldPosition);
    }

    public static bool IsPositionBlocked(Vector2 position, bool isAdmin = false, bool ignorePlayers = false) {
        int blockMask = 0;
        if(!ignorePlayers) {
            blockMask = blockMask | ((1 << 0) | (1 << 12));
        }
        if(!isAdmin){
            blockMask = blockMask | (1 << 10);
        }
        position.y+=0.5f; // Player and map collisions are offset by 0.5f.
        RaycastHit2D hit = Physics2D.Raycast(position, Vector3.forward, Mathf.Infinity, blockMask);
        if (hit.collider != null && !hit.collider.tag.Equals("ItemDrop")) {
            // TODO if player, check if actually at this position
            return true;
        }
        return false;
    }

	ItemDrop GetItemDropAtPosition(int x, int y) {
		Vector3 position = WorldPosition(x, y, true);
		position.y += 0.5f;
		position.z += 10f;
		RaycastHit2D[] raycastHits = Physics2D.RaycastAll(position, Vector3.forward, Mathf.Infinity, 1 << 8);
		foreach(RaycastHit2D hit in raycastHits) {
        	if (hit.collider != null && hit.collider.tag.Equals("ItemDrop")) {
				return hit.collider.gameObject.GetComponent<ItemDrop>();
			}
		}
		return null;
	}

	Spell GetSpellAtPosition(int x, int y) {
		Vector3 position = WorldPosition(x, y, true);
		position.y += 0.5f;
		position.z += 10f;
		RaycastHit2D[] raycastHits = Physics2D.RaycastAll(position, Vector3.forward, Mathf.Infinity, 1 << 9);
		foreach(RaycastHit2D hit in raycastHits) {
        	if (hit.collider != null && hit.collider.tag.Equals("Spell")) {
				return hit.collider.gameObject.GetComponent<Spell>();
			}
		}
		return null;
	}

	public WindowUI LoadWindow(int windowId, WindowType windowType, string title = "", bool combineEnabled = false, 
		bool closeEnabled = false, bool backEnabled = false, bool nextEnabled = false, bool okEnabled = false, 
		int npcId = 0, int unknown = 0, int unknown2 = 0) {
		
		if(GetWindowUI(windowId) != null) {
			return null;
		}
		WindowUI windowObject = null;
		switch(windowType) {
			case WindowType.OptionsBar:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "OptionsBar");
				break;
			case WindowType.InventoryWindow:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "Inventory");
				break;
			case WindowType.SpellsWindow:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "Spells");
				break;
			case WindowType.CommandBar:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "CommandBar");
				break;
			case WindowType.BuffBar:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "BuffBar");
				break;
			case WindowType.FpsBar:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "FPS");
				break;
			case WindowType.HealthBar:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "HealthBar");
				break;
			case WindowType.ManaBar:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "ManaBar");
				break;
			case WindowType.SpiritBar:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "SpiritBar");
				break;
			case WindowType.ExperienceBar:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "ExperienceBar");
				break;
			case WindowType.CharacterWindow:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "Character");
				break;
			case WindowType.ChatWindow:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "Chat");
				break;
			case WindowType.VendorWindow:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "Vendor");
				break;
			case WindowType.PartyWindow:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "Party");
				break;
			case WindowType.Combine2:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "Combine2");
				break;
			case WindowType.Combine4:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "Combine4");
				break;
			case WindowType.Combine6:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "Combine6");
				break;
			case WindowType.Combine8:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "Combine8");
				break;
			case WindowType.Combine10:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "Combine10");
				break;
			case WindowType.QuestWindow:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "Quest");
				break;
			case WindowType.LargeQuestWindow:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "LargeQuest");
				break;
			case WindowType.TextWindow:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "TextWindow");
				break;
			case WindowType.DiscardButton:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "DiscardButton");
				break;
			case WindowType.LetterWindow:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "Letter");
				break;
			case WindowType.TradeWindow:
				windowObject = Resources.Load<WindowUI>("Prefabs" + SLASH + "Trade");
				break;
		}
		if(windowObject != null) {
			WindowUI window = Instantiate(windowObject, Vector3.zero, Quaternion.identity);
			window.SetWindowId(windowId);
			window.SetTitle(title);
			window.AddButtons(combineEnabled, closeEnabled, backEnabled, nextEnabled, okEnabled);
			window.SetNPCId(npcId);
			window.SetUnknownId(unknown);
			window.SetUnknown2Id(unknown2);
			window.gameObject.name = windowId.ToString();
			window.gameObject.tag = "Window";
			window.gameObject.SetActive(false);
			m_state.AddWindowToHierarchy(window);
			m_state.LoadWindow(windowId, windowType, window);
			return window;
		}
		return null;
	}

	public void RemoveWindow(int windowId) {
		WindowUI window = GetWindowUI(windowId);
		if(window != null) {
			if(windowId < 100) {
				window.gameObject.SetActive(false);
			}
			else {
				WindowType windowType = window.GetWindowType();
				m_state.RemoveWindow(windowId, windowType, window);
				Destroy(window.gameObject);
			}
		}
	}

	public void DoneSendingMessages() {
		HandleCurrentMessages();
		m_doneSendingMessages = true;
		m_state.RefreshCommandBar();
	}

	public void LoadItemDrop(int spriteId, int x, int y, string itemName, int count, Color itemColor) {
		ItemDrop itemDrop = GetItemDropAtPosition(x, y);
		if(itemDrop != null) {
			itemDrop.UpdateItem(spriteId, itemName, count, itemColor);
		}
		else {
			ItemDrop itemDropObject = Resources.Load<ItemDrop>("Prefabs" + SLASH + "ItemDrop");
			if (itemDropObject != null) {
				itemDrop = Instantiate(itemDropObject, WorldPosition(x, y), Quaternion.identity);
				itemDrop.transform.SetParent(m_worldObjects.transform, false);
				itemDrop.UpdateItem(spriteId, itemName, count, itemColor);
			}
		}
	}

	public void LoadSpell(int x, int y, int animationId) {
		Spell spell = GetSpellAtPosition(x, y);
		if(spell != null) {
			AnimationClip anim = Resources.Load<AnimationClip>("Animations" + SLASH + animationId);
			spell.PlayAnimation(anim);
		}
		else {
			Spell spellObject = Resources.Load<Spell>("Prefabs" + SLASH + "Spell");
			if (spellObject != null) {
				spell = Instantiate(spellObject, WorldPosition(x, y), Quaternion.identity);
				spell.transform.SetParent(m_worldObjects.transform, false);
				AnimationClip anim = Resources.Load<AnimationClip>("Animations" + SLASH + animationId);
				spell.PlayAnimation(anim);
			}
		}
	}

	public void LoadPlayerSpell(int playerId, int animationId) {
		AnimationClip anim = Resources.Load<AnimationClip>("Animations" + SLASH + animationId);
		(GetPlayerManager(playerId))?.LoadPlayerSpell(anim);
	}

	public void LoadPlayerEmoticon(int playerId, int emoteId) {
		Emote emote = EnumHelper.GetValueName<Emote>(emoteId);
        int animId;
        switch(emote) {
            case Emote.Heart:
                animId = 180;
                break;
            case Emote.Question:
                animId = 181;
                break;
            case Emote.Grumble:
                animId = 182;
                break;
            case Emote.Ellipsis:
                animId = 183;
                break;
            case Emote.Poop:
                animId = 184;
                break;
            case Emote.Surprise:
                animId = 185;
                break;
            case Emote.Sleep:
                animId = 186;
                break;
            case Emote.Angry:
                animId = 187;
                break;
            case Emote.Cry:
                animId = 188;
                break;
            case Emote.Music:
                animId = 189;
                break;
            case Emote.Dollar:
                animId = 190;
                break;
            case Emote.Wink:
                animId = 191;
                break;
            default:
                Debug.Log("Emote Failure.");
                return;
        }
		AnimationClip anim = Resources.Load<AnimationClip>("Animations" + SLASH + animId);
		(GetPlayerManager(playerId))?.LoadPlayerEmoticon(anim);
	}

	public void RemoveItemDrop(int x, int y) {
		ItemDrop itemDrop = GetItemDropAtPosition(x, y);
		if(itemDrop != null) {
            Destroy(itemDrop.gameObject);
        }
	}

	public Vector3 WorldPosition(int x, int y, bool applyWorldOffset = false) {
		Vector3 worldPosition;
		if(applyWorldOffset) {
			worldPosition = gameObject.transform.position;
		}
		else {
			worldPosition = new Vector3();
		}
		worldPosition.x += x - 51;
		worldPosition.y += 51 - y;
		return worldPosition;
	}

	public void ServerPosition(Vector3 worldPosition, out int x, out int y) {
		Vector3 worldOffset = gameObject.transform.position;
		worldPosition-= worldOffset;
		x = (int) Mathf.Round(worldPosition.x) + 51;
		y = - (int) Mathf.Round(worldPosition.y - 0.5f) + 51;
	}

    int Mod(int val, int denom) {
        return (val % denom + denom) % denom;
    }

	public void ToggleSound() {
		m_soundEnabled = !m_soundEnabled;
		if(m_soundEnabled) {
			AddColorChatMessage(7, "Sound has been enabled.");
		}
		else {
			AddColorChatMessage(7, "Sound has been disabled.");
		}
	}

	public void HandleFilter(string filter) {
		filter = filter.ToLower();
		if(filter.Equals(FILTER_CHAT)) {
			m_chatFilterEnabled = !m_chatFilterEnabled;
			FilterMessage(filter, m_chatFilterEnabled);
		}
		else if(filter.Equals(FILTER_GUILD)) {
			m_guildFilterEnabled = !m_guildFilterEnabled;
			FilterMessage(filter, m_guildFilterEnabled);
		}
		else if(filter.Equals(FILTER_GROUP)) {
			m_groupFilterEnabled = !m_groupFilterEnabled;
			FilterMessage(filter, m_groupFilterEnabled);
		}
		else if(filter.Equals(FILTER_TELL)) {
			m_tellFilterEnabled = !m_tellFilterEnabled;
			FilterMessage(filter, m_tellFilterEnabled);
		}
		else {
			AddColorChatMessage(7, "Usage: /filter <filter type>");
			AddColorChatMessage(7, "Filter types: Chat Guild Group Tell");
		}
	}

	void FilterMessage(string filter, bool enabled) {
		string formatedFilter = char.ToUpper(filter[0]) + filter.Substring(1);
		if(enabled) {
			AddColorChatMessage(7, formatedFilter + " filter has been Enabled.");
		}
		else {
			AddColorChatMessage(7, formatedFilter + " filter has been Disabled.");
		}
	}

	public void TradeDone() {
		m_state.TradeDone();
	}

	public void AddPlayerChatMessage(int playerId, string message) {
		(GetPlayerManager(playerId))?.DisplayChatBubble(message);
		AddColorChatMessage(1, message);
	}

	public void AddColorChatMessage(int messageType, string message) {
		//TODO Update to use enum
		switch(messageType) {
			case 1:	// Chat
				if(m_chatFilterEnabled) {
					AddChatMessage(message, AsperetaTextColor.white);
				}
				break;
			case 2: // Guild
				if(m_guildFilterEnabled) {
					AddChatMessage(message, AsperetaTextColor.yellow);
				}
				break;
			case 3: // Group
				if(m_groupFilterEnabled) {
					AddChatMessage(message, AsperetaTextColor.yellow);
				}
				break;
			case 6: // Tell
				if(m_tellFilterEnabled) {
					AddChatMessage(message, AsperetaTextColor.yellow);
				}
				break;
			case 7: // Server
				AddChatMessage(message, AsperetaTextColor.green);
				break;
			default:
				break;
		}
	}

	void AddChatMessage(string message, Color color) {
		m_state.AddChatMessage(message, color);
	}

    public bool HasBeenLoaded() {
        return m_handleMessages;
    }

	public void ContinueHandlingMessages() {
		m_handleMessages = true;
        m_doneSendingMessages = false;
		m_messageTimer = Time.time;
	}

	public void StopHandlingMessages() {
		m_handleMessages = false;
	}

    private void InitializeEvents() {
        //NOTE: See Goose.EventHandler for unknown commands
		messageToEvent.Add("$", new ColorMessageEvent());
		messageToEvent.Add("^", new PlayerMessageEvent());
		messageToEvent.Add("#", new MessageEvent());
		messageToEvent.Add("LOK", new LoginSuccessEvent());
		messageToEvent.Add("LNO", new LoginFailEvent());
		messageToEvent.Add("PING", new PingEvent());
		messageToEvent.Add("SCM", new MapChangeEvent());
		messageToEvent.Add("BUF", new BuffInfoEvent());
		messageToEvent.Add("SSS", new SpellInfoEvent());
		messageToEvent.Add("SIS", new InventoryInfoEvent());
		messageToEvent.Add("MKC", new CharacterInfoEvent());
		messageToEvent.Add("MKW", new MakeWindowEvent());
		messageToEvent.Add("ENW", new ShowWindowEvent());
		messageToEvent.Add("WNF", new WindowInfoEvent());
		messageToEvent.Add("ERC", new CharacterOffScreenEvent());
		messageToEvent.Add("MOC", new MovementEvent());
		messageToEvent.Add("CHH", new FacingEvent());
		messageToEvent.Add("SPA", new SpellEvent());
		messageToEvent.Add("SPP", new PlayerSpellEvent());
		messageToEvent.Add("MOB", new ItemDropEvent());
		messageToEvent.Add("EOB", new EraseObjectEvent());
		messageToEvent.Add("ATT", new AttackEvent());
		messageToEvent.Add("SMN", new MapNameEvent());
		messageToEvent.Add("SUC", new SelfEvent());
		messageToEvent.Add("SUP", new SelfPositionEvent());
		messageToEvent.Add("SNF", new StatInfoEvent());
		messageToEvent.Add("BT", new DisplayEvent());
		messageToEvent.Add("VC", new HealthManaEvent());
		messageToEvent.Add("UPV", new UpdateHealthManaEvent());
		messageToEvent.Add("TNL", new TillNextLevelEvent());
		messageToEvent.Add("CHP", new PlayerAppearanceEvent());
		messageToEvent.Add("AMA", new PlayerAdminEvent());
		messageToEvent.Add("WPS", new WeaponSpeedEvent());
		messageToEvent.Add("GUD", new GroupUpdateEvent());
		messageToEvent.Add("EMOT", new EmoticonEvent());
		messageToEvent.Add("SINVS", new SeeInvisEvent());
		messageToEvent.Add("INVS", new InvisEvent());
		messageToEvent.Add("SHUTDOWN", new ShutdownEvent());
		messageToEvent.Add("TRADEDONE", new TradeDoneEvent());
		messageToEvent.Add("TRADEUPDATE", new TradeUpdateEvent());
		messageToEvent.Add("DSM", new DoneSendingMessagesEvent());
    }

    private void HandleMessage(string message) {
		if(!string.IsNullOrEmpty(message)) {
			foreach(KeyValuePair<string, Event> entry in messageToEvent) {
				if(message.StartsWith(entry.Key)) {
					Event e = entry.Value;
					e.Run(this, message.Substring(entry.Key.Length));
					return;
				}
			}
		}
    }
}
