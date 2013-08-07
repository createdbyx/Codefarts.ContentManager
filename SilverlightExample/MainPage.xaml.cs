namespace SilverlightExample
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;

    using Codefarts.ContentManager;

    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void btnGetHtml_Click(object sender, RoutedEventArgs e)
        {
            // get the content manager singleton and load the google home page. 
            var manager = ContentManager<string>.Instance;
            manager.Load<string>("http://www.google.com/", data => { this.txtHtml.Text = data; });
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // remember to register the reader(s) with the content manager
            var manager = ContentManager<string>.Instance;
            manager.Register(new HtmlReader());
            manager.Register(new WritableBitmapReader());
        }

        private void btnGetImage_Click(object sender, RoutedEventArgs e)
        {
            // get the content manager singleton and load the google logo image. 
            var manager = ContentManager<string>.Instance;
            manager.Load<WriteableBitmap>("http://www.google.ca/images/srpr/logo4w.png", data => { this.imgPreview.Source = data; });
        }
    }
}
