using Asceils;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xabe.FFmpeg; // <<<<<<<<<<<<<<<<<<<<<

namespace AsceilsMotion
{
    class Program
    {
        const string FFMPEG_PATH = @"C:\dev\ffmpeg\bin";
        const string MOVIESAMPLE_PATH = @"c:\dev\mov\johnny.mp4";
        const string DEST_PATH = @"C:\dev\mov\out\";
        const string RESULT_PATH = @"C:\dev\mov\res\";
        const string MOV_PATH = @"C:\dev\mov\";

        static async Task Main(string[] args)
        {
            FFmpeg.SetExecutablesPath(FFMPEG_PATH);
            IConversion conv;

            IMediaInfo info = await FFmpeg.GetMediaInfo(MOVIESAMPLE_PATH);
            IVideoStream videoStream = info.VideoStreams
                .First()
                ?.SetCodec(VideoCodec.png);

            int everyFrame = 1;

            //////////////////////////////////////////////////

            conv = FFmpeg.Conversions.New()
                .AddStream(videoStream)
                .ExtractEveryNthFrame(everyFrame, x => DEST_PATH + x + ".png");

            conv.OnProgress += (s, a) => {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"{a.Percent}%");
            };

            await conv.Start();

            //////////////////////////////////////////////////

            var options = new PicToAsciiOptions() {
                FixedDimension = PicToAsciiOptions.Fix.Vertical,
                FixedSize = 50,
                SymbolAspectRatio = 11f / 18f,
                AsciiTable = PicToAsciiOptions.ASCIITABLE_SYMBOLIC_LIGHT
            };

            var pic2ascii = new PicToAscii(options);

            var font = SystemFonts.CreateFont("Lucida Console", 18);

            Parallel.ForEach(ImageSamples, filename => {
                IReadOnlyList<ColorTape> colorTapes;
                try {
                    using Stream stream = File.OpenRead(filename);
                    colorTapes = pic2ascii.Convert(stream);
                }
                catch {
                    return;
                }

                string fname = new FileInfo(filename).Name;
                Console.WriteLine(fname);

                ProcessTapes(colorTapes, fname, font);

            });

            ///////////////////////////////////////////////

            List<string> files = Directory.EnumerateFiles(RESULT_PATH).ToList();

            conv = FFmpeg.Conversions.New()
                //.SetInputFrameRate((int)videoStream.Framerate / everyFrame)
                .BuildVideoFromImages(files)
                .SetFrameRate(videoStream.Framerate)
                .SetOutputFormat(Format.mp4)
                .SetOutput($"{MOV_PATH}~{Environment.TickCount}.mp4");

            conv.OnProgress += (s, a) => {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"{a.Percent}%");
            };

            await conv.Start();

        }

        private static void ProcessTapes(IReadOnlyList<ColorTape> colorTapes, string name, Font font)
        {
            Image<Rgb24> img = new Image<Rgb24>(1920, 1080);
            RendererOptions ro = new RendererOptions(font);

            img.Mutate(x => {
                PointF pos = new PointF(0, 0);
                foreach (var tape in colorTapes) {
                    string[] lines = tape.Chunk.Split('\n');

                    for(int i = 0; i < lines.Length; i++) { 
                        string text = lines[i];
                        var textSize = TextMeasurer.Measure(text, ro);
                        x.DrawText(text, font, Cc2rgb(tape.ForeColor), pos);
                        pos.X += textSize.Width;
                        if (i < lines.Length - 1) {
                            pos.X = 0;
                            pos.Y += textSize.Height;
                        }
                    }
                }
                x.Contrast(2);
            });

            img.SaveAsPng(RESULT_PATH + name);
        }

        private static Rgb24 Cc2rgb(ConsoleColor cc)
        {
            switch (cc) {
                case ConsoleColor.Black: return new Rgb24(0, 0, 0);
                case ConsoleColor.DarkBlue: return new Rgb24(0, 0, 250);
                case ConsoleColor.DarkGreen: return new Rgb24(0, 250, 0);
                case ConsoleColor.DarkCyan: return new Rgb24(0, 250, 250);
                case ConsoleColor.DarkRed: return new Rgb24(250, 0, 0);
                case ConsoleColor.DarkMagenta: return new Rgb24(250, 0, 250);
                case ConsoleColor.DarkYellow: return new Rgb24(250, 250, 0);
                case ConsoleColor.Gray: return new Rgb24(250, 250, 250);
                case ConsoleColor.DarkGray: return new Rgb24(220, 220, 220);
                case ConsoleColor.Blue: return new Rgb24(0, 0, 255);
                case ConsoleColor.Green: return new Rgb24(0, 255, 0);
                case ConsoleColor.Cyan: return new Rgb24(0, 255, 255);
                case ConsoleColor.Red: return new Rgb24(255, 0, 0);
                case ConsoleColor.Magenta: return new Rgb24(0, 200, 255);
                case ConsoleColor.Yellow: return new Rgb24(255, 255, 0);
                default:
                case ConsoleColor.White: return new Rgb24(255, 255, 255);
            }
        }

        private static IEnumerable<string> ImageSamples => Directory
            .GetFiles(DEST_PATH, "*", SearchOption.TopDirectoryOnly)
            .Where(f => f.LastIndexOf(".jpg") > -1
                     || f.LastIndexOf(".jpeg") > -1
                     || f.LastIndexOf(".png") > -1);

        private static void PrintTapes(IReadOnlyList<ColorTape> colorTapes)
        {
            foreach (var tape in colorTapes) {
                Console.ForegroundColor = tape.ForeColor;
                Console.Write(tape.Chunk);
            }

            Console.ResetColor();
        }
    }
}