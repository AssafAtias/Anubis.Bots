# Anubis LinkedIn Bot

A LinkedIn automation bot built with .NET Core and Selenium WebDriver.

## Prerequisites

- .NET 8.0 SDK (8.0.409 or later)
- Chrome browser installed
- ChromeDriver compatible with your Chrome version

## Installation

1. Install .NET 8.0 SDK:
   ```bash
   # On macOS with Homebrew
   brew install dotnet
   
   # Or download from https://dotnet.microsoft.com/download
   ```

2. Install ChromeDriver:
   ```bash
   # On macOS with Homebrew
   brew install chromedriver
   
   # Or download from https://chromedriver.chromium.org/
   ```

## Building and Running

1. Restore dependencies:
   ```bash
   dotnet restore
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

## Configuration

- Update credentials in `Program.cs` (lines 35-36)
- Ensure Chrome extensions are in the `ChromeExtensions` folder
- The bot will create a `cookies.txt` file for session persistence

## Features

- LinkedIn login automation
- Post publishing
- Connection requests
- Group interactions
- Hashtag-based interactions
- Cookie management for session persistence

## Notes

- This project has been converted from .NET Framework to .NET Core for Mac compatibility
- All paths are now cross-platform compatible
- Nullable reference types are enabled for better type safety 