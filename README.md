# xabbo extension
A cross-platform, multi-feature extension for [G-Earth](https://github.com/sirjonasxx/G-Earth) compatible with both Flash and Origins clients.

<img src="https://raw.githubusercontent.com/xabbo/xabbo/refs/heads/main/ext/screenshot.png" width="500">

## Run the latest release

To run the extension, you must install the
[.NET 8 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

You should see *.NET Runtime 8.x.x* at the bottom right of the page.
Choose one of the installers listed for your operating system.

Note that this is not the *.NET Desktop Runtime* which is only available for Windows, as xabbo is
built with the cross-platform [Avalonia](https://avaloniaui.net) UI. However, the desktop runtime
will work since it also includes the .NET runtime.

## Features

For details on the features provided by xabbo, see the [wiki](https://github.com/xabbo/xabbo/wiki).

## Run the latest development source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

```sh
# Clone the 'dev' branch from the repository
git clone https://github.com/xabbo/xabbo -b dev
# Change into the 'xabbo' directory
cd xabbo
# Fetch the submodules (xabbo common, gearth, messages & core)
git submodule update --init
# Run the Avalonia application
dotnet run --project src/Xabbo.Avalonia
```
