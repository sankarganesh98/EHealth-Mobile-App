using eHealth.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace eHealth.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EContactDetailPage : ContentPage
    {
        EContactDetailViewModel viewModel;

        public EContactDetailPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new EContactDetailViewModel();
        }
    }
}
