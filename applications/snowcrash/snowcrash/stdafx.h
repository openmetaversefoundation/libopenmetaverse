// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once


// Change these values to use different versions
#define WINVER		0x0400
//#define _WIN32_WINNT	0x0400

#include <atlbase.h>
#include <atlapp.h>
#include <shellapi.h>

extern CAppModule _Module;

#include <atlwin.h>
#include <atlres.h>
#include <atlframe.h>
#include <atlctrls.h>
#include <atlctrlw.h>
#include <atlmisc.h>
#include "../snowflake/snowflake.h"

#ifdef _DEBUG
#define ECHO
#endif

#ifdef ECHO
void dprintf(char *format, ...);
#else
#define dprintf
#endif
