#!/bin/bash

mono bin/Prebuild.exe /target nant
mono bin/Prebuild.exe /target monodev
mono bin/Prebuild.exe /target vs2005

if [ x$1 == xnant ]; then
    nant -buildfile:OpenMetaverse.build
    RES=$?
    echo Build Exit Code: $RES
    exit $RES
fi

if [ x$1 == xprimrender ]; then
    nant -buildfile:OpenMetaverse.Rendering.GPL.build
    exit $?
fi

if [ x$1 == xopenjpeg ]; then
   ARCH=`arch`
   cd openjpeg-dotnet
   if [ $ARCH == x86_64 ]; then
      # since we're a 64bit host, compile a 32bit vesion of openjpeg
      make ARCH=-i686 ARCHFLAGS=-m32 install
   fi
      # compile for default detected platform
      make install
fi
