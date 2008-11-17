# Linux makefile for OpenJPEG

VER_MAJOR = 2
VER_MINOR = 1.3.0-dotnet-1

SRCS = ./libopenjpeg/bio.c ./libopenjpeg/cio.c ./libopenjpeg/dwt.c ./libopenjpeg/event.c ./libopenjpeg/image.c ./libopenjpeg/j2k.c ./libopenjpeg/j2k_lib.c ./libopenjpeg/jp2.c ./libopenjpeg/jpt.c ./libopenjpeg/mct.c ./libopenjpeg/mqc.c ./libopenjpeg/openjpeg.c ./libopenjpeg/pi.c ./libopenjpeg/raw.c ./libopenjpeg/t1.c ./libopenjpeg/t2.c ./libopenjpeg/tcd.c ./libopenjpeg/tgt.c
CPPSRCS = ./dotnet/dotnet.cpp
INCLS = ./libopenjpeg/bio.h ./libopenjpeg/cio.h ./libopenjpeg/dwt.h ./libopenjpeg/event.h ./libopenjpeg/fix.h ./libopenjpeg/image.h ./libopenjpeg/int.h ./libopenjpeg/j2k.h ./libopenjpeg/j2k_lib.h ./libopenjpeg/jp2.h ./libopenjpeg/jpt.h ./libopenjpeg/mct.h ./libopenjpeg/mqc.h ./libopenjpeg/openjpeg.h ./libopenjpeg/pi.h ./libopenjpeg/raw.h ./libopenjpeg/t1.h ./libopenjpeg/t2.h ./libopenjpeg/tcd.h ./libopenjpeg/tgt.h ./libopenjpeg/opj_malloc.h ./libopenjpeg/opj_includes.h ./dotnet/dotnet.h
INCLUDE = -Ilibopenjpeg

# General configuration variables:
CC = gcc
AR = ar

OSNAME = $(shell uname -s)
ifeq ($(OSNAME), Linux)
  ARCH = $(shell uname -m)
  ifeq ($(ARCH), x86_64)
    ARCH=-x86_64
  else
    ARCH=
  endif
endif
# Converts cr/lf to just lf
DOS2UNIX = dos2unix

COMPILERFLAGS = -O3 -fPIC
LIBRARIES = -lstdc++

MODULES = $(SRCS:.c=.o)
CPPMODULES = $(CPPSRCS:.cpp=.o)
CFLAGS = $(COMPILERFLAGS) $(INCLUDE)

TARGET  = openjpeg-dotnet
SHAREDLIB = lib$(TARGET)-$(VER_MAJOR).$(VER_MINOR)$(ARCH).so
LIBNAME = lib$(TARGET).so.$(VER_MAJOR)


default: all

all: OpenJPEG

dist: OpenJPEG
	install -d ../bin
	cp $(SHAREDLIB) ../bin/

dos2unix:
	@$(DOS2UNIX) $(SRCS) $(INCLS)

OpenJPEG: $(SHAREDLIB)

$(MODULES): %.o: %.c
	$(CC) $(CFLAGS) -c $< -o $@

$(CPPMODULES): %.o: %.cpp
	$(CC) $(CFLAGS) -c $< -o $@

$(SHAREDLIB): $(MODULES) $(CPPMODULES)
	$(CC) -s -shared -Wl,-soname,$(LIBNAME) -o $@ $(MODULES) $(CPPMODULES) $(LIBRARIES)

install: OpenJPEG
	install -d ../bin
	cp $(SHAREDLIB) ../bin/

clean:
	rm -rf core dist/ u2dtmp* $(MODULES) $(CPPMODULES) $(SHAREDLIB) $(LIBNAME)

osx:
	make -f Makefile.osx

osxinstall:
	make -f Makefile.osx install

osxclean:
	make -f Makefile.osx clean
