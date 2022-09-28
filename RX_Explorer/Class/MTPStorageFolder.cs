﻿using Microsoft.Win32.SafeHandles;
using RX_Explorer.Interface;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace RX_Explorer.Class
{
    public class MTPStorageFolder : FileSystemStorageFolder, IMTPStorageItem, INotWin32StorageFolder
    {
        public string DeviceId => @$"\\?\{new string(Path.Skip(4).ToArray()).Split(@"\", StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()}";

        protected override Task<BitmapImage> GetThumbnailCoreAsync(ThumbnailMode Mode, bool ForceUpdate = false)
        {
            return Task.FromResult(new BitmapImage(WindowsVersionChecker.IsNewerOrEqual(Version.Windows11)
                                                            ? new Uri("ms-appx:///Assets/FolderIcon_Win11.png")
                                                            : new Uri("ms-appx:///Assets/FolderIcon_Win10.png")));
        }

        protected override async Task<IRandomAccessStream> GetThumbnailRawStreamCoreAsync(ThumbnailMode Mode, bool ForceUpdate = false)
        {
            try
            {
                StorageFile ThumbnailFile = await StorageFile.GetFileFromApplicationUriAsync(WindowsVersionChecker.IsNewerOrEqual(Version.Windows11)
                                                                                                                ? new Uri("ms-appx:///Assets/FolderIcon_Win11.png")
                                                                                                                : new Uri("ms-appx:///Assets/FolderIcon_Win10.png"));
                return await ThumbnailFile.OpenReadAsync();
            }
            catch (Exception ex)
            {
                LogTracer.Log(ex, "Could not get the raw stream of thumbnail");
            }

            return null;
        }

        protected override Task LoadCoreAsync(bool ForceUpdate)
        {
            return Task.CompletedTask;
        }

        protected override Task<StorageFolder> GetStorageItemCoreAsync()
        {
            return Task.FromResult<StorageFolder>(null);
        }

        public override Task<IReadOnlyDictionary<string, string>> GetPropertiesAsync(IEnumerable<string> Properties)
        {
            return Task.FromResult<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>(Properties.Select((Prop) => new KeyValuePair<string, string>(Prop, string.Empty))));
        }

        public override async IAsyncEnumerable<FileSystemStorageItemBase> GetChildItemsAsync(bool IncludeHiddenItems = false,
                                                                                             bool IncludeSystemItems = false,
                                                                                             bool IncludeAllSubItems = false,
                                                                                             [EnumeratorCancellation] CancellationToken CancelToken = default,
                                                                                             BasicFilters Filter = BasicFilters.File | BasicFilters.Folder,
                                                                                             Func<string, bool> AdvanceFilter = null)
        {
            using (AuxiliaryTrustProcessController.Exclusive Exclusive = await AuxiliaryTrustProcessController.GetControllerExclusiveAsync())
            {
                foreach (MTPFileData Data in await Exclusive.Controller.GetMTPChildItemsDataAsync(Path, IncludeHiddenItems, IncludeSystemItems, IncludeAllSubItems, Filter, CancelToken))
                {
                    if (Data.Attributes.HasFlag(System.IO.FileAttributes.Directory))
                    {
                        yield return new MTPStorageFolder(Data);
                    }
                    else
                    {
                        yield return new MTPStorageFile(Data);
                    }
                }
            }
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
            if (IsAQSExpression)
            {
                throw new ArgumentException($"{nameof(IsAQSExpression)} is not supported");
            }

            IAsyncEnumerable<FileSystemStorageItemBase> Result = GetChildItemsAsync(IncludeHiddenItems, IncludeSystemItems, SearchInSubFolders, CancelToken: CancelToken);

            return IsRegexExpression ? Result.Where((Item) => Regex.IsMatch(Item.Name, SearchWord, IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None))
                                     : Result.Where((Item) => Item.Name.Contains(SearchWord, IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
        }

        public override async Task<FileSystemStorageItemBase> CreateNewSubItemAsync(string Name, CreateType ItemType, CreateOption Option)
        {
            using (AuxiliaryTrustProcessController.Exclusive Exclusive = await AuxiliaryTrustProcessController.GetControllerExclusiveAsync())
            {
                MTPFileData Data = await Exclusive.Controller.MTPCreateSubItemAsync(Path, Name, ItemType, Option);

                if (Data != null)
                {
                    switch (ItemType)
                    {
                        case CreateType.File:
                            {
                                return new MTPStorageFile(Data);
                            }
                        case CreateType.Folder:
                            {
                                return new MTPStorageFolder(Data);
                            }
                    }
                }

                return null;
            }
        }

        public override Task<SafeFileHandle> GetNativeHandleAsync(AccessMode Mode, OptimizeOption Option)
        {
            return Task.FromResult(new SafeFileHandle(IntPtr.Zero, true));
        }

        protected override Task<BitmapImage> GetThumbnailOverlayAsync()
        {
            return Task.FromResult<BitmapImage>(null);
        }

        public async Task<MTPFileData> GetRawDataAsync()
        {
            try
            {
                if (GetBulkAccessSharedController(out var ControllerRef))
                {
                    using (ControllerRef)
                    {
                        return await ControllerRef.Value.Controller.GetMTPItemDataAsync(Path);
                    }
                }
                else
                {
                    using (AuxiliaryTrustProcessController.Exclusive Exclusive = await AuxiliaryTrustProcessController.GetControllerExclusiveAsync(Priority: PriorityLevel.Low))
                    {
                        return await Exclusive.Controller.GetMTPItemDataAsync(Path);
                    }
                }
            }
            catch (Exception ex)
            {
                LogTracer.Log(ex, $"An unexpected exception was threw in {nameof(GetRawDataAsync)}");
            }

            return null;
        }

        public MTPStorageFolder(MTPFileData Data) : base(Data)
        {

        }
    }
}
