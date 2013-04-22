using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.UI.Popups;

namespace ImgurUploader
{
    public class InAppPurchases : ObservableObject
    {
        private static InAppPurchases _inAppPurchasesInstance;
        public static InAppPurchases Instance
        {
            get
            {
                if (_inAppPurchasesInstance == null)
                {
                    _inAppPurchasesInstance = new InAppPurchases();
                }

                return _inAppPurchasesInstance;
            }
        }

        private const string AD_REMOVAL_OFFER_TOKEN = "adRemoval";
        private LicenseInformation LicenseInformation
        {
            get;
            set;
        }

        public InAppPurchases()
        {
            LicenseInformation = CurrentAppSimulator.LicenseInformation;
        }

        private bool? _appShouldShowAds;
        public bool AppShouldShowAds
        {
            get
            {
                if (_appShouldShowAds == null)
                {
                    _appShouldShowAds = !LicenseInformation.ProductLicenses[AD_REMOVAL_OFFER_TOKEN].IsActive;
                }

                return (bool) _appShouldShowAds;
            }
            set
            {
                _appShouldShowAds = value;
                NotifyPropertyChanged();
            }
        }

        public async Task PurchaseAdRemoval()
        {
            if (!LicenseInformation.ProductLicenses[AD_REMOVAL_OFFER_TOKEN].IsActive)
            {
                try
                {
                    await CurrentAppSimulator.RequestProductPurchaseAsync(AD_REMOVAL_OFFER_TOKEN, false);
                }
                catch (Exception) { }

                _appShouldShowAds = null; //Invalidate cached value

                if (!LicenseInformation.ProductLicenses[AD_REMOVAL_OFFER_TOKEN].IsActive)
                {
                    MessageDialog errMsg = new MessageDialog("Unable to make purchase");
                    await errMsg.ShowAsync();
                }
                else
                {
                    AppShouldShowAds = false; //Set to notify listeners

                    MessageDialog errMsg = new MessageDialog("Ads have now been removed.", "Thank you for your purchase");
                    await errMsg.ShowAsync();
                }

            }
            else
            {
                AppShouldShowAds = false;
            }
        }
    }
}
