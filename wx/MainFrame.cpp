#include "MainFrame.h"
#include <wx/msgdlg.h>

MainFrame::MainFrame(wxWindow *parent, wxWindowID id, const wxString &title, const wxPoint &pos, const wxSize &size, long style)
    : MainFrame_generated(parent, id, title, pos, size, style)
{
    // 事件绑定
    Bind(wxEVT_BUTTON, &MainFrame::OnBtnLogin, this, nsp_btnLogin);
    Bind(wxEVT_BUTTON, &MainFrame::OnBtnStart, this, nsp_btnStart);
    Bind(wxEVT_BUTTON, &MainFrame::OnBtnExit, this, nsp_btnExit);
}

void MainFrame::OnBtnLogin(wxCommandEvent &event)
{
    wxMessageBox("Login");
}

void MainFrame::OnBtnStart(wxCommandEvent &event)
{
    wxMessageBox("Start");
}

void MainFrame::OnBtnExit(wxCommandEvent &event)
{
    wxMessageBox("Exit");
}

MainFrame::~MainFrame()
{
}