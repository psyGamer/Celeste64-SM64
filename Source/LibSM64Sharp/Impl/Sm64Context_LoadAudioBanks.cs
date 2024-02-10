using LibSM64Sharp.LowLevel;

namespace LibSM64Sharp.Impl;

public partial class Sm64Context
{
    private class Sm64CtlEntry : ISm64CtlEntry
    {
        private LowLevelSm64CtlEntry lowLevelImpl_;

        public Sm64CtlEntry(LowLevelSm64CtlEntry lowLevelImpl)
        {
            lowLevelImpl_ = lowLevelImpl;

            {
                var instruments = new List<ISm64Instrument>();
                var lowLevelInstruments =
                    MarshalUtil.MarshalArrayOfRefs_<LowLevelSm64Instrument>(
                        lowLevelImpl.instruments, lowLevelImpl.numInstruments);
                foreach (var lowLevelInstrument in lowLevelInstruments)
                {
                    instruments.Add(new Sm64Instrument(lowLevelInstrument));
                }

                Instruments = instruments;
            }

            {
                var drums = new List<ISm64Drum>();
                var lowLevelDrums =
                    MarshalUtil.MarshalArrayOfRefs_<LowLevelSm64Drum>(
                        lowLevelImpl.drums, lowLevelImpl.numDrums);
                foreach (var lowLevelDrum in lowLevelDrums)
                {
                    drums.Add(new Sm64Drum(lowLevelDrum));
                }

                Drums = drums;
            }
        }

        public IReadOnlyList<ISm64Instrument> Instruments { get; }
        public IReadOnlyList<ISm64Drum> Drums { get; }
    }

    private class Sm64Drum : ISm64Drum
    {
        private LowLevelSm64Drum lowLevelImpl_;

        public Sm64Drum(LowLevelSm64Drum lowLevelImpl)
        {
            lowLevelImpl_ = lowLevelImpl;
        }

        public bool Loaded { get; }
        public byte ReleaseRate { get; }
        public byte Pan { get; }
        public ISm64AudioBankSound Sound { get; } = null!;
    }

    private class Sm64Instrument : ISm64Instrument
    {
        private LowLevelSm64Instrument lowLevelImpl_;

        public Sm64Instrument(LowLevelSm64Instrument lowLevelImpl)
        {
            lowLevelImpl_ = lowLevelImpl;
            Loaded = lowLevelImpl.loaded != 0;
            ReleaseRate = lowLevelImpl.releaseRate;

            NormalRangeHi = lowLevelImpl.normalRangeHi;
            NormalRangeLo = lowLevelImpl.normalRangeLo;

            if (lowLevelImpl.normalNotesSound.sample.ToInt64() != 0)
            {
                NormalNotesSound =
                    new Sm64AudioBankSound(lowLevelImpl.normalNotesSound);
            }

            if (lowLevelImpl.highNotesSound.sample.ToInt64() != 0)
            {
                HighNotesSound =
                    new Sm64AudioBankSound(lowLevelImpl.highNotesSound);
            }

            if (lowLevelImpl.lowNotesSound.sample.ToInt64() != 0)
            {
                LowNotesSound =
                    new Sm64AudioBankSound(lowLevelImpl.lowNotesSound);
            }
        }

        public bool Loaded { get; }
        public byte ReleaseRate { get; }
        public byte NormalRangeLo { get; }
        public byte NormalRangeHi { get; }
        public ISm64AudioBankSound? LowNotesSound { get; }
        public ISm64AudioBankSound NormalNotesSound { get; } = null!;
        public ISm64AudioBankSound? HighNotesSound { get; }
    }

    private class Sm64AudioBankSound : ISm64AudioBankSound
    {
        private LowLevelSm64AudioBankSound lowLevelImpl_;

        public Sm64AudioBankSound(LowLevelSm64AudioBankSound lowLevelImpl)
        {
            lowLevelImpl_ = lowLevelImpl;

            if (lowLevelImpl.sample.ToInt64() == 0)
            {
                throw new ArgumentException("Sound pointer is null.");
            }

            Sample = new Sm64AudioBankSample(
                MarshalUtil.MarshalRef<LowLevelSm64AudioBankSample>(
                    lowLevelImpl.sample));
            Tuning = lowLevelImpl.tuning;
        }

        public ISm64AudioBankSample Sample { get; }
        public float Tuning { get; }
    }

    private class Sm64AudioBankSample : ISm64AudioBankSample
    {
        private LowLevelSm64AudioBankSample lowLevelImpl_;

        public Sm64AudioBankSample(LowLevelSm64AudioBankSample lowLevelImpl)
        {
            lowLevelImpl_ = lowLevelImpl;

            Loaded = lowLevelImpl.loaded != 0;
            Loop =
                new Sm64AdpcmLoop(
                    MarshalUtil
                        .MarshalRef<LowLevelSm64AdpcmLoop>(lowLevelImpl.loop));
            Book =
                new Sm64AdpcmBook(
                    MarshalUtil
                        .MarshalRef<LowLevelSm64AdpcmBook>(lowLevelImpl.book));
            Samples =
                MarshalUtil.MarshalArray<byte>(lowLevelImpl.sampleAddr,
                    (int)lowLevelImpl.sampleSize);
        }

        public bool Loaded { get; }
        public byte[] Samples { get; }
        public ISm64AdpcmLoop Loop { get; }
        public ISm64AdpcmBook Book { get; }
    }

    public class Sm64AdpcmLoop : ISm64AdpcmLoop
    {
        public Sm64AdpcmLoop(LowLevelSm64AdpcmLoop lowLevelImpl)
        {
            Start = lowLevelImpl.start;
            End = lowLevelImpl.end;
            Count = lowLevelImpl.count;
            Pad = lowLevelImpl.pad;
            State = lowLevelImpl.state;
        }

        public uint Start { get; }
        public uint End { get; }
        public uint Count { get; }
        public uint Pad { get; }
        public short[] State { get; }
    }

    public class Sm64AdpcmBook : ISm64AdpcmBook
    {
        public Sm64AdpcmBook(LowLevelSm64AdpcmBook lowLevelImpl)
        {
            Order = lowLevelImpl.order;
            NPredictors = lowLevelImpl.npredictors;
            Predictors = lowLevelImpl.book;
        }

        public int Order { get; }
        public int NPredictors { get; }
        public short[] Predictors { get; }
    }
}