using ManagedCuda;
using ManagedCuda.CudaFFT;
using ManagedCuda.VectorTypes;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using SkySigService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI;

namespace SkySig.ViewModels
{
    public class IQStreamViewModel : BindableBase
    {
        private readonly int SignalSize;
        private readonly ISDR sdr;
        private readonly Vector2[] points;
        private readonly double[] avgs;
        private readonly double[] vals;
        private int currentAvg = 0;
        private readonly Vector2[] zeroes;
        private object signalOutputLock = new object();
        cuFloatComplex[] signalOutput;
        private float minY;

        private object geoLock = new object();
        CanvasGeometry geo;

        public IQStreamViewModel(ISDR sdr)
        {

            if (sdr == null)
            {
                return;
            }
            this.sdr = sdr;

            SignalSize = 1 << 13;

            signalOutput = new cuFloatComplex[SignalSize];

            Task.Run(() => TransformStream(sdr, SignalSize));

            points = new Vector2[SignalSize + 2];
            zeroes = new Vector2[SignalSize];
            avgs = new double[SignalSize];
            vals = new double[SignalSize];

            for (int i = 0; i < SignalSize; i++)
            {
                points[i] = new Vector2(i, 0);
                zeroes[i] = new Vector2(i, 0);
                avgs[i] = 0;
                vals[i] = 0;
            }
        }

        public float dbFS(double rms)
        {
            return (float)(10 * Math.Log10(rms));
        }

        public double rms(cuFloatComplex v)
        {
            return (v.real * v.real) + (v.imag * v.imag);
        }

        public void UpdatePoints(ICanvasResourceCreator resourceCreator, Size size) 
        {
            lock (signalOutputLock)
            {
                if (points == null)
                    return;

                float wScale = (float)(size.Width / SignalSize);
                float hScale = (float)(size.Height / 100.0f);

                minY = float.MaxValue;

                for (int i = 0; i < signalOutput.Length; i++)
                {
                    points[i].X = i * wScale;
                    points[i].Y = Math.Abs(dbFS(vals[i])) * hScale;

                    if (points[i].Y < minY)
                        minY = points[i].Y;
                }

                points[points.Length - 2].X = (float)size.Width;
                points[points.Length - 2].Y = (float)size.Height;

                points[points.Length - 1].X = 0;
                points[points.Length - 1].Y = (float)size.Height;

                lock (geoLock)
                {
                    geo = CanvasGeometry.CreatePolygon(resourceCreator, points);
                }
            }
        }

        public void DrawPoints(CanvasDrawingSession session, Size size)
        {
            session.Units = CanvasUnits.Dips;
            session.Antialiasing = CanvasAntialiasing.Aliased;
            
            var colorStart = Color.FromArgb(0xCC, 0x00, 0x00, 0xFF);
            var colorStop = Color.FromArgb(0x22, 0x00, 0x00, 0xFF);
            var colorLine = Color.FromArgb(0x33, 0xFF, 0x00, 0x00);
            var colorText = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);

            var brush = new CanvasLinearGradientBrush(session, colorStart, colorStop);

            brush.StartPoint = new Vector2
            {
                X = (float)(size.Width / 2),
                Y = minY
            };

            brush.EndPoint = new Vector2
            {
                X = (float)(size.Width / 2),
                Y = (float)size.Height
            };

            float hScale = (float)(size.Height / 100.0f);

            lock (geoLock)
            {
                if (geo != null)
                {
                    session.FillGeometry(geo, brush);
                }

                for (int i = 0; i < 100; i += 10)
                {
                    session.DrawLine(0, (float)i * hScale, (float)size.Width, (float)i * hScale, colorLine, 1);
                    session.DrawText($"-{i}dBFS", 5.0f, (float)i * hScale, colorText, new Microsoft.Graphics.Canvas.Text.CanvasTextFormat
                    {
                        FontSize = 10
                    });
                }
            }
        }

        public Matrix3x2 GetTransform(Size size)
        {
            return Matrix3x2.CreateScale((float)size.Width / 40000, (float)size.Height / 100);
        }

        void TransformStream(ISDR sdr, int SignalSize)
        {
            // Lookup table inspiration from
            // http://cgit.osmocom.org/gr-osmosdr/tree/lib/rtl/rtl_source_c.cc#n178
            const int lutSize = 0x100;
            var lookup = new float[lutSize];
            for (int i = 0; i < lutSize; i++)
            {
                lookup[i] = (i - 127.4f) / 128.0f;
            }

            var hannLookup = new float[SignalSize];
            for (int i = 0; i < SignalSize; i++)
            {
                hannLookup[i] = (float)(0.5 * (1 - Math.Cos(2 * Math.PI * i / 2047)));
            }

            using (CudaContext ctx = new CudaContext())
            {
                CudaFFTPlan1D fftplan = new CudaFFTPlan1D(SignalSize, cufftType.C2C, 1);

                cuFloatComplex[] signal = new cuFloatComplex[SignalSize];
                
                CudaDeviceVariable<cuFloatComplex> signalData = new CudaDeviceVariable<cuFloatComplex>(signal.Length);
                CudaDeviceVariable<cuFloatComplex> outputData = new CudaDeviceVariable<cuFloatComplex>(signal.Length);

                byte[] buf = new byte[SignalSize * 2];
                while (true)
                {
                    int totalRead = 0;
                    while (totalRead < buf.Length)
                    {
                        int n = sdr.IQ.Read(buf, totalRead, buf.Length - totalRead);
                        totalRead += n;
                    }

                    for (int i = 0; i < buf.Length; i += 2)
                    {
                        int sigIndex = i / 2;
                        signal[sigIndex].real = lookup[buf[i]];
                        signal[sigIndex].imag = lookup[buf[i + 1]];
                    }

                    cuFloatComplex avg = new cuFloatComplex {
                        real = 0,
                        imag = 0
                    };
                    for (int i = 0; i < signal.Length; i++)
                    {
                        avg.real += signal[i].real;
                        avg.imag += signal[i].imag;
                    }

                    avg.real /= signal.Length;
                    avg.imag /= signal.Length;

                    for (int i = 0; i < signal.Length; i++)
                    {
                        signal[i].real -= avg.real;
                        signal[i].imag -= avg.imag;

                        signal[i].real *= hannLookup[i];
                        signal[i].imag *= hannLookup[i];
                    }

                    signalData.CopyToDevice(signal, 0, 0, signal.Length);

                    //execute plan
                    fftplan.Exec(signalData.DevicePointer, outputData.DevicePointer, TransformDirection.Forward);

                    lock (signalOutputLock)
                    {
                        outputData.CopyToHost(signalOutput);

                        var off = SignalSize / 2;
                        for (int i = 0; i < off; i++)
                        {
                            avgs[i] += rms(signalOutput[i + off]);
                            avgs[i + off] += rms(signalOutput[i]);
                        }

                        ++currentAvg;

                        if (currentAvg == 30)
                        {
                            for (int i = 0; i < avgs.Length; i++)
                            {
                                vals[i] = avgs[i] / sdr.SampleRate / currentAvg;
                                avgs[i] = 0;
                            }
                            currentAvg = 0;
                        }
                    }
                }
            }
        }
    }
}
