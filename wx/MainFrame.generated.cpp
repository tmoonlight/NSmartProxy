///////////////////////////////////////////////////////////////////////////
// C++ code generated with wxFormBuilder (version 4.2.1-0-g80c4cb6)
// http://www.wxformbuilder.org/
//
// PLEASE DO *NOT* EDIT THIS FILE!
///////////////////////////////////////////////////////////////////////////

#include "MainFrame.generated.h"

///////////////////////////////////////////////////////////////////////////

MainFrame_generated::MainFrame_generated( wxWindow* parent, wxWindowID id, const wxString& title, const wxPoint& pos, const wxSize& size, long style ) : wxFrame( parent, id, title, pos, size, style )
{
	this->SetSizeHints( wxSize( 840,600 ), wxDefaultSize );
	this->SetBackgroundColour( wxColour( 234, 234, 234 ) );

	wxBoxSizer* topSizer;
	topSizer = new wxBoxSizer( wxVERTICAL );

	m_tabctrlMain = new wxNotebook( this, wxID_ANY, wxDefaultPosition, wxDefaultSize, 0 );
	m_tabctrlMain->SetBackgroundColour( wxSystemSettings::GetColour( wxSYS_COLOUR_HIGHLIGHTTEXT ) );

	m_panelApp = new wxPanel( m_tabctrlMain, wxID_ANY, wxDefaultPosition, wxDefaultSize, wxTAB_TRAVERSAL );
	wxBoxSizer* bSizer11;
	bSizer11 = new wxBoxSizer( wxVERTICAL );

	wxStaticBoxSizer* sbSizer4;
	sbSizer4 = new wxStaticBoxSizer( new wxStaticBox( m_panelApp, wxID_ANY, _("外网服务器") ), wxHORIZONTAL );

	m_staticText1 = new wxStaticText( sbSizer4->GetStaticBox(), wxID_ANY, _("服务器地址"), wxDefaultPosition, wxDefaultSize, 0 );
	m_staticText1->Wrap( -1 );
	sbSizer4->Add( m_staticText1, 0, wxALL, 5 );

	m_textCtrl1 = new wxTextCtrl( sbSizer4->GetStaticBox(), wxID_ANY, wxEmptyString, wxDefaultPosition, wxSize( -1,-1 ), 0 );
	sbSizer4->Add( m_textCtrl1, 1, wxALL, 5 );


	sbSizer4->Add( 30, 0, 0, wxEXPAND, 5 );

	m_staticText2 = new wxStaticText( sbSizer4->GetStaticBox(), wxID_ANY, _("端口"), wxDefaultPosition, wxDefaultSize, 0 );
	m_staticText2->Wrap( -1 );
	sbSizer4->Add( m_staticText2, 0, wxALL, 5 );

	m_textCtrl2 = new wxTextCtrl( sbSizer4->GetStaticBox(), wxID_ANY, wxEmptyString, wxDefaultPosition, wxSize( 80,-1 ), 0 );
	sbSizer4->Add( m_textCtrl2, 0, wxALL, 5 );

	m_btnTest = new wxButton( sbSizer4->GetStaticBox(), wxID_ANY, _("测试"), wxDefaultPosition, wxDefaultSize, 0 );
	sbSizer4->Add( m_btnTest, 0, wxALL, 5 );


	bSizer11->Add( sbSizer4, 0, wxEXPAND|wxLEFT|wxRIGHT|wxTOP, 10 );

	wxBoxSizer* bSizer12;
	bSizer12 = new wxBoxSizer( wxHORIZONTAL );

	wxBoxSizer* bSizerLeft;
	bSizerLeft = new wxBoxSizer( wxVERTICAL );

	m_listBox1 = new wxListBox( m_panelApp, wxID_ANY, wxDefaultPosition, wxDefaultSize, 0, NULL, 0 );
	m_listBox1->Append( _("测试1") );
	m_listBox1->Append( _("测试2") );
	m_listBox1->Append( _("测试3") );
	m_listBox1->Append( wxEmptyString );
	m_listBox1->Append( wxEmptyString );
	bSizerLeft->Add( m_listBox1, 1, wxALL|wxEXPAND, 10 );

	m_panel8 = new wxPanel( m_panelApp, wxID_ANY, wxDefaultPosition, wxDefaultSize, wxTAB_TRAVERSAL );
	wSizer3 = new wxWrapSizer( wxHORIZONTAL, 0 );

	m_btnAddNode = new wxButton( m_panel8, wxID_ANY, _("添加"), wxDefaultPosition, wxDefaultSize, 0 );
	wSizer3->Add( m_btnAddNode, 0, wxALL, 5 );

	m_btnDelNode = new wxButton( m_panel8, wxID_ANY, _("删除"), wxDefaultPosition, wxDefaultSize, 0 );
	wSizer3->Add( m_btnDelNode, 0, wxALL, 5 );

	m_btnDupNode = new wxButton( m_panel8, wxID_ANY, _("复制"), wxDefaultPosition, wxDefaultSize, 0 );
	wSizer3->Add( m_btnDupNode, 0, wxALL, 5 );

	m_btnRestoreNodes = new wxButton( m_panel8, wxID_ANY, _("还原配置"), wxDefaultPosition, wxDefaultSize, 0 );
	wSizer3->Add( m_btnRestoreNodes, 0, wxALL, 5 );

	m_btnSaveNodes = new wxButton( m_panel8, wxID_ANY, _("保存配置"), wxDefaultPosition, wxDefaultSize, 0 );
	wSizer3->Add( m_btnSaveNodes, 0, wxALL, 5 );


	m_panel8->SetSizer( wSizer3 );
	m_panel8->Layout();
	wSizer3->Fit( m_panel8 );
	bSizerLeft->Add( m_panel8, 0, wxEXPAND | wxALL, 5 );


	bSizer12->Add( bSizerLeft, 1, wxEXPAND, 5 );

	m_panel10 = new wxPanel( m_panelApp, wxID_ANY, wxDefaultPosition, wxSize( -1,-1 ), wxTAB_TRAVERSAL );
	wxBoxSizer* bSizer15;
	bSizer15 = new wxBoxSizer( wxVERTICAL );

	wxStaticBoxSizer* sbSizer2;
	sbSizer2 = new wxStaticBoxSizer( new wxStaticBox( m_panel10, wxID_ANY, _("节点配置") ), wxVERTICAL );

	m_panel81 = new wxPanel( sbSizer2->GetStaticBox(), wxID_ANY, wxDefaultPosition, wxDefaultSize, wxTAB_TRAVERSAL );
	wxGridSizer* gSizer1;
	gSizer1 = new wxGridSizer( 0, 2, 0, 0 );

	m_staticText3 = new wxStaticText( m_panel81, wxID_ANY, _("连接协议："), wxDefaultPosition, wxDefaultSize, 0 );
	m_staticText3->Wrap( -1 );
	gSizer1->Add( m_staticText3, 0, wxALL, 5 );

	wxArrayString m_choice1Choices;
	m_choice1 = new wxChoice( m_panel81, wxID_ANY, wxDefaultPosition, wxDefaultSize, m_choice1Choices, 0 );
	m_choice1->SetSelection( 0 );
	gSizer1->Add( m_choice1, 0, wxALL, 5 );

	m_staticText5 = new wxStaticText( m_panel81, wxID_ANY, _("内网地址："), wxDefaultPosition, wxDefaultSize, 0 );
	m_staticText5->Wrap( -1 );
	gSizer1->Add( m_staticText5, 0, wxALL, 5 );

	m_textCtrl5 = new wxTextCtrl( m_panel81, wxID_ANY, wxEmptyString, wxDefaultPosition, wxDefaultSize, 0 );
	gSizer1->Add( m_textCtrl5, 0, wxALL, 5 );

	m_staticText6 = new wxStaticText( m_panel81, wxID_ANY, _("内网端口："), wxDefaultPosition, wxDefaultSize, 0 );
	m_staticText6->Wrap( -1 );
	gSizer1->Add( m_staticText6, 0, wxALL, 5 );

	m_textCtrl6 = new wxTextCtrl( m_panel81, wxID_ANY, wxEmptyString, wxDefaultPosition, wxDefaultSize, 0 );
	gSizer1->Add( m_textCtrl6, 0, wxALL, 5 );

	m_staticText7 = new wxStaticText( m_panel81, wxID_ANY, _("外网端口(可选)："), wxDefaultPosition, wxDefaultSize, 0 );
	m_staticText7->Wrap( -1 );
	gSizer1->Add( m_staticText7, 0, wxALL, 5 );

	m_textCtrl7 = new wxTextCtrl( m_panel81, wxID_ANY, wxEmptyString, wxDefaultPosition, wxDefaultSize, 0 );
	gSizer1->Add( m_textCtrl7, 0, wxALL, 5 );

	m_staticText8 = new wxStaticText( m_panel81, wxID_ANY, _("主机域名(可选)："), wxDefaultPosition, wxDefaultSize, 0 );
	m_staticText8->Wrap( -1 );
	gSizer1->Add( m_staticText8, 0, wxALL, 5 );

	m_textCtrl8 = new wxTextCtrl( m_panel81, wxID_ANY, wxEmptyString, wxDefaultPosition, wxDefaultSize, 0 );
	gSizer1->Add( m_textCtrl8, 0, wxALL, 5 );

	m_staticText9 = new wxStaticText( m_panel81, wxID_ANY, _("节点描述(可选)："), wxDefaultPosition, wxDefaultSize, 0 );
	m_staticText9->Wrap( -1 );
	gSizer1->Add( m_staticText9, 0, wxALL, 5 );

	m_textCtrl9 = new wxTextCtrl( m_panel81, wxID_ANY, wxEmptyString, wxDefaultPosition, wxDefaultSize, 0 );
	gSizer1->Add( m_textCtrl9, 0, wxALL, 5 );

	m_staticText10 = new wxStaticText( m_panel81, wxID_ANY, _("传输压缩："), wxDefaultPosition, wxDefaultSize, 0 );
	m_staticText10->Wrap( -1 );
	gSizer1->Add( m_staticText10, 0, wxALL, 5 );

	m_checkBox1 = new wxCheckBox( m_panel81, wxID_ANY, _("启用"), wxDefaultPosition, wxDefaultSize, 0 );
	gSizer1->Add( m_checkBox1, 0, wxALL, 5 );


	m_panel81->SetSizer( gSizer1 );
	m_panel81->Layout();
	gSizer1->Fit( m_panel81 );
	sbSizer2->Add( m_panel81, 0, wxEXPAND | wxALL, 5 );


	bSizer15->Add( sbSizer2, 1, wxEXPAND, 5 );

	m_checkBox2 = new wxCheckBox( m_panel10, wxID_ANY, _("使用服务端配置"), wxDefaultPosition, wxDefaultSize, 0 );
	bSizer15->Add( m_checkBox2, 0, wxALL, 5 );


	m_panel10->SetSizer( bSizer15 );
	m_panel10->Layout();
	bSizer15->Fit( m_panel10 );
	bSizer12->Add( m_panel10, 0, wxEXPAND | wxALL, 5 );


	bSizer11->Add( bSizer12, 1, wxEXPAND, 5 );


	m_panelApp->SetSizer( bSizer11 );
	m_panelApp->Layout();
	bSizer11->Fit( m_panelApp );
	m_tabctrlMain->AddPage( m_panelApp, _("应用"), true );
	m_panelLog = new wxPanel( m_tabctrlMain, wxID_ANY, wxDefaultPosition, wxDefaultSize, wxTAB_TRAVERSAL );
	wxBoxSizer* bSizer7;
	bSizer7 = new wxBoxSizer( wxVERTICAL );

	m_textCtrl4 = new wxTextCtrl( m_panelLog, wxID_ANY, _("测试日志测试日志测试日志测试日志测试日志测试日志测试日志测试日志测试日志测试日志测试日志测试日志测试日志测试日志测试日志测试日志测试日志测试日志测试日志测试日志测试日志测试日志测试日志测试日志"), wxDefaultPosition, wxDefaultSize, wxTE_CHARWRAP|wxTE_MULTILINE );
	m_textCtrl4->SetForegroundColour( wxColour( 255, 255, 255 ) );
	m_textCtrl4->SetBackgroundColour( wxSystemSettings::GetColour( wxSYS_COLOUR_BACKGROUND ) );

	bSizer7->Add( m_textCtrl4, 1, wxALL|wxEXPAND, 5 );


	m_panelLog->SetSizer( bSizer7 );
	m_panelLog->Layout();
	bSizer7->Fit( m_panelLog );
	m_tabctrlMain->AddPage( m_panelLog, _("日志"), false );
	m_panel6 = new wxPanel( m_tabctrlMain, wxID_ANY, wxDefaultPosition, wxDefaultSize, wxTAB_TRAVERSAL );
	m_tabctrlMain->AddPage( m_panel6, _("服务"), false );
	m_panel7 = new wxPanel( m_tabctrlMain, wxID_ANY, wxDefaultPosition, wxDefaultSize, wxTAB_TRAVERSAL );
	m_tabctrlMain->AddPage( m_panel7, _("关于"), false );

	topSizer->Add( m_tabctrlMain, 1, wxEXPAND | wxALL, 5 );

	m_pnlBottomBtns = new wxPanel( this, wxID_ANY, wxDefaultPosition, wxDefaultSize, wxTAB_TRAVERSAL );
	wxBoxSizer* sizer_pnlBottomBtns;
	sizer_pnlBottomBtns = new wxBoxSizer( wxHORIZONTAL );

	m_btnLogin = new wxButton( m_pnlBottomBtns, nsp_btnLogin, _("未登录"), wxDefaultPosition, wxDefaultSize, 0 );
	sizer_pnlBottomBtns->Add( m_btnLogin, 0, wxALL, 5 );


	sizer_pnlBottomBtns->Add( 0, 0, 1, wxEXPAND, 5 );

	m_btnStart = new wxButton( m_pnlBottomBtns, nsp_btnStart, _("开始"), wxDefaultPosition, wxDefaultSize, 0 );
	sizer_pnlBottomBtns->Add( m_btnStart, 0, wxALL, 5 );


	sizer_pnlBottomBtns->Add( 0, 0, 1, wxEXPAND, 5 );

	m_btnExit = new wxButton( m_pnlBottomBtns, nsp_btnExit, _("退出程序"), wxDefaultPosition, wxDefaultSize, 0 );
	sizer_pnlBottomBtns->Add( m_btnExit, 0, wxALL, 5 );


	m_pnlBottomBtns->SetSizer( sizer_pnlBottomBtns );
	m_pnlBottomBtns->Layout();
	sizer_pnlBottomBtns->Fit( m_pnlBottomBtns );
	topSizer->Add( m_pnlBottomBtns, 0, wxEXPAND | wxALL, 5 );


	this->SetSizer( topSizer );
	this->Layout();

	this->Centre( wxBOTH );
}

MainFrame_generated::~MainFrame_generated()
{
}
