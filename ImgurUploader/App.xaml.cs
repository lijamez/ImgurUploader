using ImgurUploader.UploadHistory;
using ImgurUploader.UploadResult;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace ImgurUploader
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private static UploadHistoryManager _uploadHistoryMgr;
        public static UploadHistoryManager UploadHistoryMgr
        {
            get
            {
                if (_uploadHistoryMgr == null)
                {
                    _uploadHistoryMgr = new UploadHistoryManager();
                }

                return _uploadHistoryMgr;
            }
        }


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                    
                }


                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            await UploadHistoryMgr.ReadUploadHistory();

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            
            if (!String.IsNullOrEmpty(args.Arguments))
            {
                string[] splitArgs = args.Arguments.Split(new char[] { ',' });

                if (String.Equals(splitArgs[0], "ShowLatestResults"))
                {
                    rootFrame.Navigate(typeof(UploadResultPage), splitArgs[1]);
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            await UploadHistoryMgr.WriteUploadHistory();

            deferral.Complete();
        }



        protected override void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            var rootFrame = new Frame();
            rootFrame.Navigate(typeof(SharePage), args.ShareOperation);
            Window.Current.Content = rootFrame;
            Window.Current.Activate();
        }
    }
}
