using Unity.Netcode;
using Unity.Collections;
using System;
public struct ChatMessage : INetworkSerializable, IEquatable<ChatMessage>
{
    public FixedString128Bytes sender;
    public FixedString512Bytes message;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref sender);
        serializer.SerializeValue(ref message);
    }
    public bool Equals(ChatMessage other)
    {
        return sender.Equals(other.sender) && message.Equals(other.message);
    }
}
