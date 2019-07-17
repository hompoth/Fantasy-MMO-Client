using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Drawing;
using UnityEngine;

public class SpriteGeneration : MonoBehaviour
{
    void Start()
    {
        //GenerateAnimationList();
        //GenerateSprites();
        //RemoveBlackBackground();
        //SearchForSpriteId("*.enc", true, true);
        SearchForSpriteId("*.adf", true, false);
    }

    void SearchForSpriteId(string fileExt, bool ignoreOffset, bool debug){
        string separator = Path.DirectorySeparatorChar.ToString();
        string dataPath = Path.GetFullPath("Assets" + separator + "BinaryFiles" + separator + "Data");
        string spritePath = Path.GetFullPath("Assets" + separator + "Sprites");
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
            
            for (int j = 3; j < bytes.Length; j++)
            {
                byte[] arr = {bytes[j-3], bytes[j-2], bytes[j-1], bytes[j]};
                int val = ApplyOffsetByte(bytes[j], offset);//ApplyOffset(BitConverter.ToInt32(arr, 0), offset);
                //val = BitConverter.ToInt32(arr, 0);
                //if (val >= 65140 && val <= 65500) {
                //if ((val >= 96053 && val <= 96053) || (val == 120002)) {
                //if ((val >= 96050 && val <= 96060)) {
                //if ((val >= 96000 && val <= 96050)) {
                //if (val >= 41528 && val <= 41547) {
                //if(val >= 96050 && val <= 96059){
                //if(val >= 96003 && val <= 96003){
                if(val >= 4 && val <= 4){
                    
                    if(debug) {
                        Debug.Log("Previous Byte: " + (ApplyOffsetByte(bytes[j-20], offset)));
                        Debug.Log("Previous Byte: " + (ApplyOffsetByte(bytes[j-19], offset)));
                        Debug.Log("Previous Byte: " + (ApplyOffsetByte(bytes[j-18], offset)));
                        Debug.Log("Previous Byte: " + (ApplyOffsetByte(bytes[j-17], offset)));
                        Debug.Log("Previous Byte: " + (ApplyOffsetByte(bytes[j-16], offset)));
                        Debug.Log("Previous Byte: " + (ApplyOffsetByte(bytes[j-15], offset)));
                        Debug.Log("Previous Byte: " + (ApplyOffsetByte(bytes[j-14], offset)));
                        Debug.Log("Previous Byte: " + (ApplyOffsetByte(bytes[j-13], offset)));
                        Debug.Log("Previous Byte: " + (ApplyOffsetByte(bytes[j-12], offset)));
                        Debug.Log("Previous Byte: " + (ApplyOffsetByte(bytes[j-11], offset)));
                        Debug.Log("Previous Byte: " + (ApplyOffsetByte(bytes[j-10], offset)));
                        Debug.Log("Previous Byte: " + (ApplyOffsetByte(bytes[j-9], offset)));
                        Debug.Log("Previous Byte: " + (ApplyOffsetByte(bytes[j-8], offset)));
                        Debug.Log("Previous Byte: " + (ApplyOffsetByte(bytes[j-7], offset)));
                        Debug.Log("Previous Byte: " + (ApplyOffsetByte(bytes[j-6], offset)));
                        Debug.Log("Previous Byte: " + (ApplyOffsetByte(bytes[j-5], offset)));
                        Debug.Log("Previous Byte: " + (ApplyOffsetByte(bytes[j-4], offset)));
                        {
                            int joffset = -4;
                            byte[] arr2 = {bytes[j+joffset-3], bytes[j+joffset-2], bytes[j+joffset-1], bytes[j+joffset]};
                            int val2 = ApplyOffset(BitConverter.ToInt32(arr2, 0), offset);
                            Debug.Log("Previous INT"+ val2);
                        }  
                        
                        for (int x=-1000; x<100; x++){
                            int joffset = 2 + 6*(x-1) + x*8*4*4;
                            if(j+joffset > bytes.Length || j+joffset-9 < 0) continue;
                            Debug.Log("---------------------");
                            
                            {
                                byte[] arr2 = {bytes[j+joffset-21], bytes[j+joffset-20], bytes[j+joffset-19], bytes[j+joffset-18]};
                                int val2 = ApplyOffset(BitConverter.ToInt32(arr2, 0), offset);
                                Debug.Log("X: "+x+" Previous INT: "+ val2);
                            }
                            {
                                byte[] arr2 = {bytes[j+joffset-17], bytes[j+joffset-16], bytes[j+joffset-15], bytes[j+joffset-14]};
                                int val2 = ApplyOffset(BitConverter.ToInt32(arr2, 0), offset);
                                Debug.Log("X: "+x+" Previous INT: "+ val2);
                            }
                            {
                                byte[] arr2 = {bytes[j+joffset-13], bytes[j+joffset-12], bytes[j+joffset-11], bytes[j+joffset-10]};
                                int val2 = ApplyOffset(BitConverter.ToInt32(arr2, 0), offset);
                                Debug.Log("X: "+x+" Previous INT: "+ val2);
                            }
                            {
                                byte[] arr2 = {bytes[j+joffset-9], bytes[j+joffset-8], bytes[j+joffset-7], bytes[j+joffset-6]};
                                int val2 = ApplyOffset(BitConverter.ToInt32(arr2, 0), offset);
                                Debug.Log("X: "+x+" Previous INT: "+ val2);
                            }
                            Debug.Log("Previous Byte3: " + (ApplyOffsetByte(bytes[j+joffset-5], offset)));
                            Debug.Log("Previous Byte4: " + (ApplyOffsetByte(bytes[j+joffset-4], offset)));
                            {
                                byte[] arr2 = {bytes[j+joffset-3], bytes[j+joffset-2], bytes[j+joffset-1], bytes[j+joffset]};
                                int val2 = ApplyOffset(BitConverter.ToInt32(arr2, 0), offset);
                                Debug.Log("X: "+x+" joffset: "+(j+joffset-3)+" Previous INT-- "+ val2);
                            }
                        }       
                    }        
                    Debug.Log("First: " + Path.GetFileName(file) + " " + j + " " + val);
                    continue;

                    Debug.Log("--------");
                    for (int z = 0; z < 40; z++)
                    {
                        int joffset = -4;//-10*4*4 - 6;
                        byte[] arr2 = {bytes[j+1+joffset+z*4], bytes[j+2+joffset+z*4], bytes[j+3+joffset+z*4], bytes[j+4+joffset+z*4]};
                        int val2 = ApplyOffset(BitConverter.ToInt32(arr2, 0), offset);
                        Debug.Log(val2);
                    }
                    /*
                    Debug.Log("Separating Byte 1: " + (ApplyOffsetByte(bytes[j+1+3*4], offset)));
                    Debug.Log("Separating Byte 2: " + (ApplyOffsetByte(bytes[j+2+3*4], offset)));
                    for (int z = 3; z < 60; z++)
                    {
                        int joffset = 2;
                        byte[] arr2 = {bytes[j+1+joffset+z*4], bytes[j+2+joffset+z*4], bytes[j+3+joffset+z*4], bytes[j+4+joffset+z*4]};
                        int val2 = ApplyOffset(BitConverter.ToInt32(arr2, 0), offset);
                        Debug.Log(val2);
                    }*/


                    continue;
                    Debug.Log("--------");
                    int animSize = ApplyOffsetByte(bytes[j+1],offset);
                    Debug.Log("Animation Size: " + animSize);
                    
                    for (int z = 0; z < animSize; z++)
                    {
                        int joffset = 1;
                        byte[] arr2 = {bytes[j+1+joffset+z*4], bytes[j+2+joffset+z*4], bytes[j+3+joffset+z*4], bytes[j+4+joffset+z*4]};
                        int val2 = ApplyOffset(BitConverter.ToInt32(arr2, 0), offset);
                        Debug.Log(val2);
                    }
                    if (j+2+animSize*4 < bytes.Length){
                        Debug.Log("After: " + ApplyOffsetByte(bytes[j+2+animSize*4],offset));
                    } 
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

    void GenerateAnimationList() {
        string separator = Path.DirectorySeparatorChar.ToString();
        string file = "Assets" + separator + "BinaryFiles" + separator + "Data" + separator + "0.adf";
        byte[] bytes = File.ReadAllBytes(file);
        StreamWriter writer = new StreamWriter("Assets" + separator + "Metadata" + separator + "AnimationMetadata.txt");

        byte offset = bytes[bytes.Length - 2];
        int count = 0;
        
        for (int j = 1118; j < bytes.Length; count++)
        {
            byte[] arr = {bytes[j-3], bytes[j-2], bytes[j-1], bytes[j]};
            int val = ApplyOffset(BitConverter.ToInt32(arr, 0), offset);
            int animSize = ApplyOffsetByte(bytes[j+1],offset);

            writer.WriteLine("Val: {0} Animation Size: {1}", val, animSize);
            
            for (int z = 0; z < animSize; z++)
            {
                byte[] arr2 = {bytes[j+2+z*4], bytes[j+3+z*4], bytes[j+4+z*4], bytes[j+5+z*4]};
                int val2 = ApplyOffset(BitConverter.ToInt32(arr2, 0), offset);
                writer.WriteLine("{0}", val2);
            }
            if (j+2+animSize*4 < bytes.Length){
                int delimiter = ApplyOffsetByte(bytes[j+2+animSize*4],offset);
                if (delimiter == 4 || delimiter == 3 || delimiter == 5) {
                    //if(delimiter != 4) Debug.Log("------------------------------------------" + delimiter);
                    j+=2+animSize*4 + 4;
                }
                else {
                    writer.WriteLine("Value: {0} End #{1} of {2} Count: {3}", delimiter, j, bytes.Length, count);
                    return;
                }
            }
            else {
                writer.WriteLine("End #{0} of {1} Count: {2}", j, bytes.Length, count);
                return;
            } 
        }
    }

    void GenerateSprites()
    {
        string separator = Path.DirectorySeparatorChar.ToString();
        string dataPath = Path.GetFullPath("Assets" + separator + "BinaryFiles" + separator + "Data");
        string spritePath = Path.GetFullPath("Assets" + separator + "Sprites");

        foreach (string file in Directory.GetFiles(dataPath, "*.adf"))
        {
            byte[] bytes = File.ReadAllBytes(file);

            // so the last - number.
            // but that doesn't matter because the second to last byte is *always?* the offset number anyways!
            byte offset = bytes[bytes.Length - 2];

            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            StreamWriter writer = new StreamWriter(spritePath + separator +Path.GetFileName(file) + "-header.txt");

            byte fileType = reader.ReadByte();
            int extraLength = reader.ReadInt32() + 1;
            writer.WriteLine("Type: {0}, Extra Length: {1}", fileType, extraLength);
            // not sure what these extra bytes are
            writer.Write("Extra Bytes: ");

            // Rw all but last extra bytes 
            for (int i = 0; i < extraLength - 1; i++) writer.Write("{0,4}", ApplyOffsetByte(reader.ReadByte(), offset));

            // If this byte is zero, old code works. Else we need to offset the offset with the difference between 100 and the number of frames.
            byte lastExtraByte = ApplyOffsetByte(reader.ReadByte(), offset);

            writer.Write("{0,4}", lastExtraByte);

            writer.WriteLine();



            int numberOfFrames = ApplyOffset(reader.ReadInt32(), offset);

            if (lastExtraByte != 0)
            {
                offset = (byte)ApplyOffset(offset, (100 - numberOfFrames));
                writer.WriteLine("Number of Frames: {0}, Offset: {1}", numberOfFrames, offset);
                numberOfFrames = 100;
            }
            
            /*for (int j = 3; j < bytes.Length; j++)
            {
                byte[] arr = {bytes[j-3], bytes[j-2], bytes[j-1], bytes[j]};
                int val = ApplyOffset(BitConverter.ToInt32(arr, 0), offset);
                if (val >= 115000 && val <= 115050) {
                    Debug.Log("Second: " + Path.GetFileName(file) + " " + j + " " + val);
                }
            }*/

            writer.WriteLine("NumberOfFrames: {0}", numberOfFrames);


            for (int i = 0; i < numberOfFrames; i++)
            {
                int frameId = ApplyOffset(reader.ReadInt32(), offset);
                // always 1 as far as i have seen
                byte unknownByte = ApplyOffsetByte(reader.ReadByte(), offset);
                int x = ApplyOffset(reader.ReadInt32(), offset);
                int y = ApplyOffset(reader.ReadInt32(), offset);
                int width = ApplyOffset(reader.ReadInt32(), offset);
                int height = ApplyOffset(reader.ReadInt32(), offset);
                
                writer.WriteLine("Id: {0,4} U: {1,4} X: {2,4} Y: {3,4} W: {4,4} H: {5,4}", frameId, unknownByte, x, y, width, height);
            }

            // not sure what this is, 36 = wav though i think
            int unknown = ApplyOffset(reader.ReadInt32(), offset);
            writer.WriteLine("U: {0}", unknown);

            writer.Close();

            int length = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
            byte[] buffer = reader.ReadBytes(length);
            byte[] data = new byte[RealSize(buffer.Length, 0x315)];
            for (int k = 0; k < buffer.Length; k++)
            {
                data[k - (k / 790)] = ApplyOffsetByte(buffer[k], offset);
            }

            string ext = ".bmp";
            if (unknown == 36) ext = ".wav";

            string fileName = Path.GetFileName(file);
            //Debug.Log(fileName + " : " + unknown);
            File.WriteAllBytes(spritePath + separator + fileName.Substring(0, fileName.IndexOf('.')) + ext, data);
        }
        Debug.Log("End");
    }

    public static void SaveTextureAsPNG(Texture2D _texture, string _fullPath) {
        byte[] _bytes =_texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullPath, _bytes);
        Debug.Log(_bytes.Length/1024  + "Kb was saved as: " + _fullPath);
    }

    void RemoveBlackBackground(Texture2D texture) {
        //var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        //GetComponent<Renderer>().material.mainTexture = texture;

        // RGBA32 texture format data layout exactly matches Color32 struct
        var data = texture.GetRawTextureData<Color32>();

        // fill texture data with a simple pattern
        Color32 orange = new Color32(255, 165, 0, 255);
        Color32 teal = new Color32(0, 128, 128, 255);
        int index = 0;
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                //if(data[index] == 255,255,255,0)
                data[index] = ((x & y) == 0 ? orange : teal);
                index++;
            }
        }
        // upload to the GPU
        texture.LoadRawTextureData(data);
        texture.Apply();
    }
}
