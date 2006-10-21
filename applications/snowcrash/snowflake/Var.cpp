#include "StdAfx.h"
#include ".\Var.h"
#include ".\keywords.h"

CVar::CVar(void)
{
	m_lpszVar = NULL;
	m_nType = 0;
	m_nTypeLen = 0;
	m_nLen = 0;
	m_lpData = NULL;

	m_lpPrev = NULL;
	m_lpNext = NULL;
}

CVar::~CVar(void)
{
	SAFE_FREE(m_lpszVar);
	SAFE_FREE(m_lpData);
}

void CVar::SetVar(char *lpszVar)
{
	if (lpszVar)
	{
		int nLen = (int)strlen(lpszVar);

		if (nLen > 0)
		{
			SAFE_FREE(m_lpszVar);
			m_lpszVar = (char *)malloc(nLen + 1);

			if (m_lpszVar)
			{
				strncpy(m_lpszVar, lpszVar, nLen);
				m_lpszVar[nLen] = '\0';
			}
		}
	}
}

void CVar::SetType(int nType, int nTypeLen)
{
	m_nType = nType;
	m_nTypeLen = nTypeLen;
}

void CVar::GetString(char &lpszStr)
{
	switch (m_nType)
	{
		case LLTYPE_U8:
			{
				unsigned char ubData;
				memcpy(&ubData, m_lpData, sizeof(ubData));
				sprintf(&lpszStr, "%hu", ubData);
			}
			break;

		case LLTYPE_U16:
			{
				WORD wData;
				memcpy(&wData, m_lpData, sizeof(wData));
				sprintf(&lpszStr, "%u", wData);
			}
			break;

		case LLTYPE_U32:
			{
				DWORD dwData;
				memcpy(&dwData, m_lpData, sizeof(dwData));
				sprintf(&lpszStr, "%lu", dwData);
			}
			break;

		case LLTYPE_U64:
			{
				ULONGLONG ullData;
				memcpy(&ullData, m_lpData, sizeof(ullData));
				sprintf(&lpszStr, "%I64u", ullData);
			}
			break;

		case LLTYPE_S8:
			{
				BYTE bData;
				memcpy(&bData, m_lpData, sizeof(bData));
				sprintf(&lpszStr, "%hd", bData);
			}
			break;

		case LLTYPE_S16:
			{
				SHORT sData;
				memcpy(&sData, m_lpData, sizeof(sData));
				sprintf(&lpszStr, "%d", sData);
			}
			break;

		case LLTYPE_S32:
			{
				LONG nData;
				memcpy(&nData, m_lpData, sizeof(nData));
				sprintf(&lpszStr, "%ld", nData);
			}
			break;

		case LLTYPE_S64:
			break;

		case LLTYPE_F8:
			break;

		case LLTYPE_F16:
			break;

		case LLTYPE_F32:
			{
				FLOAT fData;
				memcpy(&fData, m_lpData, sizeof(fData));
				sprintf(&lpszStr, "%f", fData);
			}
			break;

		case LLTYPE_F64:
			{
				double dData;
				memcpy(&dData, m_lpData, sizeof(dData));
				sprintf(&lpszStr, "%f", dData);
			}
			break;

		case LLTYPE_LLUUID:
			{
				char szHex[8];
				sprintf(&lpszStr, "");

				for (int u = 0; u < m_nLen; u++)
				{
					sprintf(szHex, "%02x", m_lpData[u]);
					strcat(&lpszStr, szHex);

					if (u == 3 || u == 5 || u == 7 || u == 9)
						strcat(&lpszStr, "-");
				}
			}
			break;

		case LLTYPE_BOOL:
			{
				BYTE bData;
				memcpy(&bData, m_lpData, sizeof(bData));
				sprintf(&lpszStr, "%s", (bData) ? "True" : "False");
			}
			break;

		case LLTYPE_LLVECTOR3:
			{
				FLOAT fData[3];
				memcpy(&fData, m_lpData, sizeof(fData));
				sprintf(&lpszStr, "%f, %f, %f", fData[0], fData[1], fData[2]);
			}
			break;

		case LLTYPE_LLVECTOR3D:
			{
				double dData[3];
				memcpy(&dData, m_lpData, sizeof(dData));
				sprintf(&lpszStr, "%f, %f, %f", dData[0], dData[1], dData[2]);
			}
			break;

		case LLTYPE_QUATERNION:
			{
				FLOAT fData[4];
				memcpy(&fData, m_lpData, sizeof(fData));
				sprintf(&lpszStr, "%f, %f, %f, %f", fData[0], fData[1], fData[2], fData[3]);
			}
			break;

		case LLTYPE_IPADDR:
			{
				BYTE ipData[4];
				memcpy(&ipData, m_lpData, sizeof(ipData));
				sprintf(&lpszStr, "%hu.%hu.%hu.%hu", ipData[0], ipData[1], ipData[2], ipData[3]);
			}
			break;

		case LLTYPE_IPPORT:
			{
				WORD wData;
				memcpy(&wData, m_lpData, sizeof(wData));
				sprintf(&lpszStr, "%hu", htons(wData));
			}
			break;

		case LLTYPE_FIXED:
		case LLTYPE_VARIABLE:
			{
				sprintf(&lpszStr, "");

				if (m_lpData)
				{
					bool bPrintable = true;

					for (int j = 0; j < m_nLen - 1; j++)
					{
						if (((unsigned char)m_lpData[j] < 0x20 || (unsigned char)m_lpData[j] > 0x7E) && (unsigned char)m_lpData[j] != 0x09 && (unsigned char)m_lpData[j] != 0x0D)
							bPrintable = false;
					}

					if (bPrintable && m_lpData[m_nLen - 1] == '\0')
					{
						sprintf(&lpszStr, "%s", m_lpData);
					}
					else
					{
						for (int j = 0; j < m_nLen; j++)
						{
							char szHex[8];
							sprintf(szHex, "%02x", (unsigned char)m_lpData[j]);
							strcat(&lpszStr, szHex);
						}
					}
				}
			}
			break;

		case LLTYPE_SINGLE:
		case LLTYPE_MULTIPLE:
		case LLTYPE_NULL:
		default:
			{
				sprintf(&lpszStr, "");
			}
			break;
	}
}

void CVar::GetBool(bool &lpbBool)
{
	switch (m_nType)
	{
		case LLTYPE_BOOL:
			{
				BYTE bData;
				memcpy(&bData, m_lpData, sizeof(bData));
				lpbBool = (bData) ? true : false;
			}
			break;

		case LLTYPE_U8:
		case LLTYPE_U16:
		case LLTYPE_U32:
		case LLTYPE_U64:
		case LLTYPE_S8:
		case LLTYPE_S16:
		case LLTYPE_S32:
		case LLTYPE_S64:
		case LLTYPE_F8:
		case LLTYPE_F16:
		case LLTYPE_F32:
		case LLTYPE_F64:
		case LLTYPE_LLUUID:
		case LLTYPE_LLVECTOR3:
		case LLTYPE_LLVECTOR3D:
		case LLTYPE_QUATERNION:
		case LLTYPE_IPADDR:
		case LLTYPE_IPPORT:
		case LLTYPE_FIXED:
		case LLTYPE_VARIABLE:
		case LLTYPE_SINGLE:
		case LLTYPE_MULTIPLE:
		case LLTYPE_NULL:
		default:
			{
				lpbBool = false;
			}
			break;
	}
}

int CVar::SetData(LPBYTE lpData)
{
	SAFE_FREE(m_lpData);
	m_nLen = 0;

	int nRead = 0;

	switch (m_nType)
	{
		case LLTYPE_U8:
			{
				m_nLen = sizeof(unsigned char);

				m_lpData = (LPBYTE)malloc(m_nLen);
				
				if (m_lpData)
				{
					memcpy(m_lpData, lpData, m_nLen);
					nRead += m_nLen;
				}
				else
					m_nLen = 0;
			}
			break;

		case LLTYPE_U16:
			{
				m_nLen = sizeof(WORD);

				m_lpData = (LPBYTE)malloc(m_nLen);
				
				if (m_lpData)
				{
					memcpy(m_lpData, lpData, m_nLen);
					nRead += m_nLen;
				}
				else
					m_nLen = 0;
			}
			break;

		case LLTYPE_U32:
			{
				m_nLen = sizeof(DWORD);

				m_lpData = (LPBYTE)malloc(m_nLen);
				
				if (m_lpData)
				{
					memcpy(m_lpData, lpData, m_nLen);
					nRead += m_nLen;
				}
				else
					m_nLen = 0;
			}
			break;

		case LLTYPE_U64:
			{
				m_nLen = sizeof(ULONGLONG);

				m_lpData = (LPBYTE)malloc(m_nLen);
				
				if (m_lpData)
				{
					memcpy(m_lpData, lpData, m_nLen);
					nRead += m_nLen;
				}
				else
					m_nLen = 0;
			}
			break;

		case LLTYPE_S8:
			{
				m_nLen = sizeof(BYTE);

				m_lpData = (LPBYTE)malloc(m_nLen);
				
				if (m_lpData)
				{
					memcpy(m_lpData, lpData, m_nLen);
					nRead += m_nLen;
				}
				else
					m_nLen = 0;
			}
			break;

		case LLTYPE_S16:
			{
				m_nLen = sizeof(SHORT);

				m_lpData = (LPBYTE)malloc(m_nLen);
				
				if (m_lpData)
				{
					memcpy(m_lpData, lpData, m_nLen);
					nRead += m_nLen;
				}
				else
					m_nLen = 0;
			}
			break;

		case LLTYPE_S32:
			{
				m_nLen = sizeof(LONG);

				m_lpData = (LPBYTE)malloc(m_nLen);
				
				if (m_lpData)
				{
					memcpy(m_lpData, lpData, m_nLen);
					nRead += m_nLen;
				}
				else
					m_nLen = 0;
			}
			break;

		case LLTYPE_S64:
			break;

		case LLTYPE_F8:
			break;

		case LLTYPE_F16:
			break;

		case LLTYPE_F32:
			{
				m_nLen = sizeof(FLOAT);

				m_lpData = (LPBYTE)malloc(m_nLen);
				
				if (m_lpData)
				{
					memcpy(m_lpData, lpData, m_nLen);
					nRead += m_nLen;
				}
				else
					m_nLen = 0;
			}
			break;

		case LLTYPE_F64:
			{
				m_nLen = sizeof(double);

				m_lpData = (LPBYTE)malloc(m_nLen);
				
				if (m_lpData)
				{
					memcpy(m_lpData, lpData, m_nLen);
					nRead += m_nLen;
				}
				else
					m_nLen = 0;
			}
			break;

		case LLTYPE_LLUUID:
			{
				m_nLen = sizeof(BYTE) * 16;

				m_lpData = (LPBYTE)malloc(m_nLen);
				
				if (m_lpData)
				{
					memcpy(m_lpData, lpData, m_nLen);
					nRead += m_nLen;
				}
				else
					m_nLen = 0;
			}
			break;

		case LLTYPE_BOOL:
			{
				m_nLen = sizeof(BYTE);

				m_lpData = (LPBYTE)malloc(m_nLen);
				
				if (m_lpData)
				{
					memcpy(m_lpData, lpData, m_nLen);
					nRead += m_nLen;
				}
				else
					m_nLen = 0;
			}
			break;

		case LLTYPE_LLVECTOR3:
			{
				m_nLen = sizeof(FLOAT) * 3;

				m_lpData = (LPBYTE)malloc(m_nLen);
				
				if (m_lpData)
				{
					memcpy(m_lpData, lpData, m_nLen);
					nRead += m_nLen;
				}
				else
					m_nLen = 0;
			}
			break;

		case LLTYPE_LLVECTOR3D:
			{
				m_nLen = sizeof(double) * 3;

				m_lpData = (LPBYTE)malloc(m_nLen);
				
				if (m_lpData)
				{
					memcpy(m_lpData, lpData, m_nLen);
					nRead += m_nLen;
				}
				else
					m_nLen = 0;
			}
			break;

		case LLTYPE_QUATERNION:
			{
				m_nLen = sizeof(FLOAT) * 4;

				m_lpData = (LPBYTE)malloc(m_nLen);
				
				if (m_lpData)
				{
					memcpy(m_lpData, lpData, m_nLen);
					nRead += m_nLen;
				}
				else
					m_nLen = 0;
			}
			break;

		case LLTYPE_IPADDR:
			{
				m_nLen = sizeof(BYTE) * 4;

				m_lpData = (LPBYTE)malloc(m_nLen);
				
				if (m_lpData)
				{
					memcpy(m_lpData, lpData, m_nLen);
					nRead += m_nLen;
				}
				else
					m_nLen = 0;
			}
			break;

		case LLTYPE_IPPORT:
			{
				m_nLen = sizeof(WORD);

				m_lpData = (LPBYTE)malloc(m_nLen);
				
				if (m_lpData)
				{
					memcpy(m_lpData, lpData, m_nLen);
					nRead += m_nLen;
				}
				else
					m_nLen = 0;
			}
			break;

		case LLTYPE_VARIABLE:
			{
				if (m_nTypeLen == 1)
				{
					BYTE cDataLen;

					memcpy(&cDataLen, lpData, sizeof(cDataLen));
					nRead += sizeof(cDataLen);
					lpData += sizeof(cDataLen);

					m_nLen = cDataLen;

					if (m_nLen > 0)
					{
						m_lpData = (LPBYTE)malloc(m_nLen);

						if (m_lpData)
						{
							memcpy(m_lpData, lpData, m_nLen);
							nRead += m_nLen;
						}
						else
							m_nLen = 0;
					}
					else
						m_nLen = 0;
				}
				else if (m_nTypeLen == 2)
				{
					WORD cDataLen;

					memcpy(&cDataLen, lpData, sizeof(cDataLen));
					nRead += sizeof(cDataLen);
					lpData += sizeof(cDataLen);

					m_nLen = cDataLen;

					if (m_nLen > 0)
					{
						m_lpData = (LPBYTE)malloc(m_nLen);

						if (m_lpData)
						{
							memcpy(m_lpData, lpData, m_nLen);
							nRead += m_nLen;
						}
						else
							m_nLen = 0;
					}
					else
						m_nLen = 0;
				}
			}
			break;

		case LLTYPE_FIXED:
			{
				m_nLen = m_nTypeLen;

				if (m_nLen > 0)
				{
					m_lpData = (LPBYTE)malloc(m_nLen);

					if (m_lpData)
					{
						memcpy(m_lpData, lpData, m_nLen);
						nRead += m_nLen;
					}
					else
						m_nLen = 0;
				}
				else
					m_nLen = 0;
			}
			break;

		case LLTYPE_SINGLE:
		case LLTYPE_MULTIPLE:
		case LLTYPE_NULL:
		default:
			{
				m_nLen = 0;
			}
			break;
	}

	return nRead;
}

void CVar::Dump(void)
{
	switch (m_nType)
	{
		case LLTYPE_U8:
			{
				unsigned char ubData;
				memcpy(&ubData, m_lpData, sizeof(ubData));
				dprintf("%s: %hu\n", m_lpszVar, ubData);
			}
			break;

		case LLTYPE_U16:
			{
				WORD wData;
				memcpy(&wData, m_lpData, sizeof(wData));
				dprintf("%s: %u\n", m_lpszVar, wData);
			}
			break;

		case LLTYPE_U32:
			{
				DWORD dwData;
				memcpy(&dwData, m_lpData, sizeof(dwData));
				dprintf("%s: %lu\n", m_lpszVar, dwData);
			}
			break;

		case LLTYPE_U64:
			{
				ULONGLONG ullData;
				memcpy(&ullData, m_lpData, sizeof(ullData));
				dprintf("%s: %I64u\n", m_lpszVar, ullData);
			}
			break;

		case LLTYPE_S8:
			{
				BYTE bData;
				memcpy(&bData, m_lpData, sizeof(bData));
				dprintf("%s: %hd\n", m_lpszVar, bData);
			}
			break;

		case LLTYPE_S16:
			{
				SHORT sData;
				memcpy(&sData, m_lpData, sizeof(sData));
				dprintf("%s: %d\n", m_lpszVar, sData);
			}
			break;

		case LLTYPE_S32:
			{
				LONG nData;
				memcpy(&nData, m_lpData, sizeof(nData));
				dprintf("%s: %ld\n", m_lpszVar, nData);
			}
			break;

		case LLTYPE_S64:
			break;

		case LLTYPE_F8:
			break;

		case LLTYPE_F16:
			break;

		case LLTYPE_F32:
			{
				FLOAT fData;
				memcpy(&fData, m_lpData, sizeof(fData));
				dprintf("%s: %f\n", m_lpszVar, fData);
			}
			break;

		case LLTYPE_F64:
			{
				double dData;
				memcpy(&dData, m_lpData, sizeof(dData));
				dprintf("%s: %f\n", m_lpszVar, dData);
			}
			break;

		case LLTYPE_LLUUID:
			{
				dprintf("%s: ", m_lpszVar);
				for (int u = 0; u < m_nLen; u++)
					dprintf("%02x", m_lpData[u]);
				dprintf("\n");
			}
			break;

		case LLTYPE_BOOL:
			{
				BYTE bData;
				memcpy(&bData, m_lpData, sizeof(bData));
				dprintf("%s: %s\n", m_lpszVar, (bData) ? "True" : "False");
			}
			break;

		case LLTYPE_LLVECTOR3:
			{
				FLOAT fData[3];
				memcpy(&fData, m_lpData, sizeof(fData));
				dprintf("%s: %f, %f, %f\n", m_lpszVar, fData[0], fData[1], fData[2]);
			}
			break;

		case LLTYPE_LLVECTOR3D:
			{
				double dData[3];
				memcpy(&dData, m_lpData, sizeof(dData));
				dprintf("%s: %f, %f, %f\n", m_lpszVar, dData[0], dData[1], dData[2]);
			}
			break;

		case LLTYPE_QUATERNION:
			{
				FLOAT fData[4];
				memcpy(&fData, m_lpData, sizeof(fData));
				dprintf("%s: %f, %f, %f, %f\n", m_lpszVar, fData[0], fData[1], fData[2], fData[3]);
			}
			break;

		case LLTYPE_IPADDR:
			{
				BYTE ipData[4];
				memcpy(&ipData, m_lpData, sizeof(ipData));
				dprintf("%s: %hu.%hu.%hu.%hu\n", m_lpszVar, ipData[0], ipData[1], ipData[2], ipData[3]);
			}
			break;

		case LLTYPE_IPPORT:
			{
				WORD wData;
				memcpy(&wData, m_lpData, sizeof(wData));
				dprintf("%s: %hu\n", m_lpszVar, htons(wData));
			}
			break;

		case LLTYPE_VARIABLE:
		case LLTYPE_FIXED:
			{
				if (m_lpData)
				{
					bool bPrintable = true;

					for (int j = 0; j < m_nLen - 1; j++)
					{
						if (((unsigned char)m_lpData[j] < 0x20 || (unsigned char)m_lpData[j] > 0x7E) && (unsigned char)m_lpData[j] != 0x09 && (unsigned char)m_lpData[j] != 0x0D)
							bPrintable = false;
					}

					if (bPrintable && m_lpData[m_nLen - 1] == '\0')
					{
						dprintf("%s: %s\n", m_lpszVar, m_lpData);
					}
					else
					{
						for (int j = 0; j < m_nLen; j += 16)
						{
							dprintf("%s: ", m_lpszVar);

							for (int k = 0; k < 16; k++)
							{
								if ((j + k) < m_nLen)
								{
									dprintf("%02x ", (unsigned char)m_lpData[j+k]);
								}
								else
								{
									dprintf("   ");
								}
							}

							for (int k = 0; k < 16 && (j + k) < m_nLen; k++)
							{
								dprintf("%c", ((unsigned char)m_lpData[j+k] >= 0x20 && (unsigned char)m_lpData[j+k] <= 0x7E) ? (unsigned char)m_lpData[j+k] : '.');
							}

							dprintf("\n");
						}
					}
				}
			}
			break;

		case LLTYPE_SINGLE:
		case LLTYPE_MULTIPLE:
		case LLTYPE_NULL:
		default:
			{
				dprintf("%s (%s / %d / %d)\n", m_lpszVar, LLTYPES[m_nType], m_nLen, m_nTypeLen);
			}
			break;
	}
}

int CVar::Pack(LPBYTE lpData)
{
	LPBYTE lpPtr = lpData;
	int nTotalWrote = 0;

	switch (m_nType)
	{
		case LLTYPE_U8:
		case LLTYPE_U16:
		case LLTYPE_U32:
		case LLTYPE_U64:
		case LLTYPE_S8:
		case LLTYPE_S16:
		case LLTYPE_S32:
		case LLTYPE_S64:
		case LLTYPE_F8:
		case LLTYPE_F16:
		case LLTYPE_F32:
		case LLTYPE_F64:
		case LLTYPE_LLUUID:
		case LLTYPE_BOOL:
		case LLTYPE_LLVECTOR3:
		case LLTYPE_LLVECTOR3D:
		case LLTYPE_QUATERNION:
		case LLTYPE_IPADDR:
		case LLTYPE_IPPORT:
		case LLTYPE_FIXED:
			{
				memcpy(lpPtr, m_lpData, m_nLen);
				nTotalWrote = m_nLen;
			}
			break;

		case LLTYPE_VARIABLE:
			{
				if (m_nTypeLen == 1)
				{
					BYTE cDataLen;

					cDataLen = m_nLen;

					memcpy(lpPtr, &cDataLen, sizeof(cDataLen));
					nTotalWrote = sizeof(cDataLen);
					
					lpPtr += sizeof(cDataLen);
					
					memcpy(lpPtr, m_lpData, m_nLen);
					nTotalWrote += m_nLen;
				}
				else if (m_nTypeLen == 2)
				{
					WORD cDataLen;

					cDataLen = m_nLen;

					memcpy(lpPtr, &cDataLen, sizeof(cDataLen));
					nTotalWrote = sizeof(cDataLen);
					
					lpPtr += sizeof(cDataLen);
					
					memcpy(lpPtr, m_lpData, m_nLen);
					nTotalWrote += m_nLen;
				}
			}
			break;

		case LLTYPE_SINGLE:
		case LLTYPE_MULTIPLE:
		case LLTYPE_NULL:
		default:
			{
				nTotalWrote = 0;
			}
			break;
	}

	return nTotalWrote;
}