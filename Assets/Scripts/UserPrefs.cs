using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UserPrefs
{
    public static string playerName;
    public static int GetCommandBarReferenceIndex(int index) {
        return PlayerPrefs.GetInt("CommandBar-ReferenceIndex-" + playerName + "-" + index, 0);
    }

    public static WindowType GetCommandBarReferenceWindowType(int index) {
            int referenceWindowTypeIndex = PlayerPrefs.GetInt("CommandBar-ReferenceWindowType-" + playerName + "-" + index, 0);
            return EnumHelper.GetName<WindowType>(referenceWindowTypeIndex);
    }

    public static void SetCommandBarReferenceIndex(int index, int referenceIndex) {
        PlayerPrefs.SetInt("CommandBar-ReferenceIndex-" + playerName + "-" + index, referenceIndex);
    }

    public static void SetCommandBarReferenceWindowType(int index, WindowType referenceWindowType) {
            int referenceWindowTypeIndex = EnumHelper.GetIndex<WindowType>(referenceWindowType);
            PlayerPrefs.SetInt("CommandBar-ReferenceWindowType-" + playerName + "-" + index, referenceWindowTypeIndex);
    }

    public static void ClearCommandBarIndex(int index) {
        PlayerPrefs.DeleteKey("CommandBar-ReferenceIndex-" + playerName + "-" + index);
        PlayerPrefs.DeleteKey("CommandBar-ReferenceWindowType-" + playerName + "-" + index);
    }
}
