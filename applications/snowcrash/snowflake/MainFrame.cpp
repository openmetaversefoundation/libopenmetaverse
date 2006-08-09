#include "StdAfx.h"
#include ".\MainFrame.h"

CMainFrame::CMainFrame()
{
}

CMainFrame::~CMainFrame()
{
}

BOOL CMainFrame::PreTranslateMessage(MSG* pMsg)
{
	if(CFrameWindowImpl<CMainFrame>::PreTranslateMessage(pMsg))
		return TRUE;

	return FALSE;
}

BOOL CMainFrame::OnIdle()
{
	dprintf(_T("idle\n"));
	return FALSE;
}

LRESULT CMainFrame::OnCreate(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& bHandled)
{
	LRESULT lResult = baseClass::DefWindowProc();

	bHandled = TRUE;

	Initialize();

	return lResult;
}

BOOL CMainFrame::SubclassWindow(HWND hwnd)
{
	dprintf(_T("Subclassing %08x\n"), hwnd);

	if (baseClass::SubclassWindow(hwnd))
	{
		Initialize();
		return TRUE;
	}

	return FALSE;
}

HWND CMainFrame::UnsubclassWindow(BOOL bForce)
{
	HWND hwnd = baseClass::UnsubclassWindow(bForce);
	dprintf(_T("Unsubclassing %08x\n"), hwnd);
	UnpatchWnds();

	return hwnd;
}

BOOL CMainFrame::Initialize()
{
	if (!PatchWnds())
		return FALSE;

	return TRUE;
}

LRESULT CMainFrame::OnParentNotify(UINT /*uMsg*/, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	switch(LOWORD(wParam))
	{
		case WM_CREATE:
			{
				PatchWnds();
			}
			break;
	}

	return 0;
}

LRESULT CMainFrame::OnDestroy(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& bHandled)
{
	PostQuitMessage(0);

	return 0;
}

BOOL CMainFrame::PatchWnds()
{
	return TRUE;
}

void CMainFrame::UnpatchWnds()
{
}

LRESULT CMainFrame::OnWindowPosChanging(UINT /*uMsg*/, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	LRESULT lResult = baseClass::DefWindowProc();
	return lResult;
}

LRESULT CMainFrame::OnWindowPosChanged(UINT /*uMsg*/, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	LRESULT lResult = baseClass::DefWindowProc();
	return lResult;
}

LRESULT CMainFrame::OnSizing(UINT /*uMsg*/, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	LRESULT lResult = baseClass::DefWindowProc();
	return lResult;
}

LRESULT CMainFrame::OnSize(UINT /*uMsg*/, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	LRESULT lResult = baseClass::DefWindowProc();
	return lResult;
}