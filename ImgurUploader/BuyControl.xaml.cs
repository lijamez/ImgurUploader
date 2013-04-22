using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ImgurUploader
{
    public sealed partial class BuyControl : UserControl
    {
        public BuyControl()
        {
            this.InitializeComponent();
        }

        private async void RemoveAdsButton_Click(object sender, RoutedEventArgs e)
        {
            if (InAppPurchases.Instance.AppShouldShowAds)
            {
                await InAppPurchases.Instance.PurchaseAdRemoval();
            }
        }
    }
}
