using Fika.Core.Networking.LiteNetLib.Utils;
using UnityEngine;

namespace BarterItemsStacksFika.Packets;

public struct TripwirePlantRequestPacket : INetSerializable
{
    public string GrenadeTemplate;
    public Vector3 FromPosition;
    public Vector3 ToPosition;
    public string ProfileId;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(GrenadeTemplate);
        writer.PutUnmanaged(FromPosition);
        writer.PutUnmanaged(ToPosition);
        writer.Put(ProfileId);
    }

    public void Deserialize(NetDataReader reader)
    {
        GrenadeTemplate = reader.GetString();
        FromPosition = reader.GetUnmanaged<Vector3>();
        ToPosition = reader.GetUnmanaged<Vector3>();
        ProfileId = reader.GetString();
    }
}
