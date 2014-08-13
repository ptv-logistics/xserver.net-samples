
// MFCMapDialogDlg.h : header file
//

#pragma once


// CMFCMapDialogDlg dialog
class CMFCMapDialogDlg : public CDialogEx
{
// Construction
public:
	CMFCMapDialogDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_MFCMAPDIALOG_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support



// Implementation
protected:
       // ...
       // Data member for the PTV xServer .NET map control
       CWinFormsControl<Ptv::XServer::Controls::Map::FormsMap> m_mapCtrl;

	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
};
