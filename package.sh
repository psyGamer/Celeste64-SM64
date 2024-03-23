#!/usr/bin/env sh

rm -rd bin Source/bin Source/obj
dotnet build -c Release

if [ $# -eq 0 ]
  then
  zip -r SuperMario64.zip Fuji.json Libraries Shaders
else
  zip -r SuperMario64-v$1.zip Fuji.json Libraries Shaders
fi
