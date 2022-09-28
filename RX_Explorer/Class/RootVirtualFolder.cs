﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace RX_Explorer.Class
{
    public sealed class RootVirtualFolder : FileSystemStorageFolder
    {
        private static RootVirtualFolder Instance;
        private static readonly object Locker = new object();

        public static RootVirtualFolder Current
        {
            get
            {
                lock (Locker)
                {
                    return Instance ??= new RootVirtualFolder();
                }
            }
        }

        public override string Name => Globalization.GetString("RootStorageFolderDisplayName");

        public override string DisplayName => Name;

        public override Task<ulong> GetFolderSizeAsync(CancellationToken CancelToken = default)
        {
            return Task.FromResult((ulong)0);
        }

        protected override Task LoadCoreAsync(bool ForceUpdate)
        {
            return Task.CompletedTask;
        }

        protected override Task<StorageFolder> GetStorageItemCoreAsync()
        {
            return Task.FromResult<StorageFolder>(null);
        }

        protected override Task<BitmapImage> GetThumbnailCoreAsync(ThumbnailMode Mode, bool ForceUpdate = false)
        {
            return Task.FromResult(new BitmapImage(new Uri("ms-appx:///Assets/ThisPC.png")));
        }

        protected override async Task<IRandomAccessStream> GetThumbnailRawStreamCoreAsync(ThumbnailMode Mode, bool ForceUpdate = false)
        {
            StorageFile ThumbnailFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/ThisPC.png"));
            return await ThumbnailFile.OpenAsync(FileAccessMode.Read);
        }

        public override IAsyncEnumerable<FileSystemStorageItemBase> GetChildItemsAsync(bool IncludeHiddenItems = false,
                                                                                       bool IncludeSystemItems = false,
                                                                                       bool IncludeAllSubItems = false,
                                                                                       CancellationToken CancelToken = default,
                                                                                       BasicFilters Filter = BasicFilters.File | BasicFilters.Folder,
                                                                                       Func<string, bool> AdvanceFilter = null)
        {
            return AsyncEnumerable.Empty<FileSystemStorageItemBase>();
        }

        public override IAsyncEnumerable<FileSystemStorageItemBase> SearchAsync(string SearchWord,
                                                                                bool SearchInSubFolders = false,
                                                                                bool IncludeHiddenItems = false,
                                                                                bool IncludeSystemItems = false,
                                                                                bool IsRegexExpression = false,
                                                                                bool IsAQSExpression = false,
                                                                                bool UseIndexerOnly = false,
                                                                                bool IgnoreCase = true,
                                                                                CancellationToken CancelToken = default)
        {
            return CommonAccessCollection.DriveList.Select((Item) => Item.DriveFolder.SearchAsync(SearchWord, SearchInSubFolders, IncludeHiddenItems, IncludeSystemItems, IsRegexExpression, IsAQSExpression, UseIndexerOnly, IgnoreCase, CancelToken)).Merge();
        }

        private RootVirtualFolder() : base(new NativeFileData("RootFolderUniquePath", default))
        {

        }
    }
}
