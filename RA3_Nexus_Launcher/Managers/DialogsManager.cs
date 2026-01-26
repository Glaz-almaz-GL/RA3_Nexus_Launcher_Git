using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RA3_Nexus_Launcher.Managers
{
    public static class DialogsManager
    {
        private const string EmptyTitleMsg = "Title cannot be null or empty";
        private static TopLevel? _topLevel;

        // Статический метод для инициализации
        public static void Initialize(TopLevel topLevel)
        {
            _topLevel = topLevel;
        }

        #region Методы выбора папок

        public static async Task<IStorageFolder?> ShowOpenSingleFolderDialogAsync(string title)
        {
            if (_topLevel == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(EmptyTitleMsg, nameof(title));
            }

            FolderPickerOpenOptions options = CreateFolderPickerOptions(title, false);
            IReadOnlyList<IStorageFolder> folder = await _topLevel.StorageProvider.OpenFolderPickerAsync(options);

            return folder.Count > 0 ? folder[0] : null;
        }

        public static async Task<List<IStorageFolder>> ShowOpenMultipleFolderDialogAsync(string title)
        {
            if (_topLevel == null)
            {
                return [];
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(EmptyTitleMsg, nameof(title));
            }

            FolderPickerOpenOptions options = CreateFolderPickerOptions(title, false);
            IReadOnlyList<IStorageFolder> folders = await _topLevel.StorageProvider.OpenFolderPickerAsync(options);

            return (List<IStorageFolder>)(folders.Count > 0 ? folders : []);
        }

        #endregion Методы выбора папок

        #region Методы выбора файлов

        public static async Task<IStorageFile?> ShowOpenSingleFileDialogAsync(string title, IEnumerable<string>? allowedExtensions = null)
        {
            if (_topLevel == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(EmptyTitleMsg, nameof(title));
            }

            FilePickerOpenOptions options = CreateFilePickerOptions(title, allowedExtensions, false);
            IReadOnlyList<IStorageFile> files = await _topLevel.StorageProvider.OpenFilePickerAsync(options);

            return files.Count > 0 ? files[0] : null;
        }

        public static async Task<List<IStorageFile>> ShowOpenMultipleFilesDialogAsync(string title, IEnumerable<string>? allowedExtensions = null)
        {
            if (_topLevel == null)
            {
                return [];
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(EmptyTitleMsg, nameof(title));
            }

            FilePickerOpenOptions options = CreateFilePickerOptions(title, allowedExtensions, true);
            IReadOnlyList<IStorageFile> files = await _topLevel.StorageProvider.OpenFilePickerAsync(options);

            return (List<IStorageFile>)(files.Count > 0 ? files : []);
        }

        #endregion Методы выбора файлов

        #region Приватные помощники

        private static FilePickerOpenOptions CreateFilePickerOptions(
            string title,
            IEnumerable<string>? allowedExtensions,
            bool allowMultiple)
        {
            FilePickerOpenOptions options = new()
            {
                Title = title,
                AllowMultiple = allowMultiple
            };

            if (allowedExtensions?.Any() == true)
            {
                options.FileTypeFilter =
                [
                    new FilePickerFileType(title)
                    {
                        Patterns = [.. allowedExtensions]
                    }
                ];
            }

            return options;
        }

        private static FolderPickerOpenOptions CreateFolderPickerOptions(
            string title,
            bool allowMultiple)
        {
            FolderPickerOpenOptions options = new()
            {
                Title = title,
                AllowMultiple = allowMultiple
            };

            return options;
        }

        #endregion Приватные помощники
    }
}