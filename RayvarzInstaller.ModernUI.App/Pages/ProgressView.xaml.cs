﻿using Newtonsoft.Json;
using RayvarzInstaller.ModernUI.App.Models;
using RayvarzInstaller.ModernUI.App.Services;
using RayvarzInstaller.ModernUI.Windows;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RayvarzInstaller.ModernUI.App.Pages
{
    /// <summary>
    /// Interaction logic for LayoutBasic.xaml
    /// </summary>
    public partial class ProgressView : UserControl, IContent
    {
        private readonly SetupServices setupServices;
        private OperationState OperationState;
        public int ProgressValuePercent { get; set; }
        public string ProgressTitle { get; set; }
        public ProgressView()
        {
            setupServices = new SetupServices(new SystemFileHelper(),
                new WebDeployHelper(), new SetupRegistry(), new PackageResolver());
            InitializeComponent();
        }

        public void OnFragmentNavigation(Windows.Navigation.FragmentNavigationEventArgs e)
        {
            OperationState = JsonConvert.DeserializeObject<OperationState>(e.Fragment);
            setupServices.onStateChanged += SetupServices_onStateChanged;
            setupServices.OnComleted += SetupServices_OnComleted;
            SetProgressbarTitle(OperationState.Operation);

            Task.Run(() =>
            {
                if (OperationState.Operation == Operation.Add)
                {
                    setupServices.Install(OperationState.Data , CommandTypeEnum.Gui);

                    //setupServices.Install(OperationState.Data).ConfigureAwait(false);
                }

                if (OperationState.Operation == Operation.Modified)
                {
                    //setupServices.Update(OperationState.Data).ConfigureAwait(false);
                    setupServices.Update(OperationState.Data, OperationState.Id , CommandTypeEnum.Gui);
                }
                if (OperationState.Operation == Operation.Delete)
                {
                    //setupServices.Update(OperationState.Data).ConfigureAwait(false);
                    setupServices.Delete(OperationState.Data, OperationState.Id , CommandTypeEnum.Gui);
                }
            });
        }

        private void SetProgressbarTitle(Operation operation)
        {
            switch (operation)
            {
                case Operation.Add:
                    progressbarTitle.Text = "در حال نصب";
                    break;
                case Operation.Modified:
                    progressbarTitle.Text = "در حال بروزرسانی";
                    break;
                case Operation.Delete:
                    progressbarTitle.Text = "در حال حذف";
                    break;
                default:
                    break;
            }
        }

        private void SetupServices_OnComleted(string message)
        {
            Application.Current.Dispatcher.BeginInvoke((ThreadStart)delegate ()
            {
                FinishButton.Visibility = Visibility.Visible;
                idpProgressbar.Visibility = Visibility.Hidden;
                progressbarTitle.Visibility = Visibility.Hidden;
                progressbarDetail.Visibility = Visibility.Hidden;
                progressbarStatus.Visibility = Visibility.Visible;

                progressbarStatus.Text = message;
            });
        }

        private void SetupServices_onStateChanged(string message)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
            {
                   progressbarDetail.Text = message; // Do all the ui thread updates here
            }));

        }

        public void OnNavigatedFrom(Windows.Navigation.NavigationEventArgs e)
        {

        }

        public void OnNavigatedTo(Windows.Navigation.NavigationEventArgs e)
        {
            //e.Content
            //Do Stuff here!

        }

        public void OnNavigatingFrom(Windows.Navigation.NavigatingCancelEventArgs e)
        {

        }

        private void Exit_Clicked(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();

        }

    }
}
