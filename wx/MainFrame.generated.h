///////////////////////////////////////////////////////////////////////////
// C++ code generated with wxFormBuilder (version 4.2.1-0-g80c4cb6)
// http://www.wxformbuilder.org/
//
// PLEASE DO *NOT* EDIT THIS FILE!
///////////////////////////////////////////////////////////////////////////

#pragma once

#include <wx/artprov.h>
#include <wx/xrc/xmlres.h>
#include <wx/intl.h>
#include <wx/string.h>
#include <wx/stattext.h>
#include <wx/gdicmn.h>
#include <wx/font.h>
#include <wx/colour.h>
#include <wx/settings.h>
#include <wx/textctrl.h>
#include <wx/button.h>
#include <wx/bitmap.h>
#include <wx/image.h>
#include <wx/icon.h>
#include <wx/sizer.h>
#include <wx/statbox.h>
#include <wx/listbox.h>
#include <wx/wrapsizer.h>
#include <wx/panel.h>
#include <wx/choice.h>
#include <wx/checkbox.h>
#include <wx/notebook.h>
#include <wx/frame.h>

///////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////
/// Class MainFrame_generated
///////////////////////////////////////////////////////////////////////////////
class MainFrame_generated : public wxFrame
{
	private:

	protected:
		enum
		{
			nsp_btnLogin = 6000,
			nsp_btnStart,
			nsp_btnExit,
		};

		wxNotebook* m_tabctrlMain;
		wxPanel* m_panelApp;
		wxStaticText* m_staticText1;
		wxTextCtrl* m_textCtrl1;
		wxStaticText* m_staticText2;
		wxTextCtrl* m_textCtrl2;
		wxButton* m_btnTest;
		wxPanel* m_panel8;
		wxWrapSizer* wSizer3;
		wxButton* m_btnAddNode;
		wxButton* m_btnDelNode;
		wxButton* m_btnDupNode;
		wxButton* m_btnRestoreNodes;
		wxButton* m_btnSaveNodes;
		wxPanel* m_panel10;
		wxPanel* m_panel81;
		wxStaticText* m_staticText3;
		wxChoice* m_choice1;
		wxStaticText* m_staticText5;
		wxTextCtrl* m_textCtrl5;
		wxStaticText* m_staticText6;
		wxTextCtrl* m_textCtrl6;
		wxStaticText* m_staticText7;
		wxTextCtrl* m_textCtrl7;
		wxStaticText* m_staticText8;
		wxTextCtrl* m_textCtrl8;
		wxStaticText* m_staticText9;
		wxTextCtrl* m_textCtrl9;
		wxStaticText* m_staticText10;
		wxCheckBox* m_checkBox1;
		wxCheckBox* m_checkBox2;
		wxPanel* m_panelLog;
		wxTextCtrl* m_textCtrl4;
		wxPanel* m_panel6;
		wxPanel* m_panel7;
		wxPanel* m_pnlBottomBtns;
		wxButton* m_btnLogin;
		wxButton* m_btnStart;
		wxButton* m_btnExit;

	public:
		wxListBox* m_listBox1;

		MainFrame_generated( wxWindow* parent, wxWindowID id = wxID_ANY, const wxString& title = _("NSmartProxyClient配置对话框"), const wxPoint& pos = wxDefaultPosition, const wxSize& size = wxSize( 840,600 ), long style = wxDEFAULT_FRAME_STYLE|wxTAB_TRAVERSAL );

		~MainFrame_generated();

};

