﻿using HyPlayer.Classes;
using NeteaseCloudMusicApi;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace HyPlayer.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Me : Page
    {
        private string uid = "";
        public Me()
        {
            InitializeComponent();
            uid = Common.LoginedUser.id;
            LoadInfo();
            NavigationView1.SelectedItem = NavigationView1.MenuItems[0];
        }

        public async void Invoke(Action action, Windows.UI.Core.CoreDispatcherPriority Priority = Windows.UI.Core.CoreDispatcherPriority.Normal)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Priority, () => { action(); });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            uid = Common.LoginedUser.id;
            LoadInfo();
        }

        public async void LoadInfo()
        {

            await Task.Run((() =>
           {
               Invoke(async () =>
               {
                   (bool isok, JObject json) = await Common.ncapi.RequestAsync(CloudMusicApiProviders.UserAccount);
                   if (isok)
                   {
                       TextBoxUserName.Text = json["profile"]["nickname"].ToString();
                       TextBoxSignature.Text = json["profile"]["signature"].ToString();
                       ImageRect.ImageSource = new BitmapImage(new Uri(json["profile"]["avatarUrl"].ToString()));
                   }
               });

           }));
            /*
            await Task.Run(() =>
            {
                this.Invoke(async () =>
                {
                    (bool isok, JObject json) = await Common.ncapi.RequestAsync(CloudMusicApiProviders.UserLevel);
                    if (isok)
                    {
                        TextBlockLevel.Text = "LV. "+json["data"]["level"].ToString();
                    }
                });
            });*/

        }

        private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            switch (args.SelectedItemContainer.Tag.ToString())
            {
                case "SongList":
                    Common.GLOBAL["SongListUID"] = uid;
                    ContentFrame.Navigate(typeof(SongListFrame), null, new EntranceNavigationTransitionInfo());
                    break;
            }
        }

        private async void Logout_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Common.ncapi = new CloudMusicApi();
                StorageFile sf = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync("Settings\\UserPassword");
                await sf.DeleteAsync();
                Common.Logined = false;
                Common.LoginedUser = new NCUser();
                Common.PageMain.MainFrame.Navigate(typeof(BlankPage));
                Common.PageMain.MainFrame.Navigate(typeof(BasePage));
            }
            catch { }

        }
    }
}
