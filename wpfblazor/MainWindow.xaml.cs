using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wpfblazor;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Width = SystemParameters.PrimaryScreenWidth *2/3;
        Height = SystemParameters.PrimaryScreenHeight *2/3;
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddWpfBlazorWebView();
        serviceCollection.AddMasaBlazor();

#if DEBUG
		serviceCollection.AddBlazorWebViewDeveloperTools();
#endif

        Resources.Add("services", serviceCollection.BuildServiceProvider());
    }
}
