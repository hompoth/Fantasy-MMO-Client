using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapTile = System.Tuple<int, int, int>;
using WarpDevice = System.Tuple<string, int>;

public class AutoControllerState
{
	bool m_active;
	int m_warpSpellPriority, m_warpItemPriority;
	List<Tuple<MapTile, MapTile>> m_warpZones;
	List<Tuple<WarpDevice, MapTile>> m_warpSpells;
	List<Tuple<WarpDevice, MapTile>> m_warpItems;

	public AutoControllerState() {
		m_warpZones = UserPrefs.GetWarpZones();
		m_warpSpells = UserPrefs.GetWarpSpells();
		m_warpItems = UserPrefs.GetWarpItems();
		m_warpSpellPriority = 0; // TODO UserPrefs.GetWarpSpellPriority();
		m_warpItemPriority = 0; // TODO UserPrefs.GetWarpItemPriority();
	}

	// TODO Move to AutoTask and accept an AutoControllerState parameter
	public LinkedList<MapTile> GetWalkPath(GameManager manager, MapTile start, MapTile goal) {
		List<MapTile> openSet = new List<MapTile>();
		Dictionary<MapTile, MapTile> cameFrom = new Dictionary<MapTile, MapTile>();
		Dictionary<MapTile, int> gScore = new Dictionary<MapTile, int>();
		Dictionary<MapTile, int> fScore = new Dictionary<MapTile, int>();
		if(start.Item1 == goal.Item1 && !start.Equals(goal)) {
			openSet.Add(start);
			fScore[start] = DistanceHeuristic(start, goal);
			gScore[start] = 0;
		}
		float timer = Time.time;
		while(openSet.Count > 0) {
			if(Time.time - timer > 5f || openSet.Count > 500) { 
				Debug.Log("GetWalkPath " + openSet.Count);
				break;
			}
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
			foreach(MapTile tile in TileNeighbours(manager, current)) {
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
		return new LinkedList<MapTile>();
	}

	// TODO Move to AutoTask and accept an AutoControllerState parameter
	// TODO Add caching
	public LinkedList<MapTile> GetMapPath(GameManager manager, MapTile start, MapTile goal, out WarpDevice warpDevice) {
		List<Tuple<WarpDevice,MapTile>> openSet = new List<Tuple<WarpDevice,MapTile>>();
		Dictionary<MapTile, MapTile> cameFrom = new Dictionary<MapTile, MapTile>();
		Dictionary<MapTile, int> gScore = new Dictionary<MapTile, int>();
		warpDevice = default(WarpDevice);
		if(!start.Equals(goal)) {
			openSet.Add(Tuple.Create(warpDevice, start));
			gScore[start] = 0;
			foreach(Tuple<WarpDevice, MapTile> warpSpell in GetWarpSpells()) {
				openSet.Add(warpSpell);
				gScore[warpSpell.Item2] = m_warpSpellPriority;
			}
			foreach(Tuple<WarpDevice, MapTile> warpItem in GetWarpItems()) {
				openSet.Add(warpItem);
				gScore[warpItem.Item2] = m_warpItemPriority;
			}
		}
		float timer = Time.time;
		while(openSet.Count > 0) {
			if(Time.time - timer > 5f || openSet.Count > 500) { 
				Debug.Log("GetMapPath " + openSet.Count);
				break;
			}
			int minDistance = Int32.MaxValue;
			Tuple<WarpDevice,MapTile> current = openSet.First();
			foreach(Tuple<WarpDevice,MapTile> warp in openSet) {
				int currentDistance = gScore[warp.Item2];
				if(currentDistance < minDistance) {
					minDistance = currentDistance;
					current = warp;
				}
			}
			WarpDevice currentWarpDevice = current.Item1;
			MapTile currentTile = current.Item2;
			openSet.Remove(current);
			if(currentTile.Equals(goal)) {
				warpDevice = currentWarpDevice;
				return ConstructPath(cameFrom, goal);
			}
			List<Tuple<MapTile, MapTile>> neighbours = MapNeighbours(currentTile);
			if(currentTile.Item1 == goal.Item1) {
				neighbours.Add(Tuple.Create(goal, goal));
			}
			foreach(Tuple<MapTile, MapTile> neighbour in neighbours) {
				MapTile tile = neighbour.Item1;
				MapTile warpedTile = neighbour.Item2;
				if(TryGetDistanceToPoint(manager, currentTile, tile, out int distance)) {
					int currentGScore = gScore[currentTile] + distance;
					if(!gScore.ContainsKey(warpedTile) || currentGScore < gScore[warpedTile]) {
						cameFrom[tile] = currentTile;
						gScore[warpedTile] = currentGScore;
						Tuple<WarpDevice, MapTile> neighbourWarp = Tuple.Create(currentWarpDevice, warpedTile);
						if(!openSet.Contains(neighbourWarp)) {
							openSet.Add(neighbourWarp);
						}
					}
				}
			}
		}
		return new LinkedList<MapTile>();
	}

	private bool TryGetDistanceToPoint(GameManager manager, MapTile start, MapTile goal, out int count) {
		count = GetWalkPath(manager, start, goal).Count;
		if(count == 0 && !start.Equals(goal)) {
			return false;
		}
		return true;
	}

	private List<Tuple<WarpDevice, MapTile>> GetWarpSpells() {
		// TODO Verify against spellbook
		return m_warpSpells;
	}

	private List<Tuple<WarpDevice, MapTile>> GetWarpItems() {
		// TODO Verify against inventory
		return m_warpItems;
	}

	private List<MapTile> TileNeighbours(GameManager manager, MapTile moveFrom) {
		List<MapTile> neighbours = new List<MapTile>();
		int map = moveFrom.Item1, x = moveFrom.Item2, y = moveFrom.Item3;
		if(!manager.IsWorldPositionBlocked(x - 1, y)) {
			neighbours.Add(Tuple.Create(map, x - 1, y));
		}
		if(!manager.IsWorldPositionBlocked(x + 1, y)) {
			neighbours.Add(Tuple.Create(map, x + 1, y));
		}
		if(!manager.IsWorldPositionBlocked(x, y - 1)) {
			neighbours.Add(Tuple.Create(map, x, y - 1));
		}
		if(!manager.IsWorldPositionBlocked(x, y + 1)) {
			neighbours.Add(Tuple.Create(map, x, y + 1));
		}
		return neighbours;
	}

	private List<Tuple<MapTile, MapTile>> MapNeighbours(MapTile moveFrom) {
		List<Tuple<MapTile, MapTile>> neighbours = new List<Tuple<MapTile, MapTile>>();
		foreach(Tuple<MapTile, MapTile> warpZone in m_warpZones) {
			MapTile warpFrom = warpZone.Item1;
			if(warpFrom.Item1 == moveFrom.Item1) {
				neighbours.Add(warpZone);
			} 
		}
		return neighbours;
	}

	private int DistanceHeuristic(MapTile moveFrom, MapTile moveTo) {
		int xDiff = moveFrom.Item2 - moveTo.Item2;
		int yDiff = moveFrom.Item3 - moveTo.Item3;
		return Mathf.Abs(xDiff) + Mathf.Abs(yDiff);
	}

	private LinkedList<MapTile> ConstructPath(Dictionary<MapTile, MapTile> cameFrom, MapTile current) {
		LinkedList<MapTile> path = new LinkedList<MapTile>();
		path.AddFirst(current);
		while(cameFrom.ContainsKey(current)) {
			current = cameFrom[current];
			path.AddFirst(current);
		}
		return path;
	}

	// Cache value?
	public bool IsSurrounded(GameManager gameManager) {
		return gameManager.MainPlayerIsSurrounded();
	}

	public bool IsActive() {
		return m_active;
	}

	public void SetActive(bool active) {
		m_active = active;
	}
}
