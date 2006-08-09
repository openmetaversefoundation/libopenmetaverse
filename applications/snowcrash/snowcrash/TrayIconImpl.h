#ifndef TRAYICONIMPL_H
#define TRAYICONIMPL_H

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

class CNotifyIconData : public NOTIFYICONDATA
{
public:	
	CNotifyIconData()
	{
		ZeroMemory(this, sizeof(NOTIFYICONDATA));
		cbSize = sizeof(NOTIFYICONDATA);
	}
};

template <class T>
class CTrayIconImpl
{
private:
	UINT WM_TRAYICON;
	CNotifyIconData m_NID;
	BOOL m_bInstalled;
	UINT m_nDefaultItem;
public:	
	CTrayIconImpl() : m_bInstalled(FALSE), m_nDefaultItem(0)
	{
		WM_TRAYICON = RegisterWindowMessage(_T("WM_TRAYICON"));
	}
	
	~CTrayIconImpl()
	{
		RemoveIcon();
	}

	BOOL InstallIcon(TCHAR* strToolTip, HICON hIcon, UINT nID)
	{
		T* pT = static_cast<T*>(this);
		m_NID.hWnd = pT->m_hWnd;
		m_NID.uID = nID;
		m_NID.hIcon = hIcon;
		m_NID.uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP;
		m_NID.uCallbackMessage = WM_TRAYICON;
		_tcscpy(m_NID.szTip, strToolTip);
		m_bInstalled = Shell_NotifyIcon(NIM_ADD, &m_NID);

		return m_bInstalled;
	}

	BOOL RemoveIcon()
	{
		if (!m_bInstalled) return FALSE;

		m_NID.uFlags = 0;
		return Shell_NotifyIcon(NIM_DELETE, &m_NID);
	}

	BOOL SetToolTipText(TCHAR* strToolTipText)
	{
		if (strToolTipText == NULL)
			return FALSE;

		m_NID.uFlags = NIF_TIP;
		_tcscpy(m_NID.szTip, strToolTipText);

		return Shell_NotifyIcon(NIM_MODIFY, &m_NID);
	}

	inline void SetDefaultItem(UINT nID) { m_nDefaultItem = nID; }

	BEGIN_MSG_MAP(CTrayIcon)
		MESSAGE_HANDLER(WM_TRAYICON, OnTrayIcon)
	END_MSG_MAP()

	LRESULT OnTrayIcon(UINT /*uMsg*/, WPARAM wParam, LPARAM lParam, BOOL& /*bHandled*/)
	{
		if (wParam != m_NID.uID) return 0;

		T* pT = static_cast<T*>(this);

		if (LOWORD(lParam) == WM_RBUTTONUP)
		{
			CMenu menu;

			if (!menu.LoadMenu(m_NID.uID))
				return 0;

			CMenuHandle submenu(menu.GetSubMenu(0));
			CPoint pos;
			GetCursorPos(&pos);

			SetForegroundWindow(pT->m_hWnd);

			submenu.SetMenuDefaultItem(m_nDefaultItem);
			submenu.TrackPopupMenu(TPM_LEFTALIGN, pos.x, pos.y, pT->m_hWnd);

			pT->PostMessage(WM_NULL);
			menu.DestroyMenu();
		} else if (LOWORD(lParam) == WM_LBUTTONDBLCLK)
		{
			SetForegroundWindow(pT->m_hWnd);

			pT->PostMessage(WM_COMMAND, m_nDefaultItem, 0);
		}

		return 0;
	}
};

#endif /* TRAYICONIMPL_H */

