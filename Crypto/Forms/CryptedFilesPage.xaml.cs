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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

 //Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace Crypto
{
     ///<summary>
     ///Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
     ///</summary>
    public sealed partial class CryptedFilesPage : Page
    {
        public CryptedFilesPage()
        {
            this.InitializeComponent();
        }

        private static StorageFolder CryptFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

         //<summary>
         //Вызывается перед отображением этой страницы во фрейме.
         //</summary>
         //<param name="e">Данные о событиях, описывающие, каким образом была достигнута эта страница.  Свойство Parameter
         //обычно используется для настройки страницы.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            string searchFilter = e.Parameter != null ? e.Parameter.ToString() : string.Empty;
            barProgress.IsIndeterminate = true;
            IReadOnlyList<StorageFile> files = await Task.Run(() => Engine.GetFiles(CryptFolder));
            ListViewItem lvi;
            foreach (StorageFile sf in files)
            {
                if (sf.Name == "logFile.txt" || Path.GetExtension(sf.Name.ToLower()).Contains(".pss")) continue;
                else
                {
                    lvi = new ListViewItem() { Content = string.Format("   {0}", sf.Name) };
                    lvi.Tag = sf;
                    lstCryptedFiles.Items.Add(lvi);
                    if (!string.IsNullOrEmpty(searchFilter) && sf.Path.ToLower().Contains(searchFilter.ToLower())) lstCryptedFiles.SelectedItem = lvi;
                    else ;
                }
            }
            barProgress.IsIndeterminate = false;
            try
            {
                DataTransferManager currentManager = DataTransferManager.GetForCurrentView();
                currentManager.DataRequested += currentManager_DataRequested;
            }
            catch (Exception ex) { }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            DataTransferManager currentManager = DataTransferManager.GetForCurrentView();
            currentManager.DataRequested -= currentManager_DataRequested;
        }


        private void lstCryptedFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BanEditButtons(lstCryptedFiles.SelectedItems.Count == 0);
        }


        private void btnGoToCrypt_Tapped(object sender, TappedRoutedEventArgs e)
        {
            BanAddDelButtons(true);
            BanEditButtons(true);
            string file = ((lstCryptedFiles.SelectedItem as ListViewItem).Tag as StorageFile).Name;
            if (!string.IsNullOrEmpty(file))
            {
                Engine.GoToAnotherFrame(typeof(MainPage), file);
            }
            else ;
            BanAddDelButtons(true);
        }

        private void btnBackToMain_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Engine.GoToAnotherFrame(typeof(MainPage), null);
        }


        private async void btnAddCryptedFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile file = await Task.Run(() => Engine.AddFile("*"));
            if (file != null)
            {
                file = await Task.Run(() => Engine.CopyFile(file, Windows.Storage.ApplicationData.Current.LocalFolder));
                ListViewItem lvi = new ListViewItem() { Content = string.Format("   {0}", file.Name) };
                lvi.Tag = file;
                lstCryptedFiles.Items.Add(lvi);
                lstCryptedFiles.SelectedIndex = lstCryptedFiles.Items.Count - 1;
            }
            else return;
        }

        private async void btnMoveCryptedFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile cryptFile = (lstCryptedFiles.SelectedItem as ListViewItem).Tag as StorageFile;
            if (cryptFile != null && await Task.Run(() => Engine.MoveFile(cryptFile)))
            {
                lstCryptedFiles.SelectedIndex = -1;
                cryptFile = null;
                Engine.SendMessage("File is exported successfully");
            }
            else return;
        }

        private async void btnDelCryptedFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile cryptFile = (lstCryptedFiles.SelectedItem as ListViewItem).Tag as StorageFile;
            if (cryptFile != null && await Engine.MessageDialog("Are you sure you want to delete file?") && 
                await Task.Run(() => Engine.DeleteFile(cryptFile)))
            {
                lstCryptedFiles.Items.Remove(lstCryptedFiles.SelectedItem);
                lstCryptedFiles.SelectedIndex = -1;
                cryptFile = null;
                Engine.SendMessage("File is deleted successfully");
            }
            else return;
        }

        private void btnRenameCryptedFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile cryptFile = (lstCryptedFiles.SelectedItem as ListViewItem).Tag as StorageFile;
            if (cryptFile != null)
            {
                pnlRenameCryptedFiles.Visibility = Windows.UI.Xaml.Visibility.Visible;
                txRenameCryptedFiles.Text = Path.GetFileNameWithoutExtension(cryptFile.Path);
                BanAddDelButtons(true);
                BanEditButtons(true);
            }
            else return;
        }

        private async void btnSaveRenameCryptedFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile cryptFile = (lstCryptedFiles.SelectedItem as ListViewItem).Tag as StorageFile;
            if (!string.IsNullOrEmpty(txRenameCryptedFiles.Text) && cryptFile != null &&
                await Task.Run(() => Engine.RenameFile(cryptFile, string.Format("{0}{1}", txRenameCryptedFiles.Text, Path.GetExtension(cryptFile.Name)))))
            {
                (lstCryptedFiles.SelectedItem as ListViewItem).Content = string.Format("   {0}", cryptFile.Name);
            }
            else return;
            txRenameCryptedFiles.Text = string.Empty;
            pnlRenameCryptedFiles.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BanAddDelButtons(false);
            BanEditButtons(false);
        }

        private void btnCancelRenameCryptedFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            txRenameCryptedFiles.Text = string.Empty;
            pnlRenameCryptedFiles.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BanAddDelButtons(false);
            BanEditButtons(false);
        }

        private async void btnDelAllCryptedFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (await Engine.MessageDialog("Are you sure you want to delete all files?"))
            {
                List<ListViewItem> temp = new List<ListViewItem>();
                foreach (ListViewItem lvi in lstCryptedFiles.Items)
                {
                    if ((lvi.Tag as StorageFile) != null && await Task.Run(() => Engine.DeleteFile(lvi.Tag as StorageFile)))
                    {
                        temp.Add(lvi);
                    }
                    else ;
                }
                if (temp.Count == lstCryptedFiles.Items.Count) Engine.SendMessage("Files are deleted successfully");
                else ;
                foreach (ListViewItem lvi in temp) lstCryptedFiles.Items.Remove(lvi);
            }
            else return;
        }

        
        private void ElementsResize()
        {
            Rect bounds = Window.Current.Bounds;
            pnlRenameCryptedFiles.MaxWidth = lstCryptedFiles.MaxWidth = (int)bounds.Width - 20;
            lstCryptedFiles.MaxHeight = (int)bounds.Height - 300;
        }

        private void BanEditButtons(bool isBanned)
        {
            btnMoveCryptedFiles.IsEnabled = btnRenameCryptedFiles.IsEnabled = btnDelCryptedFiles.IsEnabled = btnGoToCrypt.IsEnabled = !isBanned;
        }

        private void BanAddDelButtons(bool isBanned)
        {
            btnAddCryptedFiles.IsEnabled = btnDelAllCryptedFiles.IsEnabled = !isBanned;
        }


        void currentManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            List<StorageFile> sharedFiles = new List<StorageFile>();
            foreach (ListViewItem lvi in lstCryptedFiles.SelectedItems) sharedFiles.Add(lvi.Tag as StorageFile);
            if (sharedFiles.Count == 0) return;
            else
            {
                try
                {
                    DataRequestDeferral defferal = args.Request.GetDeferral();
                    args.Request.Data.Properties.Title = "Crypted files";
                    args.Request.Data.SetStorageItems(sharedFiles);
                    defferal.Complete();
                }
                catch (Exception ex)
                {
                    Engine.LogMessage(ex);
                    Engine.SendMessage(ex.Message);
                }
            }
        }

    }
}
