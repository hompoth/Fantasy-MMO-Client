using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerState : MonoBehaviour
{
	public CursorUI cursor;
    public Camera mainCamera;
    public GameManager m_gameManager;

    public Dictionary<int,WindowUI> mappedWindowIds;
    public Dictionary<WindowType,List<WindowUI>> mappedWindowTypes;

    const float MOVEMENT_WAIT_TIME = 0.15f, NEW_WINDOW_OFFSET = 0.15625f;
    const string TELL_FROM = "[tell from] ";
    const string COLON = ":";
    const string FILTER_MESSAGE = "/filter", VITABAR_MESSAGE = "/vitabar";
    const string NAMES_MESSAGE = "/names", QUIT_MESSAGE = "/quit", SLASH = "/";
    const string TOGGLESOUND_MESSAGE = "/togglesound";
    PlayerManager m_playerManager;
    int m_playerId, m_spellTargetIndex, m_spellTargetPlayerId;
    bool m_chatEnabled, m_spellTargetEnabled;
    float movementTimer;
    string m_messageFromPlayerName;

    void Awake()
    {
        mappedWindowIds = new Dictionary<int,WindowUI>();
        mappedWindowTypes = new Dictionary<WindowType,List<WindowUI>>();
    }

    void LateUpdate() {
        if(m_playerManager != null) {
            Vector3 position = m_playerManager.transform.localPosition;
            transform.localPosition = position;
        }
    }

    public GameObject GetGameObject() {
        return gameObject;
    }

    public void AddWindowToHierarchy(WindowUI window) {
        GameObject windowObject = window.gameObject;
        windowObject.transform.SetParent(transform);
        windowObject.transform.SetAsLastSibling();
        windowObject.transform.localPosition = GetWindowPosition(window);
        UpdateSorting();
    }

    Vector3 GetWindowPosition(WindowUI window) {
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

    public bool TryGetWindowUI(int windowId, out WindowUI window) {
        return mappedWindowIds.TryGetValue(windowId, out window);
    }

    public void ShowWindow(int windowId) {
        if(TryGetWindowUI(windowId, out WindowUI window)) {
            window.gameObject.SetActive(true);
        }
    }

    public int GetWindowTypeCount(WindowType windowType) {
        if(!mappedWindowTypes.TryGetValue(windowType, out List<WindowUI> list)) {
            return 0;
        }
        return list.Count();
    }

    public void LoadWindow(int windowId, WindowType windowType, WindowUI window) {
        mappedWindowIds.Add(windowId, window);
        if(!mappedWindowTypes.TryGetValue(windowType, out List<WindowUI> list)) {
            list = new List<WindowUI>();
            mappedWindowTypes.Add(windowType, list);
        }
        else {
            WindowUI originalWindow = list.FirstOrDefault();
            if(originalWindow != null) {
                window.Copy(originalWindow);
            }
        }
        mappedWindowTypes[windowType].Add(window);
    }

    public void RemoveWindow(int windowId, WindowType windowType, WindowUI window) {
        mappedWindowIds.Remove(windowId);
        if(mappedWindowTypes.TryGetValue(windowType, out List<WindowUI> list)) {
            list.Remove(window);
            if(list.Count == 0) {
                mappedWindowTypes.Remove(windowType);
            }
        }
    }

    public void LeftMouseUp(bool controlKeyPressed) {
        cursor.LeftMouseUp(controlKeyPressed);
    }

    public void LeftMouseDown() {
        cursor.LeftMouseDown();
    }

    public void LeftDoubleClick() {
        cursor.LeftDoubleClick();
    }

    public void RightMouseUp() {
        cursor.RightMouseUp();
    }

    public void RightMouseDown(bool controlKeyPressed) {
        cursor.RightMouseDown(controlKeyPressed);
    }

    public void EnableCamera() {
        SetGameUIActive(true);
    }

    public void DisableCamera() {
        SetGameUIActive(false);
    }

    private void SetGameUIActive(bool isActive) {
        gameObject.SetActive(isActive);
        mainCamera.gameObject.SetActive(isActive);
        foreach (Transform child in gameObject.transform) {
            SpriteRenderer spriteRenderer = child.gameObject.GetComponent<SpriteRenderer>();
            if(spriteRenderer != null) {
                spriteRenderer.enabled = isActive;
            }
        }
    }

    public void Destroy() {
        Destroy(gameObject);
    }

    public void RefreshCommandBar() {
        if(TryGetCommandBar(out List<CommandBarUI> commandBarList)) {
            foreach(CommandBarUI commandBar in commandBarList) {
                commandBar.LoadDefaults(this);
            }
        }
    }

    public SlotUI GetWindowSlot(WindowType windowType, int index) {
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
    
    void CastTargetSpell(int index) {
        m_spellTargetEnabled = true;
        m_spellTargetIndex = index;
        GameObject targetedPlayer = m_gameManager.GetPlayer(m_spellTargetPlayerId);
        if(!WithinSpellRange(targetedPlayer)) {
            m_spellTargetPlayerId = m_playerId;
        }
        m_gameManager.TargetSpellCast(m_spellTargetPlayerId);
    }

    bool WithinSpellRange(GameObject player) {
        if(player != null) {
            GameObject mainPlayer = m_playerManager.gameObject;
            if(m_gameManager.WithinSpellRange(mainPlayer, player)) {
                return true;
            }
        }
        return false;
    }

    public void UseSpellSlot(int index) {
        if(!m_chatEnabled && !m_spellTargetEnabled) {
            if(TryGetSpellsWindow(out List<SpellsWindowUI> spellWindowList)) {
                SlotUI slot = spellWindowList.FirstOrDefault()?.GetSlot(index);
                if(slot != null) {
                    string spellTarget = slot.GetSpellTarget();
                    if(spellTarget.Equals("T")) {
                        CastTargetSpell(index);
                    }
                    else {
                        m_gameManager.SendCast(index, m_playerId);
                    }
                }
            }
        }
    }

    public void UseCommandSlot(int index) {
        if(!m_chatEnabled && !m_spellTargetEnabled) {
            if(TryGetCommandBar(out List<CommandBarUI> commandBarList)) {
                SlotUI slot = commandBarList.FirstOrDefault()?.GetSlot(index);
                if(slot != null && slot.GetReferenceWindow() != null) {    
                    WindowType referenceWindowType = slot.GetReferenceWindow().GetWindowType();
                    int referenceIndex = slot.GetReferenceIndex();
                    
                    if(referenceWindowType.Equals(WindowType.InventoryWindow) || referenceWindowType.Equals(WindowType.CharacterWindow)) {
                        m_gameManager.SendUse(referenceIndex);
                    }
                    else if(referenceWindowType.Equals(WindowType.SpellsWindow)) {
                        string spellTarget = slot.GetSpellTarget();
                        if(spellTarget.Equals("T")) {
                            CastTargetSpell(referenceIndex);
                        }
                        else {
                            m_gameManager.SendCast(referenceIndex, m_playerId);
                        }
                    }
                }
            }
        }
    }

    public void AddCommandSlot(int slotIndex, SlotUI slot) {
        if(TryGetCommandBar(out List<CommandBarUI> commandBarList)) {
            foreach(CommandBarUI commandBar in commandBarList) {
                commandBar.CopySlot(slotIndex, slot);
            }
            UserPrefs.Save();
        }
    }

    public void SwapCommandSlot(int slotIndex, int newSlotIndex) {
        if(TryGetCommandBar(out List<CommandBarUI> commandBarList)) {
            foreach(CommandBarUI commandBar in commandBarList) {
                commandBar.SwapSlot(slotIndex, newSlotIndex);
            }
            UserPrefs.Save();
        }
    }

    public void ClearCommandSlot(int slotIndex) {
        if(TryGetCommandBar(out List<CommandBarUI> commandBarList)) {
            foreach(CommandBarUI commandBar in commandBarList) {
                commandBar.ClearSlot(slotIndex);
            }
        }
    }

    public void LeftArrow() {
        if(m_spellTargetEnabled) {
            if(m_gameManager.TryGetTargetPlayerUp(m_spellTargetPlayerId, out int playerId)) {
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

    public void RightArrow() {
        if(m_spellTargetEnabled) {
            if(m_gameManager.TryGetTargetPlayerDown(m_spellTargetPlayerId, out int playerId)) {
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

    public void UpArrow() {
        if(m_spellTargetEnabled) {
            if(m_gameManager.TryGetTargetPlayerUp(m_spellTargetPlayerId, out int playerId)) {
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

    public void DownArrow() {
        if(m_spellTargetEnabled) {
            if(m_gameManager.TryGetTargetPlayerDown(m_spellTargetPlayerId, out int playerId)) {
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

    void SwapSpellTarget(int playerId) {
        if(playerId != m_spellTargetPlayerId) {
            m_gameManager.CancelTargetSpellCast(m_spellTargetPlayerId);
            m_spellTargetPlayerId = playerId;
            m_gameManager.TargetSpellCast(m_spellTargetPlayerId);
        }
    }

    public void PageUp() {
        if(TryGetChatWindow(out List<ChatWindowUI> characterWindowList)) {
            foreach(ChatWindowUI chatWindow in characterWindowList) {
                chatWindow.PageUp();
            }
        }
    }

    public void PageDown() {
        if(TryGetChatWindow(out List<ChatWindowUI> characterWindowList)) {
            foreach(ChatWindowUI chatWindow in characterWindowList) {
                chatWindow.PageDown();
            }
        }
    }

    public void UseEmote(Emote emote) {
        if(!m_chatEnabled && !m_spellTargetEnabled) {
            int value = EnumHelper.GetNameValue<Emote>(emote);
            m_gameManager.SendEmote(value);
        }
    }

    public void PickUp() {
        if(!m_chatEnabled && !m_spellTargetEnabled) {
            m_gameManager.SendPickUp();
        }
    }

    public void ToggleSpellsWindow() {
        if(!m_chatEnabled && !m_spellTargetEnabled && TryGetFirstWindow(WindowType.SpellsWindow, out GameObject spellsWindowObject)) {
            ToggleActive(spellsWindowObject);
        }
    }

    public void ToggleInventory() {
        if(!m_chatEnabled && !m_spellTargetEnabled && TryGetFirstWindow(WindowType.InventoryWindow, out GameObject inventoryWindowObject)) {
            ToggleActive(inventoryWindowObject);
        }
    }

    public void ToggleCharacterWindow() {
        if(!m_chatEnabled && !m_spellTargetEnabled && TryGetFirstWindow(WindowType.CharacterWindow, out GameObject characterWindowObject)) {
            ToggleActive(characterWindowObject);
        }
    }

    public void MinimizeScreen() {
        m_gameManager.MinimizeScreen();
    }

    public void ToggleCommandBar() {
        if(TryGetFirstWindow(WindowType.CommandBar, out GameObject commandBarObject)) {
            ToggleActive(commandBarObject);
        }
    }

    public void ToggleBuffBar() {
        if(TryGetFirstWindow(WindowType.BuffBar, out GameObject buffBarObject)) {
            ToggleActive(buffBarObject);
        }
    }

    public void ToggleChatWindow() {
        if(TryGetFirstWindow(WindowType.ChatWindow, out GameObject chatWindowObject)) {
            ToggleActive(chatWindowObject);
            //if(!instance.chatWindow.gameObject.activeSelf) {
            //    instance.chatWindow.DisableInputText();
            //    m_chatEnabled = false;
            //}
            // TODO Decide if this should be re-added
        }
    }

    public void ToggleFPSBar() {
        if(TryGetFirstWindow(WindowType.FpsBar, out GameObject fpsBarObject)) {
            ToggleActive(fpsBarObject);
        }
    }

    public void ToggleHealthBar() {
        if(TryGetFirstWindow(WindowType.HealthBar, out GameObject healthBarObject)) {
            ToggleActive(healthBarObject);
        }
    }

    public void ToggleManaBar() {
        if(TryGetFirstWindow(WindowType.ManaBar, out GameObject manaBarObject)) {
            ToggleActive(manaBarObject);
        }
    }

    public void ToggleSpiritBar() {
        if(TryGetFirstWindow(WindowType.SpiritBar, out GameObject spiritBarObject)) {
            ToggleActive(spiritBarObject);
        }
    }

    public void ToggleExperienceBar() {
        if(TryGetFirstWindow(WindowType.ExperienceBar, out GameObject experienceBarObject)) {
            ToggleActive(experienceBarObject);
        }
    }

    public void TogglePartyWindow(bool toggleWhenChatDisabled) {
        if(!toggleWhenChatDisabled || toggleWhenChatDisabled && !m_chatEnabled && !m_spellTargetEnabled) {
            if(TryGetFirstWindow(WindowType.PartyWindow, out GameObject partyWindowObject)) {
                ToggleActive(partyWindowObject);
            }
        }
    }

    public void ToggleOptionsBar() {
        if(TryGetFirstWindow(WindowType.OptionsBar, out GameObject optionsBarObject)) {
            ToggleActive(optionsBarObject);
        }
    }

    public void ToggleDiscardButton() {
        if(TryGetFirstWindow(WindowType.DiscardButton, out GameObject discardButtonObject)) {
            ToggleActive(discardButtonObject);
        }
    }

    public void HandlePlayerMovement(Vector3 inputVector) {
        if(!m_spellTargetEnabled) {
            if((m_playerManager != null) && !m_chatEnabled && (!inputVector.Equals(Vector3.zero))) {
                if (!m_playerManager.IsFacingDirection(inputVector)) {
                    if (m_playerManager.Face(inputVector, true)) {
                        movementTimer = Time.time;
                        FacingDirection direction = m_playerManager.GetPlayerFacingDirection();
                        m_gameManager.SendFace(direction);
                    }
                }
                else if(Time.time - movementTimer > MOVEMENT_WAIT_TIME) {
                    if(m_playerManager.Move(true)){
                        MovingDirection direction = m_playerManager.GetPlayerMovingDirection();
                        m_gameManager.SendMove(direction);
                    }
                }
            }
        }
    }

    public void Attack() {
        if(!m_chatEnabled && !m_spellTargetEnabled) {
            if(m_playerManager != null && m_playerManager.Attack(true)){
                m_gameManager.SendAttack();
            }
        }
    }

    public void EnableChat() {
        if(!m_chatEnabled && !m_spellTargetEnabled) {
            if(TryGetChatWindow(out List<ChatWindowUI> characterWindowList)) {
                foreach(ChatWindowUI chatWindow in characterWindowList) {
                    chatWindow.EnableInputText();
                }
            }
            m_chatEnabled = true;
        }
    }

    public void Escape() {
        if(m_spellTargetEnabled) {
            m_gameManager.CancelTargetSpellCast(m_spellTargetPlayerId);
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

    public void Home() {
        if(m_spellTargetEnabled) {
            SwapSpellTarget(m_playerId);
        }
    }

    public void Enter() {
        if(m_spellTargetEnabled) {
            m_gameManager.HandleTargetSpellCast(m_spellTargetIndex, m_spellTargetPlayerId);
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

    void HandleChatInput(string inputText) {
        if(!string.IsNullOrEmpty(inputText)) {
            if(inputText.StartsWith(SLASH)) {
                if(inputText.Equals(QUIT_MESSAGE)) {
                    m_gameManager.Disconnect();
                }
                else if(inputText.Equals(NAMES_MESSAGE)) {
                    ClientManager.SwitchNameFormat();
                }
                else if(inputText.Equals(VITABAR_MESSAGE)) {
                    ClientManager.SwitchHealthManaFormat();
                }
                else if(inputText.StartsWith(FILTER_MESSAGE)) {
                    if(inputText.Length > FILTER_MESSAGE.Length) {
                        inputText = inputText.Substring(FILTER_MESSAGE.Length + 1);
                    }
                    m_gameManager.HandleFilter(inputText);
                }
                else if(inputText.StartsWith(TOGGLESOUND_MESSAGE)) {
                    m_gameManager.ToggleSound();
                }
                else if(inputText.StartsWith("/send")){ // TODO Remove this
                    m_gameManager.SendMessageToServer(inputText.Substring(6));
                }
                else {
                    m_gameManager.SendChatCommand(inputText);
                }
            }
            else {
                m_gameManager.SendChatMessage(inputText);
            }
        }
    }

    public void AddInputText(string input) {
        if(m_chatEnabled && !m_spellTargetEnabled) {
            if(TryGetChatWindow(out List<ChatWindowUI> characterWindowList)) {
                foreach(ChatWindowUI chatWindow in characterWindowList) {
                    chatWindow.AddInputText(input);
                }
            }
        }
    }

    public void AddGuildText() {
        if(!m_chatEnabled && !m_spellTargetEnabled) {
            EnableChat();
            AddInputText("/guild ");
        }
    }

    public void AddTellText() {
        if(!m_chatEnabled && !m_spellTargetEnabled) {
            EnableChat();
            AddInputText("/tell ");
        }
    }

    public void AddReplyText() {
        if(!m_chatEnabled && !m_spellTargetEnabled) {
            EnableChat();
            AddInputText("/tell " + m_messageFromPlayerName + " ");
        }
    }

    void ToggleActive(GameObject gameObject) {
        gameObject.SetActive(!gameObject.activeSelf);
        if(gameObject.activeSelf) {
            MoveWindowToFront(gameObject);
        }
    }

    public void MoveWindowToFront(GameObject gameObject) {
        gameObject.transform.SetAsLastSibling();
        UpdateSorting();
    }

    void UpdateSorting() {
        foreach(Transform windowTransform in transform) {
            SortingGroup group = windowTransform.gameObject.GetComponent<SortingGroup>();
            if(group != null) {
                int index = windowTransform.GetSiblingIndex();
                group.sortingOrder = index;
            }
        }
    }

    public bool IsChatEnabled() {
        return m_chatEnabled;
    }

    public bool IsMainPlayer(int playerId) {
        return playerId.Equals(m_playerId);
    }

    public PlayerManager GetMainPlayerManager() {
        return m_playerManager;
    }

    public void TradeDone() {
        if(TryGetTradeWindow(out List<TradeWindowUI> tradeWindowList)) {
            foreach(TradeWindowUI tradeWindow in tradeWindowList) {
                m_gameManager.RemoveWindow(tradeWindow.GetWindowId());
            }
        }
    }

    public void UpdateTradeEnabled(bool enabledForSelf, bool tradeEnabled) {
        if(TryGetTradeWindow(out List<TradeWindowUI> tradeWindowList)) {
            foreach(TradeWindowUI tradeWindow in tradeWindowList) {
                tradeWindow.SetTradeEnabled(enabledForSelf, tradeEnabled);
            }
        }
    }
    
    public void UpdateTradeGold(bool goldForSelf, int amount) {
        if(TryGetTradeWindow(out List<TradeWindowUI> tradeWindowList)) {
            foreach(TradeWindowUI tradeWindow in tradeWindowList) {
                tradeWindow.SetTradeGold(goldForSelf, amount);
            }
        }
    }

    public void UpdateTradeSlot(bool itemForSelf, int slotIndex, string itemName, int amount, int graphicId, int itemSlotId, Color itemSlotColor) {
        if(TryGetTradeWindow(out List<TradeWindowUI> tradeWindowList)) {
            foreach(TradeWindowUI tradeWindow in tradeWindowList) {
                tradeWindow.SetTradeSlotId(itemForSelf, slotIndex, itemSlotId);
                tradeWindow.SetTradeSlotDescription(itemForSelf, slotIndex, itemName);
                tradeWindow.SetTradeSlotAmount(itemForSelf, slotIndex, amount);
                tradeWindow.UpdateTradeSlotGraphic(itemForSelf, slotIndex, graphicId, itemSlotColor);
            }
        }
    }

    public void UpdateItemSlot(int slotIndex, int itemId, string itemName, int amount, int graphicId, Color itemSlotColor) {
        if(TryGetInventoryWindow(out List<InventoryWindowUI> inventoryWindowList)) {
            foreach(InventoryWindowUI inventoryWindow in inventoryWindowList) {
                inventoryWindow.SetSlotId(slotIndex, itemId);
                inventoryWindow.SetSlotDescription(slotIndex, itemName);
                inventoryWindow.SetSlotAmount(slotIndex, amount);
                inventoryWindow.UpdateSlotGraphic(slotIndex, graphicId, itemSlotColor);
            }
        }
    }

    public void UpdateSpellSlot(int slotIndex, string spellName, int soundId, int spellId, string spellTarget, int graphicId) {
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

	public void UpdateBuffSlot(int slotIndex, string spellName, int spellId) {
        if(TryGetBuffBar(out List<BuffBarUI> buffBarList)) {
            foreach(BuffBarUI buffBar in buffBarList) {
                buffBar.SetSpellName(slotIndex, spellName);
                buffBar.UpdateSpellGraphic(slotIndex, spellId);
            }
        }
	}

	public void UpdateWindowLine(int windowId, int windowLine, string description, int amount, int slotId, int graphicId, Color slotColor) {
        if(TryGetWindowUI(windowId, out WindowUI window)) {
            window.SetSlotId(windowLine, slotId);
            window.SetSlotDescription(windowLine, description);
            window.SetSlotAmount(windowLine, amount);
            window.UpdateSlotGraphic(windowLine, graphicId, slotColor);
        }
	}

    public void SetMainPlayerName(int playerId, string name) {
        if(IsMainPlayer(playerId)) {
            UserPrefs.playerName = name;
            if(TryGetCharacterWindow(out List<CharacterWindowUI> characterWindowList)) {
                foreach(CharacterWindowUI characterWindow in characterWindowList) {
                    characterWindow.SetPlayerName(name);
                }
            }
        }
    }

    public void SetMainPlayer(int playerId, GameObject playerObject) {
        RemoveMainPlayer();
        m_playerId = playerId;
        if (playerObject != null) {
            m_playerManager = playerObject.GetComponent<PlayerManager>();
            m_playerManager.SetIsMainPlayer(true);
            string name = m_playerManager.GetPlayerName();
            SetMainPlayerName(playerId, name);
            if(playerObject.layer != 12) {
                playerObject.layer = 11;
            }
        }
		EnableCamera();
    }

    void RemoveMainPlayer() {
        if(m_playerManager != null) {
            GameObject playerObject = m_playerManager.gameObject;
            if(playerObject != null && playerObject.layer == 11) {
                playerObject.layer = 0;
            }
            m_playerId = 0;
            m_playerManager = null;
        }
    }

    public void SetMainPlayerPosition(int x, int y) {
        m_playerManager?.SetPlayerPosition(x, y);
    }

    public void SetMainPlayerAttackSpeed(int weaponSpeed) {
        m_playerManager?.SetPlayerAttackSpeed(weaponSpeed);
    }

    public void SetMainPlayerStatInfo(string guildName, string unknown, string className, int level, int maxHp, int maxMp, int maxSp, int curHp, int curMp, int curSp, 
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

	public void SetMainPlayerHPMPSP(int hpMax, int mpMax, int spMax, int hp, int mp, int sp) {
		SetMainPlayerHealth(hp, hpMax);
        SetMainPlayerMana(mp, mpMax);
        SetMainPlayerSpirit(sp, spMax);
	}

    void SetMainPlayerHealth(int curHp, int maxHp) {
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

    void SetMainPlayerMana(int curMp, int maxMp) {
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

    void SetMainPlayerSpirit(int curSp, int maxSp) {
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

    void UpdateStatBar(StatBarUI statBar, int curStat, int maxStat) {
        int percent = 0;
        if(maxStat > 0) {
            percent = (int) Mathf.Ceil(100f * curStat / maxStat);
        }
        statBar.SetStatAmount(curStat);
        statBar.SetStatPercent(percent);
    }

    public void SetMainPlayerExperience(int percent, int experience, int experienceTillNextLevel) {
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
    
    public void UpdatePartyIndex(int index, int playerId, string name, int level, string className) {
        if(TryGetPartyWindow(out List<PartyWindowUI> partyWindowList)) {
            foreach(PartyWindowUI partyWindow in partyWindowList) {
                partyWindow.UpdatePartyIndex(index, playerId, name, level, className);
            }
        }
	}

    public void UpdatePartyPlayerHP(int playerId, int hpPercent) {
        if(TryGetPartyWindow(out List<PartyWindowUI> partyWindowList)) {
            foreach(PartyWindowUI partyWindow in partyWindowList) {
                partyWindow.SetHPBar(playerId, hpPercent);
            }
        }
    }

    public void UpdatePartyPlayerMP(int playerId, int mpPercent) {
        if(TryGetPartyWindow(out List<PartyWindowUI> partyWindowList)) {
            foreach(PartyWindowUI partyWindow in partyWindowList) {
                partyWindow.SetMPBar(playerId, mpPercent);
            }
        }
    }

    public bool IsPlayerInParty(int playerId) {
        if(TryGetPartyWindow(out List<PartyWindowUI> partyWindowList)) {
            foreach(PartyWindowUI partyWindow in partyWindowList) {
                if(partyWindow.IsPlayerInParty(playerId)) {
                    return true;
                }
            }
        }
        return false;
    }

    public void AddChatMessage(string message, Color color) {
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

    bool TryGetCommandBar(out List<CommandBarUI> commandBarList) {
        return TryGetWindow<CommandBarUI>(WindowType.CommandBar, out commandBarList);
    }

    bool TryGetBuffBar(out List<BuffBarUI> buffBarList) {
        return TryGetWindow<BuffBarUI>(WindowType.BuffBar, out buffBarList);
    }

    bool TryGetSpellsWindow(out List<SpellsWindowUI> spellsWindowList) {
        return TryGetWindow<SpellsWindowUI>(WindowType.SpellsWindow, out spellsWindowList);
    }

    bool TryGetInventoryWindow(out List<InventoryWindowUI> inventoryWindowList) {
        return TryGetWindow<InventoryWindowUI>(WindowType.InventoryWindow, out inventoryWindowList);
    }

    bool TryGetTradeWindow(out List<TradeWindowUI> tradeWindowList) {
        return TryGetWindow<TradeWindowUI>(WindowType.TradeWindow, out tradeWindowList);
    }

    bool TryGetChatWindow(out List<ChatWindowUI> chatWindowList) {
        return TryGetWindow<ChatWindowUI>(WindowType.ChatWindow, out chatWindowList);
    }

    bool TryGetPartyWindow(out List<PartyWindowUI> partyWindowList) {
        return TryGetWindow<PartyWindowUI>(WindowType.PartyWindow, out partyWindowList);
    }

    bool TryGetHealthBar(out List<StatBarUI> statBarList) {
        return TryGetWindow<StatBarUI>(WindowType.HealthBar, out statBarList);
    }

    bool TryGetManaBar(out List<StatBarUI> statBarList) {
        return TryGetWindow<StatBarUI>(WindowType.ManaBar, out statBarList);
    }

    bool TryGetSpiritBar(out List<StatBarUI> statBarList) {
        return TryGetWindow<StatBarUI>(WindowType.SpiritBar, out statBarList);
    }

    bool TryGetExperienceBar(out List<StatBarUI> statBarList) {
        return TryGetWindow<StatBarUI>(WindowType.ExperienceBar, out statBarList);
    }

    bool TryGetCharacterWindow(out List<CharacterWindowUI> characterWindowList) {
        return TryGetWindow<CharacterWindowUI>(WindowType.CharacterWindow, out characterWindowList);
    }

    bool TryGetWindow<T>(WindowType windowType, out List<T> listOfType) {
        listOfType = null;
        if(mappedWindowTypes != null && mappedWindowTypes.TryGetValue(windowType, out List<WindowUI> list)) {
            listOfType = list.Cast<T>().ToList();
            return true;
        }
        return false;
    }

    bool TryGetFirstWindow(WindowType windowType, out GameObject firstWindow) {
        firstWindow = null;
        int maxSiblingIndex = -1;
        if(mappedWindowTypes != null && mappedWindowTypes.TryGetValue(windowType, out List<WindowUI> list)) {
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
