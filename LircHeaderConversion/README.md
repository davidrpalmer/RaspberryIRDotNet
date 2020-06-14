# LIRC C Header to C# Constants Conversion

The `LircConstants.cs` file in this repo contains the constants as defined in `lirc.h` file (found on the Pi).

Since macros are used to calculate the values in `lirc.h` simply copying from that file does not work. That is where this little C app comes in. This app prints the constants defined in the header file with their values so they can be copied to C#.

Build this app on the Pi with `gcc lirc-consts.c -o lirc-consts`
