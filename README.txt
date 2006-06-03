To build libsecondlife you will need several libraries installed on your system.
It is recommended you install all of the boost libraries, however the ones that 
are specifically used by libsecondlife and the example programs are:

libboost
libboost-date-time
libboost-program-options
libboost-thread
libboost-signals
libcurl3
libcurl3-gnutls
libcurl3-openssl

You will also need boost-build to compile the library. Run bjam in the root 
folder, or bjam release to compile the release version. Consult the Boost.Build 
documentation (http://boost.sourceforge.net/boost-build2/) for instructions on 
compiling against specific compilers, static linking, or more detailed options.

http://gna.org/projects/libsecondlife/
