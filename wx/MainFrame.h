#pragma once
#include "MainFrame.generated.h"

// 继承自MainFrame_generated
class MainFrame : public MainFrame_generated
{
public:
    MainFrame(wxWindow *parent, wxWindowID id, const wxString &title, const wxPoint &pos, const wxSize &size, long style);
    ~MainFrame();

private:
    void OnBtnLogin(wxCommandEvent &event);
    void OnBtnStart(wxCommandEvent &event);
    void OnBtnExit(wxCommandEvent &event);
};
