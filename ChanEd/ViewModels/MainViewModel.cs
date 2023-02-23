using System.Text;
using ChanEd.Core.Models;
using ChanEd.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChanEd.ViewModels;

public class MainViewModel : ObservableRecipient
{
    public MainViewModel()
    {
        var lib = new ChanSortLib();
        lib.ShowOpenFileDialog();

        const string fileName = @"c:\\temp\\channels.dat";
        List<SatChannelModel> list = new List<SatChannelModel>();
        SatChannelModel channel;
        byte[] data;
        byte[] data2;
        string str1, str2, str3;

        using (var stream = File.Open(fileName, FileMode.Open))
        {
            using (var reader = new BinaryReader(stream, Encoding.Unicode, false))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {

                    channel = new SatChannelModel();
                    channel.ChannelNo = reader.ReadInt16();
                    
                 if (channel.ChannelNo == 0) break;

                    channel.Vpid = reader.ReadInt16();
                    channel.Pid = reader.ReadInt16();
                    channel.Mpeg4 = reader.ReadInt16();
                    channel.Bytes6 = reader.ReadBytes(6);
                    channel.ServiceType = reader.ReadInt16();
                    channel.Sid = reader.ReadUInt16();
                    channel.TransPonderId = reader.ReadUInt16();
                    channel.SatId = reader.ReadUInt16();
                    channel.Bytes00 = reader.ReadUInt16();
                    channel.Tsid = reader.ReadUInt16();
                    channel.Bytes01 = reader.ReadUInt16();
                    channel.Onid = reader.ReadUInt16();
                    channel.BytesFF = reader.ReadUInt16();
                    channel.NewChannel1 = reader.ReadUInt16();
                    channel.NewChannel2 = reader.ReadUInt16();
                    data = reader.ReadBytes(100);
                    //chars = reader.ReadChars(51);
                    //channel.ChannelName = Encoding.Unicode.GetString(data);
                    str1 = Encoding.UTF8.GetString(data);
                    str2 = Encoding.BigEndianUnicode.GetString(data);
                    str3 = str2.Substring(0, str2.IndexOf('\0'));
                    channel.ChannelName = str3;
                    data2 = reader.ReadBytes(26);
                    channel.Bouqet = reader.ReadUInt16();
                    channel.Dummy1 = reader.ReadByte();
                    channel.ParentalLockMarker = reader.ReadByte();
                    channel.FavoriteBitMask = reader.ReadByte();
                    channel.CheckSum = reader.ReadByte();

                    list.Add(channel);
                }
            }
        }
        AppData.ChannelList = list;
    }
}
