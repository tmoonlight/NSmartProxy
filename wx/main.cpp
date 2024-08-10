#include <wx/wx.h>
#include "MainFrame.h"

class MyApp : public wxApp
{
public:
    virtual bool OnInit();
};

wxIMPLEMENT_APP(MyApp);

bool MyApp::OnInit()
{
    // MyFrame *frame = new MyFrame("Hello Everyone!", wxDefaultPosition, wxDefaultSize);
    MainFrame *frame = new MainFrame(NULL, wxID_ANY, _("NSmartProxyClientÅäÖÃ¶Ô»°¿ò"), wxDefaultPosition, wxDefaultSize, wxDEFAULT_FRAME_STYLE);
    //MainFrame_generated *frame = new MainFrame_generated(NULL, wxID_ANY, _("Hello Everyone!"), wxDefaultPosition, wxDefaultSize, wxDEFAULT_FRAME_STYLE);
	frame->SetSize(1024, 768);
    frame->Centre();
    frame->Show(true);
    return true;
}

// MyFrame::MyFrame(const wxString &title, const wxPoint &pos, const wxSize &size)
//     : wxFrame(NULL, wxID_ANY, title, pos, size)
// {
//     new wxStaticText(this, wxID_ANY, "Good Morning!"); // no need to delete - the parent will do it automatically
// }
