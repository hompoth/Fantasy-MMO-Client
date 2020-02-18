using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UserPrefs
{
    public static string playerName;

    public static void DeleteAll() {
        PlayerPrefs.DeleteAll();
    }

    public static void Save() {
        PlayerPrefs.Save();
    }

    public static bool HasCommandBarIndex(int index) {
        return PlayerPrefs.HasKey("CommandBar-ReferenceIndex-" + playerName + "-" + index) || PlayerPrefs.HasKey("CommandBar-ReferenceWindowType-" + playerName + "-" + index);
    }

    public static int GetCommandBarReferenceIndex(int index) {
        return PlayerPrefs.GetInt("CommandBar-ReferenceIndex-" + playerName + "-" + index, 0);
    }

    public static WindowType GetCommandBarReferenceWindowType(int index) {
            return GetUserPrefEnumValue<WindowType>("CommandBar-ReferenceWindowType-" + playerName + "-" + index);
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

    public static NameFormat GetNextNameFormat(NameFormat nameFormat) {
        return GetNextUserPrefEnumValue<NameFormat>("PlayerState-NameFormat", nameFormat);
    }

    public static HealthManaFormat GetNextHealthManaFormat(HealthManaFormat healthManaFormat) {
        return GetNextUserPrefEnumValue<HealthManaFormat>("PlayerState-HealthManaFormat", healthManaFormat);
    }

    public static NameFormat GetNameFormat() {
        return GetUserPrefEnumValue<NameFormat>("PlayerState-NameFormat", NameFormat.Visible);
    }

    public static HealthManaFormat GetHealthManaFormat() {
        return GetUserPrefEnumValue<HealthManaFormat>("PlayerState-HealthManaFormat", HealthManaFormat.VisibleOnUpdate);
    }

    public static string GetUsername() {
        return PlayerPrefs.GetString("LoginManager-Username");
    }

    public static string GetServerIp() {
        return PlayerPrefs.GetString("LoginManager-Ip");
    }

    public static int GetServerPort() {
        return PlayerPrefs.GetInt("LoginManager-Port");
    }

    public static void SetUsername(string username) {
        PlayerPrefs.SetString("LoginManager-Username", username);
    }

    public static void SetServerIp(string ip) {
        PlayerPrefs.SetString("LoginManager-Ip", ip);
    }

    public static void SetServerPort(int port) {
        PlayerPrefs.SetInt("LoginManager-Port", port);
    }

    static T GetNextUserPrefEnumValue<T>(string key, T enumValue) where T : Enum {
        enumValue = EnumHelper.GetNextName<T>(enumValue);
        PlayerPrefs.SetInt(key, EnumHelper.GetIndex<T>(enumValue));
        return enumValue;
    }

    static T GetUserPrefEnumValue<T>(string key, T defaultValue = default(T)) where T : Enum {
        int enumIndex = PlayerPrefs.GetInt(key, EnumHelper.GetIndex<T>(defaultValue));
        return EnumHelper.GetName<T>(enumIndex);
    }
}
