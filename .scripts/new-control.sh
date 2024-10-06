#!/bin/bash

controlName=$(gum input --header="Create new control" --placeholder="Control name") || exit 1

echo "Creating control ${controlName}..."
dotnet new avalonia.templatedcontrol \
  -n "$controlName" \
  -o "./src/Xabbo.Avalonia/Controls" \
  --namespace Xabbo.Avalonia.Controls
