using System.IO;

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
            } catch
            {
                throw;
            }


        }
    }

    public class HeaderChunk : Chunk 
    {
        private MidiFormat Format { get; set; }
        


    }

    public class Chunk
    {
        private ChunkType Type { get; set; }
        private uint Length { get; set; } 
    }

    public enum MidiFormat
    {
        SingleTrack = 0,
        MultiTrack = 1,
        AsyncMultiTrack = 2
    };

    public enum ChunkType
    {
        Header = 0,
        Track = 1
    };
}