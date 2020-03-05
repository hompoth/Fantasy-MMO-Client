using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapTile = System.Tuple<int, int, int>;
using WarpDevice = System.Tuple<string, int>;

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

    public static List<Tuple<MapTile, MapTile>> GetWarpZones() {
        List<Tuple<MapTile, MapTile>> warpZoneList = new List<Tuple<MapTile, MapTile>>();
        int total = PlayerPrefs.GetInt("WarpZone-Total");
        for(int i = 0; i < total; ++i) {
            MapTile warpFrom = GetWarpTile("WarpZone", i, false);
            MapTile warpTo = GetWarpTile("WarpZone", i);
            Tuple<MapTile, MapTile> warpZone = Tuple.Create(warpFrom, warpTo);
            warpZoneList.Add(warpZone);
        }
        return warpZoneList;
    }

    public static void AddWarpZone(MapTile warpFrom, MapTile warpTo) {
        int total = PlayerPrefs.GetInt("WarpZone-Total");
        for(int i = 0; i < total; ++i) {
            MapTile mapTile = GetWarpTile("WarpZone", i, false);
            if(mapTile.Equals(warpFrom)) {
                SetWarpTile("WarpZone", i, true, warpTo);
                return;
            }
        }
        total = total + 1;
        PlayerPrefs.SetInt("WarpZone-Total", total);
        SetWarpTile("WarpZone", total, false, warpFrom);
        SetWarpTile("WarpZone", total, true, warpTo);
    }

    public static List<Tuple<WarpDevice, MapTile>> GetWarpSpells() {
        List<Tuple<WarpDevice, MapTile>> warpSpellList = new List<Tuple<WarpDevice, MapTile>>();
        int total = PlayerPrefs.GetInt("WarpSpell-Total");
        for(int i = 0; i < total; ++i) {
            WarpDevice warpFrom = GetWarpDevice("WarpSpell", i);
            MapTile warpTo = GetWarpTile("WarpSpell", i);
            Tuple<WarpDevice, MapTile> warpSpell = Tuple.Create(warpFrom, warpTo);
            warpSpellList.Add(warpSpell);
        }
        return warpSpellList;
    }

    public static void AddWarpSpell(WarpDevice warpFrom, MapTile warpTo) {
        int total = PlayerPrefs.GetInt("WarpSpell-Total");
        for(int i = 0; i < total; ++i) {
            WarpDevice warpDevice = GetWarpDevice("WarpSpell", i);
            if(warpDevice.Equals(warpFrom)) {
                SetWarpTile("WarpSpell", i, true, warpTo);
                return;
            }
        }
        total = total + 1;
        PlayerPrefs.SetInt("WarpSpell-Total", total);
        SetWarpDevice("WarpSpell", total, warpFrom);
        SetWarpTile("WarpSpell", total, true, warpTo);
    }

    public static List<Tuple<WarpDevice, MapTile>> GetWarpItems() {
        List<Tuple<WarpDevice, MapTile>> warpItemList = new List<Tuple<WarpDevice, MapTile>>();
        int total = PlayerPrefs.GetInt("WarpItem-Total");
        for(int i = 0; i < total; ++i) {
            WarpDevice warpFrom = GetWarpDevice("WarpItem", i);
            MapTile warpTo = GetWarpTile("WarpItem", i);
            Tuple<WarpDevice, MapTile> warpItem = Tuple.Create(warpFrom, warpTo);
            warpItemList.Add(warpItem);
        }
        return warpItemList;
    }

    public static void AddWarpItem(WarpDevice warpFrom, MapTile warpTo) {
        int total = PlayerPrefs.GetInt("WarpItem-Total");
        for(int i = 0; i < total; ++i) {
            WarpDevice warpDevice = GetWarpDevice("WarpItem", i);
            if(warpDevice.Equals(warpFrom)) {
                SetWarpTile("WarpItem", i, true, warpTo);
                return;
            }
        }
        total = total + 1;
        PlayerPrefs.SetInt("WarpItem-Total", total);
        SetWarpDevice("WarpItem", total, warpFrom);
        SetWarpTile("WarpItem", total, true, warpTo);
    }

    static WarpDevice GetWarpDevice(string key, int index) {
        string tileKey = key + "-" + index + "-";
        string deviceName = PlayerPrefs.GetString(tileKey + "Name");
        int deviceId = PlayerPrefs.GetInt(tileKey + "ID");
        return Tuple.Create(deviceName, deviceId);
    }

    static void SetWarpDevice(string key, int index, WarpDevice warpDevice) {
        string tileKey = key + "-" + index + "-";
        PlayerPrefs.SetString(tileKey + "Name", warpDevice.Item1);
        PlayerPrefs.SetInt(tileKey + "ID", warpDevice.Item2);
    }

    static MapTile GetWarpTile(string key, int index, bool isDestination = true) {
        string directionKey;
        if(isDestination) {
            directionKey = "Destination";
        }
        else {
            directionKey = "Origin";
        }
        string tileKey = key + "-" + directionKey + "-" + index + "-";
        int map = PlayerPrefs.GetInt(tileKey + "Map");
        int x = PlayerPrefs.GetInt(tileKey + "X");
        int y = PlayerPrefs.GetInt(tileKey + "Y");
        return Tuple.Create(map, x, y);
    }

    static void SetWarpTile(string key, int index, bool isDestination, MapTile mapTile) {
        string directionKey;
        if(isDestination) {
            directionKey = "Destination";
        }
        else {
            directionKey = "Origin";
        }
        string tileKey = key + "-" + directionKey + "-" + index + "-";
        PlayerPrefs.SetInt(tileKey + "Map", mapTile.Item1);
        PlayerPrefs.SetInt(tileKey + "X", mapTile.Item2);
        PlayerPrefs.SetInt(tileKey + "Y", mapTile.Item3);
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
