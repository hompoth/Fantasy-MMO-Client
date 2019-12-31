using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

public class ChatWindowUI : WindowUI
{
    public GameObject m_indexGameObject;
    public AsperetaTextObject m_inputLine;
    public AsperetaTextObject[] m_textLines;
    
    const int INPUT_MAX_CAPACITY = 140, TEXT_VISIBLE_LENGTH = 58;
    const int MESSAGE_BOX_SIZE = 100, PREVIOUS_INPUT_TEXT_SIZE = 100;

    int m_inputIndex, m_firstCharIndex, m_messageBoxIndex, m_messageBoxOffset, m_previousInputIndex, m_previousInputOffset, m_previousInputTotal;
    string m_currentInputText = "";
    string[] m_previousInputText;
    string[] m_messageBoxText;
    Color[] m_messageBoxColor;

    void Awake() {
        m_previousInputText = new string[PREVIOUS_INPUT_TEXT_SIZE];
        m_messageBoxText = new string[MESSAGE_BOX_SIZE];
        m_messageBoxColor = new Color[MESSAGE_BOX_SIZE];
    }

    public override void Copy(WindowUI window) {
        if(window is ChatWindowUI) {
            ChatWindowUI chatWindow = window as ChatWindowUI;
            m_inputIndex = chatWindow.GetInputIndex();
            m_firstCharIndex = chatWindow.GetFirstCharIndex();
            m_messageBoxIndex = chatWindow.GetMessageBoxIndex();
            m_messageBoxOffset = chatWindow.GetMessageBoxOffset();
            m_previousInputIndex = chatWindow.GetPreviousInputIndex();
            m_previousInputOffset = chatWindow.GetPreviousInputOffset();
            m_previousInputTotal = chatWindow.GetPreviousInputTotal();
            m_currentInputText = chatWindow.GetCurrentInputText();
            chatWindow.GetPreviousInputText().CopyTo(m_previousInputText, 0);
            chatWindow.GetMessageBoxText().CopyTo(m_messageBoxText, 0);
            chatWindow.GetMessageBoxColor().CopyTo(m_messageBoxColor, 0);
            UpdateChatBoxDisplay();
        }
    }

    public int GetInputIndex() {
        return m_inputIndex;
    }

    public int GetFirstCharIndex() {
        return m_firstCharIndex;
    }

    public int GetMessageBoxIndex() {
        return m_messageBoxIndex;
    }

    public int GetMessageBoxOffset() {
        return m_messageBoxOffset;
    }

    public int GetPreviousInputIndex() {
        return m_previousInputIndex;
    }

    public int GetPreviousInputOffset() {
        return m_previousInputOffset;
    }

    public int GetPreviousInputTotal() {
        return m_previousInputTotal;
    }

    public string GetCurrentInputText() {
        return m_currentInputText;
    }

    public string[] GetPreviousInputText() {
        return m_previousInputText;
    }

    public string[] GetMessageBoxText() {
        return m_messageBoxText;
    }

    public Color[] GetMessageBoxColor() {
        return m_messageBoxColor;
    }

    public void EnableInputText() {
        SetInputText("");
        m_indexGameObject.SetActive(true);
        UpdateIndexPosition();
        m_previousInputOffset = 0;
    }

    public void DisableInputText() {
        SetInputText("");
        m_indexGameObject.SetActive(false);
        UpdateIndexPosition();
        m_previousInputOffset = 0;
    }

    public void PageUp() {
        int offset = Mathf.Max(m_messageBoxOffset - 6, -MESSAGE_BOX_SIZE + m_textLines.Length);
        UpdateChatMessageOffset(offset);
    }

    public void PageDown() {
        int offset = Mathf.Min(m_messageBoxOffset + 6, 0);
        UpdateChatMessageOffset(offset);
    }

    private void UpdateChatMessageOffset(int offset) {
        m_messageBoxOffset = offset;
        UpdateChatBoxDisplay();
    }

    public void MoveInputIndexLeft() {
        UpdateInputIndex(-1);
        UpdateIndexPosition();
        UpdateInputText();
    }
    
    public void MoveInputIndexRight() {
        UpdateInputIndex(1);
        UpdateIndexPosition();
        UpdateInputText();
    }
    
    public void GetPreviousInput() {
        m_previousInputOffset = Mathf.Min(m_previousInputOffset + 1, m_previousInputTotal);
        GetInputFromCache();
    }
    
    public void GetNextInput() {
        m_previousInputOffset = Mathf.Max(m_previousInputOffset - 1, 0);
        GetInputFromCache();
    }

    private void GetInputFromCache() {
        int inputIndex = Mod(m_previousInputIndex - m_previousInputOffset, PREVIOUS_INPUT_TEXT_SIZE);
        m_currentInputText = m_previousInputText[inputIndex] ?? "";
        UpdateInputIndex(m_currentInputText.Length);
        UpdateIndexPosition();
        UpdateInputText();
    }

    public void AddInputText(string inputString) {
        if(inputString.Length < 1) {
            return;
        }
        StringBuilder sb = new StringBuilder(m_currentInputText, INPUT_MAX_CAPACITY);
        sb.Insert(m_inputIndex, inputString);
        SetInputText(sb.ToString());
        UpdateIndexPosition();
    }

    void SetInputText(string text) {
        string processedText = ProcessInputText(text);
        int indexDiff = processedText.Length - m_currentInputText.Length;
        m_currentInputText = processedText;
        UpdateInputIndex(indexDiff);
        UpdateInputText();
    }

    public string GetChatInputToHandle() {
        string inputText = m_currentInputText;
        if(!string.IsNullOrEmpty(inputText)) {
            AppendInputToCache(inputText);
        }
        DisableInputText();
        return inputText;
    }

    void AppendInputToCache(string inputText) {
        m_previousInputText[m_previousInputIndex] = inputText;
        m_previousInputIndex = Mod(m_previousInputIndex + 1, PREVIOUS_INPUT_TEXT_SIZE);
        m_previousInputTotal = Mathf.Min(m_previousInputTotal + 1, PREVIOUS_INPUT_TEXT_SIZE - 1);
    }

    public void AddChatMessage(string message, Color color) {
        if(message == null) {
            message = "";
        }
        if(color == null) {
            color = AsperetaTextColor.white;
        }

        string[] tokens = message.Split(' ');
        string textLine = "";
        bool tabOver = false;
        int textLineLength = TEXT_VISIBLE_LENGTH;
        for(int i = 0; i < tokens.Length; ++i) {
            if(tokens[i].Length > textLineLength) {
                for(int j = 0; j < tokens[i].Length; j+=textLineLength) {
                    if(!string.IsNullOrEmpty(textLine)) {
                        AddChatBoxLine(textLine, color, tabOver);
                        textLineLength = TEXT_VISIBLE_LENGTH - 3;
                        tabOver = true;
                    }
                    textLine = Truncate(tokens[i], j, textLineLength);
                }
            }
            else {
                if(tokens[i].Length + textLine.Length > textLineLength) {
                    if(!string.IsNullOrEmpty(textLine)) {
                        AddChatBoxLine(textLine, color, tabOver);
                        textLineLength = TEXT_VISIBLE_LENGTH - 3;
                        tabOver = true;
                    }
                    textLine = tokens[i];
                }
                else {
                    textLine = (!string.IsNullOrEmpty(textLine) ? textLine + " " + tokens[i] : tokens[i]);
                }
            }
        }
        AddChatBoxLine(textLine, color, tabOver);
        UpdateChatBoxDisplay();
    }

    void AddChatBoxLine(string text, Color color, bool tabOver = false) {
        if(!string.IsNullOrEmpty(text)) {
            m_messageBoxIndex = Mod(m_messageBoxIndex + 1, MESSAGE_BOX_SIZE);
            m_messageBoxText[m_messageBoxIndex] = (tabOver ? "   " + text : text);
            m_messageBoxColor[m_messageBoxIndex] = color;
        }
    }

    void UpdateChatBoxDisplay() {
        for(int i = 0; i < m_textLines.Length; ++i) {
            int messageIndex = Mod(m_messageBoxIndex + m_messageBoxOffset - m_textLines.Length + i + 1, MESSAGE_BOX_SIZE);
            if(m_messageBoxText[messageIndex] != null && m_messageBoxColor[messageIndex] != null) {
                m_textLines[i].SetText(m_messageBoxText[messageIndex]);
                m_textLines[i].SetTextColor(m_messageBoxColor[messageIndex]);
            }
            else {
                m_textLines[i].SetText("");
            }
        }
    }

    int Mod(int val, int denom) {
        return (val % denom + denom) % denom;
    }

    void UpdateIndexPosition() {
        Transform indexTransform = m_indexGameObject.transform;
        Vector3 position = indexTransform.localPosition;
        position.x = (m_inputIndex - m_firstCharIndex) * 6f / 32f;
        indexTransform.localPosition = position;
    }

    void UpdateInputText() {
        m_inputLine.SetText(Truncate(m_currentInputText, m_firstCharIndex, TEXT_VISIBLE_LENGTH));
    }

    string ProcessInputText(string text) {
        string filteredText = "";
        char[] ignoreChars = {'\n', '\r'};
        foreach (char c in text)
        {
            if(c.Equals('\b')) {
                if(filteredText.Length > 0) {
                    filteredText = filteredText.Remove(filteredText.Length - 1);
                }
            }
            else if(!ignoreChars.Contains(c)) {
                filteredText+=c;
            }
        }
        return Truncate(filteredText, 0, INPUT_MAX_CAPACITY);
    }

    void UpdateInputIndex(int indexDiff) {
        m_inputIndex = Mathf.Max(Mathf.Min(m_inputIndex + indexDiff, m_currentInputText.Length), 0);
        if(indexDiff > 0) {
            if(m_inputIndex - TEXT_VISIBLE_LENGTH > m_firstCharIndex) {
                m_firstCharIndex = m_inputIndex - TEXT_VISIBLE_LENGTH;
            }
        }
        else if(m_inputIndex < m_firstCharIndex) {
            m_firstCharIndex = m_inputIndex;
        }
        else {
            m_firstCharIndex = Mathf.Max(Mathf.Min(m_firstCharIndex, m_currentInputText.Length - TEXT_VISIBLE_LENGTH), 0);
        }
    }

    string Truncate(string text, int index, int length) {
        return text.Substring(index, text.Length - index > length ? length : text.Length - index);
    }
}
