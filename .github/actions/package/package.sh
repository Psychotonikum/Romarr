#!/bin/bash

outputFolder=_output
artifactsFolder=_artifacts
uiFolder="$outputFolder/net10.0/UI"
framework="${FRAMEWORK:=net10.0}"

rm -rf $artifactsFolder
mkdir $artifactsFolder

for runtime in _output/*
do
  name="${runtime##*/}"
  folderName="$runtime/$framework"
  romarrFolder="$folderName/Romarr"
  archiveName="Romarr.$BRANCH.$ROMARR_VERSION.$name"

  if [[ "$name" == 'net10.0' ]]; then
    continue
  fi
    
  echo "Creating package for $name"

  echo "Copying UI"
  cp -r $uiFolder $romarrFolder
  
  echo "Setting permissions"
  find $romarrFolder -name "ffprobe" -exec chmod a+x {} \;
  find $romarrFolder -name "Romarr" -exec chmod a+x {} \;
  find $romarrFolder -name "Romarr.Update" -exec chmod a+x {} \;
  
  if [[ "$name" == *"osx"* ]]; then
    echo "Creating macOS package"
      
    packageName="$name-app"
    packageFolder="$outputFolder/$packageName"
      
    rm -rf $packageFolder
    mkdir $packageFolder
      
    cp -r distribution/macOS/Romarr.app $packageFolder
    mkdir -p $packageFolder/Romarr.app/Contents/MacOS
      
    echo "Copying Binaries"
    cp -r $romarrFolder/* $packageFolder/Romarr.app/Contents/MacOS
      
    echo "Removing Update Folder"
    rm -r $packageFolder/Romarr.app/Contents/MacOS/Romarr.Update
              
    echo "Packaging macOS app Artifact"
    (cd $packageFolder; zip -rq "../../$artifactsFolder/$archiveName-app.zip" ./Romarr.app)
  fi

  echo "Packaging Artifact"
  if [[ "$name" == *"linux"* ]] || [[ "$name" == *"osx"* ]] || [[ "$name" == *"freebsd"* ]]; then
    tar -zcf "./$artifactsFolder/$archiveName.tar.gz" -C $folderName Romarr
	fi
    
  if [[ "$name" == *"win"* ]]; then
    if [ "$RUNNER_OS" = "Windows" ]
      then
        (cd $folderName; 7z a -tzip "../../../$artifactsFolder/$archiveName.zip" ./Romarr)
      else
      (cd $folderName; zip -rq "../../../$artifactsFolder/$archiveName.zip" ./Romarr)
    fi
	fi
done
