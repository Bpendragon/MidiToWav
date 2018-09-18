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
            Header.NumTracks = numTracks;
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

                MetaEventType LastMessage = MetaEventType.None;

                while (LastMessage != MetaEventType.EndOfTrack)
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


                    if ((data[dataPointer] >> 4) < 0x8)
                    {
                        throw new FileLoadException(data[dataPointer] + " is not a valid message identifier");
                    }
                    else if ((data[dataPointer] >> 4) != 0xF)
                    {
                        tmpEvent.EventType = (MidiTrackEventType)(data[dataPointer] >> 4);
                    }
                    else
                    {
                        tmpEvent.EventType = (MidiTrackEventType)data[dataPointer];
                    }



                    if (tmpEvent.EventType == MidiTrackEventType.Meta)
                    {
                        dataPointer++; // do this here because we still need the datapointer to know the channel for the voice messages
                        tmpEvent.MetaType = (MetaEventType)(data[dataPointer++]);

                        uint msgLength = 0;
                        while (data[dataPointer] >= 0x80)
                        {
                            msgLength = (msgLength << 7 | (byte)(data[dataPointer] & 0x7F));
                            dataPointer++;
                        }

                        //this gets the last byte added on.
                        msgLength = (msgLength << 7 | (byte)(data[dataPointer] & 0x7F));
                        dataPointer++;

                        for (int j = 0; j < msgLength; j++)
                        {
                            tmpEvent.Data.Add(data[dataPointer++]);
                        }

                        if (tmpEvent.MetaType == MetaEventType.TrackName)
                        {
                            track.Name = Encoding.ASCII.GetString(tmpEvent.Data.ToArray());
                        }
                    }
                    else
                    {
                        switch (tmpEvent.EventType)
                        {
                            case MidiTrackEventType.NoteOff:
                            case MidiTrackEventType.NoteOn:
                            case MidiTrackEventType.PolyphonicAftertouch:
                                tmpEvent.ChannelNumber = data[dataPointer++] & 0x0F;
                                tmpEvent.KeyNumber = data[dataPointer++];
                                tmpEvent.Value = data[dataPointer++];
                                break;

                            case MidiTrackEventType.ControlChange:
                                tmpEvent.ChannelNumber = data[dataPointer++] & 0x0F;
                                tmpEvent.Controller = (MidiController)(data[dataPointer++]);
                                tmpEvent.Value = data[dataPointer++];
                                break;
                            case MidiTrackEventType.PitchWheelChange:
                            case MidiTrackEventType.SongPositionPointer:
                                dataPointer++;
                                tmpEvent.FourteenBit = (ushort)(data[dataPointer++] | data[dataPointer++] << 7);
                                break;

                            case MidiTrackEventType.SongSelect:
                            case MidiTrackEventType.ChannelAftertouch:
                            case MidiTrackEventType.ProgramChange:
                                dataPointer++;
                                tmpEvent.Value = data[dataPointer++];
                                break;

                            case MidiTrackEventType.SystemExclusive:
                                dataPointer++;
                                while (data[dataPointer] != 0xF7)
                                {
                                    tmpEvent.Data.Add(data[dataPointer++]);
                                }
                                dataPointer++;

                                break;
                            default:
                                break;
                        }
                    }
                    track.Events.Add(tmpEvent);
                    LastMessage = tmpEvent.MetaType;
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
        public List<byte> Data { get; set; } //used for variable length meta events
        public MidiTrackEventType EventType { get; set; }
        public MetaEventType MetaType { get; set; } = MetaEventType.None;
        public MidiController Controller { get; set; } = MidiController.None;
        public int ChannelNumber { get; set; }
        public byte KeyNumber { get; set; }
        public byte Value { get; set; } //Velocity in most cases, value in Control Change, new program in Program Change
        public ushort FourteenBit { get; set; } //for Pitch Wheel and Song Pointer

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