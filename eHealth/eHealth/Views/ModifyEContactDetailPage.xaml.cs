using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using eHealth.ViewModels;

namespace eHealth.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModifyEContactDetailPage : ContentPage
    {
        public ModifyEContactDetailPage()
        {
            InitializeComponent();
            BindingContext = new ModifyEContactDetailViewModel();
        }
    }
}
