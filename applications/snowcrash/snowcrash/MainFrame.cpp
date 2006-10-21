// MainFrame.cpp : implmentation of the CMainFrame class
//
/////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "resource.h"

#include "MainFrame.h"

BOOL CMainFrame::PreTranslateMessage(MSG* pMsg)
{
	return CFrameWindowImpl<CMainFrame>::PreTranslateMessage(pMsg);
}

BOOL CMainFrame::OnIdle()
{
	return FALSE;
}

LRESULT CMainFrame::OnCreate(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/)
{
	m_bEnabled = TRUE;

	// register object for message filtering and idle updates
	CMessageLoop* pLoop = _Module.GetMessageLoop();
	ATLASSERT(pLoop != NULL);
	pLoop->AddMessageFilter(this);
	pLoop->AddIdleHandler(this);

	// install tray icon
	InstallIcon("Snowcrash", ::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_SNOWCRASH)), IDC_TRAY);
//	SetDefaultItem(ID_APP_SHOW);

	InstallSystemHook();

	return 0;
}

LRESULT CMainFrame::OnDestroy(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/)
{
	dprintf("OnDestroy\n");
	RemoveSystemHook();
	PostQuitMessage(0);

	return 0;
}

LRESULT CMainFrame::OnAppExit(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	dprintf("OnAppExit\n");
	DestroyWindow();

	return 0;
}
