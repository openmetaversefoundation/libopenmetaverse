#pragma once

class CMainFrame : public CFrameWindowImpl<CMainFrame>,
	public CMessageFilter, public CIdleHandler
{
	typedef CMainFrame thisClass;
	typedef CFrameWindowImpl<CMainFrame> baseClass;
public:
	CMainFrame();
	~CMainFrame();

	virtual BOOL PreTranslateMessage(MSG* pMsg);
	virtual BOOL OnIdle();

	BEGIN_MSG_MAP(thisClass)
//		MESSAGE_HANDLER(WM_CREATE, OnCreate)
//		MESSAGE_HANDLER(WM_DESTROY, OnDestroy)
//		MESSAGE_HANDLER(WM_PARENTNOTIFY, OnParentNotify)
//		MESSAGE_HANDLER(WM_WINDOWPOSCHANGING, OnWindowPosChanging)
//		MESSAGE_HANDLER(WM_WINDOWPOSCHANGED, OnWindowPosChanged)
//		MESSAGE_HANDLER(WM_SIZING, OnSizing)
//		MESSAGE_HANDLER(WM_SIZE, OnSize)

		CHAIN_MSG_MAP(CFrameWindowImpl<CMainFrame>)
	END_MSG_MAP()

	// Handler prototypes (uncomment arguments if needed):
//	LRESULT MessageHandler(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/)
//	LRESULT CommandHandler(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
//	LRESULT NotifyHandler(int /*idCtrl*/, LPNMHDR /*pnmh*/, BOOL& /*bHandled*/)

	LRESULT OnCreate(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/);
	LRESULT OnDestroy(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/);
	LRESULT OnParentNotify(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/);
	LRESULT OnWindowPosChanging(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/);
	LRESULT OnWindowPosChanged(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/);
	LRESULT OnSizing(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/);
	LRESULT OnSize(UINT /*uMsg*/, WPARAM /*wParam*/, LPARAM /*lParam*/, BOOL& /*bHandled*/);

	BOOL SubclassWindow(HWND hwnd);
	HWND UnsubclassWindow(BOOL bForce);

protected:
	BOOL Initialize();
	BOOL PatchWnds();
	void UnpatchWnds();
};
