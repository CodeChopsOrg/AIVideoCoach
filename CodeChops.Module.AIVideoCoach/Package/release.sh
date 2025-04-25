#!/bin/bash

TargetFramework=$1
ProjectName=$2

mv  Trifoia.Module.AIVideoCoach.nuspec.REMOVE Trifoia.Module.AIVideoCoach.nuspec 

"..\..\oqtane.framework\oqtane.package\nuget.exe" pack Trifoia.Module.AIVideoCoach.nuspec -Properties targetframework=net9.0;projectname=Trifoia.Module.AIVideoCoach
cp -f "*.nupkg" "..\..\oqtane.framework\Oqtane.Server\Packages\"