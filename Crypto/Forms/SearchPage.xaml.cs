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
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace Crypto
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        public SearchPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Вызывается перед отображением этой страницы во фрейме.
        /// </summary>
        /// <param name="e">Данные о событиях, описывающие, каким образом была достигнута эта страница.  Свойство Parameter
        /// обычно используется для настройки страницы.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                string searchFilter = (string)e.Parameter;
                barProgress.IsIndeterminate = true;
                IReadOnlyList<StorageFile> files = await Task.Run(() => Engine.GetFiles(Windows.Storage.ApplicationData.Current.LocalFolder));
                List<StorageFile> passFiles = new List<StorageFile>(), cryptFiles = new List<StorageFile>();
                foreach (StorageFile sf in files)
                {
                    if (sf.Name.ToLower().Contains(searchFilter.ToLower()))
                    {
                        if (Path.GetExtension(sf.Path.ToLower()).Equals(".pss"))
                            passFiles.Add(sf);
                        else cryptFiles.Add(sf);
                    }
                    else ;
                }
                barProgress.IsIndeterminate = false;
                tkSearchLabel.Text = string.Format("Searching for {0}: pass'files - {1}, crypted files - {2}", searchFilter, passFiles.Count, cryptFiles.Count);
                foreach (StorageFile f in passFiles) lvPassFiles.Items.Add(new ListViewItem(){ Content = Path.GetFileNameWithoutExtension(f.Name), Tag = f });
                foreach (StorageFile f in cryptFiles) lvCryptedFiles.Items.Add(new ListViewItem() { Content = f.Name, Tag = f });
            }
            catch (Exception ex)
            {
                Engine.LogMessage(ex);
                Engine.SendMessage(ex.Message);
            }
        }

        private void btnShowFile_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile file;
            if (lvCryptedFiles.SelectedIndex != -1)
            {
                file = (lvCryptedFiles.SelectedItem as ListViewItem).Tag as StorageFile;
                Engine.GoToAnotherFrame(typeof(CryptedFilesPage), file.Name);
            }
            else
            {
                if (lvPassFiles.SelectedIndex != -1)
                {
                    file = (lvPassFiles.SelectedItem as ListViewItem).Tag as StorageFile;
                    Engine.GoToAnotherFrame(typeof(MainPage), file.Name);
                }
                else return;
            }
            Window.Current.Activate();
        }

        private void btnBackToMain_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Engine.GoToAnotherFrame(typeof(MainPage), null);
        }


        private void Files_Tapped(object sender, TappedRoutedEventArgs e)
        {
            btnShowFile.IsEnabled = (lvCryptedFiles.SelectedItems.Count != 0) || (lvPassFiles.SelectedItems.Count != 0);
        }

    }
}
