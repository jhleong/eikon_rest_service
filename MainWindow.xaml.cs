// <copyright file="MainWindow.xaml.cs" company="Thomson Reuters">
// Copyright (c) 2014 All Right Reserved
// </copyright>
// <author>$Author: GHEZ, Gregory (Financial&Risk) $</author>
// <date>$LastChangedDate: 01/30/2014 $</date>

namespace DailyIntervalDemo
{
    using System.ComponentModel;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.uiMainControl.CleanUp();
            base.OnClosing(e);
        }
    }
}