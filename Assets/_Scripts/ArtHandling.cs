using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System;


public class ArtHandling : MonoBehaviourPunCallbacks
{
    //Variables

    //Isaks functions
    public void writeRegistry(string filepath, string artist, string exhibit, string artname, string id) {
        DirectoryInfo directory = new DirectoryInfo(".");
        string registry = "registry.txt";
        string s =  artname + "$$" + filepath + "$$" + artist + "$$" + exhibit; 
        File.AppendAllText(registry, s + Environment.NewLine);
    }


    public string[] getContents(string filename) {
        var lines = File.ReadAllLines(filename);
        int numberOfLines = 0;

        foreach (var line in lines) {
            numberOfLines += 1;
        }

        string[] results = new string[numberOfLines*4];


        int index = 0;
        foreach (var line in lines) {
            string[] lineContent = line.Split(new string[] { "$$" }, StringSplitOptions.None);
            results[index] = lineContent[0];
            results[index+1] = lineContent[1];
            results[index+2] = lineContent[2];
            results[index+3] = lineContent[3];
            index = index + 4;
        }

        return results;
    }

    //Other Functions
    public void ExportAssets()
    {
        PhotonView pv = PhotonView.Get(this);
        string path = @"C:\Users\tompa\Pictures\Freddie01.jpg";
        byte[] byteArray = File.ReadAllBytes(path);
        pv.RPC("AssetRPC", RpcTarget.OthersBuffered, byteArray, "Tavla1", "Freddie01.jpg");
    }

    [PunRPC]
    void AssetRPC(byte[] file, string pos, string filename)
    {
        Debug.Log("Här kommer Data!!!");
        string pathRoot = "";
        File.WriteAllBytes(pathRoot + filename, file);
        Sprite MySprite = IMG2Sprite.instance.LoadNewSprite(pathRoot + filename);
        Debug.Log("Sprite Loaded!");
        GameObject sprite = GameObject.Find(pos);
        sprite.GetComponent<SpriteRenderer>().sprite = MySprite;
    }
}