#include "includes.h"

// Convert a "hex string" to an integer by Anders Molin
int httoi(const char* value)
{
	struct HEXMAP
	{
		byte c;
		int value;
	};

	const int nHexMap = 16;
	
	HEXMAP hmLookup[nHexMap] =
	{
		{'0', 0}, {'1', 1},
		{'2', 2}, {'3', 3},
		{'4', 4}, {'5', 5},
		{'6', 6}, {'7', 7},
		{'8', 8}, {'9', 9},
		{'A', 10}, {'B', 11},
		{'C', 12}, {'D', 13},
		{'E', 14}, {'F', 15}
	};

	const char* s = value;
	int result = 0;
	
	if (*s == '0' && *(s + 1) == 'x')
		s += 2;
	
	bool firsttime = true;
	
	while (*s != '\0')
	{
		bool found = false;

		for (int i = 0; i < nHexMap; i++)
		{
			if (*s == hmLookup[i].c)
			{
				if (!firsttime)
					result <<= 4;
				
				result |= hmLookup[i].value;
				found = true;
				break;
			}
		}
		
		if (!found)
			break;
		
		s++;
		firsttime = false;
	}

	return result;
}
