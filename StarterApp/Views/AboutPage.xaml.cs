// LOGIC - part 1 - MAUI page - self-contained

// AboutPage class belongs to the Views part of the StarterApp project, namespace a label tells where code belongs.

namespace StarterApp.Views;


// The  class/blueprint can be used from other parts of app = it is a AboutPage. 
// partial = the class is defined in multiple files (UI layout C# code logic)


public partial class AboutPage : ContentPage
{
	public AboutPage()
	{
		InitializeComponent(); // loads the UI
	}
}