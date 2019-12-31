using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public enum NameFormat {
    Hidden, Visible, VisibleOnHover,
}

public enum HealthManaFormat {
    Hidden, Visible, VisibleOnUpdate,
}

public class PlayerState : MonoBehaviour
{
	public CursorUI cursor;
    public Camera mainCamera;

    public Dictionary<int,WindowUI> mappedWindowIds;
    public Dictionary<WindowType,List<WindowUI>> mappedWindowTypes;

    const float MOVEMENT_WAIT_TIME = 0.15f, NEW_WINDOW_OFFSET = 0.15625f;
    const string TELL_FROM = "[tell from] ";
    const string COLON = ":";
    const string FILTER_MESSAGE = "/filter", VITABAR_MESSAGE = "/vitabar";
    const string NAMES_MESSAGE = "/names", QUIT_MESSAGE = "/quit", SLASH = "/";
    const string TOGGLESOUND_MESSAGE = "/togglesound";

    static PlayerState instance;
    static PlayerManager m_playerManager;
    static int m_playerId, m_spellTargetIndex, m_spellTargetPlayerId;
    static bool m_chatEnabled, m_canSeeInvisible, m_spellTargetEnabled;
    static float movementTimer;
    static string m_messageFromPlayerName;
    static NameFormat m_nameFormat;
    static HealthManaFormat m_healthManaFormat;

    void Awake()
    {
		if(instance == null) {
			instance = this;
			DontDestroyOnLoad(gameObject);
            mappedWindowIds = new Dictionary<int,WindowUI>();
            mappedWindowTypes = new Dictionary<WindowType,List<WindowUI>>();
            LoadPlayerPreferences();
        }
        else {
            Destroy(gameObject);
        }
    }

    void LateUpdate() {
        if(m_playerManager != null && instance != null) {
            Vector3 position = m_playerManager.transform.localPosition;
            instance.transform.localPosition = position;
        }
    }

    void LoadPlayerPreferences() {
        int nameFormatIndex;
        if(PlayerPrefs.HasKey("PlayerState-NameFormat")) {
            nameFormatIndex = PlayerPrefs.GetInt("PlayerState-NameFormat");
        }
        else {
            nameFormatIndex = 1;
            PlayerPrefs.SetInt("PlayerState-NameFormat", nameFormatIndex);
        }
        m_nameFormat = EnumHelper.GetName<NameFormat>(nameFormatIndex);
        GameManager.instance.UpdatePlayerNameFormat();
        int healthManaFormatIndex;
        if(PlayerPrefs.HasKey("PlayerState-HealthManaFormat")) {
            healthManaFormatIndex = PlayerPrefs.GetInt("PlayerState-HealthManaFormat");
        }
        else {
            healthManaFormatIndex = 2;
            PlayerPrefs.SetInt("PlayerState-HealthManaFormat", nameFormatIndex);
        }
        m_healthManaFormat = EnumHelper.GetName<HealthManaFormat>(healthManaFormatIndex);
        GameManager.instance.UpdatePlayerHealthManaFormat();
        PlayerPrefs.Save();
    }

    public static GameObject GetGameObject() {
        return instance.gameObject;
    }

    public static void AddWindowToHierarchy(WindowUI window) {
        GameObject windowObject = window.gameObject;
        windowObject.transform.SetParent(instance.transform);
        windowObject.transform.SetAsLastSibling();
        windowObject.transform.localPosition = GetWindowPosition(window);
        UpdateSorting();
    }

    static Vector3 GetWindowPosition(WindowUI window) {
        Vector3 position = Vector3.zero;
        WindowType windowType = window.GetWindowType();
        BoxCollider2D collider = window.GetComponent<BoxCollider2D>();
        Vector3 sizeOffset = new Vector3(-collider.size.x / 2, (collider.size.y / 2) + 2f, 0);
        int windowCount = GetWindowTypeCount(windowType);
        sizeOffset += new Vector3(NEW_WINDOW_OFFSET * (windowCount % 12), -NEW_WINDOW_OFFSET * ((windowCount % 12) + Mathf.Floor(windowCount / 12)), 0);
		switch(windowType) {
			case WindowType.OptionsBar:
				position = new Vector3(7.21875f, -5.875f - 0.75f, 0);
				break;
			case WindowType.InventoryWindow:
				position = new Vector3(7.03125f, 5.5f, 0);
				break;
			case WindowType.SpellsWindow:
				position = new Vector3(-8.21875f, 5.5f, 0);
				break;
			case WindowType.CommandBar:
				position = new Vector3(-5.5f, 7.5f + 0.75f, 0);
				break;
			case WindowType.BuffBar:
				position = new Vector3(-12.21875f, 7.5f + 0.75f, 0);
				break;
			case WindowType.FpsBar:
				position = new Vector3(10.65625f, 7.5f + 0.75f, 0);
				break;
			case WindowType.HealthBar:
				position = new Vector3(9.15625f, -2.75f, 0);
				break;
			case WindowType.ManaBar:
				position = new Vector3(9.15625f, -3.4375f, 0);
				break;
			case WindowType.SpiritBar:
				position = new Vector3(9.15625f, -4.125f, 0);
				break;
			case WindowType.ExperienceBar:
				position = new Vector3(9.15625f, -4.8125f, 0);
				break;
			case WindowType.CharacterWindow:
				position = new Vector3(-5.53125f, 6, 0);
				break;
			case WindowType.ChatWindow:
				position = new Vector3(-12.21875f, -2.5f - 0.75f, 0);
				break;
			case WindowType.VendorWindow:
                position = sizeOffset;
				break;
			case WindowType.PartyWindow:
				position = new Vector3(0.09375f, -1.1875f - 0.75f, 0);
				break;
			case WindowType.Combine2:
                position = sizeOffset;
				break;
			case WindowType.Combine4:
                position = sizeOffset;
				break;
			case WindowType.Combine6:
                position = sizeOffset;
				break;
			case WindowType.Combine8:
                position = sizeOffset;
				break;
			case WindowType.Combine10:
                position = sizeOffset;
				break;
			case WindowType.QuestWindow:
                position = sizeOffset;
				break;
			case WindowType.LargeQuestWindow:
                position = sizeOffset;
				break;
			case WindowType.TextWindow:
                position = sizeOffset;
				break;
			case WindowType.DiscardButton:
				position = new Vector3(4.65625f, -5.71875f - 0.75f, 0);
				break;
			case WindowType.LetterWindow:
                position = sizeOffset;
				break;
			case WindowType.TradeWindow:
                position = sizeOffset;
				break;
		}
        return position;
    }

    public static bool TryGetWindowUI(int windowId, out WindowUI window) {
        return instance.mappedWindowIds.TryGetValue(windowId, out window);
    }

    public static void ShowWindow(int windowId) {
        if(TryGetWindowUI(windowId, out WindowUI window)) {
            window.gameObject.SetActive(true);
        }
    }

    public static int GetWindowTypeCount(WindowType windowType) {
        if(!instance.mappedWindowTypes.TryGetValue(windowType, out List<WindowUI> list)) {
            return 0;
        }
        return list.Count();
    }

    public static void LoadWindow(int windowId, WindowType windowType, WindowUI window) {
        instance.mappedWindowIds.Add(windowId, window);
        if(!instance.mappedWindowTypes.TryGetValue(windowType, out List<WindowUI> list)) {
            list = new List<WindowUI>();
            instance.mappedWindowTypes.Add(windowType, list);
        }
        else {
            WindowUI originalWindow = list.FirstOrDefault();
            if(originalWindow != null) {
                window.Copy(originalWindow);
            }
        }
        instance.mappedWindowTypes[windowType].Add(window);
    }

    public static void RemoveWindow(int windowId, WindowType windowType, WindowUI window) {
        instance.mappedWindowIds.Remove(windowId);
        if(instance.mappedWindowTypes.TryGetValue(windowType, out List<WindowUI> list)) {
            list.Remove(window);
            if(list.Count == 0) {
                instance.mappedWindowTypes.Remove(windowType);
            }
        }
    }

    public static void LeftMouseUp(bool controlKeyPressed) {
        instance.cursor.LeftMouseUp(controlKeyPressed);
    }

    public static void LeftMouseDown() {
        instance.cursor.LeftMouseDown();
    }

    public static void LeftDoubleClick() {
        instance.cursor.LeftDoubleClick();
    }

    public static void RightMouseUp() {
        instance.cursor.RightMouseUp();
    }

    public static void RightMouseDown(bool controlKeyPressed) {
        instance.cursor.RightMouseDown(controlKeyPressed);
    }

    public static void EnableCamera() {
        SetGameUIActive(true);
    }

    public static void DisableCamera() {
        SetGameUIActive(false);
    }

    private static void SetGameUIActive(bool isActive) {
        if(instance != null) {
            instance.gameObject.SetActive(isActive);
            instance.mainCamera.gameObject.SetActive(isActive);
            foreach (Transform child in instance.gameObject.transform) {
                SpriteRenderer spriteRenderer = child.gameObject.GetComponent<SpriteRenderer>();
                if(spriteRenderer != null) {
                    spriteRenderer.enabled = isActive;
                }
            }
        }
    }

    public static void Destroy() {
        if(instance != null) {
            Destroy(instance.gameObject);
            instance = null;
        }
    }

    public static void RefreshCommandBar() {
        if(TryGetCommandBar(out List<CommandBarUI> commandBarList)) {
            foreach(CommandBarUI commandBar in commandBarList) {
                commandBar.LoadDefaults();
            }
        }
    }

    public static SlotUI GetWindowSlot(WindowType windowType, int index) {
        if(windowType.Equals(WindowType.InventoryWindow)) {
            if(TryGetInventoryWindow(out List<InventoryWindowUI> inventoryWindowList)) {
                return inventoryWindowList.FirstOrDefault()?.GetSlot(index);
            }
        }
        else if(windowType.Equals(WindowType.SpellsWindow)) {
            if(TryGetSpellsWindow(out List<SpellsWindowUI> spellsWindowList)) {
                return spellsWindowList.FirstOrDefault()?.GetSlot(index);
            }
        }
        else if(windowType.Equals(WindowType.CharacterWindow)) {
            if(TryGetCharacterWindow(out List<CharacterWindowUI> characterWindowList)) {
                return characterWindowList.FirstOrDefault()?.GetSlot(index);
            }
        }
        return null;
    }
    
    static void CastTargetSpell(int index) {
        m_spellTargetEnabled = true;
        m_spellTargetIndex = index;
        GameObject targetedPlayer = GameManager.instance?.GetPlayer(m_spellTargetPlayerId);
        if(!WithinSpellRange(targetedPlayer)) {
            m_spellTargetPlayerId = m_playerId;
        }
        GameManager.instance?.TargetSpellCast(m_spellTargetPlayerId);
    }

    static bool WithinSpellRange(GameObject player) {
        if(player != null) {
            GameObject mainPlayer = m_playerManager.gameObject;
            if(GameManager.instance != null && GameManager.instance.WithinSpellRange(mainPlayer, player)) {
                return true;
            }
        }
        return false;
    }

    public static void UseCommandSlot(int index) {
        if(!m_chatEnabled && !m_spellTargetEnabled) {
            if(TryGetCommandBar(out List<CommandBarUI> commandBarList)) {
                SlotUI slot = commandBarList.FirstOrDefault()?.GetSlot(index);
                if(slot != null && slot.GetReferenceWindow() != null) {    
                    WindowType referenceWindowType = slot.GetReferenceWindow().GetWindowType();
                    int referenceIndex = slot.GetReferenceIndex();
                    
                    if(referenceWindowType.Equals(WindowType.InventoryWindow) || referenceWindowType.Equals(WindowType.CharacterWindow)) {
                        GameManager.instance?.SendMessageToServer(Packet.Use(referenceIndex));
                    }
                    else if(referenceWindowType.Equals(WindowType.SpellsWindow)) {
                        string spellTarget = slot.GetSpellTarget();
                        if(spellTarget.Equals("T")) {
                            CastTargetSpell(referenceIndex);
                        }
                        else {
                            GameManager.instance?.SendMessageToServer(Packet.Cast(referenceIndex, m_playerId));
                        }
                    }
                }
            }
        }
    }

    public static void AddCommandSlot(int slotIndex, SlotUI slot) {
        if(TryGetCommandBar(out List<CommandBarUI> commandBarList)) {
            foreach(CommandBarUI commandBar in commandBarList) {
                commandBar.CopySlot(slotIndex, slot);
            }
            PlayerPrefs.Save();
        }
    }

    public static void ClearCommandSlot(int slotIndex) {
        if(TryGetCommandBar(out List<CommandBarUI> commandBarList)) {
            foreach(CommandBarUI commandBar in commandBarList) {
                commandBar.ClearSlot(slotIndex);
            }
        }
    }

    public static void LeftArrow() {
        if(m_spellTargetEnabled) {
            if(GameManager.instance != null && GameManager.instance.TryGetTargetPlayerUp(m_spellTargetPlayerId, out int playerId)) {
                SwapSpellTarget(playerId);
            }
            else {
                SwapSpellTarget(m_playerId);
            }
        }
        else if(m_chatEnabled) {
            if(TryGetChatWindow(out List<ChatWindowUI> characterWindowList)) {
                foreach(ChatWindowUI chatWindow in characterWindowList) {
                    chatWindow.MoveInputIndexLeft();
                }
            }
        }
    }

    public static void RightArrow() {
        if(m_spellTargetEnabled) {
            if(GameManager.instance != null && GameManager.instance.TryGetTargetPlayerDown(m_spellTargetPlayerId, out int playerId)) {
                SwapSpellTarget(playerId);
            }
            else {
                SwapSpellTarget(m_playerId);
            }
        }
        else if(m_chatEnabled) {
            if(TryGetChatWindow(out List<ChatWindowUI> characterWindowList)) {
                foreach(ChatWindowUI chatWindow in characterWindowList) {
                    chatWindow.MoveInputIndexRight();
                }
            }
        }
    }

    public static void UpArrow() {
        if(m_spellTargetEnabled) {
            if(GameManager.instance != null && GameManager.instance.TryGetTargetPlayerUp(m_spellTargetPlayerId, out int playerId)) {
                SwapSpellTarget(playerId);
            }
            else {
                SwapSpellTarget(m_playerId);
            }
        }
        else if(m_chatEnabled) {
            if(TryGetChatWindow(out List<ChatWindowUI> characterWindowList)) {
                foreach(ChatWindowUI chatWindow in characterWindowList) {
                    chatWindow.GetPreviousInput();
                }
            }
        }
    }

    public static void DownArrow() {
        if(m_spellTargetEnabled) {
            if(GameManager.instance != null && GameManager.instance.TryGetTargetPlayerDown(m_spellTargetPlayerId, out int playerId)) {
                SwapSpellTarget(playerId);
            }
            else {
                SwapSpellTarget(m_playerId);
            }
        }
        else if(m_chatEnabled) {
            if(TryGetChatWindow(out List<ChatWindowUI> characterWindowList)) {
                foreach(ChatWindowUI chatWindow in characterWindowList) {
                    chatWindow.GetNextInput();
                }
            }
        }
    }

    static void SwapSpellTarget(int playerId) {
        if(playerId != m_spellTargetPlayerId) {
            GameManager.instance?.CancelTargetSpellCast(m_spellTargetPlayerId);
            m_spellTargetPlayerId = playerId;
            GameManager.instance?.TargetSpellCast(m_spellTargetPlayerId);
        }
    }

    public static void PageUp() {
        if(TryGetChatWindow(out List<ChatWindowUI> characterWindowList)) {
            foreach(ChatWindowUI chatWindow in characterWindowList) {
                chatWindow.PageUp();
            }
        }
    }

    public static void PageDown() {
        if(TryGetChatWindow(out List<ChatWindowUI> characterWindowList)) {
            foreach(ChatWindowUI chatWindow in characterWindowList) {
                chatWindow.PageDown();
            }
        }
    }

    public static void UseEmote(Emote emote) {
        if(!m_spellTargetEnabled) {
            int value = EnumHelper.GetNameValue<Emote>(emote);
            GameManager.instance?.SendMessageToServer(Packet.Emote(value));
        }
    }

    public static void PickUp() {
        if(!m_chatEnabled && !m_spellTargetEnabled) {
            GameManager.instance?.SendMessageToServer(Packet.PickUp());
        }
    }

    public static void ToggleSpellsWindow() {
        if(!m_chatEnabled && !m_spellTargetEnabled && TryGetFirstWindow(WindowType.SpellsWindow, out GameObject spellsWindowObject)) {
            ToggleActive(spellsWindowObject);
        }
    }

    public static void ToggleInventory() {
        if(!m_chatEnabled && !m_spellTargetEnabled && TryGetFirstWindow(WindowType.InventoryWindow, out GameObject inventoryWindowObject)) {
            ToggleActive(inventoryWindowObject);
        }
    }

    public static void ToggleCharacterWindow() {
        if(!m_chatEnabled && !m_spellTargetEnabled && TryGetFirstWindow(WindowType.CharacterWindow, out GameObject characterWindowObject)) {
            ToggleActive(characterWindowObject);
        }
    }

    public static void MinimizeScreen() {
        GameManager.instance?.MinimizeScreen();
    }

    public static void ToggleCommandBar() {
        if(TryGetFirstWindow(WindowType.CommandBar, out GameObject commandBarObject)) {
            ToggleActive(commandBarObject);
        }
    }

    public static void ToggleBuffBar() {
        if(TryGetFirstWindow(WindowType.BuffBar, out GameObject buffBarObject)) {
            ToggleActive(buffBarObject);
        }
    }

    public static void ToggleChatWindow() {
        if(TryGetFirstWindow(WindowType.ChatWindow, out GameObject chatWindowObject)) {
            ToggleActive(chatWindowObject);
            //if(!instance.chatWindow.gameObject.activeSelf) {
            //    instance.chatWindow.DisableInputText();
            //    m_chatEnabled = false;
            //}
            // TODO Decide if this should be re-added
        }
    }

    public static void ToggleFPSBar() {
        if(TryGetFirstWindow(WindowType.FpsBar, out GameObject fpsBarObject)) {
            ToggleActive(fpsBarObject);
        }
    }

    public static void ToggleHealthBar() {
        if(TryGetFirstWindow(WindowType.HealthBar, out GameObject healthBarObject)) {
            ToggleActive(healthBarObject);
        }
    }

    public static void ToggleManaBar() {
        if(TryGetFirstWindow(WindowType.ManaBar, out GameObject manaBarObject)) {
            ToggleActive(manaBarObject);
        }
    }

    public static void ToggleSpiritBar() {
        if(TryGetFirstWindow(WindowType.SpiritBar, out GameObject spiritBarObject)) {
            ToggleActive(spiritBarObject);
        }
    }

    public static void ToggleExperienceBar() {
        if(TryGetFirstWindow(WindowType.ExperienceBar, out GameObject experienceBarObject)) {
            ToggleActive(experienceBarObject);
        }
    }

    public static void TogglePartyWindow(bool toggleWhenChatDisabled) {
        if(!toggleWhenChatDisabled || toggleWhenChatDisabled && !m_chatEnabled && !m_spellTargetEnabled) {
            if(TryGetFirstWindow(WindowType.PartyWindow, out GameObject partyWindowObject)) {
                ToggleActive(partyWindowObject);
            }
        }
    }

    public static void ToggleOptionsBar() {
        if(TryGetFirstWindow(WindowType.OptionsBar, out GameObject optionsBarObject)) {
            ToggleActive(optionsBarObject);
        }
    }

    public static void ToggleDiscardButton() {
        if(TryGetFirstWindow(WindowType.DiscardButton, out GameObject discardButtonObject)) {
            ToggleActive(discardButtonObject);
        }
    }

    public static void HandlePlayerMovement(Vector3 inputVector) {
        if(!m_spellTargetEnabled) {
            if((m_playerManager != null) && !m_chatEnabled && (!inputVector.Equals(Vector3.zero))) {
                if (!m_playerManager.IsFacingDirection(inputVector)) {
                    if (m_playerManager.Face(inputVector, true)) {
                        movementTimer = Time.time;
                        FacingDirection direction = m_playerManager.GetPlayerFacingDirection();
                        GameManager.instance.SendMessageToServer(Packet.Face(direction));
                    }
                }
                else if(Time.time - movementTimer > MOVEMENT_WAIT_TIME) {
                    if(m_playerManager.Move(true)){
                        MovingDirection direction = m_playerManager.GetPlayerMovingDirection();
                        GameManager.instance.SendMessageToServer(Packet.Move(direction));
                    }
                }
            }
        }
    }

    public static void Attack() {
        if(!m_chatEnabled && !m_spellTargetEnabled) {
            if(m_playerManager != null && m_playerManager.Attack(true)){
                GameManager.instance.SendMessageToServer(Packet.Attack());
            }
        }
    }

    public static NameFormat GetNameFormat() {
        return m_nameFormat;
    }

    public static HealthManaFormat GetHealthManaFormat() {
        return m_healthManaFormat;
    }

    public static void SwitchNameFormat() {
        m_nameFormat = EnumHelper.GetNextName<NameFormat>(m_nameFormat);
        PlayerPrefs.SetInt("PlayerState-NameFormat", EnumHelper.GetIndex<NameFormat>(m_nameFormat));
        switch(m_nameFormat) {
            case NameFormat.Hidden:
                GameManager.instance.AddColorChatMessage(7, "Player names are now disabled.");
                break;
            case NameFormat.VisibleOnHover:
                GameManager.instance.AddColorChatMessage(7, "Player names are now visible when mouse is over target.");
                break;
            case NameFormat.Visible:
                GameManager.instance.AddColorChatMessage(7, "Player names are now visible.");
                break;
        }
        GameManager.instance.UpdatePlayerNameFormat();
        PlayerPrefs.Save();
    }
    
    public static void SwitchHealthManaFormat() {
        m_healthManaFormat = EnumHelper.GetNextName<HealthManaFormat>(m_healthManaFormat);
        PlayerPrefs.SetInt("PlayerState-HealthManaFormat", EnumHelper.GetIndex<HealthManaFormat>(m_healthManaFormat)); 
        switch(m_healthManaFormat) {
            case HealthManaFormat.Hidden:
                GameManager.instance.AddColorChatMessage(7, "Player vitality bars are now hidden.");
                break;
            case HealthManaFormat.VisibleOnUpdate:
                GameManager.instance.AddColorChatMessage(7, "Player vitality bars are being shown when updated.");
                break;
            case HealthManaFormat.Visible:
                GameManager.instance.AddColorChatMessage(7, "Player vitality bars are now visible.");
                break;
        }
        GameManager.instance.UpdatePlayerHealthManaFormat();
        PlayerPrefs.Save();
    }

    public static void EnableChat() {
        if(!m_chatEnabled && !m_spellTargetEnabled) {
            if(TryGetChatWindow(out List<ChatWindowUI> characterWindowList)) {
                foreach(ChatWindowUI chatWindow in characterWindowList) {
                    chatWindow.EnableInputText();
                }
            }
            m_chatEnabled = true;
        }
    }

    public static void Escape() {
        if(m_spellTargetEnabled) {
            GameManager.instance?.CancelTargetSpellCast(m_spellTargetPlayerId);
            m_spellTargetEnabled = false;
        }
        else if(m_chatEnabled) {
            if(TryGetChatWindow(out List<ChatWindowUI> characterWindowList)) {
                foreach(ChatWindowUI chatWindow in characterWindowList) {
                    chatWindow.DisableInputText();
                }
            }
            m_chatEnabled = false;
        }
    }

    public static void Home() {
        if(m_spellTargetEnabled) {
            SwapSpellTarget(m_playerId);
        }
    }

    public static void Enter() {
        if(m_spellTargetEnabled) {
            GameManager.instance?.HandleTargetSpellCast(m_spellTargetIndex, m_spellTargetPlayerId);
            m_spellTargetEnabled = false;
        }
        else {
            m_chatEnabled = !m_chatEnabled;
            bool inputHandled = false;
            if(TryGetChatWindow(out List<ChatWindowUI> characterWindowList)) {
                foreach(ChatWindowUI chatWindow in characterWindowList) {
                    if(m_chatEnabled) {
                        chatWindow.EnableInputText();
                    }
                    else {
                        string inputText = chatWindow.GetChatInputToHandle();
                        if(!inputHandled) {
                            HandleChatInput(inputText);
                            inputHandled = true;
                        }
                    }
                }
            }
        }
    }

    static void HandleChatInput(string inputText) {
        if(inputText.StartsWith(SLASH)) {
            if(inputText.Equals(QUIT_MESSAGE)) {
                GameManager.instance.Disconnect();
            }
            else if(inputText.Equals(NAMES_MESSAGE)) {
                PlayerState.SwitchNameFormat();
            }
            else if(inputText.Equals(VITABAR_MESSAGE)) {
                PlayerState.SwitchHealthManaFormat();
            }
            else if(inputText.StartsWith(FILTER_MESSAGE)) {
                if(inputText.Length > FILTER_MESSAGE.Length) {
                    inputText = inputText.Substring(FILTER_MESSAGE.Length + 1);
                }
                GameManager.instance.HandleFilter(inputText);
            }
            else if(inputText.StartsWith(TOGGLESOUND_MESSAGE)) {
                GameManager.instance.ToggleSound();
            }
            else if(inputText.StartsWith("/send")){ // TODO Remove this
                GameManager.instance.SendMessageToServer(inputText.Substring(6));
            }
            else {
                GameManager.instance.SendMessageToServer(Packet.ChatCommand(inputText));
            }
        }
        else {
            GameManager.instance.SendMessageToServer(Packet.ChatMessage(inputText));
        }
    }

    public static void AddInputText(string input) {
        if(m_chatEnabled && !m_spellTargetEnabled) {
            if(TryGetChatWindow(out List<ChatWindowUI> characterWindowList)) {
                foreach(ChatWindowUI chatWindow in characterWindowList) {
                    chatWindow.AddInputText(input);
                }
            }
        }
    }

    public static void AddGuildText() {
        if(!m_chatEnabled && !m_spellTargetEnabled) {
            EnableChat();
            AddInputText("/guild ");
        }
    }

    public static void AddTellText() {
        if(!m_chatEnabled && !m_spellTargetEnabled) {
            EnableChat();
            AddInputText("/tell ");
        }
    }

    public static void AddReplyText() {
        if(!m_chatEnabled && !m_spellTargetEnabled) {
            EnableChat();
            AddInputText("/tell " + m_messageFromPlayerName + " ");
        }
    }

    static void ToggleActive(GameObject gameObject) {
        gameObject.SetActive(!gameObject.activeSelf);
        if(gameObject.activeSelf) {
            MoveWindowToFront(gameObject);
        }
    }

    public static void MoveWindowToFront(GameObject gameObject) {
        gameObject.transform.SetAsLastSibling();
        UpdateSorting();
    }

    static void UpdateSorting() {
        if(instance != null) {
            foreach(Transform windowTransform in instance.transform) {
                SortingGroup group = windowTransform.gameObject.GetComponent<SortingGroup>();
                if(group != null) {
                    int index = windowTransform.GetSiblingIndex();
                    group.sortingOrder = index;
                }
            }
        }
    }

    public static bool IsChatEnabled() {
        return m_chatEnabled;
    }

    public static bool IsMainPlayer(int playerId) {
        return playerId.Equals(m_playerId);
    }

    public static PlayerManager GetMainPlayerManager() {
        return m_playerManager;
    }

    public static void TradeDone() {
        if(TryGetTradeWindow(out List<TradeWindowUI> tradeWindowList)) {
            foreach(TradeWindowUI tradeWindow in tradeWindowList) {
                Destroy(tradeWindow.gameObject);
            }
        }
    }

    public static void UpdateTradeEnabled(bool enabledForSelf, bool tradeEnabled) {
        if(TryGetTradeWindow(out List<TradeWindowUI> tradeWindowList)) {
            foreach(TradeWindowUI tradeWindow in tradeWindowList) {
                tradeWindow.SetTradeEnabled(enabledForSelf, tradeEnabled);
            }
        }
    }
    
    public static void UpdateTradeGold(bool goldForSelf, int amount) {
        if(TryGetTradeWindow(out List<TradeWindowUI> tradeWindowList)) {
            foreach(TradeWindowUI tradeWindow in tradeWindowList) {
                tradeWindow.SetTradeGold(goldForSelf, amount);
            }
        }
    }

    public static void UpdateTradeSlot(bool itemForSelf, int slotIndex, string itemName, int amount, int graphicId, int itemSlotId, Color itemSlotColor) {
        if(TryGetTradeWindow(out List<TradeWindowUI> tradeWindowList)) {
            foreach(TradeWindowUI tradeWindow in tradeWindowList) {
                tradeWindow.SetTradeSlotId(itemForSelf, slotIndex, itemSlotId);
                tradeWindow.SetTradeSlotDescription(itemForSelf, slotIndex, itemName);
                tradeWindow.SetTradeSlotAmount(itemForSelf, slotIndex, amount);
                tradeWindow.UpdateTradeSlotGraphic(itemForSelf, slotIndex, graphicId, itemSlotColor);
            }
        }
    }

    public static void UpdateItemSlot(int slotIndex, int itemId, string itemName, int amount, int graphicId, Color itemSlotColor) {
        if(TryGetInventoryWindow(out List<InventoryWindowUI> inventoryWindowList)) {
            foreach(InventoryWindowUI inventoryWindow in inventoryWindowList) {
                inventoryWindow.SetSlotId(slotIndex, itemId);
                inventoryWindow.SetSlotDescription(slotIndex, itemName);
                inventoryWindow.SetSlotAmount(slotIndex, amount);
                inventoryWindow.UpdateSlotGraphic(slotIndex, graphicId, itemSlotColor);
            }
        }
    }

    public static void UpdateSpellSlot(int slotIndex, string spellName, int soundId, int spellId, string spellTarget, int graphicId) {
        if(TryGetSpellsWindow(out List<SpellsWindowUI> spellsWindowList)) {
            foreach(SpellsWindowUI spellsWindow in spellsWindowList) {
                spellsWindow.SetSlotId(slotIndex, spellId);
                spellsWindow.SetSlotDescription(slotIndex, spellName);
                spellsWindow.UpdateSlotGraphic(slotIndex, graphicId);
                spellsWindow.SetSoundId(slotIndex, soundId);
                spellsWindow.SetSpellTarget(slotIndex, spellTarget);
            }
        }
    }

	public static void UpdateBuffSlot(int slotIndex, string spellName, int spellId) {
        if(TryGetBuffBar(out List<BuffBarUI> buffBarList)) {
            foreach(BuffBarUI buffBar in buffBarList) {
                buffBar.SetSpellName(slotIndex, spellName);
                buffBar.UpdateSpellGraphic(slotIndex, spellId);
            }
        }
	}

	public static void UpdateWindowLine(int windowId, int windowLine, string description, int amount, int slotId, int graphicId, Color slotColor) {
        if(TryGetWindowUI(windowId, out WindowUI window)) {
            window.SetSlotId(windowLine, slotId);
            window.SetSlotDescription(windowLine, description);
            window.SetSlotAmount(windowLine, amount);
            window.UpdateSlotGraphic(windowLine, graphicId, slotColor);
        }
	}

    public static void SetMainPlayerName(int playerId, string name) {
        if(IsMainPlayer(playerId)) {
            if(TryGetCharacterWindow(out List<CharacterWindowUI> characterWindowList)) {
                foreach(CharacterWindowUI characterWindow in characterWindowList) {
                    characterWindow.SetPlayerName(name);
                }
            }
        }
    }

    public static void SetMainPlayer(int playerId, GameObject playerObject) {
        RemoveMainPlayer();
        m_playerId = playerId;
        if (playerObject != null) {
            m_playerManager = playerObject.GetComponent<PlayerManager>();
            string name = m_playerManager.GetPlayerName();
            SetMainPlayerName(playerId, name);
            if(playerObject.layer != 12) {
                playerObject.layer = 11;
            }
        }
		EnableCamera();
    }

    static void RemoveMainPlayer() {
        if(m_playerManager != null) {
            GameObject playerObject = m_playerManager.gameObject;
            if(playerObject != null && playerObject.layer == 11) {
                playerObject.layer = 0;
            }
            m_playerId = 0;
            m_playerManager = null;
        }
    }

    public static void SetMainPlayerCanSeeInvisible(bool canSeeInvisible) {
        m_canSeeInvisible = canSeeInvisible;
    }

    public static bool GetMainPlayerCanSeeInvisible() {
        return m_canSeeInvisible;
    }

    public static void SetMainPlayerPosition(int x, int y) {
        m_playerManager?.SetPlayerPosition(x, y);
    }

    public static void SetMainPlayerAttackSpeed(int weaponSpeed) {
        m_playerManager?.SetPlayerAttackSpeed(weaponSpeed);
    }

    public static void SetMainPlayerStatInfo(string guildName, string unknown, string className, int level, int maxHp, int maxMp, int maxSp, int curHp, int curMp, int curSp, 
            int statStr, int statSta, int statInt, int statDex, int armor, int resFire, int resWater, int resEarth, int resAir, int resSpirit, int gold) {
        SetMainPlayerHealth(curHp, maxHp);
        SetMainPlayerMana(curMp, maxMp);
        SetMainPlayerSpirit(curSp, maxSp);
        
        if(TryGetCharacterWindow(out List<CharacterWindowUI> characterWindowList)) {
            foreach(CharacterWindowUI characterWindow in characterWindowList) {
                characterWindow.SetGuildName(guildName);
                characterWindow.SetClassName(className);
                characterWindow.SetPlayerLevel(level);
                characterWindow.SetStrength(statStr);
                characterWindow.SetStamina(statSta);
                characterWindow.SetIntelligence(statInt);
                characterWindow.SetDexterity(statDex);
                characterWindow.SetArmor(armor);
                characterWindow.SetFireResistance(resFire);
                characterWindow.SetWaterResistance(resWater);
                characterWindow.SetEarthResistance(resEarth);
                characterWindow.SetAirResistance(resAir);
                characterWindow.SetSpiritResistance(resSpirit);
                characterWindow.SetGold(gold);
            }
        }
    }

	public static void SetMainPlayerHPMPSP(int hpMax, int mpMax, int spMax, int hp, int mp, int sp) {
		SetMainPlayerHealth(hp, hpMax);
        SetMainPlayerMana(mp, mpMax);
        SetMainPlayerSpirit(sp, spMax);
	}

    static void SetMainPlayerHealth(int curHp, int maxHp) {
        if(TryGetHealthBar(out List<StatBarUI> healthBarList)) {
            foreach(StatBarUI healthBar in healthBarList) {
                UpdateStatBar(healthBar, curHp, maxHp);
            }
        }
        if(TryGetCharacterWindow(out List<CharacterWindowUI> characterWindowList)) {
            foreach(CharacterWindowUI characterWindow in characterWindowList) {
                characterWindow.SetHealth(curHp, maxHp);
            }
        }
    }

    static void SetMainPlayerMana(int curMp, int maxMp) {
        if(TryGetManaBar(out List<StatBarUI> manaBarList)) {
            foreach(StatBarUI manaBar in manaBarList) {
                UpdateStatBar(manaBar, curMp, maxMp);
            }
        }
        if(TryGetCharacterWindow(out List<CharacterWindowUI> characterWindowList)) {
            foreach(CharacterWindowUI characterWindow in characterWindowList) {
                characterWindow.SetMana(curMp, maxMp);
            }
        }
    }

    static void SetMainPlayerSpirit(int curSp, int maxSp) {
        if(TryGetSpiritBar(out List<StatBarUI> spiritBarList)) {
            foreach(StatBarUI spiritBar in spiritBarList) {
                UpdateStatBar(spiritBar, curSp, maxSp);
            }
        }
        if(TryGetCharacterWindow(out List<CharacterWindowUI> characterWindowList)) {
            foreach(CharacterWindowUI characterWindow in characterWindowList) {
                characterWindow.SetSpirit(curSp, maxSp);
            }
        }
    }	

    static void UpdateStatBar(StatBarUI statBar, int curStat, int maxStat) {
        int percent = 0;
        if(maxStat > 0) {
            percent = (int) Mathf.Ceil(100f * curStat / maxStat);
        }
        statBar.SetStatAmount(curStat);
        statBar.SetStatPercent(percent);
    }

    public static void SetMainPlayerExperience(int percent, int experience, int experienceTillNextLevel) {
        if(TryGetExperienceBar(out List<StatBarUI> experienceBarList)) {
            foreach(StatBarUI experienceBar in experienceBarList) {
                experienceBar.SetStatAmount(experienceTillNextLevel);
                experienceBar.SetStatPercent(percent);
            }
        }
        if(TryGetCharacterWindow(out List<CharacterWindowUI> characterWindowList)) {
            foreach(CharacterWindowUI characterWindow in characterWindowList) {
                characterWindow.SetExperience(experience);
            }
        }
    }
    
    public static void UpdatePartyIndex(int index, int playerId, string name, int level, string className) {
        if(TryGetPartyWindow(out List<PartyWindowUI> partyWindowList)) {
            foreach(PartyWindowUI partyWindow in partyWindowList) {
                partyWindow.UpdatePartyIndex(index, playerId, name, level, className);
            }
        }
	}

    public static void UpdatePartyPlayerHP(int playerId, int hpPercent) {
        if(TryGetPartyWindow(out List<PartyWindowUI> partyWindowList)) {
            foreach(PartyWindowUI partyWindow in partyWindowList) {
                partyWindow.SetHPBar(playerId, hpPercent);
            }
        }
    }

    public static void UpdatePartyPlayerMP(int playerId, int mpPercent) {
        if(TryGetPartyWindow(out List<PartyWindowUI> partyWindowList)) {
            foreach(PartyWindowUI partyWindow in partyWindowList) {
                partyWindow.SetMPBar(playerId, mpPercent);
            }
        }
    }

    public static bool IsPlayerInParty(int playerId) {
        if(TryGetPartyWindow(out List<PartyWindowUI> partyWindowList)) {
            foreach(PartyWindowUI partyWindow in partyWindowList) {
                if(partyWindow.IsPlayerInParty(playerId)) {
                    return true;
                }
            }
        }
        return false;
    }

    public static void AddChatMessage(string message, Color color) {
        if(TryGetChatWindow(out List<ChatWindowUI> characterWindowList)) {
            foreach(ChatWindowUI chatWindow in characterWindowList) {
                chatWindow.AddChatMessage(message, color);
            }
        }
        if(message.StartsWith(TELL_FROM)) {
            int playerNameLength = message.IndexOf(COLON) - TELL_FROM.Length;
            string playerName = message.Substring(TELL_FROM.Length, playerNameLength);
            m_messageFromPlayerName = playerName;
        }
        
    }

    static bool TryGetCommandBar(out List<CommandBarUI> commandBarList) {
        return TryGetWindow<CommandBarUI>(WindowType.CommandBar, out commandBarList);
    }

    static bool TryGetBuffBar(out List<BuffBarUI> buffBarList) {
        return TryGetWindow<BuffBarUI>(WindowType.BuffBar, out buffBarList);
    }

    static bool TryGetSpellsWindow(out List<SpellsWindowUI> spellsWindowList) {
        return TryGetWindow<SpellsWindowUI>(WindowType.SpellsWindow, out spellsWindowList);
    }

    static bool TryGetInventoryWindow(out List<InventoryWindowUI> inventoryWindowList) {
        return TryGetWindow<InventoryWindowUI>(WindowType.InventoryWindow, out inventoryWindowList);
    }

    static bool TryGetTradeWindow(out List<TradeWindowUI> tradeWindowList) {
        return TryGetWindow<TradeWindowUI>(WindowType.TradeWindow, out tradeWindowList);
    }

    static bool TryGetChatWindow(out List<ChatWindowUI> chatWindowList) {
        return TryGetWindow<ChatWindowUI>(WindowType.ChatWindow, out chatWindowList);
    }

    static bool TryGetPartyWindow(out List<PartyWindowUI> partyWindowList) {
        return TryGetWindow<PartyWindowUI>(WindowType.PartyWindow, out partyWindowList);
    }

    static bool TryGetHealthBar(out List<StatBarUI> statBarList) {
        return TryGetWindow<StatBarUI>(WindowType.HealthBar, out statBarList);
    }

    static bool TryGetManaBar(out List<StatBarUI> statBarList) {
        return TryGetWindow<StatBarUI>(WindowType.ManaBar, out statBarList);
    }

    static bool TryGetSpiritBar(out List<StatBarUI> statBarList) {
        return TryGetWindow<StatBarUI>(WindowType.SpiritBar, out statBarList);
    }

    static bool TryGetExperienceBar(out List<StatBarUI> statBarList) {
        return TryGetWindow<StatBarUI>(WindowType.ExperienceBar, out statBarList);
    }

    static bool TryGetCharacterWindow(out List<CharacterWindowUI> characterWindowList) {
        return TryGetWindow<CharacterWindowUI>(WindowType.CharacterWindow, out characterWindowList);
    }

    static bool TryGetWindow<T>(WindowType windowType, out List<T> listOfType) {
        listOfType = null;
        if(instance.mappedWindowTypes.TryGetValue(windowType, out List<WindowUI> list)) {
            listOfType = list.Cast<T>().ToList();
            return true;
        }
        return false;
    }

    static bool TryGetFirstWindow(WindowType windowType, out GameObject firstWindow) {
        firstWindow = null;
        int maxSiblingIndex = -1;
        if(instance.mappedWindowTypes.TryGetValue(windowType, out List<WindowUI> list)) {
            foreach(WindowUI window in list) {
                int currentSiblingIndex = window.transform.GetSiblingIndex();
                if(currentSiblingIndex > maxSiblingIndex) {
                    maxSiblingIndex = currentSiblingIndex;
                    firstWindow = window.gameObject;
                }
            }
        }
        return firstWindow != null;
    }
}
