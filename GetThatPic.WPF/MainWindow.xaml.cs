﻿// <copyright file="MainWindow.xaml.cs" company="Marc A. Modrow">
// Copyright (c) 2018 All Rights Reserved
// <author>Marc A. Modrow</author>
// </copyright>

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using GetThatPic.Data.IO;
using GetThatPic.WPF.Models;
using WpfAnimatedGif;
using Clipboard = System.Windows.Clipboard;
using Color = System.Windows.Media.Color;
using DragEventArgs = System.Windows.DragEventArgs;

namespace GetThatPic.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The state.
        /// </summary>
        private readonly MainWindowState state;

        /// <summary>
        /// The log call to action for the initializing of the drop-target-log TextBox.
        /// </summary>
        private string logCallToAction = "Drop URLs here!";

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            state = new MainWindowState();
            LogTextBox.Text = logCallToAction;

            Logger.LogToGui += message => LogTextBox.Text += message;
            Logger.SignalErrorToGui += () =>
            {
                ////void Work() => ErrorFlash();
                ////Thread thread = new Thread(Work);
                ////thread.Start();
                ErrorFlash();
            };

            // TODO: Click image to open it in the file system.
            // TODO: Download.
            // TODO: Async status updates on big downloads.
            // TODO: Error Notification.
            // TODO: Check download target directory for file before saving copy. (Check by name and/or identity)
            // TODO: Command line Interface?
            // TODO: Login functionality for private/mature content.
            // TODO: Transpile old XML configs to JSON.
        }

        /// <summary>
        /// Processes the download queue.
        /// </summary>
        public void ProcessDownloadQueue()
        {
            QueueSizeLabel.Content = state.DownloadQueue.Count;
            if (state.DownloadQueue.Any() && !state.IsDownloading)
            {
                UpdatePreview(state.DownloadQueue.Dequeue());
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.Closed" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }

        /// <summary>
        /// Lets the GUI flash to indicate errors.
        /// </summary>
        /// <param name="toRed">if set to <c>true</c> [to red].</param>
        /// <param name="cycles">The cycles.</param>
        private void ErrorFlash(bool toRed = true, int cycles = 3)
        {
            if (cycles < 0)
            {
                return;
            }

            SolidColorBrush fromgGreyColor = toRed ? new SolidColorBrush(Color.FromRgb(255,0,0)) : new SolidColorBrush(Color.FromRgb(229, 229, 229));
            SolidColorBrush fromgWhiteColor = toRed ? new SolidColorBrush(Color.FromRgb(255,0,0)) : new SolidColorBrush(Color.FromRgb(255, 255, 255));

            LoadGrid.Background = fromgGreyColor;
            PreviewGrid.Background = fromgGreyColor;
            LegalGrid.Background = fromgGreyColor;

            Background = fromgWhiteColor;
            
            System.Threading.Thread.Sleep(250);
            if (toRed)
            {
                ErrorFlash(false, cycles);
            }
            else
            {
                ErrorFlash(true, cycles - 1);
            }
        }

        /// <summary>
        /// Previews the previous image.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void PreviewPreviousImage(object sender, RoutedEventArgs e)
        {
            ImageEntry newPreviewItem = state.History.Previous;
            UpdatePreview(newPreviewItem);
        }

        /// <summary>
        /// Previews the next image.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void PreviewNextImage(object sender, RoutedEventArgs e)
        {
            ImageEntry newPreviewItem = state.History.Next;
            UpdatePreview(newPreviewItem);
        }

        /// <summary>
        /// Updates the preview.
        /// </summary>
        /// <param name="newPreviewItem">The new preview item.</param>
        private void UpdatePreview(ImageEntry newPreviewItem)
        {
            if (null == newPreviewItem)
            {
                return;
            }

            state.PreviewItem = newPreviewItem;

            PreviewImageName.Text = newPreviewItem.MetaData.Name;
            PreviewImageUrl.Text = newPreviewItem.MetaData.ImageUrl;
            PreviewImageDownloadPath.Text = newPreviewItem.MetaData.TargetFileSystemLocation;

            if (newPreviewItem.MetaData.TargetFileSystemLocation?.EndsWith(".gif") ?? false)
            {
                BitmapSource source = newPreviewItem.Bitmap;
                ImageBehavior.SetAnimatedSource(PreviewImage, source);
            }
            else
            {
                ImageBehavior.SetAnimatedSource(PreviewImage, null);
                PreviewImage.Source = newPreviewItem.Bitmap;
            }
        }

        /// <summary>
        /// Handles the Click event of the ClearLogButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Text = logCallToAction;
        }

        /// <summary>
        /// Handles the Drop event of the LogTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private async void DropUrl(object sender, System.Windows.DragEventArgs e)
        {
            // TODO: Migrate to using Link-Class here
            string droppedUrl = (string)e.Data.GetData(System.Windows.DataFormats.Text);
            Logger.Log(droppedUrl);

            foreach (string url in new Regex(@"\r?\n").Split(droppedUrl ?? string.Empty))
            {
                if (!state.DroppedUrls.Contains(url))
                {
                    state.DroppedUrls.Add(url);
                    await ParseUrl(url);
                }
                else
                {
                    Logger.Error("Got that one covered.");
                }
            }

            if (!state.IsDownloading)
            {
                ProcessDownloadQueue();
            }
        }

        /// <summary>
        /// Parses the URL.
        /// </summary>
        /// <param name="droppedUrl">The dropped URL.</param>
        /// <returns>An empty Task.</returns>
        private async Task ParseUrl(string droppedUrl)
        {
            IList<ImageEntry> images = (await state.LinkParser.GetImages(droppedUrl))
                .Select(image => new ImageEntry()
                {
                    MetaData = image,
                    Window = this,
                    State = state
                }).ToList();

            foreach (ImageEntry image in images)
            {
                state.DownloadQueue.Enqueue(image);
                state.History.Push(image);
                QueueSizeLabel.Content = state.DownloadQueue.Count;
            }
        }

        /// <summary>
        /// Handles the DragOver event of the LogTextBox control.
        /// Indicating that dropping is possible.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void LogTextBox_DragOver(object sender, DragEventArgs e)
        {
            // Method intentionally left empty - for now.
        }

        /// <summary>
        /// Handles the DragLeave event of the LogTextBox control.
        /// Indicating that dropping is no longer possible.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void LogTextBox_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            // Method intentionally left empty - for now.
        }

        /// <summary>
        /// Handles the PreviewDragOver event of the LogTextBox control.
        /// Needed to make the Log Text Box droppable.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void EnableDropping(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Handles the Click event of the CopyLastFilePathButton control.
        /// Copies the last downloaded file's path to the clipboard.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CopyLastFilePathButton_Click(object sender, RoutedEventArgs e)
        {
            if (null == state.History?.LastWritten?.MetaData.TargetFileSystemLocation)
            {
                return;
            }

            Clipboard.SetText(state.History.LastWritten.MetaData.TargetFileSystemLocation);
        }

        /// <summary>
        /// Handles the Click event of the OpenLastFileButton control.
        /// Opens the last downloaded file with the default means.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OpenLastFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (null == state.History?.LastWritten?.MetaData.TargetFileSystemLocation)
            {
                return;
            }

            System.Diagnostics.Process.Start(state.History.LastWritten.MetaData.TargetFileSystemLocation);
        }

        /// <summary>
        /// Handles the RequestNavigate event of the Hyperlink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RequestNavigateEventArgs"/> instance containing the event data.</param>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        /// <summary>
        /// Handles the Click event of the DownloadImageButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void DownloadImageButton_Click(object sender, RoutedEventArgs e)
        {
            state.History?.Current?.Save();
        }
    }
}
