using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MidiToWav
{
    partial class MidiStream
    {
        string fileName;
        List<TrackChunk> Tracks;
        HeaderChunk Header;


        public MidiStream(string fileName)
        {
            string[] splitName = fileName.Split("\\/".ToCharArray());
            this.fileName = splitName[splitName.Length - 1];
            Header = new HeaderChunk();
            Tracks = ConstructStreams(fileName);
        }

        public List<TrackChunk> ConstructStreams(string file)
        {
            byte[] data;
            uint dataPointer = 0;
            var tracks = new List<TrackChunk>();
            try
            {
                data = File.ReadAllBytes(file);
            }
            catch
            {
                throw;
            }

            if (data[dataPointer++] != 0x4D
                || data[dataPointer++] != 0x54
                || data[dataPointer++] != 0x68
                || data[dataPointer++] != 0x64
                )
            {
                throw new FileLoadException("Not a MIDI File");
            }


            Header.Length = (uint)(data[dataPointer++] << 24 | data[dataPointer++] << 16 | data[dataPointer++] << 8 | data[dataPointer++]);

            Header.Format = (MidiFormat)(data[dataPointer++] << 8 | data[dataPointer++]);
            uint numTracks = (uint)(data[dataPointer++] << 8 | data[dataPointer++]);

            Header.Division = new DivisionInfo((ushort)(data[dataPointer++] << 8 | data[dataPointer++]));


            for (int i = 0; i < numTracks; i++)
            {
                if (data[dataPointer++] != 0x4D
                || data[dataPointer++] != 0x54
                || data[dataPointer++] != 0x72
                || data[dataPointer++] != 0x6B
                )
                {
                    throw new FileLoadException("Missing Track Header");
                }

                var track = new TrackChunk();
                var length = (uint)(data[dataPointer++] << 24 | data[dataPointer++] << 16 | data[dataPointer++] << 8 | data[dataPointer++]);
                track.Length = length;

                var target = dataPointer + length;

                while (dataPointer < target)
                {
                    //First item on each time through the loop get the Delta Time
                    uint delta_counter = 0;
                    while (data[dataPointer] >= 0x80)
                    {
                        delta_counter = (delta_counter << 7 | (byte)(data[dataPointer] & 0x7F));
                        dataPointer++;
                    }

                    //this gets the last byte added on.
                    delta_counter = (delta_counter << 7 | (byte)(data[dataPointer] & 0x7F));
                    dataPointer++;



                    var tmpEvent = new MidiTrackEvent(delta_counter);

                    if (data[dataPointer] < 0xF0 && (data[dataPointer] & 0xC0) != 0xC0 && (data[dataPointer] & 0xD0) != 0xD0) //all 2 command "voice" messages
                    {
                        //unrolling the loop because it's so short
                        tmpEvent.Data.Add(data[dataPointer++]);
                        tmpEvent.Data.Add(data[dataPointer++]);
                        tmpEvent.Data.Add(data[dataPointer++]);
                    }
                    else if ((data[dataPointer] >> 4) == 0xC || (data[dataPointer] >> 4) == 0xD) //these two only take 2 args
                    {
                        tmpEvent.Data.Add(data[dataPointer++]);
                        tmpEvent.Data.Add(data[dataPointer++]);
                    }
                    else if (data[dataPointer] == 0xF0) //system exclusive, really don't care here
                    {
                        while (data[dataPointer] != 0xF7)
                        {
                            dataPointer++;
                        }
                        dataPointer++;
                    }
                    else if (data[dataPointer] == 0xF2)  //song position pointer
                    {
                        tmpEvent.Data.Add(data[dataPointer++]);
                        tmpEvent.Data.Add(data[dataPointer++]);
                        tmpEvent.Data.Add(data[dataPointer++]);
                    }
                    else if (data[dataPointer] == 0xF3) //song select
                    {
                        tmpEvent.Data.Add(data[dataPointer++]);
                        tmpEvent.Data.Add(data[dataPointer++]);
                    }
                    else if (data[dataPointer] != 0xFF) //all other common and real-times except metas
                    {
                        tmpEvent.Data.Add(data[dataPointer++]);
                    }
                    else //all that's left is 0xFF, the META events.
                    {
                        tmpEvent.Data.Add(data[dataPointer++]);
                        switch (data[dataPointer])
                        {
                            case 0x00: //Sequence Number
                                tmpEvent.Data.Add(data[dataPointer++]);
                                tmpEvent.Data.Add(data[dataPointer++]);
                                tmpEvent.Data.Add(data[dataPointer++]);
                                break;
                            case 0x01:
                            case 0x02:
                            case 0x03:
                            case 0x04:
                            case 0x05:
                            case 0x06:
                            case 0x07:
                            case 0x7F://Text Events of various types (except 7f which is sequencer specific)
                                tmpEvent.Data.Add(data[dataPointer++]);
                                uint len_counter = 0;
                                while (data[dataPointer] >= 0x80)
                                {
                                    delta_counter = (len_counter << 7 | (byte)(data[dataPointer] & 0x7F));
                                    dataPointer++;
                                }

                                //this gets the last byte added on.
                                len_counter = (len_counter << 7 | (byte)(data[dataPointer] & 0x7F));
                                dataPointer++;

                                for (int k = 0; k < len_counter; k++)
                                {
                                    tmpEvent.Data.Add(data[dataPointer + k]);
                                }
                                dataPointer += len_counter;
                                break;

                            case 0x20: //midi channel prefix
                                tmpEvent.Data.Add(data[dataPointer++]);
                                tmpEvent.Data.Add(data[dataPointer++]);
                                tmpEvent.Data.Add(data[dataPointer++]);
                                break;
                            case 0x2F: //end of track
                                tmpEvent.Data.Add(data[dataPointer++]);
                                tmpEvent.Data.Add(data[dataPointer++]);
                                break;
                            case 0x51: //Set Tempo

                                for (int k = 0; k < 5; k++)
                                {
                                    tmpEvent.Data.Add(data[dataPointer++]);
                                }
                                break;
                            case 0x54: //smpte offset
                                for (int k = 0; k < 7; k++)
                                {
                                    tmpEvent.Data.Add(data[dataPointer++]);
                                }
                                break;
                            case 0x58: //Time Signature
                                for (int k = 0; k < 6; k++)
                                {
                                    tmpEvent.Data.Add(data[dataPointer++]);
                                }
                                break;
                            case 0x59: //Key Signature
                                for (int k = 0; k < 4; k++)
                                {
                                    tmpEvent.Data.Add(data[dataPointer++]);
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    if (tmpEvent.Data[0] == 0xFF && tmpEvent.Data[1] == 0x03) //if the event is a name, add it to the track now.
                    {
                        track.Name = Encoding.ASCII.GetString(tmpEvent.Data.ToArray(), 2, tmpEvent.Data.Count - 2);

                    }
                    track.Events.Add(tmpEvent);

                }

                tracks.Add(track);

            }
            return tracks;


        }
    }

    public class HeaderChunk
    {
        public MidiFormat Format { get; set; }
        public ChunkType Type { get; set; }
        public uint Length { get; set; }
        public uint NumTracks { get; set; }
        public DivisionInfo Division { get; set; }

        public HeaderChunk(MidiFormat Format, uint Length, uint NumTracks, ushort Division)
        {
            this.Format = Format;
            this.Type = ChunkType.Header;
            this.Length = Length;
            this.NumTracks = NumTracks;
            this.Division = new DivisionInfo(Division);
        }

        public HeaderChunk()
        {
            this.Type = ChunkType.Header;
        }

    }

    public class DivisionInfo
    {
        public DivisionFormat Format { get; set; }
        public uint Resolution { get; set; }

        public DivisionInfo(ushort div)
        {
            if ((div >> 15 & 0xF) == 1)
            {
                switch (div >> 8)
                {
                    case 0xE8:
                        Format = DivisionFormat.TwentyFour;
                        break;
                    case 0xE7:
                        Format = DivisionFormat.TwentyFive;
                        break;
                    case 0xE3:
                        Format = DivisionFormat.ThirtyDrop;
                        break;
                    case 0xE2:
                        Format = DivisionFormat.Thirty;
                        break;
                    default:
                        throw new InvalidDataException("Not a Valid SMPTE Time");
                }

                Resolution = (uint)(div & 0x0F);
            }
            else
            {
                Format = DivisionFormat.Metrical;
                Resolution = div;
            }
        }
    }

    public class TrackChunk
    {
        public ChunkType Type { get; set; }
        public uint Length { get; set; }
        public List<MidiTrackEvent> Events { get; set; }
        public string Name { get; set; }

        public TrackChunk()
        {
            Type = ChunkType.Track;
            Events = new List<MidiTrackEvent>();
        }

    }

    public class MidiTrackEvent
    {
        //MTrk events can be Midi Events, sysex eventss, or meta events.
        public uint DeltaTime { get; set; }
        public List<byte> Data { get; set; }
        public MidiTrackEventType EventType { get; set; }
        public MetaEventType MetaType { get; set; }
        public MidiController Controller { get; set; } = MidiController.None;
        public int ChannelNumber { get; set; }

        public MidiTrackEvent(uint DeltaTime, List<byte> Message)
        {
            this.DeltaTime = DeltaTime;
            this.Data = Message;
        }

        public MidiTrackEvent(uint DeltaTime)
        {
            this.DeltaTime = DeltaTime;
            Data = new List<byte>();
        }
    }




}