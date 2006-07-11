// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once


#define WIN32_LEAN_AND_MEAN		// Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>
#include <shellapi.h>
#include <stdio.h>
#include <stdlib.h>
#include <atlbase.h>
#include <atlapp.h>

extern CAppModule _Module;

#include <atlwin.h>

#include <atlframe.h>
#include <atlctrls.h>
#include <atlctrlw.h>
#include <atlctrlx.h>
#include <atlmisc.h>
#include <atldlgs.h>
#include <atlddx.h>
#include <atlcoll.h>
#include "resource.h"

//-----------------------------------------------------------------------------
// Miscellaneous helper functions
//-----------------------------------------------------------------------------
#define SAFE_FREE(p)		 { if(p) { free (p);       (p)=NULL; } }
#define SAFE_DELETE(p)       { if(p) { delete (p);     (p)=NULL; } }
#define SAFE_DELETE_ARRAY(p) { if(p) { delete[] (p);   (p)=NULL; } }
#define SAFE_RELEASE(p)      { if(p) { (p)->Release(); (p)=NULL; } }

#ifdef _DEBUG
#define ECHODEBUG
#define ECHONORMAL
#define ECHO
#else
#undef ECHODEBUG
#undef ECHONORMAL
#undef ECHO
#endif

#ifdef ECHODEBUG
void dprintf(TCHAR *format, ...);
#else
#define dprintf
#endif

#ifdef ECHONORMAL
void myprintf(TCHAR *format, ...);
#else
#define myprintf
#endif

void pktsave(char *buf, int len, int extra = 0);

extern FILE *fpLog;

// Bitmasks
#define BM_FLAG1				0x00000001
#define BM_FLAG2				0x00000002
#define BM_FLAG3				0x00000004
#define BM_FLAG4				0x00000008
#define BM_FLAG5				0x00000010
#define BM_FLAG6				0x00000020
#define BM_FLAG7				0x00000040
#define BM_FLAG8				0x00000080
#define BM_FLAG9				0x00000100
#define BM_FLAG10				0x00000200
#define BM_FLAG11				0x00000400
#define BM_FLAG12				0x00000800
#define BM_FLAG13				0x00001000
#define BM_FLAG14				0x00002000
#define BM_FLAG15				0x00004000
#define BM_FLAG16				0x00008000
#define BM_FLAG18				0x00010000
#define BM_FLAG19				0x00020000
#define BM_FLAG20				0x00040000
#define BM_FLAG21				0x00080000
#define BM_FLAGALL				0x000fffff

#define	MAP_NOT_SAFE			0x00000001
#define	MAP_UNKNOWN1			0x00000002
#define	MAP_UNKNOWN2			0x00000004
#define	MAP_UNKNOWN3			0x00000020
#define	MAP_UNKNOWN4			0x00000040
#define	MAP_UNKNOWN5			0x00000080
#define	MAP_SANDBOX				0x00000100
#define	MAP_UNKNOWN6			0x00008000
#define	MAP_UNKNOWN7			0x00010000

#define MSG_APPENDED_ACKS		0x10
#define MSG_RESENT				0x20
#define MSG_RELIABLE			0x40
#define MSG_ZEROCODED			0x80

#define MSG_FREQ_HIGH			0x0000
#define MSG_FREQ_MED			0xFF00
#define MSG_FREQ_LOW			0xFFFF

#define REGION_MULTIPLIER		4294967296