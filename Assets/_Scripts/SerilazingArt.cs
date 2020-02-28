using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System;
using Dummiesman;

// Holds all artwork in the current room.
// Might be a property of the room settings.
// (So something like roomSettings.Manifest)
// Hold complete slot settings or only a subset?
[Serializable]
public class ArtManifest
{
    public List<SlotSettings> Slots { get; }

    public ArtManifest(List<SlotSettings> slots)
    {
        Slots = slots;
    }

}

// Assuming art pieces will be completely identified by their checksum.
// TODO: slots are going to be numbered in the future, not named. Right?
// TODO: Add support for 3D art as well.
// TODO: Check whether RPC methods can be made private.

public class SerilazingArt : MonoBehaviourPunCallbacks
{

    private Dictionary<Checksum,SlotSettings> slotSettings;
    private ArtRegistry artReg;

    // TODO: Take settings as a parameter, or take no parameters
    //       and find the room settings somewhere?
    // Invoked locally on master client from OnJoinedRoom.
    // Distribute full art manifest for the room to all visitors.
    // Perform RPC on all clients when they connect (targeting OthersBuffered).
    private string DeclareRoot() {
        return Application.persistentDataPath;
    }
    public void ExportArt(RoomSettings room)
    {
        // XXX: For testing. Master client has the assets.
        artReg = ArtRegistry.GetArtRegistry();
        //DistributeManifest(GetArtManifest());
        DistributeManifest(room.GetManifest());
    }

    // Invoked on visitors by master client.
    // Receives the manifest and checks if there are missing assets that need
    // to be requested.
    [PunRPC]
    private void ReceiveManifestRPC(byte[] manifest)
    {
        // XXX: For testing. Visitor client has no assets.
        artReg = ArtRegistry.GetEmptyArtRegistry();
        ArtManifest artManifest = Util.DeserializeByteArray<ArtManifest>(manifest);
        slotSettings = new Dictionary<Checksum,SlotSettings>();
        List<Checksum> missing = new List<Checksum>();
        foreach (SlotSettings slot in artManifest.Slots)
        {
            SlotSettings ss = slot;
            Checksum checksum = slot.MetaData.Checksum;
            if (artReg.HasArt(checksum))
            {
                Debug.Log("Visitor already has art " + checksum.ToString());
                // Use existing meta data from registry.
                ss = slot.WithMeta(artReg.Get(checksum));
                slotSettings.Add(checksum, ss);
                string filename = ss.MetaData.AbsolutePath;
                Sprite MySprite = IMG2Sprite.instance.LoadNewSprite(filename);
                string tag = slotNrToTag(ss.SlotNumber);
                SetSprite(tag, MySprite);
            } else
            {
                Debug.Log("Visitor does NOT have art " + checksum.ToString());
                missing.Add(checksum);
                string root = DeclareRoot();
                Debug.Log("Root2: " +  root);
                ArtMetaData absolute = ss.MetaData.MakeAbsolutePath(root);
                ss = slot.WithMeta(absolute);
                slotSettings.Add(ss.MetaData.Checksum, ss);
                // TODO:
                // instantiate some place holder for missing?
            }
        }
        if (missing.Count > 0)
            RequestArtAssets(missing);
    }

    private string slotNrToTag(int slot) => "Tavla" + slot.ToString();

    // Runs on master client (called by regular clients).
    [PunRPC]
    private void ExportArtAssetsRPC(byte[] checksums, PhotonMessageInfo info)
    {
        List<Checksum> missing = Util.DeserializeByteArray<List<Checksum>>(checksums);
        ExportArtAssets(missing, info.Sender);
    }

    // Runs on regular client (called by master client).
    // Receive a single asset.
    // The byte array is the actual file contents.
    [PunRPC]
    private void ReceiveArtAssetRPC(byte[] asset)
    {
        Checksum checksum = Checksum.Compute(asset);
        if (!slotSettings.ContainsKey(checksum))
        {
            Debug.Log("!! Something went wrong! Recived unknown asset.");
            return;
        }
        Debug.Log("Adding received asset to registry.");
        SlotSettings slot = slotSettings[checksum];
        ArtMetaData metaData = slot.MetaData;
        File.WriteAllBytes(metaData.AbsolutePath, asset);
        artReg.AddArt(metaData);
        // Use settings to instantiate (show in room)?
  
         Debug.Log("Showing sprite");
         string filename = slot.MetaData.AbsolutePath;
         Sprite MySprite = IMG2Sprite.instance.LoadNewSprite(filename);
         string tag = slotNrToTag(slot.SlotNumber);
         SetSprite(tag, MySprite);
      
        // Transfer of asset complete.
    }


    [PunRPC]
    private void ReceiveArtAssetRPC2(byte[] asset, ArtType type)
    {
        Checksum checksum = Checksum.Compute(asset);
        if (!slotSettings.ContainsKey(checksum))
        {
            Debug.Log("!! Something went wrong! Recived unknown asset.");
            return;
        }
        Debug.Log("Adding received asset to registry.");
        SlotSettings slot = slotSettings[checksum];
        ArtMetaData metaData = slot.MetaData;
        File.WriteAllBytes(metaData.AbsolutePath, asset);
        artReg.AddArt(metaData);
        // Use settings to instantiate (show in room)?
        if (type == ArtType.Painting)
        {
            Debug.Log("Showing sprite");
            string filename = slot.MetaData.AbsolutePath;
            Sprite MySprite = IMG2Sprite.instance.LoadNewSprite(filename);
            string tag = slotNrToTag(slot.SlotNumber);
            SetSprite(tag, MySprite);
        }
        else if (type == ArtType.Sculpture)
        {
            Debug.Log("Showing Sculpture");
            string filename = slot.MetaData.AbsolutePath;
            GameObject obj = new OBJLoader().Load(filename);
            string tag = "Objekt1";
            SetObj(tag, obj);
        }

        // Transfer of asset complete.
    }


    /*
     * The one-line helper methods RequestArtAssets, ExportArtAssets, and
     * DistributeManifest exist because those names say much more clearly what
     * is being done than the method names they invoke via RPC.
     */

    private void DistributeManifest(ArtManifest artManifest)
    {
        byte[] manifest = Util.SerializableToByteArray(artManifest);
        This().RPC("ReceiveManifestRPC", RpcTarget.OthersBuffered, manifest);
    }

    // Request these assets by telling the master client to export them.
    private void RequestArtAssets(List<Checksum> missing)
    {
        byte[] checksums = Util.SerializableToByteArray(missing);
        This().RPC("ExportArtAssetsRPC", RpcTarget.MasterClient, checksums);
    }

    // Export these assets by telling the client to receive them.
    // Exports one at a time.
    private void ExportArtAssets(List<Checksum> checksums, Player receiver)
    {
        ArtRegistry reg = ArtRegistry.GetArtRegistry();
        foreach (Checksum checksum in checksums)
        {
            Debug.Log("Exporting asset with checksum " + checksum.ToString());
            Debug.Log("Reading asset file...");
            byte[] asset = File.ReadAllBytes(reg.Get(checksum).AbsolutePath);
            ArtType type = reg.Get(checksum).Type;                  //--------------------------------------------------------------------------
            Debug.Log("Invoking ReceiveArtAssetRPC2");
            This().RPC("ReceiveArtAssetRPC2", receiver, asset, type);
        }
    }

    // For invocations of RPC().
    // TODO: use a member variable instead?
    private PhotonView This() {
        return PhotonView.Get(this);
    }


    // ----------------------
    // Early proof of concept
    // (Only handles 2D art.)
    // ----------------------

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
        //GameObject sprite = GameObject.Find(pos);
        //sprite.GetComponent<SpriteRenderer>().sprite = MySprite;
        SetSprite(pos, MySprite);
    }

    public void SetSprite(string tag, Sprite sprite)
    {
      GameObject.Find(tag).GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public void SetObj(string tag, GameObject obj) {
        GameObject slot = GameObject.Find(tag);
        Vector3 position = slot.transform.position;
        Destroy(slot);
        GameObject loadedObj = obj;
        Debug.Log("Loaded obj");
        loadedObj.transform.position = position;
        loadedObj.name = "Objekt1";
        Debug.Log("moved obj");
        Vector3 desiredSize = new Vector3(1.6f, 1.6f, 1.6f);
        foreach (Transform t in loadedObj.transform) {

            /*Bounds a = t.GetComponent<MeshRenderer>().bounds;
            Vector3 desiredSize = a.max - a.min;
            Debug.Log(desiredSize.ToString());*/
            Bounds b = t.GetComponent<MeshRenderer>().bounds;
            Vector3 a = b.max - b.min;
            float max = Mathf.Max(Mathf.Max(a.x, a.y), a.z);
            t.transform.localScale = t.transform.localScale * (1.6f/max);

        } 
    }
}
