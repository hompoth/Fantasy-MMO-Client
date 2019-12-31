#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor;
using UnityEngine.Tilemaps;
using B83.Image.BMP;

public class GameObjectGeneration : MonoBehaviour
{
    public Tilemap layer1;
    public Tilemap layer2;
    public Tilemap layer3;
    public Tilemap layer4;
    public Tilemap blockedTiles;

    public int MAP_WIDTH = 100;
    public int MAP_HEIGHT = 100;

    void Start()
    {
        if (EditorUtility.DisplayDialog("Warning: File Generation", 
            "This will overwrite existing sprites and animations."
            + " Continuing could reset any changes to animation speeds."
            + " Would you like to continue?", "Yes", "No"))
        {
            //SearchForSpriteId("*.enc", true, true);
            //SearchForSpriteId("*.adf", false, false);
            //ConvertSpritesToPNG(false);
            
            GenerateSpritesAndAnimations(false, false);
            //GenerateAnimationGroups();
            //GenerateMaps();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    void SearchForSpriteId(string fileExt, bool ignoreOffset, bool debug){
        string separator = Path.DirectorySeparatorChar.ToString();
        string dataPath = Path.GetFullPath("Assets" + separator + "BinaryFiles" + separator + "Data");
        Debug.Log("Search for Sprite");

        foreach (string file in Directory.GetFiles(dataPath, fileExt))
        {
            byte[] bytes = File.ReadAllBytes(file);

            // so the last - number.
            // but that doesn't matter because the second to last byte is *always?* the offset number anyways!
            byte offset = bytes[bytes.Length - 2];

            byte tmp;

            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));

            byte fileType = reader.ReadByte();
            int extraLength = reader.ReadInt32() + 1;

            // Rw all but last extra bytes 
            for (int i = 0; i < extraLength - 1; i++) tmp=reader.ReadByte();

            // If this byte is zero, old code works. Else we need to offset the offset with the difference between 100 and the number of frames.
            byte lastExtraByte = ApplyOffsetByte(reader.ReadByte(), offset);



            int numberOfFrames = ApplyOffset(reader.ReadInt32(), offset);

            if (lastExtraByte != 0)
            {
                offset = (byte)ApplyOffset(offset, (100 - numberOfFrames));
            }

            if(ignoreOffset){
                offset = 0;
            }
            
            for (int j = 3; j < bytes.Length; j++) // 
            {
                byte[] arr = {bytes[j-3], bytes[j-2], bytes[j-1], bytes[j]};
                int val = ApplyOffset(BitConverter.ToInt32(arr, 0), offset);

                //if(val >= 110012 && val <= 110012){ // remove
                if(val >= 100000 && val <= 100000){ // remove
                //if(val >= 96053 && val <= 96053){ // Use for animation groupers
                //if(val >=120280&&val<=120280){ // Scratch Icon ID
                //if(val >=2145&&val<=2145){ // Unknown int for file 30002
                //if(val >=30002&&val<=30002){ //File ID
                //if(val >=3080195&&val<=3080195){ //Grouper ID?
                
                //if((val == 3 || val22 == 3 || val3 == 3 || val4 == 3)&&(val == 47 || val22 == 47 || val3 == 47 || val4 == 47)){ //Grouper ID bytes
                //if((val == 3)&&(val3 == 47)){ //Grouper ID bytes
                    {
                        //byte[] arr2 = {bytes[j-3], bytes[j-2], bytes[j-1], bytes[j]};
                        //int val2 = ApplyOffset(BitConverter.ToInt32(arr2, 0), offset);
                        //Debug.Log("X: "+j+" Value: "+ val2);
                    }
                    if(debug) { 
                        
                              
                    }        
                    Debug.Log("First: " + Path.GetFileName(file) + " " + j + " " + val);
                }
            }
        }
    }

    int ApplyOffset(int data, int offset)
    {
        return (data - offset);
    }

    byte ApplyOffsetByte(byte data, int offset)
    {
        if (offset > data)
        {
            data = (byte)(data + (0x100 - offset));
            return data;
        }
        data = (byte)(data - offset);
        return data;
    }

    int RealSize(int datasize, int chunksize)
    {
        return (datasize - (datasize / (chunksize + 1)));
    }

    enum AnimType {Body=1, Face, Weapon, Top, Helm, Bottom, Shoes};

    void GenerateAnimationGroups()
    {
        string separator = Path.DirectorySeparatorChar.ToString();
        string dataPath = Path.GetFullPath("Assets" + separator + "BinaryFiles" + separator + "Data");
        string groupAnimationsPath = "Assets" + separator + "Resources" + separator + "Animations" + separator + "Groupings" + separator;

        foreach (string file in Directory.GetFiles(dataPath, "*.enc"))
        {
            byte[] bytes = File.ReadAllBytes(file);

            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            StreamWriter writer = new StreamWriter("Assets" + separator + "Metadata" + separator + "AnimationGrouperMetadata.txt");

            while (reader.BaseStream.Position != reader.BaseStream.Length) {
                int animationType = reader.ReadInt16();
                int animationIndex = reader.ReadInt32();
                AnimGroup animGroup = ScriptableObject.CreateInstance<AnimGroup>();
                writer.WriteLine("Type: {0}, Index: {1}", animationType, animationIndex);
                foreach (AnimDirectionType animDirectionType in (AnimDirectionType[]) Enum.GetValues(typeof(AnimDirectionType)))
                {  
                    foreach (AnimAttackType animAttackType in (AnimAttackType[]) Enum.GetValues(typeof(AnimAttackType))) {
                        int animationId = reader.ReadInt32();
                        writer.WriteLine("Anim: {0} animDirectionType: {1} animAttackType: {2}", animationId, (AnimDirectionType) animDirectionType, (AnimAttackType) animAttackType);
                        animGroup.AddAnim(animationId, animDirectionType, animAttackType);
                    }
                }
                string path = "";
                switch ((AnimType) animationType)
                {
                    case AnimType.Body:
                        path = groupAnimationsPath + "Body" + separator;
                        break;
                    case AnimType.Face:
                        path = groupAnimationsPath + "Face" + separator;
                        break;
                    case AnimType.Weapon:
                        path = groupAnimationsPath + "Weapon" + separator;
                        break;
                    case AnimType.Top:
                        path = groupAnimationsPath + "Top" + separator;
                        break;
                    case AnimType.Helm:
                        path = groupAnimationsPath + "Helm" + separator;
                        break;
                    case AnimType.Bottom:
                        path = groupAnimationsPath + "Bottom" + separator;
                        break;
                    case AnimType.Shoes:
                        path = groupAnimationsPath + "Shoes" + separator;
                        break;
                }
                if(!String.IsNullOrEmpty(path) && animGroup != null){
                    AssetDatabase.CreateAsset(animGroup, path + animationIndex + ".asset");
                }
            }
            writer.Close();
        }
        Debug.Log("End");
    }

    void CreateAnimation(string path, string animationId, Sprite[] animSprites, int sampleRate, bool isLooping) {
        AnimationClip anim = AnimClipBuilder.CreateClip(animSprites, animationId, 8, false);
        EditorUtility.SetDirty(anim);
        AssetDatabase.CreateAsset(anim, path + animationId + ".anim");
    }

    void ConvertSpritesToPNG(bool removeBlackBackground){
        string separator = Path.DirectorySeparatorChar.ToString();
        string dataPath = Path.GetFullPath("Assets" + separator + "Maisemore");
        foreach (string file in Directory.GetFiles(dataPath, "*.bmp"))
        {
            string fileName = Path.GetFileName(file);
            byte[] data = File.ReadAllBytes(file);
            BMPLoader bmpLoader = new BMPLoader();
            BMPImage bmpImg = bmpLoader.LoadBMP(data);
            if (bmpImg == null) continue;

            Texture2D tex = bmpImg.ToTexture2D();
            if(removeBlackBackground){
                RemoveBlackBackground(tex);
            }
            SaveTextureAsPNG(tex, dataPath + separator + fileName.Substring(0, fileName.IndexOf('.')) + ".png");
        }
    }

    void GenerateSpritesAndAnimations(bool createSprites, bool createAnimations)
    {
        string separator = Path.DirectorySeparatorChar.ToString();
        string dataPath = Path.GetFullPath("Assets" + separator + "BinaryFiles" + separator + "Data");
        string spritePath = Path.GetFullPath("Assets" + separator + "Resources" + separator + "Sprites");
        string compiledFilesPath = Path.GetFullPath("Assets" + separator + "CompiledFiles");
        string animationsPath = "Assets" + separator + "Resources" + separator + "Animations" + separator;
        List<AnimationFrameReferences> animationFrames = new List<AnimationFrameReferences>(); 
        
        foreach (string file in Directory.GetFiles(dataPath, "*.adf"))
        {
            string fileName = Path.GetFileName(file);

            byte[] bytes = File.ReadAllBytes(file);

            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            StreamWriter writer = new StreamWriter(compiledFilesPath + separator + Path.GetFileName(file) + "-header.txt");

            byte fileType = reader.ReadByte();
            int extraLength = reader.ReadInt32() + 1;
            writer.WriteLine("Type: {0}, Extra Length: {1}", fileType, extraLength);

            writer.Write("Extra Bytes: ");

            for (int i = 0; i < extraLength - 1; i++) writer.Write("{0,4}", reader.ReadByte()); // Not sure if offset is needed here

            byte offset = reader.ReadByte();

            writer.WriteLine();

            int numberOfFrames = ApplyOffset(reader.ReadInt32(), offset);

            writer.WriteLine("NumberOfFrames: {0}", numberOfFrames);

            ChildSpriteCoordinates[] childSprites = new ChildSpriteCoordinates[numberOfFrames];
            int totalSprites = 0;
            for (int i = 0; i < numberOfFrames; i++)
            {
                int id = ApplyOffset(reader.ReadInt32(), offset);
                byte animationLength = ApplyOffsetByte(reader.ReadByte(), offset);
                if(animationLength == 1) {
                    int imageId = id;
                    int x = ApplyOffset(reader.ReadInt32(), offset);
                    int y = ApplyOffset(reader.ReadInt32(), offset);
                    int width = ApplyOffset(reader.ReadInt32(), offset);
                    int height = ApplyOffset(reader.ReadInt32(), offset);
                    childSprites[totalSprites++] = new ChildSpriteCoordinates(imageId, x, y, width, height);
                
                    writer.WriteLine("Id: {0,4} X: {1,4} Y: {2,4} W: {3,4} H: {4,4}", imageId, x, y, width, height);
                }
                else {
                    int animationId = id;
                    AnimationFrameReferences animReference = new AnimationFrameReferences(animationId);
                    writer.Write("Animation Id: {0,6} Frame Ids:",animationId);
                    for(int j = 0; j < animationLength; ++j) {
                        int frameId = ApplyOffset(reader.ReadInt32(), offset);
                        animReference.AddFrame(frameId);
                        writer.Write(" {0,6}",frameId);
                    }
                    byte delimiter = ApplyOffsetByte(reader.ReadByte(), offset);
                    animationFrames.Add(animReference);

                    writer.WriteLine(" Delimiter: {0}", delimiter);
                }
            }
            if(numberOfFrames != totalSprites) {
                Debug.Log(fileName + " Frames: "+ numberOfFrames + " Sprites: " + totalSprites);
            }
            if(offset != bytes[bytes.Length - 2]) {    
                Debug.Log(fileName + " Offset: "+ offset + " SecondLastByte: " + bytes[bytes.Length - 2]);
            }
            int unknown = ApplyOffset(reader.ReadInt32(), offset);
            writer.WriteLine("U: {0}", unknown);
            writer.Close();

            if(createSprites) {
                int length = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
                byte[] buffer = reader.ReadBytes(length);
                byte[] data = new byte[RealSize(buffer.Length, 0x315)];
                for (int k = 0; k < buffer.Length; k++)
                {
                    data[k - (k / 790)] = ApplyOffsetByte(buffer[k], offset);
                }
                string compiledFileNamePath = compiledFilesPath + separator + fileName.Substring(0, fileName.IndexOf('.'));
                if(unknown == 36) {
                    File.WriteAllBytes(compiledFileNamePath + ".wav", data);
                    continue;
                }

                BMPLoader bmpLoader = new BMPLoader();
                BMPImage bmpImg = bmpLoader.LoadBMP(data);
                if (bmpImg == null) continue;

                Texture2D tex = bmpImg.ToTexture2D();
                RemoveBlackBackground(tex);
                SaveTextureAsPNG(tex, compiledFileNamePath + ".png");
        
                for (int i = 0; i < totalSprites; ++i) {
                    tex = GetChildSprite(bmpImg.ToTexture2D(), childSprites[i]);
                    RemoveBlackBackground(tex);
                    SaveTextureAsPNG(tex, spritePath + separator + childSprites[i].frameId + ".png");
                }
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if(createAnimations) {
            foreach(AnimationFrameReferences animReference in animationFrames) {
                Sprite[] animSprites = new Sprite[animReference.frameList.Count];
                int spriteIndex = 0;
                foreach(int frameId in animReference.frameList) {
                    animSprites[spriteIndex++] = Resources.Load<Sprite>("Sprites" +separator + frameId); //atlas.GetSprite(spriteId);
                }
                CreateAnimation(animationsPath, animReference.animationId.ToString(), animSprites, 8, true);
            }
        }
        Debug.Log("End");
    }

    Texture2D GetChildSprite(Texture2D spriteSheetTexture, ChildSpriteCoordinates childSprites) {
        int x = childSprites.x, y = childSprites.y, width = childSprites.width, height = childSprites.height;
        Texture2D childSpriteTexture = new Texture2D(width, height);
        Color[] data = spriteSheetTexture.GetPixels(x, spriteSheetTexture.height - y - height, width, height);
        childSpriteTexture.SetPixels(data);
        childSpriteTexture.Apply();
        return childSpriteTexture;
    }

    void RemoveBlackBackground(Texture2D texture) {
        var data = texture.GetRawTextureData<Color32>();

        Color32 color;
        for (int index = 0; index < texture.height * texture.width; index++)
        {
            color = data[index];
            if(color.r == 0 && color.g == 0 && color.b == 0) {
                data[index] =  new Color32(0,0,0,0);
            }
        }
        texture.LoadRawTextureData(data);
        texture.Apply();
    }

    void SaveTextureAsPNG(Texture2D texture, string fullPath) {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(fullPath, bytes);
    }

    void GenerateMaps(){
        string separator = Path.DirectorySeparatorChar.ToString();
        string tilePath = "Assets" + separator + "Resources" + separator + "Tiles";
        string path = Path.GetFullPath("Assets" + separator + "BinaryFiles" + separator + "Maps");
        foreach (string file in Directory.GetFiles(path, "*.map"))
        {
            ClearMap();
            byte[] bytes = File.ReadAllBytes(file);
            
            int line, spriteId;
            bool blocked;
            byte unknown, unknown2, unknown3, unknown4;
            Tile blockedTile = Resources.Load<Tile>("Tiles" + separator + "block");

            // Block top, bottom sides
            for (int x = 0; x <= 101; x++)
            {
                SetMapTile(blockedTiles, x, 0, blockedTile);
                SetMapTile(blockedTiles, x, 101, blockedTile);
            }
            // Block left, right sides
            for (int y = 1; y <= 100; y++)
            {
                SetMapTile(blockedTiles, 0, y, blockedTile);
                SetMapTile(blockedTiles, 101, y, blockedTile);
            }
            // Tile map
            for (int y = 1; y <= 100; y++)
            {
                for (int x = 1; x <= 100; x++)
                {
                    int offset = 0;
                    Tile tile;
                    line = ((y - 1) * 100 + (x - 1)) * 17 + 4;
                    
                    blocked = Convert.ToBoolean(bytes[line + offset++]);
                    if(blocked) {
                        SetMapTile(blockedTiles, x, y, blockedTile);
                    }

                    unknown = bytes[line + offset++];
                    unknown2 = bytes[line + offset++];
                    unknown3 = bytes[line + offset++];
                    unknown4 = bytes[line + offset++];
                    byte[] arr = {unknown, unknown2, unknown3, unknown4};
                    spriteId = BitConverter.ToInt32(arr, 0);
                    tile = FindOrCreateTile(tilePath, spriteId.ToString());
                    if(tile != null) {
                        SetMapTile(layer1, x, y, tile);
                    }

                    unknown = bytes[line + offset++];
                    unknown2 = bytes[line + offset++];
                    unknown3 = bytes[line + offset++];
                    unknown4 = bytes[line + offset++];
                    byte[] arr2 = {unknown, unknown2, unknown3, unknown4};
                    spriteId = BitConverter.ToInt32(arr2, 0);
                    tile = FindOrCreateTile(tilePath, spriteId.ToString());
                    if(tile != null) {
                        SetMapTile(layer2, x, y, tile);
                    }

                    unknown = bytes[line + offset++];
                    unknown2 = bytes[line + offset++];
                    unknown3 = bytes[line + offset++];
                    unknown4 = bytes[line + offset++];
                    byte[] arr3 = {unknown, unknown2, unknown3, unknown4};
                    spriteId = BitConverter.ToInt32(arr3, 0);
                    tile = FindOrCreateTile(tilePath, spriteId.ToString());
                    if(tile != null) {
                        SetMapTile(layer3, x, y, tile);
                    }

                    unknown = bytes[line + offset++];
                    unknown2 = bytes[line + offset++];
                    unknown3 = bytes[line + offset++];
                    unknown4 = bytes[line + offset++];
                    byte[] arr4 = {unknown, unknown2, unknown3, unknown4};
                    spriteId = BitConverter.ToInt32(arr4, 0);
                    tile = FindOrCreateTile(tilePath, spriteId.ToString());
                    if(tile != null) {
                        SetMapTile(layer4, x, y, tile);
                    }
                }
            }
            string fileName = Path.GetFileName(file);
            fileName = fileName.Substring(0, fileName.IndexOf('.'));
            SaveAssetMap(fileName);
        }
    }

    Tile FindOrCreateTile(string path, string spriteName) {
        string separator = Path.DirectorySeparatorChar.ToString();
        Tile tile = Resources.Load<Tile>("Tiles" + separator + spriteName);
        if(tile == null) {
            Sprite sprite = Resources.Load<Sprite>("Sprites" +separator + spriteName);
            if (sprite != null) {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                AssetDatabase.CreateAsset(tile, path + separator + spriteName + ".asset");
            }
        }
        return tile;
    }

    void SetMapTile(Tilemap layer, int x, int y, Tile tile) {
        layer.SetTile(new Vector3Int((x - 1) - (MAP_WIDTH) / 2, -(y - 1) + (MAP_HEIGHT) / 2, 0), tile);
    }

    void ClearMap() {
        layer1.ClearAllTiles();
        layer2.ClearAllTiles();
        layer3.ClearAllTiles();
        layer4.ClearAllTiles();
        blockedTiles.ClearAllTiles();
    }

    void SaveAssetMap(string name) {
        string separator = Path.DirectorySeparatorChar.ToString();
        var mf = GameObject.Find("Grid");
        if (mf) {
            var savePath = "Assets" + separator + "Resources" + separator + "Prefabs" + separator + name.ToLower() + ".prefab";
            if(PrefabUtility.SaveAsPrefabAsset(mf, savePath)) {
                Debug.Log("Tile map "+ name + " was saved under "+savePath+".");
            }
            else {
                Debug.Log("Tile map "+name+" didn't save.");
            }
        }
    }
}

class AnimationFrameReferences {
    public int animationId;
    public List<int> frameList;
    public AnimationFrameReferences(int animationId) {
        frameList = new List<int>();
        this.animationId = animationId;
    }
    public void AddFrame(int frameId) {
        frameList.Add(frameId);
    }
}

class ChildSpriteCoordinates {
    public int frameId;
    public int x, y, width, height;
    public ChildSpriteCoordinates(int frameId, int x, int y, int width, int height) {
        this.frameId = frameId;
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }
}
#endif
