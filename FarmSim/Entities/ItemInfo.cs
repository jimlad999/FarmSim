using System.Runtime.Serialization;

namespace FarmSim.Entities;

[DataContract]
class ItemInfo
{
    [DataMember]
    public string Id;
    [DataMember]
    public Tags[] Tags;
    [DataMember]
    public int Quality;
    [DataMember]
    public long PickedUpTimestampTicks;
}
