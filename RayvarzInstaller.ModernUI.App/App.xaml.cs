using Microsoft.Extensions.CommandLineUtils;
using RayvarzInstaller.ModernUI.App.Services;
using RayvarzInstaller.ModernUI.Presentation;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace RayvarzInstaller.ModernUI.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool FreeConsole();
       
        [STAThread]
        private void Application_Startup(object sender, StartupEventArgs e)
        {

            // Check if user is NOT admin
            

            if (!e.Args.Any())
            {
                //RunAsAdministrator();
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                AttachConsole(-1);
                if (!IsRunningAsAdministrator())
                {
                    Console.WriteLine("Please running command prompt as administrator !!!");
                    FreeConsole();
                }
                else
                {
                    InitCli(e.Args);
                }

                //CLI
                //AttachConsole(-1);
                //InitCli(e.Args);
            }
        }
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs"/> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            AppearanceManager.Current.AccentColor = Color.FromRgb(47, 61, 136);
            AppearanceManager.Current.ThemeSource = AppearanceManager.DarkThemeSource;
            base.OnStartup(e);
            
        }

        private static void InitCli(string[] args)
        {
            var packageResolver = new PackageResolver();
            var registery = new SetupRegistry();
            var cliService = new CommandLineService();
            CommandLineApplication commandLineApplication = new CommandLineApplication(true)
            {
                Name = "Rayvarz Setup",
                FullName = "Rayvarz Setup Installer"
            };

            commandLineApplication.Command("install-package", command =>
            {
                command.Description = "example: install-package --package BPMS_9603.0.2.0 --jsonConfigFile ./config.json [--jsonConfigBase64 base64Value]";
                CommandOption packageIdCommand = command.Option("--package", "Setup Package Id", CommandOptionType.SingleValue);
                CommandOption jsonConfigFileCommand = command.Option("--jsonConfigFile", "Full Path to json config file", CommandOptionType.SingleValue);
                CommandOption jsonConfigBase64Command = command.Option("--jsonConfigBase64", "Base64 encoded content of json config file", CommandOptionType.SingleValue);
                command.OnExecute(() =>
                {
                    if (!packageIdCommand.HasValue() && (!jsonConfigFileCommand.HasValue() && !jsonConfigBase64Command.HasValue()))
                    {
                        command.ShowHelp(null);
                        return -1;
                    }
                    var manifest = packageResolver.GetPackage();
                    if (manifest == null)
                    {
                        Console.Error.WriteLine(string.Format("{0} does not exists.", (object)packageIdCommand.Value()));
                        return -1;
                    }

                    string jsonConfig = string.Empty;
                    if (jsonConfigFileCommand.HasValue())
                        jsonConfig = File.ReadAllText(jsonConfigFileCommand.Value());
                    else if (jsonConfigBase64Command.HasValue())
                        jsonConfig = Encoding.UTF8.GetString(Convert.FromBase64String(jsonConfigBase64Command.Value()));
                    var installationResult = cliService.CommandLineInstall(jsonConfig).Result;
                    if (installationResult)
                    {
                        return 1;
                    }
                    return -1;
                });
            }, true);
            commandLineApplication.Command("uninstall-package", (Action<CommandLineApplication>)(command =>
            {
                CommandOption packageIdCommand = command.Option("--package", "Setup Package Id", CommandOptionType.SingleValue);
                CommandOption jsonConfigFileCommand = command.Option("--jsonConfigFile", "Full Path to json config file", CommandOptionType.SingleValue);
                CommandOption jsonConfigBase64Command = command.Option("--jsonConfigBase64", "Base64 encoded content of json config file", CommandOptionType.SingleValue);
                command.OnExecute(() =>
                {
                    if (!packageIdCommand.HasValue() && (!jsonConfigFileCommand.HasValue() && !jsonConfigBase64Command.HasValue()))
                    {
                        command.ShowHelp((string)null);
                        return -1;
                    }

                    string jsonConfig = string.Empty;
                    if (jsonConfigFileCommand.HasValue())
                        jsonConfig = File.ReadAllText(jsonConfigFileCommand.Value());
                    else if (jsonConfigBase64Command.HasValue())
                        jsonConfig = Encoding.UTF8.GetString(Convert.FromBase64String(jsonConfigBase64Command.Value()));
                    var installationResult = cliService.CommandLineDelete(jsonConfig).Result;
                    if (installationResult)
                    {
                        return 1;
                    }
                    return -1;

                });
            }), true);
            commandLineApplication.Command("update-package", command =>
            {

                CommandOption packageIdCommand = command.Option("--package", "Setup Package Id", CommandOptionType.SingleValue);
                CommandOption jsonConfigFileCommand = command.Option("--jsonConfigFile", "Full Path to json config file", CommandOptionType.SingleValue);
                CommandOption jsonConfigBase64Command = command.Option("--jsonConfigBase64", "Base64 encoded content of json config file", CommandOptionType.SingleValue);
                command.OnExecute(() =>
                {
                    if (!packageIdCommand.HasValue() && (!jsonConfigFileCommand.HasValue() && !jsonConfigBase64Command.HasValue()))
                    {
                        command.ShowHelp(null);
                        return -1;
                    }
                    //InstallPathInfo installPathInfo = registery.InstallPaths.SingleOrDefault<InstallPathInfo>((Func<InstallPathInfo, bool>)(ip => ip.Id == new Guid(installPathId.Value())));
                    //if (registery == null)
                    //{
                    //    Console.WriteLine(string.Format("\r\n'{0}' not found", (object)result));
                    //    return 1;
                    //}
                    //Manifest manifest = packageResolver.GetPackage();


                    string jsonConfig = string.Empty;
                    if (jsonConfigFileCommand.HasValue())
                        jsonConfig = File.ReadAllText(jsonConfigFileCommand.Value());
                    else if (jsonConfigBase64Command.HasValue())
                        jsonConfig = Encoding.UTF8.GetString(Convert.FromBase64String(jsonConfigBase64Command.Value()));
                    var installationResult = cliService.CommandLineUpdate(jsonConfig).Result;
                    if (installationResult)
                    {
                        return 1;
                    }
                    return -1;
                });
            }, true);
            try
            {
                commandLineApplication.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                commandLineApplication.ShowHelp(null);
            }
            finally
            {
                FreeConsole();
                //SendKeys.SendWait("{ENTER}");
            }
        }

        private void RunAsAdministrator() {
            if (!IsRunningAsAdministrator())
            {
                // Setting up start info of the new process of the same application
                ProcessStartInfo processStartInfo = new ProcessStartInfo(Assembly.GetEntryAssembly().CodeBase);

                // Using operating shell and setting the ProcessStartInfo.Verb to “runas” will let it run as admin
                processStartInfo.UseShellExecute = true;
                processStartInfo.CreateNoWindow = true;
               // processStartInfo.RedirectStandardOutput = true;
                processStartInfo.Verb = "runas";
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //processStartInfo.CreateNoWindow = true;
                // Start the application as new process

                
                Process.Start(processStartInfo);
                Current.Shutdown();
                
                // Shut down the current (old) process

            }

        }

        private static bool IsRunningAsAdministrator()
        {
            // Get current Windows user
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();

            // Get current Windows user principal
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(windowsIdentity);

            // Return TRUE if user is in role "Administrator"
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
