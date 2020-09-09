using AForge.Video;
using AForge.Video.FFMPEG; // <<<<<<<<<<<<<<<<<<<<<
using Asceils;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace AsceilsForge
{
    class Program
    {
        const string MOVIESAMPLE_PATH = @"c:\dev\mov\rick.mp4";
        const string MOV_PATH = @"C:\dev\mov\";
        
        static VideoFileWriter writer;
        static PicToAscii pic2ascii;
        static Font font;
        static int width  = 1920;
        static int height = 1080;
        static Color[] Cc2rgb = {
                Color.FromArgb(0, 0, 0),
                Color.FromArgb(0, 0, 250),
                Color.FromArgb(0, 250, 0),
                Color.FromArgb(0, 250, 250),
                Color.FromArgb(250, 0, 0),
                Color.FromArgb(250, 0, 250),
                Color.FromArgb(250, 250, 0),
                Color.FromArgb(250, 250, 250),
                Color.FromArgb(220, 220, 220),
                Color.FromArgb(0, 0, 255),
                Color.FromArgb(0, 255, 0),
                Color.FromArgb(0, 255, 255),
                Color.FromArgb(255, 0, 0),
                Color.FromArgb(0, 200, 255),
                Color.FromArgb(255, 255, 0),
                Color.FromArgb(255, 255, 255) };

        static void Main(string[] args)
        {
            var options = new PicToAsciiOptions() {
                FixedDimension = PicToAsciiOptions.Fix.Vertical,
                FixedSize = 54,
                SymbolAspectRatio = 7f / 12f,
                AsciiTable = PicToAsciiOptions.ASCIITABLE_SYMBOLIC_LIGHT
            };

            pic2ascii = new PicToAscii(options);
            
            font = new Font("Lucida Console", 12, FontStyle.Regular, GraphicsUnit.Pixel);

            writer = new VideoFileWriter( );
            writer.Open($"{MOV_PATH}~test.avi", width, height, 30, VideoCodec.MPEG4, 6000000);
            
            VideoFileSource videoSource = new VideoFileSource(MOVIESAMPLE_PATH);
            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.PlayingFinished += VideoSource_PlayingFinished;
            videoSource.Start();

            Console.ReadLine();
            if (videoSource.IsRunning) {
                videoSource.SignalToStop();
                Console.ReadLine();
            }
        }

        private static void VideoSource_PlayingFinished(object sender, ReasonToFinishPlaying reason)
        {
            writer.Close();

            Console.WriteLine();
            Console.WriteLine("*** FIN ***");
        }

        private static void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var tapes = pic2ascii.Convert(eventArgs.Frame);
            Bitmap result = ProcessTapes(tapes);
            writer.WriteVideoFrame(result);

            Console.Write(".");
        }

        private static Bitmap ProcessTapes(IReadOnlyList<ColorTape> colorTapes)
        {
            Bitmap img = new Bitmap(1920, 1080);
            Graphics g = Graphics.FromImage(img);
            var charSize = g.MeasureString("'", font);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            PointF pos = new PointF(0, 0);
            foreach (var tape in colorTapes) {
                Color c = Cc2rgb[(int)tape.ForeColor];
                var brush = new SolidBrush(c);

                for (int i = 0; i < tape.Chunk.Length; i++) {
                    if (tape.Chunk[i] == '\n') {
                        pos.X = 0;
                        pos.Y += charSize.Height;
                    }
                    else {
                        g.DrawString(tape.Chunk[i].ToString(), font, brush, pos);
                        pos.X += charSize.Width;
                    }
                }
            }

            return img;
        }
    }
}
