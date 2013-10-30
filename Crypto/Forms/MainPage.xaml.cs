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


using Crypto.Classes;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Crypto
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private StorageFile cryptFile = null;
        private static StorageFolder CryptFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            cryptFile = null;
            string fileFilter = e.Parameter != null ? e.Parameter.ToString() : string.Empty;
            barProgress.IsIndeterminate = true;
            IReadOnlyList<StorageFile> files = await Task.Run(() => Engine.GetFiles(CryptFolder));
            ListViewItem lvi;
            foreach (StorageFile sf in files)
            {
                if (sf.Name == "logFile.txt") continue;
                else
                {
                    lvi = new ListViewItem() { Content = Path.GetFileName(sf.Path) };
                    lvi.Tag = sf;
                    if (Path.GetExtension(sf.Path.ToLower()).Equals(".pss"))
                    {
                        lvi.Content = string.Format("   {0}", Path.GetFileNameWithoutExtension(sf.Path));
                        lstPass.Items.Add(lvi);
                        if (!string.IsNullOrEmpty(fileFilter) && sf.Path.ToLower().Contains(fileFilter.ToLower())) lstPass.SelectedItem = lvi;
                        else ;
                    }
                    else 
                    {
                        if (!string.IsNullOrEmpty(fileFilter) && sf.Path.ToLower().Contains(fileFilter.ToLower()))
                        {
                            txFile.Text = sf.Name;
                            cryptFile = sf;
                        }
                        else continue;
                    }
                }
            }
            barProgress.IsIndeterminate = false;
            ElementsResize();
            BanEditButtons(true);
            try
            {
                DataTransferManager currentManager = DataTransferManager.GetForCurrentView();
                currentManager.DataRequested += currentManager_DataRequested;
            }
            catch(Exception ex){}
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            DataTransferManager currentManager = DataTransferManager.GetForCurrentView();
            currentManager.DataRequested -= currentManager_DataRequested;
        }

 
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ElementsResize();
        }
 

        private void btnCollapseFile_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (grFile.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
            {
                grFile.Visibility = Windows.UI.Xaml.Visibility.Visible;
                btnCollapseFile.Content = "- ";
            }
            else
            {
                grFile.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                btnCollapseFile.Content = "+ ";
            }
        }

        private void btnCollapsePassFile_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (pnlCollapsePasses.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
            {
                pnlCollapsePasses.Visibility = Windows.UI.Xaml.Visibility.Visible;
                btnCollapsePassFile.Content = "- ";
            }
            else
            {
                pnlCollapsePasses.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                btnCollapsePassFile.Content = "+ ";
            }
        }


        private async void CryptButton_Tapped(object sender, RoutedEventArgs e)
        {
            tbMessage.Text = "File encryption in process. Please wait... ";
            BanAddDelButtons(true);
            BanEditButtons(true);
            StorageFile passFile = lstPass.SelectedItems.Count == 0 ? null : (lstPass.SelectedItem as ListViewItem).Tag as StorageFile;
            barProgress.IsIndeterminate = true;
            if (ValueValidate(cryptFile, passFile))
            {
                cryptFile = await Task.Run(() => Engine.Encrypt(cryptFile, passFile));
                if (cryptFile != null)
                    Engine.SendMessage("File is encrypted");
                else ;
            }
            else ;
            barProgress.IsIndeterminate = false;
            txFile.Text = string.Empty;
            tbMessage.Text = string.Empty;
            lstPass.SelectedIndex = -1;
            BanAddDelButtons(false);
            if (cryptFile != null) Engine.GoToAnotherFrame(typeof(CryptedFilesPage), cryptFile.Name);
        }

        private async void DecryptButton_Tapped(object sender, RoutedEventArgs e)
        {
            tbMessage.Text = "File decryption in process. Please wait... ";
            BanAddDelButtons(true);
            BanEditButtons(true);
            StorageFile passFile = lstPass.SelectedItems.Count == 0 ? null : (lstPass.SelectedItem as ListViewItem).Tag as StorageFile;
            barProgress.IsIndeterminate = true;
            if (ValueValidate(cryptFile, passFile))
            {
                cryptFile = await Task.Run(() => Engine.Decrypt(cryptFile, passFile));
                if (cryptFile != null)
                    Engine.SendMessage("File is decrypted");
                else ;
            }
            else ;
            barProgress.IsIndeterminate = false;
            txFile.Text = string.Empty;
            tbMessage.Text = string.Empty;
            lstPass.SelectedIndex = -1;
            BanAddDelButtons(false);
            if (cryptFile != null) Engine.GoToAnotherFrame(typeof(CryptedFilesPage), cryptFile.Name);
        }

        private void CryptFilesButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Engine.GoToAnotherFrame(typeof(CryptedFilesPage), null);
        }


        private void lstPass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BanEditButtons(lstPass.SelectedItems.Count == 0);
        }

        
        private async void btnFile_Tapped(object sender, RoutedEventArgs e)
        {
            StorageFile file = await Task.Run(() => Engine.AddFile("*"));
            if (file != null)
            {
                txFile.Text = file.Path;
                cryptFile = file;
            }
            else return;
        }

        private void btnNewPass_Tapped(object sender, RoutedEventArgs e)
        {
            pnlNewPass.Visibility = Windows.UI.Xaml.Visibility.Visible;
            BanAddDelButtons(true);
            BanEditButtons(true);
        }
 
        private async void btnSavePass_Tapped(object sender, RoutedEventArgs e)
        {
            tbMessage.Text = "Saving pass. Please wait... ";
            string fileName = DateTime.Now.ToString("ddMMyyyyhhmmss") + ".pss";
            int passSize = (int)sldPassSize.Value;
            barProgress.IsIndeterminate = true;
            StorageFile file = await Task.Run(() => Pass.MakePass(passSize, fileName, CryptFolder));
            barProgress.IsIndeterminate = false;
            if (file != null)
            {
                ListViewItem lvi = new ListViewItem() { Content = string.Format("   {0}", Path.GetFileNameWithoutExtension(file.Path)) };
                lvi.Tag = file;
                lstPass.Items.Add(lvi);
                lstPass.SelectedIndex = lstPass.Items.Count - 1;
            }
            else ;
            pnlNewPass.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BanAddDelButtons(false);
            tbMessage.Text = string.Empty;
        }

        private void btnCancelPass_Tapped(object sender, TappedRoutedEventArgs e)
        {
            pnlNewPass.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BanAddDelButtons(false);
        }

        private async void btnAddPass_Tapped(object sender, RoutedEventArgs e)
        {
            StorageFile file = await Task.Run(() => Engine.AddFile(".pss"));
            if (file != null)
            {
                if (file.Name.ToLower().Contains(".pss"))
                {
                    file = await Task.Run(() => Engine.CopyFile(file, Windows.Storage.ApplicationData.Current.LocalFolder));
                    if (file != null)
                    {
                        ListViewItem lvi = new ListViewItem() { Content = string.Format("   {0}", Path.GetFileNameWithoutExtension(file.Path)) };
                        lvi.Tag = file;
                        lstPass.Items.Add(lvi);
                    }
                    lstPass.SelectedIndex = lstPass.Items.Count - 1;
                }
                else Engine.SendMessage("File is not found or has the wrong format");
            }
            else return;
        }

        private async void btnMovePass_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile passFile = (lstPass.SelectedItem as ListViewItem).Tag as StorageFile;
            if (passFile != null && await Task.Run(() => Engine.MoveFile(passFile)))
            {
                lstPass.SelectedIndex = -1;
                passFile = null;
                Engine.SendMessage("File is exported successfully");
            }
            else return;
        }

        private void btnRenamePass_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile passFile = (lstPass.SelectedItem as ListViewItem).Tag as StorageFile;
            if (passFile != null)
            {
                pnlRenamePass.Visibility = Windows.UI.Xaml.Visibility.Visible;
                txRenamePass.Text = Path.GetFileNameWithoutExtension(passFile.Path);
                BanAddDelButtons(true);
                BanEditButtons(true);
            }
            else return;
        }

        private async void btnSaveRenamePass_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile passFile = (lstPass.SelectedItem as ListViewItem).Tag as StorageFile;
            if (!string.IsNullOrEmpty(txRenamePass.Text) && passFile != null) 
            {
                cryptFile = await Task.Run(() => Engine.Encrypt(cryptFile, passFile));
                if (await Task.Run(() => Engine.RenameFile(passFile, string.Format("{0}{1}", txRenamePass.Text, Path.GetExtension(passFile.Name)))))
                    (lstPass.SelectedItem as ListViewItem).Content = string.Format("   {0}", Path.GetFileNameWithoutExtension(passFile.Path));
                else ;
            }
            else ;
            txRenamePass.Text = string.Empty;
            pnlRenamePass.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BanAddDelButtons(false);
            BanEditButtons(lstPass.Items.Count == 0);
        }

        private void btnCancelRenamePass_Tapped(object sender, TappedRoutedEventArgs e)
        {
            txRenamePass.Text = string.Empty;
            pnlRenamePass.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            BanAddDelButtons(false);
            BanEditButtons(false);
        }

        private async void btnDelPass_Tapped(object sender, RoutedEventArgs e)
        {
            StorageFile passFile = (lstPass.SelectedItem as ListViewItem).Tag as StorageFile;
            if (passFile != null && await Engine.MessageDialog("Are you sure you want to delete file?") && await Task.Run(() => Engine.DeleteFile(passFile)))
            {
                lstPass.Items.Remove(lstPass.SelectedItem);
                lstPass.SelectedIndex = -1;
                passFile = null;
                Engine.SendMessage("File is deleted successfully");
            }
            else return;
        }

        private async void btnDelAllPass_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (await Engine.MessageDialog("Are you sure you want to delete all files?"))
            {
                List<ListViewItem> temp = new List<ListViewItem>();
                foreach (ListViewItem lvi in lstPass.Items)
                {
                    if ((lvi.Tag as StorageFile) != null && await Task.Run(() => Engine.DeleteFile(lvi.Tag as StorageFile)))
                    {
                        temp.Add(lvi);
                    }
                    else ;
                }
                if (temp.Count == lstPass.Items.Count) Engine.SendMessage("Files are deleted successfully");
                else ;
                foreach (ListViewItem lvi in temp) lstPass.Items.Remove(lvi);
            }
            else return;
        }
        
        private void ElementsResize()
        {
            Rect bounds = Window.Current.Bounds;
            grFile.MaxWidth = pnlNewPass.MaxWidth = pnlRenamePass.MaxWidth = lstPass.MaxWidth = (int)bounds.Width - 20;
            lstPass.MaxHeight = (int)bounds.Height - 470;
        }

        private bool ValueValidate(StorageFile cryptFile, StorageFile passFile)
        {
            if (cryptFile == null)
            {
                Engine.SendMessage("Please, choose file for encrypt/decrypt!");
                return false;
            }
            else
            {
                if (passFile == null)
                {
                    Engine.SendMessage("Please, choose pass' file!");
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        private void BanEditButtons(bool isBanned)
        {
            btnMovePass.IsEnabled = btnRenamePass.IsEnabled = btnDelPass.IsEnabled = CryptButton.IsEnabled = DecryptButton.IsEnabled = !isBanned;
        }

        private void BanAddDelButtons(bool isBanned)
        {
            btnFile.IsEnabled = btnNewPass.IsEnabled = btnAddPass.IsEnabled = btnDelAllPass.IsEnabled = CryptFilesButton.IsEnabled = !isBanned;
        }

        
        void currentManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            List<StorageFile> sharedFiles = new List<StorageFile>();
            foreach (ListViewItem lvi in lstPass.SelectedItems) sharedFiles.Add(lvi.Tag as StorageFile);
            if (sharedFiles.Count == 0) return;
            else
            {
                try
                {
                    DataRequestDeferral defferal = args.Request.GetDeferral();
                    args.Request.Data.Properties.Title = "Pass' files";
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


