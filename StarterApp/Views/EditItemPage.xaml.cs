namespace StarterApp.Views;

public partial class EditItemPage : ContentPage
{
    public EditItemPage(ViewModels.EditItemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
