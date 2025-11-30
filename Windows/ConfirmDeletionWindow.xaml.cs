using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VPM.Services;

namespace VPM
{
    public partial class ConfirmDeletionWindow : Window
    {

        public ConfirmDeletionWindow(string title, string message, IEnumerable<string> details, string warningMessage)
        {
            InitializeComponent();

            Title = title;
            TitleTextBlock.Text = title;
            MessageTextBlock.Text = message;

            if (details != null)
            {
                DetailsItemsControl.ItemsSource = details.ToList();
            }
            else
            {
                DetailsItemsControl.ItemsSource = Array.Empty<string>();
            }

            if (string.IsNullOrWhiteSpace(warningMessage))
            {
                WarningTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                WarningTextBlock.Text = warningMessage;
                WarningTextBlock.Visibility = Visibility.Visible;
            }

            DarkTitleBarHelper.Apply(this);
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

