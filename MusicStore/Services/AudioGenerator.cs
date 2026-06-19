using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using MeltySynth;
using MusicStore.Services.Interfaces;
using NWaves.Audio;
using NWaves.Signals;
using DryWetMidiFile = Melanchall.DryWetMidi.Core.MidiFile;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace MusicStore.Services;

public class AudioGenerator(IWebHostEnvironment env) : IAudioGenerator
{
    private const int SampleRate = 44100;

    private static readonly int[][] Progressions =
    [
        [0, 7, 9, 5],
        [0, 5, 7, 0],
        [0, 4, 5, 0],
        [0, 9, 5, 7],
        [0, 7, 4, 5]
    ];

    private static readonly int[][] Scales =
    [
        [0, 2, 4, 5, 7, 9, 11],
        [0, 2, 3, 5, 7, 8, 10],
        [0, 2, 4, 7, 9]
    ];

    private static readonly int[] MelodyInstruments = [1, 5, 11, 13, 40, 74, 81];
    private static readonly int[] PadInstruments = [48, 49, 52, 53, 89];
    private static readonly int[] BassInstruments = [32, 33, 34, 38];

    private readonly Synthesizer _synth = new(
        Path.Combine(env.ContentRootPath, "Assets", "TimGM6mb.sf2"), SampleRate);

    public byte[] Generate(int localSeed, int durationSeconds)
    {
        var random = new Random(localSeed);
        var midiFile = Compose(random, durationSeconds);
        return RenderToWav(midiFile, durationSeconds);
    }

    private static DryWetMidiFile Compose(Random random, int durationSeconds)
    {
        var scale = Scales[random.Next(Scales.Length)];
        var progression = Progressions[random.Next(Progressions.Length)];
        var rootMidi = random.Next(48, 60);
        var bpm = random.Next(84, 118);
        var barSeconds = 4.0 * 60.0 / bpm;
        var barCount = Math.Max(4, (int)Math.Ceiling(durationSeconds / barSeconds) + 1);

        var melodyInstrument = (SevenBitNumber)MelodyInstruments[random.Next(MelodyInstruments.Length)];
        var padInstrument = (SevenBitNumber)PadInstruments[random.Next(PadInstruments.Length)];
        var bassInstrument = (SevenBitNumber)BassInstruments[random.Next(BassInstruments.Length)];

        var tempoMap = TempoMap.Create(Tempo.FromBeatsPerMinute(bpm));
        var triad = new[] { Interval.Zero, Interval.Four, Interval.Seven };

        var pad = new PatternBuilder()
            .ProgramChange(padInstrument)
            .SetNoteLength(MusicalTimeSpan.Whole);

        var bass = new PatternBuilder()
            .ProgramChange(bassInstrument)
            .SetNoteLength(MusicalTimeSpan.Quarter);

        var melody = new PatternBuilder()
            .ProgramChange(melodyInstrument)
            .SetNoteLength(MusicalTimeSpan.Eighth);

        var melodyDegree = random.Next(scale.Length);

        for (var bar = 0; bar < barCount; bar++)
        {
            var chord = rootMidi + progression[bar % progression.Length];
            pad.Chord(triad, Note.Get((SevenBitNumber)chord));
            bass.Note(Note.Get((SevenBitNumber)Math.Clamp(chord - 24, 28, 55))).Repeat(3);

            for (var step = 0; step < 8; step++)
            {
                if (random.NextDouble() < 0.22) continue;

                melodyDegree = Math.Clamp(melodyDegree + (random.Next(2) * 2 - 1), 0, scale.Length - 1);
                melody.Note(Note.Get((SevenBitNumber)Math.Clamp(rootMidi + scale[melodyDegree] + 12, 55, 96)));
            }
        }

        return new DryWetMidiFile(
            pad.Build().ToTrackChunk(tempoMap, (FourBitNumber)0),
            bass.Build().ToTrackChunk(tempoMap, (FourBitNumber)1),
            melody.Build().ToTrackChunk(tempoMap, (FourBitNumber)2));
    }

    private byte[] RenderToWav(DryWetMidiFile midiFile, int durationSeconds)
    {
        using var midiStream = new MemoryStream();
        midiFile.Write(midiStream);
        midiStream.Position = 0;

        var sequencer = new MidiFileSequencer(_synth);
        sequencer.Play(new MidiFile(midiStream), false);

        var buffer = new float[SampleRate * durationSeconds];
        sequencer.RenderMono(buffer);

        return new WaveFile(new DiscreteSignal(SampleRate, buffer)).GetBytes(normalized: true);
    }
}