using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChanEd.Core.Models;
public class SatChannelModel
{
    public Int16 ChannelNo { get; set; }             // Bytes 01-02 channel number
    public Int16 Vpid { get; set; }                  // Bytes 03-04 VPID
    public Int16 Pid { get; set; }                   // Bytes 05-06 PID (same as VPID on video channels)
    public Int16 Mpeg4 { get; set; }                 // Bytes 07-08 MPEG4 (0x00 on MPEG2, 0x01 on MPEG4)
    public byte[] Bytes6 { get; set; }               // Bytes 09-14 0x0001 0307 0500
    public Int16 ServiceType { get; set; }           // Bytes 15-16 Service type (0x01 TV, 0x02 Radio, 0x0C Data, 0x19 HD)
    public UInt16 Sid { get; set; }                  // Bytes 17-18 SID
    public UInt16 TransPonderId { get; set; }        // Bytes 19-20 Transponder ID (matched against TransponderDataBase.dat)
    public UInt16 SatId { get; set; }                // Bytes 21-22 Sat ID (matched against SatDataBase.dat)
    public UInt16 Bytes00 { get; set; }              // Bytes 23-24 0x0000
    public UInt16 Tsid { get; set; }                 // Bytes 25-26 TSID
    public UInt16 Bytes01 { get; set; }              // Bytes 27-28 0x0000
    public UInt16 Onid { get; set; }                 // Bytes 29-30 ONID
    public UInt16 BytesFF { get; set; }              // Bytes 31-32 0xFFFF
    public UInt16 NewChannel1 { get; set; }          // Bytes 33-34 ???? 0xFFFF when channel is new, after that an unknown number
    public UInt16 NewChannel2 { get; set; }          // Bytes 35-36 ???? 0xFFFF when channel is new, after that an unknown number
    public string ChannelName { get; set; }          // Bytes 37-138 channel name (50 chars)
    public UInt16 Bouqet { get; set; }               // Bytes 139-140 bouqet(always thr same for channel of one provider)
    public byte Dummy1 { get; set; }                 // Byte  141     parental lock marker (not tested yet)
    public byte ParentalLockMarker { get; set; }     // Byte  142     parental lock marker (not tested yet)
    public byte FavoriteBitMask { get; set; }        // Byte  143     favorite bitmask (thr lower 4 bits represented a favorite list from 1-4, if its 1, the channel is on it, if 0, its not)
    public byte CheckSum { get; set; }               // Byte  144     checksum over bytes 1-143
}
