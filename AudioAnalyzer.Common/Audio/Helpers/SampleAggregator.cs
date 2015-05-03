﻿using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AudioAnalyzer.Common.Audio.Helpers
{
    
    [Serializable()]
    public class SampleAggregator
    {
        // volume
        public event EventHandler<MaxSampleEventArgs> MaximumCalculated;
        private float maxValue;
        private float minValue;
        public int NotificationCount { get; set; }
        int count;

        // FFT
        public event EventHandler<FftEventArgs> FftCalculated;
        public bool PerformFFT { get; set; }
        private Complex[] fftBuffer = new Complex[1024];
        private FftEventArgs fftArgs;
        private int fftPos;

        public SampleAggregator()
        {
            fftArgs = new FftEventArgs(fftBuffer);
        }

        public void Reset()
        {
            count = 0;
            maxValue = minValue = 0;
        }

        public void Add(float value)
        {
            if (PerformFFT && FftCalculated != null)
            {
                fftBuffer[fftPos].X = value;
                fftBuffer[fftPos].Y = 0;
                fftPos++;
                if (fftPos >= fftBuffer.Length)
                {
                    fftPos = 0;
                    // 1024 = 2^10
                    FastFourierTransform.FFT(true, 10, fftBuffer);
                    FftCalculated(this, fftArgs);
                }
            }

            maxValue = Math.Max(maxValue, value);
            minValue = Math.Min(minValue, value);
            count++;
            if (count >= NotificationCount && NotificationCount > 0)
            {
                if (MaximumCalculated != null)
                {
                    MaximumCalculated(this, new MaxSampleEventArgs(minValue, maxValue));
                }
                Reset();
            }
        }
    }
    [Serializable()]
    public class MaxSampleEventArgs : EventArgs
        {
            public float MaxSample { get; private set; }
            public float MinSample { get; private set; }
            [DebuggerStepThrough]
            public MaxSampleEventArgs(float minValue, float maxValue)
            {
                this.MaxSample = maxValue;
                this.MinSample = minValue;
            }
            
        }
    [Serializable()]
    public class FftEventArgs : EventArgs
        {
            public Complex[] Result { get; private set; }
            [DebuggerStepThrough]
            public FftEventArgs(Complex[] result)
            {
                this.Result = result;
            }
           
        }
    }