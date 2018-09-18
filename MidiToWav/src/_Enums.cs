namespace MidiToWav
{
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

    public enum DivisionFormat
    {
        Metrical = 0,
        TwentyFour = 24,
        TwentyFive = 25,
        ThirtyDrop = 29,
        Thirty = 30
    }

    public enum MidiTrackEventType
    {
        Meta = 0xFF, //Also Reset but it should never be used as such in a file
        NoteOff = 0x8,
        NoteOn = 0x9,
        PolyphonicAftertouch = 0xA,
        ControlChange = 0xB,  //note that Channel Mode are very specific instances of these messages
        ProgramChange = 0xC,
        ChannelAftertouch = 0xD,
        PitchWheelChange = 0xE,
        SystemExclusive = 0xF0,
        SongPositionPointer = 0xF2,
        SongSelect = 0xF3,
        TuneRequest = 0xF6,
        EndExclusive = 0xF7,
        TimingClock = 0xF8,
        Start = 0xFA,
        Continue = 0xFB,
        Stop = 0xFC,
        ActiveSensing = 0xFD
    }

    public enum MetaEventType
    {
        SequenceNumber = 0x00,
        TextEvent = 0x01,
        CopyrightNotice = 0x02,
        TrackName = 0x03,
        InstrumentName = 0x04,
        Lyric = 0x05,
        Marker = 0x06,
        CuePoint = 0x07,
        MidiChannelPrefix = 0x20,
        EndOfTrack = 0x2F,
        SetTempo = 0x51,
        SMPTEOffset = 0x54,
        TimeSignature = 0x58,
        KeySignature = 0x59,
        SequencerSpecific = 0x7F,
        None = 0x80,
    }

    public enum MidiController // for sanity I'm going to just use the dec values, but know that in the file they are hex
    {
        None = 128, //no controller has this value, so it'll be the default for when an event doesn't touch the controllers.
        BankSelect = 0,
        ModulationWheel = 1,
        BreathControl = 2,
        FootController = 4,
        PortamentoTime = 5,
        DataEntry = 6,
        ChannelVolume = 7,
        Balance = 8,
        Pan = 9,
        ExpressionController = 11,
        EffectControl1 = 12,
        EffectControl2 = 13,
        GenPurpose1 = 16,
        GenPurpose2 = 17,
        GenPurpose3 = 18,
        GenPurpose4 = 19,
        BankSelectLSB = 32,
        ModulationWheelLSB = 33,
        BreathControlLSB = 34,
        FootControllerLSB = 35,
        PortamentoTimeLSB = 36,
        DataEntryLSB = 38,
        ChannelVolumeLSB = 39,
        BalanceLSB = 40,
        PanLSB = 42,
        ExpressionControllerLSB = 43,
        EffectControl1LSB = 44,
        EffectControl2LSB = 45,
        GenPurpose1LSB = 48,
        GenPurpose2LSB = 49,
        GenPurpose3LSB = 51,
        GenPurpose4LSB = 51,
        DamperPedal = 64,
        Portamento = 65,
        Sustenuto = 66,
        SoftPedal = 67,
        LegatoFootswitch = 68,
        Hold2 = 69,
        SoundVariation = 70,
        Timbre = 71,
        ReleaseTime = 72,
        AttackTime = 73,
        Brightness = 74,
        SoundController6 = 75,
        SoundController7 = 76,
        SoundController8 = 77,
        SoundController9 = 78,
        SoundController10 = 79,
        GeneralPurpose5 = 80,
        GeneralPurpose6 = 81,
        GeneralPurpose7 = 82,
        GeneralPurpose8 = 83,
        PortamentoControl = 84,
        Effects1Depth = 91,
        Effects2Depth = 92,
        Effects3Depth = 93,
        Effects4Depth = 94,
        Effects5Depth = 95,
        DataEntryPlus1 = 96,
        DataEntryMinus1 = 97,
        NonRegisteredParameterLSB = 98,
        NonRegisteredParameterMSB = 99,
        RegisteredParameterLSB = 100,
        RegisteredParameterMSB = 101,
        AllSoundOff = 120,
        ResetAllControllers = 121,
        LocalControlOnOff = 122,
        AllNotesOff = 123,
        OmniModeOff = 124,
        OmniModeOn = 125,
        MonoModeOn = 126,  //This one and the next one go back and forth at each other.
        PolyModeOn = 127
    }
}
