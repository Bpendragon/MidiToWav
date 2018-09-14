using System.IO;
using System.Collections.Generic;

namespace MidiToWav
{
    public class MidiStream
    {
        private string fileName;

        public MidiStream(string fileName)
        {
            this.fileName = fileName;

            ConstructStreams(fileName);
        }

        public void ConstructStreams(string file)
        {
            FileStream dataFile;
            try
            {
                dataFile = File.OpenRead(file);
            }
            catch
            {
                throw;
            }


        }
    }

    public enum MidiFormat
    {
        SingleTrack = 0,
        MultiTrack = 1,
        AsyncMultiTrack = 2
    }

    public enum ChunkType
    {
        Header = 0,
        Track = 1
    }


    public class HeaderChunk
    {
        private MidiFormat Format { get; }
        private ChunkType Type { get; }
        private uint Length { get; }

        public HeaderChunk(MidiFormat Format, uint Length)
        {
            this.Format = Format;
            this.Type = ChunkType.Header;
            this.Length = Length;
        }

        public HeaderChunk()
        {
            this.Type = ChunkType.Header;
        }

    }

    public class TrackChunk
    {
        private ChunkType Type { get; }
        private uint Length { get; }
        private List<MidiTrackEvent> Events { get; set; }

    }

    public class MidiTrackEvent
    {
        private int DeltaTime { get; }
        private MidiMessage Message { get; set; }

        public MidiTrackEvent(int DeltaTime, MidiMessage Message)
        {
            this.DeltaTime = DeltaTime;
            this.Message = Message;
        }
    }

    public class MidiMessage
    {
        private byte Status { get; }
        private List<byte> Data { get; }

        public MidiMessage(byte Status, List<byte> Data)
        {
            this.Status = Status;
            this.Data = Data;
        }
    }
}