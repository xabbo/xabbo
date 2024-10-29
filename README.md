# xabbo extension
A multi feature extension for G-Earth.

<img src="https://github.com/user-attachments/assets/11225be4-a8db-4422-8fec-68583d39f320" width="500">

## Run the latest release

To run the extension, you must install the
[.NET 8 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

You should see *.NET Runtime 8.x.x* at the bottom right of the page.
Choose one of the installers listed for your operating system.

Note that this is not the *.NET Desktop Runtime* which is only available for Windows, as xabbo is
built with the cross-platform [Avalonia](https://avaloniaui.net) UI. However, the desktop runtime
will work since it also includes the .NET runtime.

# Features

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
