#!/bin/bash

# Add .NET tools to PATH
export PATH="$PATH:/Users/user/.dotnet/tools"

# Install Playwright browsers
echo "Installing Playwright browsers..."
playwright install chromium

echo "Playwright browsers installed successfully!" 