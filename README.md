# Video Processing Hints

## Xabe.FFmpeg

- Possible can't streaming/converting on the fly in the memory. Video --> Exctracting frames into the imageXXX.png --> Encoding new video.

## AForge.Video

- Not available for .NET Core :(
- Possible very poor quality of the result video.
- With NuGet installation you have to copy all .dll files from `\packages\AForge.Video.FFMPEG.2.2.5.1-rc\Externals` to `\bin\...`
