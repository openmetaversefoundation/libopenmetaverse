// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the SNOWFLAKE_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// SNOWFLAKE_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef SNOWFLAKE_EXPORTS
#define SNOWFLAKE_API __declspec(dllexport)
#else
#define SNOWFLAKE_API __declspec(dllimport)
#endif

SNOWFLAKE_API bool InstallSystemHook(void);
SNOWFLAKE_API BOOL RemoveSystemHook();
