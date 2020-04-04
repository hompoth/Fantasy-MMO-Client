﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using MapTile = System.Tuple<int, int, int>;
using WarpDevice = System.Tuple<string, int>;

public sealed class PathManager
{
    private static readonly Lazy<PathManager> lazy = new Lazy<PathManager>(() => new PathManager());
    int m_warpSpellPriority, m_warpItemPriority, m_distanceToPlayer, m_mapAreaId;
	List<Tuple<MapTile, MapTile>> m_warpZones;
	List<Tuple<WarpDevice, MapTile>> m_warpSpells;
	List<Tuple<WarpDevice, MapTile>> m_warpItems;
    List<Tuple<int, int, int, Tuple<LinkedList<MapTile>, WarpDevice, int>>> m_playerMapPathCache;
    List<Tuple<int, int, int, LinkedList<MapTile>, float>> m_playerWalkPathCache;
    Dictionary<int, List<Tuple<MapTile, MapTile>>> m_warpZoneMap;
    Dictionary<Tuple<MapTile, MapTile>, int> m_walkDistanceMap;
    Dictionary<Tuple<int, int>, Dictionary<int, List<Tuple<MapTile, MapTile>>>> m_mapAreaWarpZones;
    Dictionary<MapTile, int> m_mapAreaMap;
    HashSet<MapTile> m_warpStartSet, m_warpEndSet;

    float WALK_PATH_RESET_TIMER = 4f;
    int WALK_PATH_RESET_COUNT = 20;
    int WALK_PATH_BLOCK_COUNT = 15;
    int WALK_PATH_DEFAULT_COUNT = 5;

    public static PathManager Instance { 
        get { return lazy.Value; }
    }

	private PathManager() {
		m_warpZones = UserPrefs.GetWarpZones();
		m_warpSpells = UserPrefs.GetWarpSpells();
		m_warpItems = UserPrefs.GetWarpItems();
        if(m_warpZones.Count == 0 && m_warpSpells.Count == 0 && m_warpItems.Count == 0) {
            LoadWarpDefaults();
        }
        m_walkDistanceMap = new Dictionary<Tuple<MapTile, MapTile>, int>();
        m_warpZoneMap = new Dictionary<int, List<Tuple<MapTile, MapTile>>>();
        m_mapAreaMap = new Dictionary<MapTile, int>();
        m_mapAreaWarpZones = new Dictionary<Tuple<int, int>, Dictionary<int, List<Tuple<MapTile, MapTile>>>>();
        m_warpStartSet = new HashSet<MapTile>();
        m_warpEndSet = new HashSet<MapTile>();
        m_playerMapPathCache = new List<Tuple<int, int, int, Tuple<LinkedList<MapTile>, WarpDevice, int>>>();
        m_playerWalkPathCache = new List<Tuple<int, int, int, LinkedList<MapTile>, float>>();
        foreach(Tuple<MapTile, MapTile> warpZone in m_warpZones) {
            MapTile warpFromTile = warpZone.Item1;
            MapTile warpToTile = warpZone.Item2;
            int warpFromMap = warpFromTile.Item1;
            m_warpStartSet.Add(warpFromTile);
            m_warpEndSet.Add(warpToTile);
            AddWarpZoneToDictionary(m_warpZoneMap, warpFromMap, warpZone);
        }
		m_warpSpellPriority = 0; // TODO UserPrefs.GetWarpSpellPriority();
		m_warpItemPriority = 0; // TODO UserPrefs.GetWarpItemPriority();
        m_distanceToPlayer = 4; // TODO UserPrefs.GetDistanceToPlayer();
	}

    public async void InitializeCache(GameManager manager) {
        foreach(Tuple<MapTile, MapTile> warpZone in m_warpZones) {
            MapTile warpFromTile = warpZone.Item1;
            await GetMapArea(manager, warpFromTile);
        }
    }

    private void AddWarpZoneToDictionary(Dictionary<int, List<Tuple<MapTile, MapTile>>> warpZoneMap, int map, Tuple<MapTile, MapTile> warpZone) {
        if(warpZoneMap.ContainsKey(map)) {
            warpZoneMap[map].Add(warpZone);
        }
        else {
            warpZoneMap[map] = new List<Tuple<MapTile, MapTile>>();
            warpZoneMap[map].Add(warpZone);
        }
    }

    private async void LoadWarpDefaults() {
        await Task.Run(()=>{
            UserPrefs.AddWarpZone(Tuple.Create(4, 25, 14), Tuple.Create(1, 42, 28));
            UserPrefs.AddWarpZone(Tuple.Create(1, 42, 27), Tuple.Create(4, 24, 14));
            UserPrefs.AddWarpZone(Tuple.Create(4, 25, 15), Tuple.Create(1, 41, 28));
            UserPrefs.AddWarpZone(Tuple.Create(1, 41, 27), Tuple.Create(4, 24, 15));
            UserPrefs.AddWarpZone(Tuple.Create(1, 40, 27), Tuple.Create(4, 24, 15));
            UserPrefs.AddWarpZone(Tuple.Create(15, 13, 15), Tuple.Create(1, 39, 28));
            UserPrefs.AddWarpZone(Tuple.Create(15, 12, 15), Tuple.Create(1, 39, 28));
            UserPrefs.AddWarpZone(Tuple.Create(15, 11, 15), Tuple.Create(1, 38, 28));
            UserPrefs.AddWarpZone(Tuple.Create(15, 10, 15), Tuple.Create(1, 37, 28));
            UserPrefs.AddWarpZone(Tuple.Create(15, 9, 15), Tuple.Create(1, 37, 28));
            UserPrefs.AddWarpZone(Tuple.Create(1, 37, 27), Tuple.Create(15, 9, 14));
            UserPrefs.AddWarpZone(Tuple.Create(1, 38, 27), Tuple.Create(15, 11, 14));
            UserPrefs.AddWarpZone(Tuple.Create(1, 39, 27), Tuple.Create(15, 13, 14));
            UserPrefs.AddWarpZone(Tuple.Create(8, 100, 25), Tuple.Create(1, 2, 49));
            UserPrefs.AddWarpZone(Tuple.Create(8, 100, 26), Tuple.Create(1, 2, 50));
            UserPrefs.AddWarpZone(Tuple.Create(8, 100, 27), Tuple.Create(1, 2, 51));
            UserPrefs.AddWarpZone(Tuple.Create(8, 100, 24), Tuple.Create(1, 2, 48));
            UserPrefs.AddWarpZone(Tuple.Create(1, 1, 48), Tuple.Create(8, 99, 24));
            UserPrefs.AddWarpZone(Tuple.Create(1, 1, 49), Tuple.Create(8, 99, 25));
            UserPrefs.AddWarpZone(Tuple.Create(1, 1, 50), Tuple.Create(8, 99, 26));
            UserPrefs.AddWarpZone(Tuple.Create(1, 1, 51), Tuple.Create(8, 99, 27));
            UserPrefs.AddWarpZone(Tuple.Create(8, 1, 49), Tuple.Create(28, 99, 50));
            UserPrefs.AddWarpZone(Tuple.Create(8, 1, 50), Tuple.Create(28, 99, 51));
            UserPrefs.AddWarpZone(Tuple.Create(8, 1, 51), Tuple.Create(28, 99, 52));
            UserPrefs.AddWarpZone(Tuple.Create(8, 1, 52), Tuple.Create(28, 99, 53));
            UserPrefs.AddWarpZone(Tuple.Create(28, 100, 50), Tuple.Create(8, 2, 49));
            UserPrefs.AddWarpZone(Tuple.Create(28, 100, 51), Tuple.Create(8, 2, 50));
            UserPrefs.AddWarpZone(Tuple.Create(28, 100, 52), Tuple.Create(8, 2, 51));
            UserPrefs.AddWarpZone(Tuple.Create(28, 100, 53), Tuple.Create(8, 2, 52));
            UserPrefs.AddWarpZone(Tuple.Create(28, 91, 50), Tuple.Create(28, 9, 37));
            UserPrefs.AddWarpZone(Tuple.Create(28, 91, 51), Tuple.Create(28, 10, 37));
            UserPrefs.AddWarpZone(Tuple.Create(28, 91, 52), Tuple.Create(28, 11, 37));
            UserPrefs.AddWarpZone(Tuple.Create(28, 91, 53), Tuple.Create(28, 12, 37));
            UserPrefs.AddWarpZone(Tuple.Create(28, 12, 36), Tuple.Create(28, 92, 53));
            UserPrefs.AddWarpZone(Tuple.Create(28, 11, 36), Tuple.Create(28, 92, 52));
            UserPrefs.AddWarpZone(Tuple.Create(28, 10, 36), Tuple.Create(28, 92, 51));
            UserPrefs.AddWarpZone(Tuple.Create(28, 9, 36), Tuple.Create(28, 92, 50));
            UserPrefs.AddWarpZone(Tuple.Create(28, 54, 72), Tuple.Create(28, 61, 23));
            UserPrefs.AddWarpZone(Tuple.Create(28, 55, 72), Tuple.Create(28, 62, 23));
            UserPrefs.AddWarpZone(Tuple.Create(28, 55, 73), Tuple.Create(28, 63, 23));
            UserPrefs.AddWarpZone(Tuple.Create(28, 54, 73), Tuple.Create(28, 64, 23));
            UserPrefs.AddWarpZone(Tuple.Create(28, 61, 24), Tuple.Create(28, 54, 71));
            UserPrefs.AddWarpZone(Tuple.Create(28, 62, 24), Tuple.Create(28, 54, 71));
            UserPrefs.AddWarpZone(Tuple.Create(28, 63, 24), Tuple.Create(28, 55, 71));
            UserPrefs.AddWarpZone(Tuple.Create(28, 64, 24), Tuple.Create(28, 55, 71));
            UserPrefs.AddWarpZone(Tuple.Create(16, 50, 100), Tuple.Create(1, 64, 2));
            UserPrefs.AddWarpZone(Tuple.Create(16, 49, 100), Tuple.Create(1, 64, 2));
            UserPrefs.AddWarpZone(Tuple.Create(16, 48, 100), Tuple.Create(1, 63, 2));
            UserPrefs.AddWarpZone(Tuple.Create(1, 63, 1), Tuple.Create(16, 48, 99));
            UserPrefs.AddWarpZone(Tuple.Create(1, 64, 1), Tuple.Create(16, 49, 99));
            UserPrefs.AddWarpZone(Tuple.Create(2, 10, 9), Tuple.Create(8, 62, 7));
            UserPrefs.AddWarpZone(Tuple.Create(8, 62, 5), Tuple.Create(2, 10, 11));
            UserPrefs.AddWarpZone(Tuple.Create(7, 1, 51), Tuple.Create(1, 99, 52));
            UserPrefs.AddWarpZone(Tuple.Create(1, 100, 52), Tuple.Create(7, 2, 51));
            UserPrefs.AddWarpZone(Tuple.Create(7, 1, 52), Tuple.Create(1, 99, 53));
            UserPrefs.AddWarpZone(Tuple.Create(1, 100, 53), Tuple.Create(7, 2, 52));
            UserPrefs.AddWarpZone(Tuple.Create(25, 44, 1), Tuple.Create(1, 67, 99));
            UserPrefs.AddWarpZone(Tuple.Create(25, 45, 1), Tuple.Create(1, 68, 99));
            UserPrefs.AddWarpZone(Tuple.Create(25, 46, 1), Tuple.Create(1, 69, 99));
            UserPrefs.AddWarpZone(Tuple.Create(1, 69, 100), Tuple.Create(25, 46, 2));
            UserPrefs.AddWarpZone(Tuple.Create(1, 68, 100), Tuple.Create(25, 45, 2));
            UserPrefs.AddWarpZone(Tuple.Create(1, 67, 100), Tuple.Create(25, 44, 2));
            UserPrefs.AddWarpZone(Tuple.Create(14, 48, 1), Tuple.Create(25, 34, 99));
            UserPrefs.AddWarpZone(Tuple.Create(14, 49, 1), Tuple.Create(25, 35, 99));
            UserPrefs.AddWarpZone(Tuple.Create(14, 50, 1), Tuple.Create(25, 36, 99));
            UserPrefs.AddWarpZone(Tuple.Create(25, 36, 100), Tuple.Create(14, 50, 2));
            UserPrefs.AddWarpZone(Tuple.Create(25, 35, 100), Tuple.Create(14, 49, 2));
            UserPrefs.AddWarpZone(Tuple.Create(25, 34, 100), Tuple.Create(14, 48, 2));
            UserPrefs.AddWarpZone(Tuple.Create(10, 100, 40), Tuple.Create(14, 2, 22));
            UserPrefs.AddWarpZone(Tuple.Create(10, 100, 41), Tuple.Create(14, 2, 23));
            UserPrefs.AddWarpZone(Tuple.Create(10, 100, 42), Tuple.Create(14, 2, 24));
            UserPrefs.AddWarpZone(Tuple.Create(14, 1, 24), Tuple.Create(10, 99, 42));
            UserPrefs.AddWarpZone(Tuple.Create(14, 1, 23), Tuple.Create(10, 99, 41));
            UserPrefs.AddWarpZone(Tuple.Create(14, 1, 22), Tuple.Create(10, 99, 40));
            UserPrefs.AddWarpZone(Tuple.Create(8, 65, 100), Tuple.Create(10, 29, 2));
            UserPrefs.AddWarpZone(Tuple.Create(8, 66, 100), Tuple.Create(10, 29, 2));
            UserPrefs.AddWarpZone(Tuple.Create(8, 67, 100), Tuple.Create(10, 30, 2));
            UserPrefs.AddWarpZone(Tuple.Create(10, 31, 1), Tuple.Create(8, 67, 99));
            UserPrefs.AddWarpZone(Tuple.Create(10, 30, 1), Tuple.Create(8, 66, 99));
            UserPrefs.AddWarpZone(Tuple.Create(10, 29, 1), Tuple.Create(8, 65, 99));
            UserPrefs.AddWarpZone(Tuple.Create(8, 68, 100), Tuple.Create(10, 30, 2));
            UserPrefs.AddWarpZone(Tuple.Create(27, 51, 1), Tuple.Create(10, 34, 99));
            UserPrefs.AddWarpZone(Tuple.Create(27, 52, 1), Tuple.Create(10, 35, 99));
            UserPrefs.AddWarpZone(Tuple.Create(27, 53, 1), Tuple.Create(10, 36, 99));
            UserPrefs.AddWarpZone(Tuple.Create(10, 36, 100), Tuple.Create(27, 53, 2));
            UserPrefs.AddWarpZone(Tuple.Create(10, 34, 100), Tuple.Create(27, 51, 2));
            UserPrefs.AddWarpZone(Tuple.Create(10, 33, 100), Tuple.Create(27, 51, 2));
            UserPrefs.AddWarpZone(Tuple.Create(10, 37, 100), Tuple.Create(27, 53, 2));
            UserPrefs.AddWarpZone(Tuple.Create(3, 50, 100), Tuple.Create(1, 30, 2));
            UserPrefs.AddWarpZone(Tuple.Create(3, 51, 100), Tuple.Create(1, 31, 2));
            UserPrefs.AddWarpZone(Tuple.Create(3, 52, 100), Tuple.Create(1, 32, 2));
            UserPrefs.AddWarpZone(Tuple.Create(1, 30, 1), Tuple.Create(3, 50, 99));
            UserPrefs.AddWarpZone(Tuple.Create(1, 31, 1), Tuple.Create(3, 51, 99));
            UserPrefs.AddWarpZone(Tuple.Create(1, 32, 1), Tuple.Create(3, 52, 99));
            UserPrefs.AddWarpZone(Tuple.Create(6, 100, 99), Tuple.Create(3, 50, 55));
            UserPrefs.AddWarpZone(Tuple.Create(6, 100, 98), Tuple.Create(3, 51, 55));
            UserPrefs.AddWarpZone(Tuple.Create(6, 100, 97), Tuple.Create(3, 52, 55));
            UserPrefs.AddWarpZone(Tuple.Create(6, 100, 96), Tuple.Create(3, 53, 55));
            UserPrefs.AddWarpZone(Tuple.Create(3, 53, 54), Tuple.Create(6, 99, 96));
            UserPrefs.AddWarpZone(Tuple.Create(3, 51, 54), Tuple.Create(6, 99, 98));
            UserPrefs.AddWarpZone(Tuple.Create(3, 50, 54), Tuple.Create(6, 99, 99));
            UserPrefs.AddWarpZone(Tuple.Create(3, 52, 54), Tuple.Create(6, 99, 97));
            UserPrefs.AddWarpZone(Tuple.Create(9, 1, 3), Tuple.Create(6, 99, 74));
            UserPrefs.AddWarpZone(Tuple.Create(9, 1, 4), Tuple.Create(6, 99, 75));
            UserPrefs.AddWarpZone(Tuple.Create(9, 1, 5), Tuple.Create(6, 99, 76));
            UserPrefs.AddWarpZone(Tuple.Create(9, 1, 6), Tuple.Create(6, 99, 77));
            UserPrefs.AddWarpZone(Tuple.Create(6, 100, 74), Tuple.Create(9, 2, 3));
            UserPrefs.AddWarpZone(Tuple.Create(6, 100, 75), Tuple.Create(9, 2, 4));
            UserPrefs.AddWarpZone(Tuple.Create(6, 100, 76), Tuple.Create(9, 2, 5));
            UserPrefs.AddWarpZone(Tuple.Create(6, 100, 77), Tuple.Create(9, 2, 6));
            UserPrefs.AddWarpZone(Tuple.Create(18, 98, 49), Tuple.Create(16, 2, 49));
            UserPrefs.AddWarpZone(Tuple.Create(18, 98, 50), Tuple.Create(16, 2, 50));
            UserPrefs.AddWarpZone(Tuple.Create(18, 98, 51), Tuple.Create(16, 2, 51));
            UserPrefs.AddWarpZone(Tuple.Create(16, 1, 49), Tuple.Create(18, 97, 49));
            UserPrefs.AddWarpZone(Tuple.Create(16, 1, 50), Tuple.Create(18, 97, 50));
            UserPrefs.AddWarpZone(Tuple.Create(16, 1, 51), Tuple.Create(18, 97, 51));
            UserPrefs.AddWarpZone(Tuple.Create(16, 100, 50), Tuple.Create(19, 2, 46));
            UserPrefs.AddWarpZone(Tuple.Create(19, 1, 46), Tuple.Create(16, 99, 50));
            UserPrefs.AddWarpZone(Tuple.Create(17, 49, 100), Tuple.Create(16, 49, 2));
            UserPrefs.AddWarpZone(Tuple.Create(17, 50, 100), Tuple.Create(16, 50, 2));
            UserPrefs.AddWarpZone(Tuple.Create(17, 51, 100), Tuple.Create(16, 51, 2));
            UserPrefs.AddWarpZone(Tuple.Create(16, 49, 1), Tuple.Create(17, 49, 99));
            UserPrefs.AddWarpZone(Tuple.Create(16, 50, 1), Tuple.Create(17, 50, 99));
            UserPrefs.AddWarpZone(Tuple.Create(16, 51, 1), Tuple.Create(17, 51, 99));
            UserPrefs.AddWarpZone(Tuple.Create(16, 50, 36), Tuple.Create(36, 96, 99));
            UserPrefs.AddWarpZone(Tuple.Create(36, 96, 100), Tuple.Create(16, 50, 37));
            UserPrefs.AddWarpZone(Tuple.Create(36, 97, 100), Tuple.Create(16, 51, 37));
            UserPrefs.AddWarpZone(Tuple.Create(36, 95, 100), Tuple.Create(16, 49, 37));
            UserPrefs.AddWarpZone(Tuple.Create(13, 92, 100), Tuple.Create(11, 82, 2));
            UserPrefs.AddWarpZone(Tuple.Create(13, 93, 100), Tuple.Create(11, 83, 2));
            UserPrefs.AddWarpZone(Tuple.Create(13, 94, 100), Tuple.Create(11, 84, 2));
            UserPrefs.AddWarpZone(Tuple.Create(11, 82, 1), Tuple.Create(13, 92, 99));
            UserPrefs.AddWarpZone(Tuple.Create(11, 83, 1), Tuple.Create(13, 93, 99));
            UserPrefs.AddWarpZone(Tuple.Create(11, 84, 1), Tuple.Create(13, 94, 99));
            UserPrefs.AddWarpZone(Tuple.Create(11, 87, 54), Tuple.Create(11, 7, 98));
            UserPrefs.AddWarpZone(Tuple.Create(11, 6, 98), Tuple.Create(11, 83, 52));
            UserPrefs.AddWarpZone(Tuple.Create(11, 6, 99), Tuple.Create(11, 84, 52));
            UserPrefs.AddWarpZone(Tuple.Create(1, 21, 25), Tuple.Create(36, 22, 15));
            UserPrefs.AddWarpZone(Tuple.Create(36, 21, 16), Tuple.Create(1, 21, 27));
            UserPrefs.AddWarpZone(Tuple.Create(1, 23, 25), Tuple.Create(36, 24, 15));
            UserPrefs.AddWarpZone(Tuple.Create(1, 22, 25), Tuple.Create(36, 23, 15));
            UserPrefs.AddWarpZone(Tuple.Create(36, 22, 16), Tuple.Create(1, 21, 27));
            UserPrefs.AddWarpZone(Tuple.Create(36, 23, 16), Tuple.Create(1, 22, 27));
            UserPrefs.AddWarpZone(Tuple.Create(36, 24, 16), Tuple.Create(1, 23, 27));
            UserPrefs.AddWarpZone(Tuple.Create(36, 6, 48), Tuple.Create(1, 22, 48));
            UserPrefs.AddWarpZone(Tuple.Create(36, 7, 48), Tuple.Create(1, 23, 48));
            UserPrefs.AddWarpZone(Tuple.Create(36, 5, 48), Tuple.Create(1, 22, 48));
            UserPrefs.AddWarpZone(Tuple.Create(36, 8, 48), Tuple.Create(1, 23, 48));
            UserPrefs.AddWarpZone(Tuple.Create(1, 22, 46), Tuple.Create(36, 6, 46));
            UserPrefs.AddWarpZone(Tuple.Create(1, 23, 46), Tuple.Create(36, 7, 46));
            UserPrefs.AddWarpZone(Tuple.Create(1, 12, 46), Tuple.Create(36, 52, 13));
            UserPrefs.AddWarpZone(Tuple.Create(1, 13, 46), Tuple.Create(36, 53, 13));
            UserPrefs.AddWarpZone(Tuple.Create(36, 51, 15), Tuple.Create(1, 12, 48));
            UserPrefs.AddWarpZone(Tuple.Create(36, 52, 15), Tuple.Create(1, 12, 48));
            UserPrefs.AddWarpZone(Tuple.Create(36, 53, 15), Tuple.Create(1, 13, 48));
            UserPrefs.AddWarpZone(Tuple.Create(36, 54, 15), Tuple.Create(1, 13, 48));
            UserPrefs.AddWarpZone(Tuple.Create(1, 54, 98), Tuple.Create(36, 6, 98));
            UserPrefs.AddWarpZone(Tuple.Create(1, 55, 98), Tuple.Create(36, 6, 98));
            UserPrefs.AddWarpZone(Tuple.Create(1, 56, 98), Tuple.Create(36, 7, 98));
            UserPrefs.AddWarpZone(Tuple.Create(36, 5, 100), Tuple.Create(1, 54, 100));
            UserPrefs.AddWarpZone(Tuple.Create(36, 6, 100), Tuple.Create(1, 55, 100));
            UserPrefs.AddWarpZone(Tuple.Create(36, 7, 100), Tuple.Create(1, 55, 100));
            UserPrefs.AddWarpZone(Tuple.Create(36, 8, 100), Tuple.Create(1, 56, 100));
            UserPrefs.AddWarpZone(Tuple.Create(1, 61, 82), Tuple.Create(36, 61, 81));
            UserPrefs.AddWarpZone(Tuple.Create(36, 60, 83), Tuple.Create(1, 60, 84));
            UserPrefs.AddWarpZone(Tuple.Create(36, 61, 83), Tuple.Create(1, 61, 84));
            UserPrefs.AddWarpZone(Tuple.Create(36, 62, 83), Tuple.Create(1, 61, 84));
            UserPrefs.AddWarpZone(Tuple.Create(36, 63, 83), Tuple.Create(1, 62, 84));
            UserPrefs.AddWarpZone(Tuple.Create(36, 6, 75), Tuple.Create(1, 37, 81));
            UserPrefs.AddWarpZone(Tuple.Create(36, 7, 75), Tuple.Create(1, 37, 81));
            UserPrefs.AddWarpZone(Tuple.Create(36, 8, 75), Tuple.Create(1, 38, 81));
            UserPrefs.AddWarpZone(Tuple.Create(36, 5, 75), Tuple.Create(1, 36, 81));
            UserPrefs.AddWarpZone(Tuple.Create(1, 37, 79), Tuple.Create(36, 6, 73));
            UserPrefs.AddWarpZone(Tuple.Create(1, 60, 51), Tuple.Create(36, 38, 46));
            UserPrefs.AddWarpZone(Tuple.Create(36, 38, 48), Tuple.Create(1, 60, 53));
            UserPrefs.AddWarpZone(Tuple.Create(36, 39, 48), Tuple.Create(1, 60, 53));
            UserPrefs.AddWarpZone(Tuple.Create(36, 40, 48), Tuple.Create(1, 61, 53));
            UserPrefs.AddWarpZone(Tuple.Create(36, 37, 48), Tuple.Create(1, 59, 53));
            UserPrefs.AddWarpZone(Tuple.Create(1, 59, 51), Tuple.Create(36, 38, 46));
            UserPrefs.AddWarpZone(Tuple.Create(1, 61, 51), Tuple.Create(36, 39, 46));
            UserPrefs.AddWarpZone(Tuple.Create(15, 9, 3), Tuple.Create(15, 73, 36));
            UserPrefs.AddWarpZone(Tuple.Create(15, 73, 38), Tuple.Create(15, 9, 5));
            UserPrefs.AddWarpZone(Tuple.Create(15, 5, 3), Tuple.Create(15, 34, 36));
            UserPrefs.AddWarpZone(Tuple.Create(15, 34, 38), Tuple.Create(15, 5, 5));
            UserPrefs.AddWarpZone(Tuple.Create(15, 35, 38), Tuple.Create(15, 6, 5));
            UserPrefs.AddWarpZone(Tuple.Create(11, 10, 3), Tuple.Create(11, 85, 98));
            UserPrefs.AddWarpZone(Tuple.Create(11, 9, 3), Tuple.Create(11, 84, 98));
            UserPrefs.AddWarpZone(Tuple.Create(11, 11, 3), Tuple.Create(11, 86, 98));
            UserPrefs.AddWarpZone(Tuple.Create(11, 85, 100), Tuple.Create(11, 10, 5));
            UserPrefs.AddWarpZone(Tuple.Create(11, 84, 100), Tuple.Create(11, 9, 5));
            UserPrefs.AddWarpZone(Tuple.Create(11, 86, 100), Tuple.Create(11, 11, 5));
            UserPrefs.AddWarpZone(Tuple.Create(12, 1, 19), Tuple.Create(11, 98, 21));
            UserPrefs.AddWarpZone(Tuple.Create(12, 1, 20), Tuple.Create(11, 98, 22));
            UserPrefs.AddWarpZone(Tuple.Create(12, 1, 21), Tuple.Create(11, 98, 23));
            UserPrefs.AddWarpZone(Tuple.Create(11, 100, 21), Tuple.Create(12, 2, 19));
            UserPrefs.AddWarpZone(Tuple.Create(11, 100, 22), Tuple.Create(12, 2, 20));
            UserPrefs.AddWarpZone(Tuple.Create(11, 100, 23), Tuple.Create(12, 2, 21));
            UserPrefs.AddWarpZone(Tuple.Create(12, 86, 15), Tuple.Create(12, 96, 97));
            UserPrefs.AddWarpZone(Tuple.Create(12, 96, 98), Tuple.Create(12, 86, 14));
            UserPrefs.AddWarpZone(Tuple.Create(12, 97, 98), Tuple.Create(12, 87, 15));
            UserPrefs.AddWarpZone(Tuple.Create(9, 1, 34), Tuple.Create(9, 42, 19));
            UserPrefs.AddWarpZone(Tuple.Create(9, 41, 21), Tuple.Create(9, 2, 29));
            UserPrefs.AddWarpZone(Tuple.Create(9, 42, 21), Tuple.Create(9, 3, 29));
            UserPrefs.AddWarpZone(Tuple.Create(9, 43, 21), Tuple.Create(9, 4, 29));
            UserPrefs.AddWarpZone(Tuple.Create(29, 50, 94), Tuple.Create(35, 78, 17));
            UserPrefs.AddWarpZone(Tuple.Create(35, 77, 15), Tuple.Create(29, 49, 93));
            UserPrefs.AddWarpZone(Tuple.Create(35, 78, 15), Tuple.Create(29, 50, 93));
            UserPrefs.AddWarpZone(Tuple.Create(35, 79, 15), Tuple.Create(29, 51, 93));
            UserPrefs.AddWarpZone(Tuple.Create(35, 79, 43), Tuple.Create(1, 50, 61));
            UserPrefs.AddWarpZone(Tuple.Create(35, 80, 43), Tuple.Create(1, 51, 61));
            UserPrefs.AddWarpZone(Tuple.Create(1, 40, 50), Tuple.Create(36, 35, 98));
            UserPrefs.AddWarpZone(Tuple.Create(36, 35, 100), Tuple.Create(1, 40, 51));
            UserPrefs.AddWarpZone(Tuple.Create(36, 36, 100), Tuple.Create(1, 41, 51));
            UserPrefs.AddWarpZone(Tuple.Create(36, 37, 100), Tuple.Create(1, 41, 51));
            UserPrefs.AddWarpZone(Tuple.Create(36, 34, 100), Tuple.Create(1, 39, 51));
            UserPrefs.AddWarpZone(Tuple.Create(24, 37, 81), Tuple.Create(24, 49, 94));
            UserPrefs.AddWarpZone(Tuple.Create(24, 49, 96), Tuple.Create(24, 35, 81));
            UserPrefs.AddWarpZone(Tuple.Create(24, 64, 81), Tuple.Create(24, 52, 94));
            UserPrefs.AddWarpZone(Tuple.Create(24, 52, 96), Tuple.Create(24, 66, 81));
            UserPrefs.AddWarpZone(Tuple.Create(24, 96, 94), Tuple.Create(29, 55, 15));
            UserPrefs.AddWarpZone(Tuple.Create(29, 53, 15), Tuple.Create(24, 96, 92));
            UserPrefs.AddWarpZone(Tuple.Create(29, 48, 6), Tuple.Create(24, 5, 92));
            UserPrefs.AddWarpZone(Tuple.Create(24, 5, 94), Tuple.Create(29, 46, 6));
            UserPrefs.AddWarpZone(Tuple.Create(24, 37, 67), Tuple.Create(24, 4, 45));
            UserPrefs.AddWarpZone(Tuple.Create(24, 4, 43), Tuple.Create(24, 39, 67));
            UserPrefs.AddWarpZone(Tuple.Create(24, 64, 67), Tuple.Create(24, 97, 45));
            UserPrefs.AddWarpZone(Tuple.Create(24, 97, 43), Tuple.Create(24, 62, 67));
            UserPrefs.AddWarpZone(Tuple.Create(24, 56, 42), Tuple.Create(24, 73, 40));
            UserPrefs.AddWarpZone(Tuple.Create(24, 73, 38), Tuple.Create(24, 56, 40));
            UserPrefs.AddWarpZone(Tuple.Create(24, 28, 37), Tuple.Create(24, 45, 40));
            UserPrefs.AddWarpZone(Tuple.Create(24, 45, 42), Tuple.Create(24, 28, 39));
            UserPrefs.AddWarpZone(Tuple.Create(2, 99, 89), Tuple.Create(2, 29, 25));
            UserPrefs.AddWarpZone(Tuple.Create(2, 29, 22), Tuple.Create(2, 97, 90));
            UserPrefs.AddWarpZone(Tuple.Create(2, 9, 69), Tuple.Create(2, 95, 8));
            UserPrefs.AddWarpZone(Tuple.Create(2, 91, 8), Tuple.Create(2, 11, 73));
            UserPrefs.AddWarpZone(Tuple.Create(2, 92, 8), Tuple.Create(2, 11, 73));
            UserPrefs.AddWarpZone(Tuple.Create(2, 92, 9), Tuple.Create(2, 11, 73));
            UserPrefs.AddWarpZone(Tuple.Create(2, 91, 9), Tuple.Create(2, 11, 73));
            UserPrefs.AddWarpZone(Tuple.Create(42, 42, 100), Tuple.Create(8, 37, 3));
            UserPrefs.AddWarpZone(Tuple.Create(42, 43, 100), Tuple.Create(8, 38, 3));
            UserPrefs.AddWarpZone(Tuple.Create(8, 37, 1), Tuple.Create(42, 42, 98));
            UserPrefs.AddWarpZone(Tuple.Create(42, 44, 100), Tuple.Create(8, 39, 3));
            UserPrefs.AddWarpZone(Tuple.Create(8, 38, 1), Tuple.Create(42, 43, 98));
            UserPrefs.AddWarpZone(Tuple.Create(42, 45, 100), Tuple.Create(8, 40, 3));
            UserPrefs.AddWarpZone(Tuple.Create(8, 39, 1), Tuple.Create(42, 44, 98));
            UserPrefs.AddWarpZone(Tuple.Create(42, 46, 100), Tuple.Create(8, 41, 3));
            UserPrefs.AddWarpZone(Tuple.Create(8, 40, 1), Tuple.Create(42, 45, 98));
            UserPrefs.AddWarpZone(Tuple.Create(8, 41, 1), Tuple.Create(42, 46, 98));
            UserPrefs.AddWarpZone(Tuple.Create(42, 54, 2), Tuple.Create(43, 53, 98));
            UserPrefs.AddWarpZone(Tuple.Create(43, 53, 100), Tuple.Create(42, 54, 4));
            UserPrefs.AddWarpZone(Tuple.Create(42, 63, 2), Tuple.Create(43, 54, 98));
            UserPrefs.AddWarpZone(Tuple.Create(43, 54, 100), Tuple.Create(42, 63, 4));
            UserPrefs.AddWarpZone(Tuple.Create(43, 55, 100), Tuple.Create(42, 71, 4));
            UserPrefs.AddWarpZone(Tuple.Create(42, 71, 2), Tuple.Create(43, 55, 98));
            UserPrefs.AddWarpZone(Tuple.Create(31, 76, 56), Tuple.Create(31, 61, 53));
            UserPrefs.AddWarpZone(Tuple.Create(31, 60, 52), Tuple.Create(31, 63, 53));
            UserPrefs.AddWarpZone(Tuple.Create(31, 62, 52), Tuple.Create(31, 61, 54));
            UserPrefs.AddWarpZone(Tuple.Create(31, 60, 53), Tuple.Create(31, 63, 54));
            UserPrefs.AddWarpZone(Tuple.Create(31, 62, 53), Tuple.Create(31, 61, 55));
            UserPrefs.AddWarpZone(Tuple.Create(31, 60, 54), Tuple.Create(31, 62, 55));
            UserPrefs.AddWarpZone(Tuple.Create(31, 61, 54), Tuple.Create(31, 61, 56));
            UserPrefs.AddWarpZone(Tuple.Create(31, 60, 55), Tuple.Create(31, 63, 56));
            UserPrefs.AddWarpZone(Tuple.Create(31, 62, 55), Tuple.Create(31, 61, 57));
            UserPrefs.AddWarpZone(Tuple.Create(31, 60, 56), Tuple.Create(31, 63, 57));
            UserPrefs.AddWarpZone(Tuple.Create(31, 62, 56), Tuple.Create(31, 65, 54));
            UserPrefs.AddWarpZone(Tuple.Create(31, 64, 53), Tuple.Create(31, 67, 54));
            UserPrefs.AddWarpZone(Tuple.Create(31, 68, 53), Tuple.Create(31, 71, 54));
            UserPrefs.AddWarpZone(Tuple.Create(31, 66, 53), Tuple.Create(31, 65, 55));
            UserPrefs.AddWarpZone(Tuple.Create(31, 64, 54), Tuple.Create(31, 67, 55));
            UserPrefs.AddWarpZone(Tuple.Create(31, 66, 54), Tuple.Create(31, 65, 56));
            UserPrefs.AddWarpZone(Tuple.Create(31, 64, 55), Tuple.Create(31, 67, 56));
            UserPrefs.AddWarpZone(Tuple.Create(31, 66, 55), Tuple.Create(31, 65, 57));
            UserPrefs.AddWarpZone(Tuple.Create(31, 64, 56), Tuple.Create(31, 67, 57));
            UserPrefs.AddWarpZone(Tuple.Create(31, 66, 56), Tuple.Create(31, 69, 54));
            UserPrefs.AddWarpZone(Tuple.Create(31, 70, 53), Tuple.Create(31, 69, 55));
            UserPrefs.AddWarpZone(Tuple.Create(31, 68, 54), Tuple.Create(31, 71, 55));
            UserPrefs.AddWarpZone(Tuple.Create(31, 70, 54), Tuple.Create(31, 69, 56));
            UserPrefs.AddWarpZone(Tuple.Create(31, 68, 55), Tuple.Create(31, 71, 56));
            UserPrefs.AddWarpZone(Tuple.Create(31, 70, 55), Tuple.Create(31, 69, 57));
            UserPrefs.AddWarpZone(Tuple.Create(31, 68, 56), Tuple.Create(31, 71, 57));
            UserPrefs.AddWarpZone(Tuple.Create(31, 70, 56), Tuple.Create(31, 73, 53));
            UserPrefs.AddWarpZone(Tuple.Create(31, 72, 52), Tuple.Create(31, 75, 53));
            UserPrefs.AddWarpZone(Tuple.Create(31, 74, 52), Tuple.Create(31, 73, 54));
            UserPrefs.AddWarpZone(Tuple.Create(31, 72, 53), Tuple.Create(31, 75, 54));
            UserPrefs.AddWarpZone(Tuple.Create(31, 74, 53), Tuple.Create(31, 73, 55));
            UserPrefs.AddWarpZone(Tuple.Create(31, 72, 54), Tuple.Create(31, 74, 55));
            UserPrefs.AddWarpZone(Tuple.Create(31, 73, 54), Tuple.Create(31, 73, 56));
            UserPrefs.AddWarpZone(Tuple.Create(31, 72, 55), Tuple.Create(31, 75, 56));
            UserPrefs.AddWarpZone(Tuple.Create(31, 74, 55), Tuple.Create(31, 73, 57));
            UserPrefs.AddWarpZone(Tuple.Create(31, 72, 56), Tuple.Create(31, 75, 57));
            UserPrefs.AddWarpZone(Tuple.Create(31, 74, 57), Tuple.Create(31, 77, 54));
            UserPrefs.AddWarpZone(Tuple.Create(31, 77, 53), Tuple.Create(31, 77, 55));
            UserPrefs.AddWarpZone(Tuple.Create(31, 74, 56), Tuple.Create(31, 77, 54));
            UserPrefs.AddWarpZone(Tuple.Create(31, 76, 53), Tuple.Create(31, 78, 54));
            UserPrefs.AddWarpZone(Tuple.Create(31, 76, 54), Tuple.Create(31, 78, 56));
            UserPrefs.AddWarpZone(Tuple.Create(31, 77, 55), Tuple.Create(31, 78, 57));
            UserPrefs.AddWarpZone(Tuple.Create(36, 83, 12), Tuple.Create(1, 22, 80));
            UserPrefs.AddWarpZone(Tuple.Create(36, 84, 12), Tuple.Create(1, 23, 80));
            UserPrefs.AddWarpZone(Tuple.Create(36, 85, 12), Tuple.Create(1, 24, 80));
            UserPrefs.AddWarpZone(Tuple.Create(1, 23, 79), Tuple.Create(36, 84, 11));
            UserPrefs.AddWarpZone(Tuple.Create(35, 87, 20), Tuple.Create(32, 34, 49));
            UserPrefs.AddWarpZone(Tuple.Create(32, 35, 49), Tuple.Create(35, 87, 21));
            UserPrefs.AddWarpZone(Tuple.Create(35, 89, 20), Tuple.Create(32, 48, 49));
            UserPrefs.AddWarpZone(Tuple.Create(32, 47, 49), Tuple.Create(35, 89, 21));
            UserPrefs.AddWarpZone(Tuple.Create(35, 87, 16), Tuple.Create(33, 43, 43));
            UserPrefs.AddWarpZone(Tuple.Create(33, 43, 42), Tuple.Create(35, 87, 17));
            UserPrefs.AddWarpZone(Tuple.Create(35, 89, 16), Tuple.Create(33, 58, 43));
            UserPrefs.AddWarpZone(Tuple.Create(33, 58, 42), Tuple.Create(35, 89, 17));
            UserPrefs.AddWarpZone(Tuple.Create(35, 87, 12), Tuple.Create(34, 54, 49));
            UserPrefs.AddWarpZone(Tuple.Create(34, 54, 50), Tuple.Create(35, 87, 13));
            UserPrefs.AddWarpZone(Tuple.Create(34, 55, 50), Tuple.Create(35, 87, 13));
            UserPrefs.AddWarpZone(Tuple.Create(35, 89, 12), Tuple.Create(34, 52, 60));
            UserPrefs.AddWarpZone(Tuple.Create(34, 52, 59), Tuple.Create(35, 89, 13));
            UserPrefs.AddWarpZone(Tuple.Create(34, 53, 59), Tuple.Create(35, 89, 13));
            UserPrefs.AddWarpZone(Tuple.Create(43, 43, 26), Tuple.Create(44, 57, 91));
            UserPrefs.AddWarpZone(Tuple.Create(44, 57, 91), Tuple.Create(43, 43, 26));
            UserPrefs.AddWarpZone(Tuple.Create(24, 50, 4), Tuple.Create(29, 54, 62));
            UserPrefs.AddWarpZone(Tuple.Create(24, 51, 4), Tuple.Create(29, 56, 62));
            UserPrefs.AddWarpZone(Tuple.Create(29, 54, 63), Tuple.Create(24, 50, 5));
            UserPrefs.AddWarpZone(Tuple.Create(29, 55, 63), Tuple.Create(24, 50, 5));
            UserPrefs.AddWarpZone(Tuple.Create(29, 56, 63), Tuple.Create(24, 51, 5));
            UserPrefs.AddWarpZone(Tuple.Create(25, 1, 53), Tuple.Create(38, 99, 23));
            UserPrefs.AddWarpZone(Tuple.Create(25, 1, 54), Tuple.Create(38, 99, 24));
            UserPrefs.AddWarpZone(Tuple.Create(25, 1, 55), Tuple.Create(38, 99, 25));
            UserPrefs.AddWarpZone(Tuple.Create(25, 1, 56), Tuple.Create(38, 99, 26));
            UserPrefs.AddWarpZone(Tuple.Create(25, 1, 57), Tuple.Create(38, 99, 27));
            UserPrefs.AddWarpZone(Tuple.Create(38, 100, 23), Tuple.Create(25, 2, 53));
            UserPrefs.AddWarpZone(Tuple.Create(38, 100, 24), Tuple.Create(25, 2, 54));
            UserPrefs.AddWarpZone(Tuple.Create(38, 100, 25), Tuple.Create(25, 2, 55));
            UserPrefs.AddWarpZone(Tuple.Create(38, 100, 26), Tuple.Create(25, 2, 56));
            UserPrefs.AddWarpZone(Tuple.Create(38, 100, 27), Tuple.Create(25, 2, 57));
            UserPrefs.AddWarpZone(Tuple.Create(38, 37, 35), Tuple.Create(39, 5, 98));
            UserPrefs.AddWarpZone(Tuple.Create(38, 38, 35), Tuple.Create(39, 6, 98));
            UserPrefs.AddWarpZone(Tuple.Create(38, 39, 35), Tuple.Create(39, 7, 98));
            UserPrefs.AddWarpZone(Tuple.Create(39, 5, 99), Tuple.Create(38, 37, 36));
            UserPrefs.AddWarpZone(Tuple.Create(39, 6, 99), Tuple.Create(38, 38, 36));
            UserPrefs.AddWarpZone(Tuple.Create(39, 7, 99), Tuple.Create(38, 39, 36));
            UserPrefs.AddWarpZone(Tuple.Create(39, 40, 4), Tuple.Create(39, 93, 99));
            UserPrefs.AddWarpZone(Tuple.Create(39, 41, 4), Tuple.Create(39, 94, 99));
            UserPrefs.AddWarpZone(Tuple.Create(39, 42, 4), Tuple.Create(39, 95, 99));
            UserPrefs.AddWarpZone(Tuple.Create(39, 93, 100), Tuple.Create(39, 40, 5));
            UserPrefs.AddWarpZone(Tuple.Create(39, 94, 100), Tuple.Create(39, 41, 5));
            UserPrefs.AddWarpZone(Tuple.Create(39, 95, 100), Tuple.Create(39, 42, 5));
            UserPrefs.AddWarpZone(Tuple.Create(29, 59, 48), Tuple.Create(40, 19, 98));
            UserPrefs.AddWarpZone(Tuple.Create(40, 17, 98), Tuple.Create(29, 60, 48));

            m_warpZones = UserPrefs.GetWarpZones();
            m_warpSpells = UserPrefs.GetWarpSpells();
            m_warpItems = UserPrefs.GetWarpItems();
        });
    }

    public int GetDistanceToPlayer() {
        return m_distanceToPlayer;
    }

    private async Task<Dictionary<int, List<Tuple<MapTile, MapTile>>>> GetWarpZonesToGoal(GameManager manager, MapTile start, MapTile goal) {
        Dictionary<int, List<Tuple<MapTile, MapTile>>> potentialWarpZones = new Dictionary<int, List<Tuple<MapTile, MapTile>>>();
        Dictionary<int, List<Tuple<MapTile, MapTile>>> usedWarpZones = new Dictionary<int, List<Tuple<MapTile, MapTile>>>();
        Queue<Tuple<int, int>> areaOpenQueue = new Queue<Tuple<int, int>>();
        List<int> closedAreaList = new List<int>();
        int startMap = start.Item1;
        int startArea = await GetMapArea(manager, start);
        int goalArea = await GetMapArea(manager, goal);
        Tuple<int, int> startGoalZone = Tuple.Create(startArea, goalArea);
        bool goalFound = false; 
        if(m_mapAreaWarpZones.ContainsKey(startGoalZone)) {
            return m_mapAreaWarpZones[startGoalZone];
        }
        if(!closedAreaList.Contains(startArea)) {
            areaOpenQueue.Enqueue(Tuple.Create(startMap, startArea));
            closedAreaList.Add(startArea);
        }
        while(areaOpenQueue.Count > 0) {
            Tuple<int, int> areaMap = areaOpenQueue.Dequeue();
            int map = areaMap.Item1;
            int area = areaMap.Item2;
            if(area != goalArea) {
                if(m_warpZoneMap.ContainsKey(map)) {
                    List<Tuple<int, int>> areasToClose = new List<Tuple<int, int>>();
                    foreach(Tuple<MapTile, MapTile> warpZone in m_warpZoneMap[map]) {   // TODO replace with area map.
                        MapTile warpFromTile = warpZone.Item1;
                        int mapAreaFrom = await GetMapArea(manager, warpFromTile);
                        if(mapAreaFrom == area) {
                            MapTile warpToTile = warpZone.Item2;
                            int warpToMap = warpToTile.Item1;
                            int warpToArea = await GetMapArea(manager, warpToTile);
                            if(!closedAreaList.Contains(warpToArea)) {
                                areasToClose.Add(Tuple.Create(warpToMap, warpToArea));
                                AddWarpZoneToDictionary(potentialWarpZones, warpToArea, warpZone);
                            }
                        }
                    }
                    if(!goalFound) {
                        foreach(Tuple<int, int> newAreaMap in areasToClose) {
                            int warpToArea = newAreaMap.Item2;
                            if(warpToArea == goalArea) {
                                goalFound = true;
                            }
                            if(!closedAreaList.Contains(warpToArea)) {
                                areaOpenQueue.Enqueue(newAreaMap);
                                closedAreaList.Add(warpToArea);
                            }
                        }
                    }
                }
            }
        }
        Queue<int> currentAreaQueue = new Queue<int>();
        Queue<int> nextAreaQueue = new Queue<int>();
        List<int> closedNextAreaList = new List<int>();
        nextAreaQueue.Enqueue(goalArea);
        int currentArea = -1;
        while(nextAreaQueue.Count > 0 && currentArea != startArea) {
            currentAreaQueue = new Queue<int>(nextAreaQueue);
            nextAreaQueue.Clear();
            while(currentAreaQueue.Count > 0 && currentArea != startArea) {
                currentArea = currentAreaQueue.Dequeue();
                if(potentialWarpZones.ContainsKey(currentArea)) {
                    foreach(Tuple<MapTile, MapTile> warpZone in potentialWarpZones[currentArea]) {
                        MapTile warpFromTile = warpZone.Item1;
                        int warpFromArea = await GetMapArea(manager, warpFromTile);
                        MapTile warpToTile = warpZone.Item2;
                        int warpToArea = await GetMapArea(manager, warpToTile);
                        AddWarpZoneToDictionary(usedWarpZones, warpFromArea, warpZone);
                        if(!closedNextAreaList.Contains(warpFromArea)) {
                            nextAreaQueue.Enqueue(warpFromArea);
                            closedNextAreaList.Add(warpFromArea);
                        }
                    }
                }
            }
        }
        m_mapAreaWarpZones[startGoalZone] = usedWarpZones;
        return usedWarpZones;
    }

	public async Task<Tuple<LinkedList<MapTile>, WarpDevice, int>> GetMapPath(GameManager manager, MapTile start, MapTile goal, bool useWarpDevices = true) {
        List<Task> mapPathList = new List<Task>();
        int startArea = await GetMapArea(manager, start);
        int goalArea = await GetMapArea(manager, goal);
        if(startArea == goalArea) {
            LinkedList<MapTile> newPath = new LinkedList<MapTile>(); 
            newPath.AddLast(start);
            newPath.AddLast(goal);
            int distance = DistanceHeuristic(start, goal);
            return Tuple.Create(newPath, default(WarpDevice), distance);
        }
        if(TryGetMapPathCache(manager, startArea, goalArea, out Tuple<LinkedList<MapTile>, WarpDevice, int> previousPath)) {
            return previousPath;
        }
        goal = await GetClosestUnblockedPosition(manager, start, goal);
        mapPathList.Add(GetMapPathCore(manager, start, goal, default(WarpDevice), 0));
        if(useWarpDevices) {    // TODO: Handle case where spell has aether (Ex. If same warp device is returned for 2 attempts, don't try again for 30 seconds)
            foreach(Tuple<WarpDevice, MapTile> warpSpell in GetWarpSpells()) {
                mapPathList.Add(GetMapPathCore(manager, start, warpSpell.Item2, warpSpell.Item1, m_warpSpellPriority));
            }
            foreach(Tuple<WarpDevice, MapTile> warpItem in GetWarpItems()) {
                mapPathList.Add(GetMapPathCore(manager, start, warpItem.Item2, warpItem.Item1, m_warpItemPriority));
            }
        }
        int minPathDistance = Int32.MaxValue;
        Tuple<LinkedList<MapTile>, WarpDevice, int> minMapPath = null;
        while(mapPathList.Count > 0) {
            Task<Tuple<LinkedList<MapTile>, WarpDevice, int>> task = (Task<Tuple<LinkedList<MapTile>, WarpDevice, int>>) await Task.WhenAny(mapPathList);
            Tuple<LinkedList<MapTile>, WarpDevice, int> result = task.Result;
            int distance = result.Item3;
            if(distance < minPathDistance) {
                minPathDistance = distance;
                minMapPath = result;
            }
            mapPathList.Remove(task);
        }
        AddMapPathCache(manager, startArea, goalArea, minMapPath);
        return minMapPath;
    }

	public async Task<Tuple<LinkedList<MapTile>, WarpDevice, int>> GetMapPathCore(GameManager manager, MapTile start, MapTile goal, WarpDevice warpDevice, int pathLength) {
        await Task.Yield();
        Dictionary<int, List<Tuple<MapTile, MapTile>>> usedWarpZones = await GetWarpZonesToGoal(manager, start, goal);
		Dictionary<MapTile, MapTile> cameFrom = new Dictionary<MapTile, MapTile>();
		Dictionary<MapTile, int> gScore = new Dictionary<MapTile, int>();
        Queue<Tuple<MapTile, MapTile>> openQueue = new Queue<Tuple<MapTile, MapTile>>();
        int startArea = await GetMapArea(manager, start);
        int goalArea = await GetMapArea(manager, goal);
        usedWarpZones = new Dictionary<int, List<Tuple<MapTile, MapTile>>>(usedWarpZones);
        AddWarpZoneToDictionary(usedWarpZones, goalArea, Tuple.Create(goal, goal));
        openQueue.Enqueue(Tuple.Create(start, start));
        gScore[start] = 0;
        while(openQueue.Count > 0) {
            Tuple<MapTile, MapTile> currentWarp = openQueue.Dequeue();
            MapTile currentFrom = currentWarp.Item1;
            MapTile currentTo = currentWarp.Item2;
            int area = await GetMapArea(manager, currentTo);
            if(usedWarpZones.ContainsKey(area)) {
                foreach(Tuple<MapTile, MapTile> warpZone in usedWarpZones[area]) {
                    MapTile warpFromTile = warpZone.Item1;
                    int distance = 0;
                    if(currentTo.Equals(warpFromTile) || (distance = await GetDistanceToPoint(manager, currentTo, warpFromTile)) > 0) {
                        int currentGScore = gScore[currentFrom] + distance;
                        if(!gScore.ContainsKey(warpFromTile) || currentGScore < gScore[warpFromTile]) {
                            cameFrom[warpFromTile] = currentFrom;
                            gScore[warpFromTile] = currentGScore;
                            if(!openQueue.Contains(warpZone)) {
                                openQueue.Enqueue(warpZone);
                            }
                        }
                    }
                }
            }
        }
        LinkedList<MapTile> path = new LinkedList<MapTile>();
        MapTile currentTile = goal;
        if(cameFrom.ContainsKey(currentTile)) {
            pathLength = gScore[currentTile] + pathLength;
            path.AddFirst(currentTile);
            while(cameFrom.ContainsKey(currentTile)) {
                currentTile = cameFrom[currentTile];
                pathLength = gScore[currentTile] + pathLength;
                path.AddFirst(currentTile);
            }
        }
        return Tuple.Create(path, warpDevice, pathLength);
    }

    public async Task<LinkedList<MapTile>> GetWalkPath(GameManager manager, MapTile start, MapTile goal, bool forceMapPosition = false) {
        int startArea = await GetMapArea(manager, start);
        int goalArea = await GetMapArea(manager, goal);
        if(TryGetWalkPathCache(manager, startArea, goalArea, out LinkedList<MapTile> previousPath, out float timer)) {
            if((previousPath.Count > WALK_PATH_RESET_COUNT) && (Time.time - timer < WALK_PATH_RESET_TIMER)) {
                int tileCount = 0;
                bool tileBlocked = false;
                Tilemap tilemap = GameManager.GetTileMap(start.Item1);
                foreach(MapTile tile in previousPath) {
                    int x = tile.Item2, y = tile.Item3;
                    if(IsPositionBlocked(manager, tilemap, true, x, y)) {
                        tileBlocked = true;
                    }
                    if(++tileCount >= WALK_PATH_BLOCK_COUNT || tileBlocked) break;
                }
                int refreshTileCount;
                if(tileBlocked) {
                    refreshTileCount = WALK_PATH_BLOCK_COUNT;
                }
                else {
                    refreshTileCount = WALK_PATH_DEFAULT_COUNT;
                }
                for(int i = 0; i < refreshTileCount; ++i) {
                    previousPath.RemoveFirst();
                }
                MapTile firstTile = previousPath.First();
                LinkedList<MapTile> newPathStart = await GetWalkPathCore(manager, start, firstTile, forceMapPosition);
                if(newPathStart.Count > 0) {
                    newPathStart.RemoveLast();
                }
                while(newPathStart.Count > 0) {
                    previousPath.AddFirst(newPathStart.Last());
                    newPathStart.RemoveLast();
                }
                AddWalkPathCache(manager, startArea, goalArea, previousPath, timer);
                return previousPath;
            }
        }
        timer = Time.time + UnityEngine.Random.Range (0f, 2f);
        LinkedList<MapTile> path = await GetWalkPathCore(manager, start, goal, forceMapPosition);
        AddWalkPathCache(manager, startArea, goalArea, path, timer);
        return path;
    }

    public async Task<LinkedList<MapTile>> GetWalkPathCore(GameManager manager, MapTile start, MapTile goal, bool forceMapPosition = false) {
        await Task.Yield();
        goal = await GetClosestUnblockedPosition(manager, start, goal);
        Tuple<MapTile, MapTile> warpZone = Tuple.Create(start, goal);
        List<MapTile> openSet = new List<MapTile>();
		Dictionary<MapTile, MapTile> cameFrom = new Dictionary<MapTile, MapTile>();
		Dictionary<MapTile, int> gScore = new Dictionary<MapTile, int>();
		Dictionary<MapTile, int> fScore = new Dictionary<MapTile, int>();
		if(start.Item1 == goal.Item1) {
            int startArea = await GetMapArea(manager, start);
            int goalArea = await GetMapArea(manager, goal);
            if(startArea == goalArea) {
                if(!start.Equals(goal)) {
                    openSet.Add(start);
                    fScore[start] = DistanceHeuristic(start, goal);
                    gScore[start] = 0;
                }
            }
            else {
                throw new Exception("Invalid area travel. (" + start + ", " + goal + ")");
            }
		}
        else {
            throw new Exception("Invalid map travel. (" + start + ", " + goal + ")");
        }
        int count = 0;
		while(openSet.Count > 0) {
			int minDistance = Int32.MaxValue;
			MapTile current = openSet.First();
			foreach(MapTile tile in openSet) {
				int currentDistance = fScore[tile];
				if(currentDistance < minDistance) {
					minDistance = currentDistance;
					current = tile;
				}
			}
			openSet.Remove(current);
			if(current.Equals(goal)) {
				return ConstructPath(cameFrom, goal);
			}
            // Find a way to make this loop parallel. Not necessary but it should be faster
			foreach(MapTile tile in TileNeighbours(manager, current, forceMapPosition)) {
                if(tile.Equals(goal) || !IsWarpTile(tile)) {
                    int currentGScore = gScore[current] + 1;
                    if(!gScore.ContainsKey(tile) || currentGScore < gScore[tile]) {
                        cameFrom[tile] = current;
                        fScore[tile] = currentGScore + DistanceHeuristic(tile, goal);
                        gScore[tile] = currentGScore;
                        if(!openSet.Contains(tile)) {
                            openSet.Add(tile);
                        }
                    }
                }
			}
            if(++count%100 == 0) {
                await Task.Yield();
            }
		}
		return new LinkedList<MapTile>();
    }

    private bool TryGetMapPathCache(GameManager manager, int startArea, int goalArea, out Tuple<LinkedList<MapTile>, WarpDevice, int> path) {
        int playerId = manager.GetMainPlayerId();
        var itemToRetrieve = m_playerMapPathCache.SingleOrDefault(pathCache => pathCache.Item1 == playerId);
        if (itemToRetrieve != null) {
            if(itemToRetrieve.Item2 == startArea && itemToRetrieve.Item3 == goalArea) {
                path = itemToRetrieve.Item4;
                return true;
            }
        }
        path = default(Tuple<LinkedList<MapTile>, WarpDevice, int>);
        return false;
    }

    private void AddMapPathCache(GameManager manager, int startArea, int goalArea, Tuple<LinkedList<MapTile>, WarpDevice, int> path) {
        int playerId = manager.GetMainPlayerId();
        var playerPath = Tuple.Create(playerId, startArea, goalArea, path);
        var itemToRemove = m_playerMapPathCache.SingleOrDefault(pathCache => pathCache.Item1 == playerId);
        if (itemToRemove != null) {
            m_playerMapPathCache.Remove(itemToRemove);
        }
        if(startArea != goalArea) {
            m_playerMapPathCache.Add(playerPath);
        }
    }

    private bool TryGetWalkPathCache(GameManager manager, int startArea, int goalArea, out LinkedList<MapTile> path, out float timer) {
        int playerId = manager.GetMainPlayerId();
        var itemToRetrieve = m_playerWalkPathCache.SingleOrDefault(pathCache => pathCache.Item1 == playerId);
        if (itemToRetrieve != null) {
            if(itemToRetrieve.Item2 == startArea && itemToRetrieve.Item3 == goalArea) {
                path = itemToRetrieve.Item4;
                timer = itemToRetrieve.Item5;
                return true;
            }
        }
        path = default(LinkedList<MapTile>);
        timer = Time.time;
        return false;
    }

    private void AddWalkPathCache(GameManager manager, int startArea, int goalArea, LinkedList<MapTile> path, float timer) {
        int playerId = manager.GetMainPlayerId();
        var playerPath = Tuple.Create(playerId, startArea, goalArea, path, timer);
        var itemToRemove = m_playerWalkPathCache.SingleOrDefault(pathCache => pathCache.Item1 == playerId);
        if (itemToRemove != null) {
            m_playerWalkPathCache.Remove(itemToRemove);
        }
        m_playerWalkPathCache.Add(playerPath);
    }

	private async Task<int> GetDistanceToPoint(GameManager manager, MapTile start, MapTile goal) {
        int startArea = await GetMapArea(manager, start);
        int goalArea = await GetMapArea(manager, goal);
        if((start.Item1 == goal.Item1) && !start.Equals(goal) && (startArea == goalArea)) {
            Tuple<MapTile, MapTile> warpZone = Tuple.Create(start, goal);
            if(m_walkDistanceMap.ContainsKey(warpZone)) {
                return m_walkDistanceMap[warpZone];
            }
            else {
                int distance = (await GetWalkPathCore(manager, start, goal, true)).Count;
                if(m_warpEndSet.Contains(start) && m_warpStartSet.Contains(goal)) { // Only cache if it's a path from one warpzone to another
                    m_walkDistanceMap[warpZone] = distance;
                }
                return distance;
            }
        }
        return 0;
	}

	private List<Tuple<WarpDevice, MapTile>> GetWarpSpells() {
		// TODO Verify against spellbook
		return m_warpSpells;
	}

	private List<Tuple<WarpDevice, MapTile>> GetWarpItems() {
		// TODO Verify against inventory
		return m_warpItems;
	}

    public async Task<bool> IsSameArea(GameManager manager, MapTile firstTile, MapTile secondTile) {
        int firstArea = await GetMapArea(manager, firstTile);
        int secondArea = await GetMapArea(manager, secondTile);
        return firstArea == secondArea;
    }

    private async Task<int> GetMapArea(GameManager manager, MapTile mapTile) {
        int map = mapTile.Item1, x = mapTile.Item2, y = mapTile.Item3;
        Tilemap tilemap = GameManager.GetTileMap(map);
        if(IsPositionBlocked(manager, tilemap, false, x, y)) {
            return 0;
        }
        if(m_mapAreaMap.ContainsKey(mapTile)) {
            return m_mapAreaMap[mapTile];
        }
        else {
            await Task.Yield();
            int areaId = Interlocked.Increment(ref m_mapAreaId);
            List<MapTile> openSet = new List<MapTile>();
            openSet.Add(mapTile);
            m_mapAreaMap[mapTile] = areaId;
            int count = 0;
            while(openSet.Count > 0) {
                MapTile current = openSet.First();
                openSet.Remove(current);
                foreach(MapTile tile in TileNeighbours(manager, current, true)) {
                    if(!m_mapAreaMap.ContainsKey(tile)) {
                        openSet.Add(tile);
                        m_mapAreaMap[tile] = areaId;
                    }
                }
                if(++count%100 == 0) {
                    await Task.Yield();
                }
            }
            return areaId;
        }
    }

	private List<MapTile> TileNeighbours(GameManager manager, MapTile moveFrom, bool forceMapPosition = false) {
		List<MapTile> neighbours = new List<MapTile>();
		int map = moveFrom.Item1, x = moveFrom.Item2, y = moveFrom.Item3;
        bool sameMap = (manager.GetMapId() == moveFrom.Item1) && !forceMapPosition;
        Tilemap tilemap = GameManager.GetTileMap(map);
        if(tilemap) {
            for(int i = -1; i <= 1; i++) {
                for(int j = -1; j <= 1; j++) {
                    if(i != j && (i == 0 || j == 0)) {
                        if(!IsPositionBlocked(manager, tilemap, sameMap, x + i, y + j)) {
                            neighbours.Add(Tuple.Create(map, x + i, y + j));
                        }
                    }
                }
            }
        }
		return neighbours;
	}

	private List<Tuple<MapTile, MapTile>> MapNeighbours(MapTile moveFrom, List<Tuple<MapTile, MapTile>> warpZoneList) {
		List<Tuple<MapTile, MapTile>> neighbours = new List<Tuple<MapTile, MapTile>>();
		foreach(Tuple<MapTile, MapTile> warpZone in warpZoneList) {
			MapTile warpFrom = warpZone.Item1;
			if(warpFrom.Item1 == moveFrom.Item1) {
				neighbours.Add(warpZone);
			} 
		}
		return neighbours;
	}

	public int DistanceHeuristic(MapTile moveFrom, MapTile moveTo) {
		int xDiff = moveFrom.Item2 - moveTo.Item2;
		int yDiff = moveFrom.Item3 - moveTo.Item3;
		return Mathf.Abs(xDiff) + Mathf.Abs(yDiff);
	}

    private bool IsPositionBlocked(GameManager manager, Tilemap tilemap, bool sameMap, int x, int y) {
        if(sameMap) {
            return manager.IsWorldPositionBlocked(x, y);
        }
        else {
            return GameManager.IsMapPositionBlocked(tilemap, x, y);
        }
    }

    private bool IsWarpTile(MapTile tile) {
        return m_warpStartSet.Contains(tile);
    }

    private async Task<MapTile> GetClosestUnblockedPosition(GameManager manager, MapTile start, MapTile goal) {
        int map = goal.Item1;
        Tilemap tilemap = GameManager.GetTileMap(map);
        bool sameMap = manager.GetMapId() == map;
        if(tilemap != null && IsPositionBlocked(manager, tilemap, sameMap, goal.Item2, goal.Item3)) {
            Queue<MapTile> neighbours = new Queue<MapTile>();
            Queue<MapTile> unblockedTiles = new Queue<MapTile>();
            int goalArea = await GetMapArea(manager, goal);
            neighbours.Enqueue(goal);
            while(neighbours.Count() > 0) {
                MapTile current = neighbours.Dequeue();
                int x = current.Item2, y = current.Item3;
                for(int i = -1; i <= 1; i++) {
                    for(int j = -1; j <= 1; j++) {
                        if(i != j && (i == 0 || j == 0)) {
                            if(x + i > 0 && x + i <= 100 && y + j > 0 && y + j <= 100) {
                                MapTile newTile = Tuple.Create(map, x + i, y + j);
                                int tileArea = await GetMapArea(manager, current);
                                if(!IsPositionBlocked(manager, tilemap, sameMap, x + i, y + j) && !IsWarpTile(newTile)) {
                                    if((goalArea == 0) || (goalArea == tileArea)) {
                                        unblockedTiles.Enqueue(newTile);
                                    }
                                }
                                else if(unblockedTiles.Count == 0){
                                    if(!neighbours.Contains(newTile)) {
                                        neighbours.Enqueue(newTile);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            int minDistance = Int32.MaxValue;
            MapTile closestTile = goal;
            while(unblockedTiles.Count > 0) {
                MapTile current = unblockedTiles.Dequeue();
                int currentDistance = DistanceHeuristic(start, current);
                if(currentDistance < minDistance) {
                    minDistance = currentDistance;
                    closestTile = current;
                }
            }
            return closestTile;
        }
        return goal;
    }

	private LinkedList<MapTile> ConstructPath(Dictionary<MapTile, MapTile> cameFrom, MapTile current) {
		LinkedList<MapTile> path = new LinkedList<MapTile>();
        if(current != null) {
            path.AddFirst(current);
            while(cameFrom.ContainsKey(current)) {
                current = cameFrom[current];
                path.AddFirst(current);
            }
        }
		return path;
	}
}
