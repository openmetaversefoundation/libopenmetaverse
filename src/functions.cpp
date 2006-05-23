#include "includes.h"

__inline static int atoin(char *s, unsigned int n)
{
	int i = 0;
	while (isdigit(*s) && n) {
		i = i*10 + *(s++) - '0';
		--n;
	}
	return i;
}

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

static int hex2num(char c)
{
	if (c >= '0' && c <= '9')
		return c - '0';
	if (c >= 'a' && c <= 'f')
		return c - 'a' + 10;
	if (c >= 'A' && c <= 'F')
		return c - 'A' + 10;
	return -1;
}

static int hex2byte(const char* hex)
{
	int a, b;

	a = hex2num(*hex++);
	if (a < 0)
		return -1;

	b = hex2num(*hex++);
	if (b < 0)
		return -1;

	return (a << 4) | b;
}

// Convert a "hex string" to binary
void hexstr2bin(const char* hex, byte* buf, size_t len)
{
	unsigned int i;
	byte a;
	const char* ipos = hex;
	byte* opos = buf;

	for (i = 0; i < len; i++) {
		a = hex2byte(ipos);
		*opos++ = a;
		ipos += 2;
	}
}

std::string rpcGetString(char* buffer, const char* name)
{
	char* pos = strstr(buffer, name);
	char* pos2 = NULL;
	unsigned int i = 0;
	std::string value = "";

	if (pos) {
		if ((pos = strstr(pos, "<string>"))) {
			pos += 8;

			if ((pos2 = strstr(pos, "</string>"))) {
				value = std::string(pos);
				value = value.substr(0, (pos2 - pos));
			}
		}
	}

	// Replace newline characters
	i = value.find_first_of('\n');
	while (i != std::string::npos) {
		value.replace(i, 1, " ");
		i = value.find_first_of('\n', i);
	}

	return value;
}

int rpcGetU32(char* buffer, const char* name)
{
	char* pos = strstr(buffer, name);
	char* pos2 = NULL;
	int value = 0;

	if (pos) {
		if ((pos = strstr(pos, "<i4>"))) {
			pos += 4;

			if ((pos2 = strstr(pos, "</i4>"))) {
				if (pos2 > pos) {
					value = atoin(pos, (int)(pos2 - pos));
				}
			}
		}
	}

	return value;
}

std::string packUUID(std::string uuid)
{
	if (uuid.length() == 36) {
		uuid.erase(8, 1);
		uuid.erase(12, 1);
		uuid.erase(16, 1);
		uuid.erase(20, 1);
	} else if (uuid.length() != 32) {
		uuid = "";
	}

	return uuid;
}
