#!/bin/bash

mono bin/Prebuild.exe /target nant
mono bin/Prebuild.exe /target monodev
mono bin/Prebuild.exe /target vs2005

if [ x$1 == xnant ]; then
    nant -buildfile:OpenMetaverse.build
    exit $?
fi

if [ x$1 == xprimrender ]; then
    nant -buildfile:OpenMetaverse.Rendering.GPL.build
    exit $?
fi
