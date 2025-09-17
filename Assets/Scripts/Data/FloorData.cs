using System;
using System.Collections.Generic;

[Serializable]
public class FloorData
{
	public int floorId;
	public bool isUnlocked;
	public List<LevelData> levels = new List<LevelData>();
}


