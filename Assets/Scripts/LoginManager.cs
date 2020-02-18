using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
 
public class LoginManager : MonoBehaviour
{
    public List<Selectable> selectables = new List<Selectable>();
    public GameObject popUpMessage;
    public GameObject[] interactableObjects;
    public AsperetaTextObject nameTextObject;
    public TMP_InputField ipField, portField, usernameField, passwordField;
    private IEnumerator pendingCoroutine;
    public GameManager m_gameManager;
 
    private void Start() { 
        popUpMessage.SetActive(false);
        usernameField.text = UserPrefs.GetUsername();
        ipField.text = UserPrefs.GetServerIp();
        portField.text = UserPrefs.GetServerPort().ToString();
        CreateGameManager();
    }

    private void Update()
    {
        if(!popUpMessage.activeSelf) {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                // Navigate backward when holding shift, else navigate forward.
                this.HandleHotkeySelect(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift), true);
            }
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
            {
                EventSystem.current.SetSelectedGameObject(null, null);
            }
        }
        if (Input.GetKeyDown(KeyCode.Return)) {
            if(popUpMessage.activeSelf) {
                ClosePopup();
            }
            else {
                LoginToServer();
            }
        }
    }
 
    public void ClosePopup() {
        InteractableObjectsEnabled(true);
        popUpMessage.SetActive(false);
        if(pendingCoroutine != null) {
            StopCoroutine(pendingCoroutine);
        }
    }

    bool TryGetIpPort(out string ip, out int port) {
        ip = ipField.text;
        port = 0;
        if(Int32.TryParse(portField.text, out port)) {
            if(port > 0 && port < 65536) {
                return true;
            }
        }
        return false;
    }

    public void CreateGameManager() {
        //m_gameManager = new GameManager();
        ClientManager.AddGameManager(m_gameManager);
    }

    public void LoginToServer() {
        if (!popUpMessage.activeSelf) {
            string username = usernameField.text, password = passwordField.text;
            if(!TryGetIpPort(out string ip, out int port)) {
                ConnectionIssue("Invalid IP or Port.");
                return;
            }
            UserPrefs.SetUsername(username);
            UserPrefs.SetServerIp(ip);
            UserPrefs.SetServerPort(port);
            UserPrefs.Save();
            m_gameManager.ConnectToServer(ip, port, username, password);
            InteractableObjectsEnabled(false);
            popUpMessage.SetActive(true);
            if(pendingCoroutine != null) {
                StopCoroutine(pendingCoroutine);
            }
            pendingCoroutine = UpdatePendingMessage();
            StartCoroutine(pendingCoroutine);
        }
    }

    public void ConnectionIssue(string message) {
        if(popUpMessage.activeSelf) {
            if(pendingCoroutine != null) {
                StopCoroutine(pendingCoroutine);
                pendingCoroutine = null;
                // If active already and pending, update the message. 
                // We don't want a "Disconnected" message after a failed login.
                nameTextObject.SetText(message);
            }
        }
        else {
            InteractableObjectsEnabled(false);
            popUpMessage.SetActive(true);
            nameTextObject.SetText(message);
        }
    }

    public void QuitGame() {
        ClientManager.QuitGame();
    }

    IEnumerator UpdatePendingMessage() {
        string msg = "Connecting To Game Server", ellipsis = "";
        while(true) {
            if (ellipsis.Length > 2) {
                ellipsis = "";
            }
            else {
                ellipsis = ellipsis + ".";
            }
            nameTextObject.SetText(msg + ellipsis);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void InteractableObjectsEnabled(bool enabled) {
        foreach (GameObject go in interactableObjects) {
                TMP_InputField inputField = go.GetComponent<TMP_InputField>();
                Button button = go.GetComponent<Button>();
                if(inputField != null) {
                    inputField.interactable = enabled;
                }
                else if (button != null) {
                    button.interactable = enabled;
                }
            }
    }

    private void HandleHotkeySelect(bool isNavigateBackward, bool isWrapAround)
    {
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        if (selectedObject != null && selectedObject.activeInHierarchy) // Ensure a selection exists and is not an inactive object.
        {
            Selectable currentSelection = selectedObject.GetComponent<Selectable>();
            if (currentSelection != null)
            {
                Selectable nextSelection = this.FindNextSelectable(
                    selectables.IndexOf(currentSelection), isNavigateBackward, isWrapAround);
                if (nextSelection != null)
                {
                    nextSelection.Select();
                }
            }
            else
            {
                this.SelectFirstSelectable();
            }
        }
        else
        {
            this.SelectFirstSelectable();
        }
    }
 
    private void SelectFirstSelectable()
    {
        if (selectables != null && selectables.Count > 0)
        {
            Selectable firstSelectable = selectables[0];
            firstSelectable.Select();
        }
    }
 
    /// <summary>
    /// Looks at ordered selectable list to find the selectable we are trying to navigate to and returns it.
    /// </summary>
    private Selectable FindNextSelectable(int currentSelectableIndex, bool isNavigateBackward, bool isWrapAround)
    {
        Selectable nextSelection = null;
 
        int totalSelectables = selectables.Count;
        if (totalSelectables > 1)
        {
            if (isNavigateBackward)
            {
                if (currentSelectableIndex == 0)
                {
                    nextSelection = (isWrapAround) ? selectables[totalSelectables - 1] : null;
                }
                else
                {
                    nextSelection = selectables[currentSelectableIndex - 1];
                }
            }
            else // Navigate forward.
            {
                if (currentSelectableIndex == (totalSelectables - 1))
                {
                    nextSelection = (isWrapAround) ? selectables[0] : null;
                }
                else
                {
                    nextSelection = selectables[currentSelectableIndex + 1];
                }
            }
        }
 
        return nextSelection;
    }
 
}