//This file is part of THE CRYPTO.
//THE CRYPTO is the program for encrypting files for Windows 8.
// Copyright (C) 2013  Daria V. Korosteleva <coooshmarik@gmail.com>

// THE CRYPTO is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// THE CRYPTO is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.IO;
using Crypto.Classes;
using System.Collections.Generic;
using Windows.Storage.FileProperties;
using Windows.Storage;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage.Streams;
using System.Runtime.Serialization;
using System.Xml;
using Windows.Storage.Pickers;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Popups;


namespace Crypto
{
    public static class Engine
    {
        static StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        internal async static Task<StorageFile> SetFile(List<short> bytes, StorageFile file, string filename)
        {
            try
            {
                byte[] byteArray = new byte[bytes.Count];
                for (int i = 0; i < bytes.Count; i++) byteArray[i] = (byte)bytes[i];
                StorageFile cryptFile =
                    await storageFolder.CreateFileAsync(filename, CreationCollisionOption.FailIfExists);
                Stream stream = await cryptFile.OpenStreamForWriteAsync();
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(byteArray);
                writer.Flush();
                return cryptFile;
            }
            catch (Exception ex)
            {
                LogMessage(ex);
                SendMessage(ex.Message);
                return null;
            }
        }

        internal async static Task<List<short>> GetFile(StorageFile file)
        {
            try
            {
                List<short> bytes = new List<short>();
                Stream stream = (await file.OpenReadAsync()).AsStreamForRead();
                BinaryReader reader = new BinaryReader(stream);
                BasicProperties props = await file.GetBasicPropertiesAsync();
                for (int i = 0; i < (int)props.Size; i++) bytes.Add((short)reader.ReadByte());
                return bytes;
            }
            catch (Exception ex) 
            {
                LogMessage(ex);
                SendMessage(ex.Message);
                return null;
            }
        }

        internal async static Task<IReadOnlyList<StorageFile>> GetFiles(StorageFolder folder)
        { 
            try
            {
                return await folder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName);
            }
            catch (Exception ex)
            {
                LogMessage(ex);
                SendMessage(ex.Message);
                return null;
            }
        }

        internal async static Task<StorageFile> AddFile(string extension)
        {
            FileOpenPicker fop = new FileOpenPicker();
            fop.FileTypeFilter.Add(extension);
            fop.SuggestedStartLocation = PickerLocationId.Desktop;
            try
            {
                return await fop.PickSingleFileAsync();
            }
            catch(Exception ex) 
            {
                LogMessage(ex);
                SendMessage(ex.Message);
                return null;
            }
        }

        internal async static Task<StorageFile> CopyFile(StorageFile file, StorageFolder folder)
        {
            try
            {
                file = await file.CopyAsync(folder);
                return file;
            }
            catch (Exception ex)
            {
                LogMessage(ex);
                SendMessage(ex.Message);
                return null;
            }
        }
        
        internal async static Task<bool> DeleteFile(StorageFile file)
        {
            try
            {
                await file.DeleteAsync();
                return true;
            }
            catch (Exception ex)
            {
                LogMessage(ex);
                SendMessage(ex.Message);
                return false;
            }
        }

        internal async static Task<bool> MoveFile(StorageFile file)
        {
            FolderPicker fp = new FolderPicker();
            fp.FileTypeFilter.Add("*");
            try
            {
                StorageFolder folder = await fp.PickSingleFolderAsync();
                if (folder != null)
                {
                    await file.CopyAsync(folder);
                }
                else return false;
                return true;
            }
            catch (Exception ex)
            {
                LogMessage(ex);
                SendMessage(ex.Message);
                return false;
            }
        }

        internal async static Task<bool> RenameFile(StorageFile file, string name)
        {
            try
            {
                await file.RenameAsync(name);
                return true;
            }
            catch (Exception ex) 
            {
                LogMessage(ex);
                SendMessage(ex.Message);
                return false;
            }
        }


        internal async static Task<StorageFile> Encrypt(StorageFile file, StorageFile pass)
        {
            try
            {
                string filename = string.Format(@"crt_{0}_{1}{2}", Path.GetFileNameWithoutExtension(file.Name), DateTime.Now.ToString("ddMMyyyyhhmmss"), Path.GetExtension(file.Name));
                List<short> fileBytes = await GetFile(file);
                Pass passBytes = await Pass.LoadPass(pass);
                fileBytes = Crypt.EnCrypt(fileBytes, passBytes);
                file = await SetFile(fileBytes, file, filename);
                return file;
            }
            catch (Exception ex)
            {
                LogMessage(ex);
                SendMessage(ex.Message);
                return null;
            }
        }

        internal async static Task<StorageFile> Decrypt(StorageFile file, StorageFile pass)
        {
            try
            {
                string filename = string.Format(@"dcrt_{0}_{1}{2}", Path.GetFileNameWithoutExtension(file.Name), DateTime.Now.ToString("ddMMyyyyhhmmss"), Path.GetExtension(file.Name));
                List<short> filebytes = await GetFile(file);
                Pass passBytes = await Pass.LoadPass(pass);
                filebytes = Crypt.DeCrypt(filebytes, passBytes);
                file = await SetFile(filebytes, file, filename);
                return file;
            }
            catch (Exception ex)
            {
                LogMessage(ex);
                SendMessage(ex.Message);
                return null;
            }
        }


        internal async static Task<bool> MessageDialog(string message)
        {
            bool result = false;
            MessageDialog dlg = new MessageDialog(message, "Attention!");
            dlg.Commands.Add(new UICommand("OK", new UICommandInvokedHandler((cmd) => result = true)));
            dlg.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler((cmd) => result = false)));
            IUICommand command = await dlg.ShowAsync();
            return result;
        }

        internal static void SendMessage(string mess)
        {
            string xml = string.Format(@"<toast>
<visual>
<binding template=""ToastText01"">
<text id=""1"">{0}</text>
</binding>
</visual>
</toast>", mess);
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xml);
                ScheduledToastNotification toast = new ScheduledToastNotification(xmlDocument, DateTimeOffset.Now.AddSeconds(10));
                ToastNotifier notifier = ToastNotificationManager.CreateToastNotifier();
                notifier.AddToSchedule(toast);
            }
            catch (Exception ex)
            {
                LogMessage(ex);
            }
        }

        internal async static void LogMessage(Exception ex)
        {
            StorageFile logFile = null;
            bool fileExist;
            try
            {
                logFile = await storageFolder.GetFileAsync("logFile.txt");
                fileExist = true;
            }
            catch (Exception en)
            {
                fileExist = false;
            }
            if (!fileExist)
            {
                logFile = await storageFolder.CreateFileAsync("logFile.txt");
            }
            else ;
            await FileIO.AppendTextAsync(logFile, string.Format("{0} {1}", ex.Message, ex.StackTrace));
        }

        internal static void GoToAnotherFrame(Type page, object parameter)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(page, parameter);
        }
    }
}
