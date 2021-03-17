using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnityEvent_String : UnityEngine.Events.UnityEvent<string> { }


public class UdpPacketCounter : MonoBehaviour
{
    public UnityEvent_String OnDebug;

    int DroppedPacketCount = 0;
    int BadPacketCount = 0;
    int LastBadPacketSize = 0;
    int PacketCount = 0;
    uint? LastPacketNumber = null;

    void Update()
    {
        var PacketNumber = LastPacketNumber.HasValue ? LastPacketNumber.Value : 0;
        var Debug = "";
        Debug += "Dropped " + DroppedPacketCount + "/" + PacketCount + " Bad=" + BadPacketCount +"(x"+LastBadPacketSize+") #" + PacketNumber;
        OnDebug.Invoke(Debug);
    }

    uint GetInt32(byte[] Packet)
    {
        int a = Packet[0] << 0;
        int b = Packet[1] << 8;
        int c = Packet[2] << 16;
        int d = Packet[3] << 24;
        var abcd = a | b | c | d;
        var abcd32 = (uint)abcd;
        return abcd32;
    }

    public void OnPacket(byte[] Packet)
    {
        //  read packet number from start
        var PacketNumber = GetInt32(Packet);
        PacketCount++;

        //  if server is active when we start, we might not get the first part of the packet properly
        //  so check for invalid number
       // if ( !LastPacketNumber.HasValue )
        {
            if ( PacketNumber > (1<<24) )
            {
                //  skip packet
                BadPacketCount++;
                LastBadPacketSize = Packet.Length;
                return;
            }
        }

        if ( LastPacketNumber.HasValue )
        {
            var Last = LastPacketNumber.Value;
            var PacketsDropped = 0;
            for ( var i=Last+1;   i<PacketNumber; i++ )
            {
                //Debug.Log("Dropped #" + i);
                PacketsDropped++;
            }
            //var PacketDelta = PacketNumber - LastPacketNumber.Value;
            //var PacketsDropped = PacketDelta-1;
            DroppedPacketCount += (int)PacketsDropped;
        }
        LastPacketNumber = PacketNumber;
    }

}
