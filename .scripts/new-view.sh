#!/bin/bash

viewName=$(gum input --header="Create new view" --placeholder="View name")

if [[ -n $viewName ]]; then
  dir=$(dirname "$viewName")
  base=$(basename "$viewName")
  echo "Creating view ${viewName}..."
  dotnet new avalonia.usercontrol -n "$base" -o "./src/Xabbo.Ext.Avalonia/Views/$dir" --namespace Xabbo.Ext.Avalonia.Views
fi