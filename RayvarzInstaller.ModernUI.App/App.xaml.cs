﻿using RayvarzInstaller.ModernUI.Presentation;
using RayvarzInstaller.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RayvarzInstaller.ModernUI.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs"/> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            AppearanceManager.Current.AccentColor = Color.FromRgb(47, 61, 136);
            AppearanceManager.Current.ThemeSource = AppearanceManager.LightThemeSource;
            base.OnStartup(e);
        }
    }
}