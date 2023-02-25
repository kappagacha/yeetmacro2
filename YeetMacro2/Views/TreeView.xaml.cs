using System.Windows.Input;
using UraniumUI.Icons.FontAwesome;

namespace YeetMacro2.Views;

public partial class TreeView : ContentView
{
    ICommand _testCommand;
    public ICommand Command
    {
        get
        {
            return _testCommand ?? (_testCommand = new Command(() =>
            {
                imgView.Glyph = Solid.Download;
            }));
        }
    }
    public TreeView()
	{
		InitializeComponent();
	}
}