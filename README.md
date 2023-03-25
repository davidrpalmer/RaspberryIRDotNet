# Raspberry IR .NET

This is a .NET Core library for accessing infrared devices (found at `/dev/lirc*`) on a Raspberry Pi. This library is designed specifically for the `gpio-ir` and `pwm-ir-tx`/`gpio-ir-tx` drivers on the Raspberry Pi, however it may (at least partially) work with other IR drivers on other platforms.

This library supports both transmitting and receiving IR codes.

## Prerequisites
 * Visual Studio 2019 (or some other tools for building a .NET Core app)
 * A Raspberry Pi running Raspberry Pi OS (previously called Raspbian)
 * [.NET Core 3.1 Runtime](https://dotnet.microsoft.com/download/dotnet-core/) installed on the Pi
 * The necessary IR drivers loaded via `/boot/config.txt` to create the `/dev/lircX` device(s)
   * For IR receiving this is `gpio-ir`
   * For IR sending (blasting) this is either `pwm-ir-tx` or `gpio-ir-tx`
 * Some hardware connected to the GPIO on the Pi for sending and/or receiving IR


## Getting Started

Get from [NuGet.org](https://www.nuget.org/packages/RaspberryIRDotNet/)

The `RaspberryIRDotNetExamples` console app (not in the NuGet package) contains some examples of how to use the library.

First check `DemoConfig.cs` in `RaspberryIRDotNetExamples` has the right paths to your `/dev/lirc` devices as per the `<summary>` descriptions in that file.

The solution and project files are setup for building with Visual Studio 2019. Use the `RasPiDebug` publish profile in the `RaspberryIRDotNetExamples` project to build the library and a sample console application. Copy the published output to your Raspberry Pi. 

Run the `RaspberryIRDotNetExamples` console app (you may need to `chmod u+x RaspberryIRDotNetExamples`). This project will demo some of the functionality of the `RaspberryIRDotNet` library. Take a look at it's source code to see how the console app makes use of the library.

## Supported Formats
 * NEC (basic and extended)
 * RC5 (basic and extended)
 * RC6
   * Mode 0
   * Mode 6A - 8bit and 16bit address modes are supported. However the payload format is not defined in this standard. So you will either have to just take the payload as raw bits, or add your own class to decode it. The `Xbox360Packet` and `Xbox360Converter` classes show how this can be done.
 * Kasiekyo (Panasonic only)


## Room for Improvement
When decoding raw IR into a formatted message standard, such as NEC, this library does not handle repeat codes.
