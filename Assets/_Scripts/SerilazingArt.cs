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

public class SerilazingArt : MonoBehaviourPunCallbacks
{

    private Dictionary<Checksum,SlotSettings> slotSettings;
    private string root;
    private ArtRegistry artReg;

    public void Awake()
    {
        root = AppSettings.GetAppSettings().DownloadPath;
        artReg = AppSettings.GetAppSettings().ArtRegistry;
        Debug.Log("Using root: " + root);
    }

    // TODO: Take settings as a parameter, or take no parameters
    //       and find the room settings somewhere?
    // Invoked locally on master client from OnJoinedRoom.
    // Distribute full art manifest for the room to all visitors.
    // Perform RPC on all clients when they connect (targeting OthersBuffered).
    public void ExportArt(RoomSettings room)
    {
        DistributeManifest(room.GetManifest());
        room.Slots.ForEachValue(DisplayArt);  // TODO: should definitely do this, but maybe somewhere else?
    }

    // TODO: This method needs cleaning up!!
    // TODO: Call a separate method for updating slotSettings.
    // Invoked on visitors by master client.
    // Receives the manifest and checks if there are missing assets that need
    // to be requested.
    [PunRPC]
    private void ReceiveManifestRPC(byte[] manifest)
    {
        // XXX: For testing. Simulate that client has no art assets.
        //artReg = ArtRegistry.GetEmptyArtRegistry();
        ArtManifest artManifest = Serial.DeserializeByteArray<ArtManifest>(manifest);
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
                DisplayArt(ss);
            } else
            {
                ArtMetaData absolute = ss.MetaData.MakeAbsolutePath(root);
                ss = slot.WithMeta(absolute);
                slotSettings.Add(ss.MetaData.Checksum, ss);
                if (alreadyDownloaded(ss.MetaData))
                {
                    artReg.AddArt(absolute);
                    DisplayArt(ss);
                } else
                {
                    Debug.Log("Visitor does NOT have art " + checksum.ToString());
                    missing.Add(checksum);
                    // TODO:
                    // instantiate some place holder for missing?
                }
            }
        }
        if (missing.Count > 0)
            RequestArtAssets(missing);
    }

    private bool alreadyDownloaded(ArtMetaData meta)
    {
        Debug.Log("Checking if " + meta.AbsolutePath + " already exists");
        if (!Util.IsFile(meta.AbsolutePath))
            return false;
        Checksum hash = Checksum.Compute(meta.AbsolutePath, meta.Type);
        return meta.Checksum.Equals(hash);
    }

    //private string slotNrToTag(int slot) => "ArtSlot" + slot.ToString();
    private string slotNrToTag(int slot) => "Tavla" + slot.ToString();

    // Runs on master client (called by regular clients).
    [PunRPC]
    private void ExportArtAssetsRPC(byte[] checksums, PhotonMessageInfo info)
    {
        List<Checksum> missing = Serial.DeserializeByteArray<List<Checksum>>(checksums);
        ExportArtAssets(missing, info.Sender);
    }

    // Runs on regular client (called by master client).
    // Receive a single asset.
    // The byte array is the actual file contents.
    [PunRPC]
    private void ReceiveArtAssetRPC(byte[] asset, byte[] hash)
    {
        Checksum checksum = Serial.DeserializeByteArray<Checksum>(hash);
        SlotSettings slot = slotSettings[checksum];
        ImportAsset(asset, slot.MetaData);
        DisplayArt(slot);

        // Transfer of asset complete.
    }

    // Store asset data and register in Art Registry.
    public void ImportAsset(byte[] asset, ArtMetaData meta)
    {
        Debug.Log("Importing received asset.");
        Util.UnzipAsset(asset, meta);
        artReg.AddArt(meta);
    }

    // Display (known / locally present) art using the slot settings.
    public void DisplayArt(SlotSettings slot)
    {
        ArtMetaData meta = slot.MetaData;
        string tag = slotNrToTag(slot.SlotNumber);
        DisplayArt(meta, tag);
    }

    // Display (known / locally present) art in the specified slot.
    public void DisplayArt(ArtMetaData meta, string slotTag)
    {
        Debug.Log("Displaying slot " + slotTag + ": " + meta.Checksum.ToString());
        string filename = meta.AbsolutePath;
        if (meta.IsPainting)
        {
            Sprite sprite = IMG2Sprite.instance.LoadNewSprite(filename);
            SetSprite(slotTag, sprite);
        }
        else if (meta.IsSculpture)
        {
            GameObject obj = new OBJLoader().Load(meta.AbsolutePath);
            SetObj(slotTag, obj);
        }
    }

    /*
     * The one-line helper methods RequestArtAssets, ExportArtAssets, and
     * DistributeManifest exist because those names say much more clearly what
     * is being done than the method names they invoke via RPC.
     */

    private void DistributeManifest(ArtManifest artManifest)
    {
        byte[] manifest = Serial.SerializableToByteArray(artManifest);
        This().RPC("ReceiveManifestRPC", RpcTarget.OthersBuffered, manifest);
        //This().RPC("ReceiveManifestRPC", RpcTarget.AllBuffered, manifest);  // XXX: testing (export to self)
    }

    // Request these assets by telling the master client to export them.
    private void RequestArtAssets(List<Checksum> missing)
    {
        byte[] checksums = Serial.SerializableToByteArray(missing);
        This().RPC("ExportArtAssetsRPC", RpcTarget.MasterClient, checksums);
    }

    // Export these assets by telling the client to receive them.
    // Exports one at a time.
    private void ExportArtAssets(List<Checksum> checksums, Player receiver)
    {
        foreach (Checksum checksum in checksums)
        {
            Debug.Log("Exporting asset with checksum " + checksum.ToString());
            Debug.Log("Reading asset file...");
            byte[] asset = getAssetBytes(checksum);
            byte[] hash = Serial.SerializableToByteArray(checksum);
            Debug.Log("Invoking ReceiveArtAssetRPC");
            This().RPC("ReceiveArtAssetRPC", receiver, asset, hash);
        }
    }

    private byte[] getAssetBytes(Checksum checksum)
    {
        ArtRegistry artReg = AppSettings.GetAppSettings().ArtRegistry;
        return Util.ZipAsset(artReg.Get(checksum));
    }

    // For invocations of RPC().
    // TODO: use a member variable instead?
    private PhotonView This() {
        return PhotonView.Get(this);
    }


    public void SetSprite(string tag, Sprite sprite)
    {
      GameObject.Find(tag).GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public void SetObj(string tag, GameObject loadedObj)
    {
        GameObject slot = GameObject.Find(tag);
        Vector3 position = slot.transform.position;
        Destroy(slot);
        loadedObj.transform.position = position;
        loadedObj.name = tag;
        Vector3 desiredSize = new Vector3(1.6f, 1.6f, 1.6f);
        foreach (Transform t in loadedObj.transform)
        {
            Bounds b = t.GetComponent<MeshRenderer>().bounds;
            Vector3 a = b.max - b.min;
            float max = Mathf.Max(Mathf.Max(a.x, a.y), a.z);
            t.transform.localScale = t.transform.localScale * (1.6f/max);
        }
    }

}
