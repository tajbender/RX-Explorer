﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace FileManager
{
    public sealed partial class ThisPC : Page
    {
        private ObservableCollection<HardDeviceInfo> HardDeviceList;
        public ObservableCollection<LibraryFolder> LibraryFolderList { get; private set; }
        public ObservableCollection<QuickStartItem> QuickStartList { get; private set; }
        public ObservableCollection<QuickStartItem> WebList { get; private set; }
        public static ThisPC ThisPage { get; private set; }

        private QuickStartItem CurrenItem;

        public ThisPC()
        {
            InitializeComponent();
            HardDeviceList = new ObservableCollection<HardDeviceInfo>();
            LibraryFolderList = new ObservableCollection<LibraryFolder>();
            QuickStartList = new ObservableCollection<QuickStartItem>();
            WebList = new ObservableCollection<QuickStartItem>();
            LibraryGrid.ItemsSource = LibraryFolderList;
            DeviceGrid.ItemsSource = HardDeviceList;
            QuickStartGridView.ItemsSource = QuickStartList;
            WebGridView.ItemsSource = WebList;
            ThisPage = this;
            OnFirstLoad();
        }

        private async void OnFirstLoad()
        {
            foreach (var Item in await SQLite.GetInstance().GetQuickStartItemAsync())
            {
                if (Item.Key == QuickStartType.Application)
                {
                    QuickStartList.Add(Item.Value);
                }
                else
                {
                    WebList.Add(Item.Value);
                }
            }

            QuickStartList.Add(new QuickStartItem(new BitmapImage(new Uri("ms-appx:///Assets/Add.png")) { DecodePixelHeight = 100, DecodePixelWidth = 100 }, null, default, string.Empty));
            WebList.Add(new QuickStartItem(new BitmapImage(new Uri("ms-appx:///Assets/Add.png")) { DecodePixelHeight = 100, DecodePixelWidth = 100 }, null, default, string.Empty));

            if (ApplicationData.Current.LocalSettings.Values["UserFolderPath"] is string UserPath)
            {
                try
                {
                    StorageFolder CurrentFolder = await StorageFolder.GetFolderFromPathAsync(UserPath);

                    IReadOnlyList<StorageFolder> LibraryFolder = await CurrentFolder.GetFoldersAsync();

                    var DesktopFolder = LibraryFolder.Where((Folder) => Folder.Name == "Desktop").FirstOrDefault();
                    LibraryFolderList.Add(new LibraryFolder(DesktopFolder, await DesktopFolder.GetThumbnailBitmapAsync()));

                    var DownloadsFolder = LibraryFolder.Where((Folder) => Folder.Name == "Downloads").FirstOrDefault();
                    LibraryFolderList.Add(new LibraryFolder(DownloadsFolder, await DownloadsFolder.GetThumbnailBitmapAsync()));

                    var VideosFolder = LibraryFolder.Where((Folder) => Folder.Name == "Videos").FirstOrDefault();
                    LibraryFolderList.Add(new LibraryFolder(VideosFolder, await VideosFolder.GetThumbnailBitmapAsync()));

                    var ObjectsFolder = LibraryFolder.Where((Folder) => Folder.Name == "3D Objects").FirstOrDefault();
                    LibraryFolderList.Add(new LibraryFolder(ObjectsFolder, await ObjectsFolder.GetThumbnailBitmapAsync()));

                    var PicturesFolder = LibraryFolder.Where((Folder) => Folder.Name == "Pictures").FirstOrDefault();
                    LibraryFolderList.Add(new LibraryFolder(PicturesFolder, await PicturesFolder.GetThumbnailBitmapAsync()));

                    var DocumentsFolder = LibraryFolder.Where((Folder) => Folder.Name == "Documents").FirstOrDefault();
                    LibraryFolderList.Add(new LibraryFolder(DocumentsFolder, await DocumentsFolder.GetThumbnailBitmapAsync()));

                    var MusicFolder = LibraryFolder.Where((Folder) => Folder.Name == "Music").FirstOrDefault();
                    LibraryFolderList.Add(new LibraryFolder(MusicFolder, await MusicFolder.GetThumbnailBitmapAsync()));
                }
                catch (FileNotFoundException)
                {
                    ContentDialog Tips = new ContentDialog
                    {
                        Title = "错误",
                        Content = "无法正确解析用户文件夹，可能已经被移动或不存在\r是否要重新选择用户文件夹",
                        PrimaryButtonText = "重新选择",
                        CloseButtonText = "忽略并继续",
                        Background = Application.Current.Resources["DialogAcrylicBrush"] as Brush
                    };
                    if (await Tips.ShowAsync() == ContentDialogResult.Primary)
                    {
                        StorageFolder UserFolder = await StorageFolder.GetFolderFromPathAsync(@"C:\Users");
                        IReadOnlyList<StorageFolder> Users = await UserFolder.GetFoldersAsync();
                        IEnumerable<StorageFolder> PotentialUsers = Users.Where((Folder) => Folder.Name != "Public");

                    FLAG1:
                        UserFolderDialog dialog = new UserFolderDialog(PotentialUsers);
                        _ = await dialog.ShowAsync();

                        StorageFolder CurrentUser = dialog.Result;

                        try
                        {
                            ApplicationData.Current.LocalSettings.Values["UserFolderPath"] = CurrentUser.Path;

                            IReadOnlyList<StorageFolder> LibraryFolder = await CurrentUser.GetFoldersAsync();

                            var DesktopFolder = LibraryFolder.Where((Folder) => Folder.Name == "Desktop").FirstOrDefault();
                            LibraryFolderList.Add(new LibraryFolder(DesktopFolder, await DesktopFolder.GetThumbnailBitmapAsync()));

                            var DownloadsFolder = LibraryFolder.Where((Folder) => Folder.Name == "Downloads").FirstOrDefault();
                            LibraryFolderList.Add(new LibraryFolder(DownloadsFolder, await DownloadsFolder.GetThumbnailBitmapAsync()));

                            var VideosFolder = LibraryFolder.Where((Folder) => Folder.Name == "Videos").FirstOrDefault();
                            LibraryFolderList.Add(new LibraryFolder(VideosFolder, await VideosFolder.GetThumbnailBitmapAsync()));

                            var ObjectsFolder = LibraryFolder.Where((Folder) => Folder.Name == "3D Objects").FirstOrDefault();
                            LibraryFolderList.Add(new LibraryFolder(ObjectsFolder, await ObjectsFolder.GetThumbnailBitmapAsync()));

                            var PicturesFolder = LibraryFolder.Where((Folder) => Folder.Name == "Pictures").FirstOrDefault();
                            LibraryFolderList.Add(new LibraryFolder(PicturesFolder, await PicturesFolder.GetThumbnailBitmapAsync()));

                            var DocumentsFolder = LibraryFolder.Where((Folder) => Folder.Name == "Documents").FirstOrDefault();
                            LibraryFolderList.Add(new LibraryFolder(DocumentsFolder, await DocumentsFolder.GetThumbnailBitmapAsync()));

                            var MusicFolder = LibraryFolder.Where((Folder) => Folder.Name == "Music").FirstOrDefault();
                            LibraryFolderList.Add(new LibraryFolder(MusicFolder, await MusicFolder.GetThumbnailBitmapAsync()));
                        }
                        catch (FileNotFoundException)
                        {
                            ContentDialog Tip = new ContentDialog
                            {
                                Title = "错误",
                                Content = "无法正确解析用户文件夹\r请重新检查用户文件夹选择是否正确",
                                PrimaryButtonText = "重新选择",
                                CloseButtonText = "忽略并继续",
                                Background = Application.Current.Resources["DialogAcrylicBrush"] as Brush
                            };
                            if (await Tip.ShowAsync() == ContentDialogResult.Primary)
                            {
                                goto FLAG1;
                            }
                        }
                    }
                }
            }
            else
            {
                StorageFolder UserFolder = await StorageFolder.GetFolderFromPathAsync(@"C:\Users");
                IReadOnlyList<StorageFolder> Users = await UserFolder.GetFoldersAsync();
                IEnumerable<StorageFolder> PotentialUsers = Users.Where((Folder) => Folder.Name != "Public");

                if (PotentialUsers.Count() > 1)
                {
                FLAG:
                    UserFolderDialog dialog = new UserFolderDialog(PotentialUsers);
                    _ = await dialog.ShowAsync();

                    StorageFolder CurrentUser = dialog.Result;

                    try
                    {
                        ApplicationData.Current.LocalSettings.Values["UserFolderPath"] = CurrentUser.Path;

                        IReadOnlyList<StorageFolder> LibraryFolder = await CurrentUser.GetFoldersAsync();

                        var DesktopFolder = LibraryFolder.Where((Folder) => Folder.Name == "Desktop").FirstOrDefault();
                        LibraryFolderList.Add(new LibraryFolder(DesktopFolder, await DesktopFolder.GetThumbnailBitmapAsync()));

                        var DownloadsFolder = LibraryFolder.Where((Folder) => Folder.Name == "Downloads").FirstOrDefault();
                        LibraryFolderList.Add(new LibraryFolder(DownloadsFolder, await DownloadsFolder.GetThumbnailBitmapAsync()));

                        var VideosFolder = LibraryFolder.Where((Folder) => Folder.Name == "Videos").FirstOrDefault();
                        LibraryFolderList.Add(new LibraryFolder(VideosFolder, await VideosFolder.GetThumbnailBitmapAsync()));

                        var ObjectsFolder = LibraryFolder.Where((Folder) => Folder.Name == "3D Objects").FirstOrDefault();
                        LibraryFolderList.Add(new LibraryFolder(ObjectsFolder, await ObjectsFolder.GetThumbnailBitmapAsync()));

                        var PicturesFolder = LibraryFolder.Where((Folder) => Folder.Name == "Pictures").FirstOrDefault();
                        LibraryFolderList.Add(new LibraryFolder(PicturesFolder, await PicturesFolder.GetThumbnailBitmapAsync()));

                        var DocumentsFolder = LibraryFolder.Where((Folder) => Folder.Name == "Documents").FirstOrDefault();
                        LibraryFolderList.Add(new LibraryFolder(DocumentsFolder, await DocumentsFolder.GetThumbnailBitmapAsync()));

                        var MusicFolder = LibraryFolder.Where((Folder) => Folder.Name == "Music").FirstOrDefault();
                        LibraryFolderList.Add(new LibraryFolder(MusicFolder, await MusicFolder.GetThumbnailBitmapAsync()));
                    }
                    catch (FileNotFoundException)
                    {
                        ContentDialog Tips = new ContentDialog
                        {
                            Title = "错误",
                            Content = "无法正确解析用户文件夹\r请重新检查用户文件夹选择是否正确",
                            PrimaryButtonText = "重新选择",
                            CloseButtonText = "忽略并继续",
                            Background = Application.Current.Resources["DialogAcrylicBrush"] as Brush
                        };
                        if (await Tips.ShowAsync() == ContentDialogResult.Primary)
                        {
                            goto FLAG;
                        }
                    }
                }
                else if (PotentialUsers.Count() == 1)
                {
                    StorageFolder CurrentUser = PotentialUsers.FirstOrDefault();

                    try
                    {
                        ApplicationData.Current.LocalSettings.Values["UserFolderPath"] = CurrentUser.Path;

                        IReadOnlyList<StorageFolder> LibraryFolder = await CurrentUser.GetFoldersAsync();

                        var DesktopFolder = LibraryFolder.Where((Folder) => Folder.Name == "Desktop").FirstOrDefault();
                        LibraryFolderList.Add(new LibraryFolder(DesktopFolder, await DesktopFolder.GetThumbnailBitmapAsync()));

                        var DownloadsFolder = LibraryFolder.Where((Folder) => Folder.Name == "Downloads").FirstOrDefault();
                        LibraryFolderList.Add(new LibraryFolder(DownloadsFolder, await DownloadsFolder.GetThumbnailBitmapAsync()));

                        var VideosFolder = LibraryFolder.Where((Folder) => Folder.Name == "Videos").FirstOrDefault();
                        LibraryFolderList.Add(new LibraryFolder(VideosFolder, await VideosFolder.GetThumbnailBitmapAsync()));

                        var ObjectsFolder = LibraryFolder.Where((Folder) => Folder.Name == "3D Objects").FirstOrDefault();
                        LibraryFolderList.Add(new LibraryFolder(ObjectsFolder, await ObjectsFolder.GetThumbnailBitmapAsync()));

                        var PicturesFolder = LibraryFolder.Where((Folder) => Folder.Name == "Pictures").FirstOrDefault();
                        LibraryFolderList.Add(new LibraryFolder(PicturesFolder, await PicturesFolder.GetThumbnailBitmapAsync()));

                        var DocumentsFolder = LibraryFolder.Where((Folder) => Folder.Name == "Documents").FirstOrDefault();
                        LibraryFolderList.Add(new LibraryFolder(DocumentsFolder, await DocumentsFolder.GetThumbnailBitmapAsync()));

                        var MusicFolder = LibraryFolder.Where((Folder) => Folder.Name == "Music").FirstOrDefault();
                        LibraryFolderList.Add(new LibraryFolder(MusicFolder, await MusicFolder.GetThumbnailBitmapAsync()));
                    }
                    catch (FileNotFoundException)
                    {
                        ContentDialog Tips = new ContentDialog
                        {
                            Title = "错误",
                            Content = "无法正确解析用户文件夹中的部分库文件夹\r可能已经被移动或不存在",
                            CloseButtonText = "确定",
                            Background = Application.Current.Resources["DialogAcrylicBrush"] as Brush
                        };
                        _ = await Tips.ShowAsync();
                    }
                }
                else
                {
                    ContentDialog Tips = new ContentDialog
                    {
                        Title = "错误",
                        Content = "无法正确解析用户文件夹，仅存在公用文件夹\r库文件无法正确显示",
                        CloseButtonText = "确定",
                        Background = Application.Current.Resources["DialogAcrylicBrush"] as Brush
                    };
                    _ = await Tips.ShowAsync();
                }
            }

            for (int i = 67; i <= 78; i++)
            {
                try
                {
                    var Device = await StorageFolder.GetFolderFromPathAsync((char)i + ":\\");
                    BasicProperties Properties = await Device.GetBasicPropertiesAsync();
                    IDictionary<string, object> PropertiesRetrieve = await Properties.RetrievePropertiesAsync(new string[] { "System.Capacity", "System.FreeSpace" });

                    HardDeviceList.Add(new HardDeviceInfo(Device, await Device.GetThumbnailBitmapAsync(), PropertiesRetrieve));
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        private void DeviceGrid_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if ((e.OriginalSource as FrameworkElement)?.DataContext is HardDeviceInfo Device)
            {
                MainPage.ThisPage.Nav.Navigate(typeof(FileControl), Device.Folder, new DrillInNavigationTransitionInfo());
            }
        }

        private void LibraryGrid_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if ((e.OriginalSource as FrameworkElement)?.DataContext is LibraryFolder Library)
            {
                MainPage.ThisPage.Nav.Navigate(typeof(FileControl), Library.Folder, new DrillInNavigationTransitionInfo());
            }
        }

        private async void QuickStartGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is QuickStartItem Item && Item.ProtocalUri != null)
            {
                await Launcher.LaunchUriAsync(Item.ProtocalUri);
            }
            else
            {
                QuickStartModifiedDialog dialog = new QuickStartModifiedDialog(QuickStartType.Application);
                _ = await dialog.ShowAsync();
            }
        }

        private async void WebGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is QuickStartItem Item && Item.ProtocalUri != null)
            {
                MainPage.ThisPage.Nav.Navigate(typeof(WebTab), Item.ProtocalUri, new DrillInNavigationTransitionInfo());
            }
            else
            {
                QuickStartModifiedDialog dialog = new QuickStartModifiedDialog(QuickStartType.WebSite);
                _ = await dialog.ShowAsync();
            }
        }

        private async void AppDelete_Click(object sender, RoutedEventArgs e)
        {
            StorageFile File = await StorageFile.GetFileFromPathAsync(CurrenItem.FullPath);
            await File.DeleteAsync(StorageDeleteOption.PermanentDelete);
            await SQLite.GetInstance().DeleteQuickStartItemAsync(CurrenItem);
            QuickStartList.Remove(CurrenItem);

            if (MainPage.ThisPage.AdminQuickStartCollection.ContainsKey(CurrenItem.DisplayName))
            {
                if (ApplicationData.Current.LocalSettings.Values["DeletedAdminQuickStart"] is string Query)
                {
                    ApplicationData.Current.LocalSettings.Values["DeletedAdminQuickStart"] = Query + "," + CurrenItem.DisplayName;
                }
                else
                {
                    ApplicationData.Current.LocalSettings.Values["DeletedAdminQuickStart"] = CurrenItem.DisplayName;
                }
            }
        }

        private async void AppEdit_Click(object sender, RoutedEventArgs e)
        {
            QuickStartModifiedDialog dialog = new QuickStartModifiedDialog(QuickStartType.UpdateApp, CurrenItem);
            _ = await dialog.ShowAsync();
        }

        private async void WebEdit_Click(object sender, RoutedEventArgs e)
        {
            QuickStartModifiedDialog dialog = new QuickStartModifiedDialog(QuickStartType.UpdateWeb, CurrenItem);
            _ = await dialog.ShowAsync();
        }

        private async void WebDelete_Click(object sender, RoutedEventArgs e)
        {
            StorageFile File = await StorageFile.GetFileFromPathAsync(CurrenItem.FullPath);
            await File.DeleteAsync(StorageDeleteOption.PermanentDelete);
            await SQLite.GetInstance().DeleteQuickStartItemAsync(CurrenItem);
            WebList.Remove(CurrenItem);

            if(MainPage.ThisPage.AdminQuickStartCollection.ContainsKey(CurrenItem.DisplayName))
            {
                if(ApplicationData.Current.LocalSettings.Values["DeletedAdminQuickStart"] is string Query)
                {
                    ApplicationData.Current.LocalSettings.Values["DeletedAdminQuickStart"] = Query + "," + CurrenItem.DisplayName;
                }
                else
                {
                    ApplicationData.Current.LocalSettings.Values["DeletedAdminQuickStart"] = CurrenItem.DisplayName;
                }
            }
        }

        private void QuickStartGridView_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            CurrenItem = (e.OriginalSource as FrameworkElement)?.DataContext as QuickStartItem;
            if (CurrenItem == null || CurrenItem.ProtocalUri == null)
            {
                QuickStartGridView.ContextFlyout = null;
            }
            else
            {
                QuickStartGridView.ContextFlyout = AppFlyout;
            }
        }

        private void WebGridView_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            CurrenItem = (e.OriginalSource as FrameworkElement)?.DataContext as QuickStartItem;
            if (CurrenItem == null || CurrenItem.ProtocalUri == null)
            {
                WebGridView.ContextFlyout = null;
            }
            else
            {
                WebGridView.ContextFlyout = WebFlyout;
            }
        }

        private async void Attribute_Click(object sender, RoutedEventArgs e)
        {
            DeviceInfoDialog Dialog = new DeviceInfoDialog(DeviceGrid.SelectedItem as HardDeviceInfo);
            _ = await Dialog.ShowAsync();
        }

        private void DeviceGrid_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            if ((e.OriginalSource as FrameworkElement)?.DataContext is HardDeviceInfo Context)
            {
                DeviceGrid.SelectedIndex = HardDeviceList.IndexOf(Context);
            }
            else
            {
                DeviceGrid.SelectedIndex = -1;
            }
        }

        private void DeviceGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Attribute.IsEnabled = DeviceGrid.SelectedIndex != -1;
        }

        private void OpenDevice_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceGrid.SelectedItem is HardDeviceInfo Device)
            {
                MainPage.ThisPage.Nav.Navigate(typeof(FileControl), Device.Folder, new DrillInNavigationTransitionInfo());
            }
        }
    }
}