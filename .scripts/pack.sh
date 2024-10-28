#!/bin/bash

set -e

log() {
  level=$1
  shift
  gum log -l $level -- $@ || echo -- $level: $@
}

info() { log info $@; }

extResourceDir=./ext
buildDir=./bin
outDir=./out

rm -rf $buildDir $outDir
mkdir -p $buildDir $outDir

# Build cross-platform binaries
# Linux/Mac builds output required so/dylib files
rids=(win-x64 linux-x64 osx-{x64,arm64})
for rid in ${rids[@]}; do
  info "Building for $rid"
  dotnet publish src/Xabbo.Avalonia/Xabbo.Avalonia.csproj -r $rid -o $buildDir/$rid
done

info 'Copying output files'
mv $buildDir/win-x64/{Xabbo.Avalonia.exe,xabbo.exe}
cp -r $buildDir/win-x64 $buildDir/xabbo

info 'Merging linux output files'
cp $buildDir/linux-x64/*.so $buildDir/xabbo/
cp $buildDir/linux-x64/Xabbo.Avalonia $buildDir/xabbo/xabbo-linux-x64

info 'Merging macOS output files'
cp $buildDir/osx-x64/*.dylib $buildDir/xabbo/
cp $buildDir/osx-x64/Xabbo.Avalonia $buildDir/xabbo/xabbo-osx-x64
cp $buildDir/osx-arm64/Xabbo.Avalonia $buildDir/xabbo/xabbo-osx-arm64

# https://github.com/konoui/lipo
info 'Creating universal macOS executable'
lipo -output $buildDir/xabbo/xabbo-osx -create $buildDir/xabbo/xabbo-osx-{arm64,x64}
rm $buildDir/xabbo/xabbo-osx-{arm64,x64}

info 'Cleaning output files'
rm $buildDir/xabbo/*.xml
rm $buildDir/xabbo/*.pdb

info 'Copying extension files'
cp $extResourceDir/*.png $outDir/

info 'Creating extension.zip'
zipDir=$(realpath $outDir --relative-to $buildDir/xabbo)
(cd $buildDir/xabbo; zip -r $zipDir/extension.zip *)

info 'Writing extension.json'
version=$(dotnet-gitversion . /ShowVariable SemVer)
frameworkVersion=$(dotnet-gitversion ./lib/gearth /ShowVariable SemVer)
updateDate=$(date -u '+%d-%m-%Y %H:%M:%S')
sed \
  -e "s/{Version}/$version/" \
  -e "s/{FrameworkVersion}/$frameworkVersion/" \
  -e "s/{UpdateDate}/$updateDate/" \
  $extResourceDir/extension.json > $outDir/extension.json

info 'Cleaning up'
rm -rf $buildDir