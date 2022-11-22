﻿using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Windows.UI.Notifications;

namespace MKBot
{
    class TrayApp : ApplicationContext
    {
        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;

        private bool checking_update = false;
        private bool can_update = false;

        private ProcessStartInfo psi2 = new ProcessStartInfo();
        private Process msu_process = new Process();

        private System.Timers.Timer checkForTime = new System.Timers.Timer(30 * 1000);

        private ToolStripMenuItem UpdateMenu;
        private Form Infowin;
        private Form Shellwin;

        private MKBotCoreManager MKBotCore;

        private string DirectoryPath;
        private string UserDataPath;
        private string[] args;

        public TrayApp()
        {
#if DEBUG
            DirectoryPath = Path.GetFullPath(Environment.CurrentDirectory + "\\..");
#else
            DirectoryPath = Path.GetDirectoryName(Application.ExecutablePath);
#endif

            args = Environment.GetCommandLineArgs();
            if (args.Contains("--debug"))
            {
                UserDataPath = "data";
            }
            else
            {
                UserDataPath = Environment.GetEnvironmentVariable("LOCALAPPDATA") + $"\\Mulgyeol\\{Version.product_name}\\data";
            }

            MKBotCore = new MKBotCoreManager(args.Contains("--debug"));
            MKBotCore.MKBotCoreStarted += MKBotCore_Started;
            MKBotCore.MKBotCoreExit += MKbotCore_Exit;

            var jsonString = File.ReadAllText(UserDataPath + "\\config.json");
            JObject configjson = JObject.Parse(jsonString);

            psi2.FileName = "bin\\msu.exe";
            psi2.WorkingDirectory = "bin";
            psi2.CreateNoWindow = true;
            psi2.UseShellExecute = false;
            msu_process.StartInfo = psi2;

            notifyIcon1 = new NotifyIcon();
            contextMenuStrip1 = new ContextMenuStrip();

            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.MouseClick += new MouseEventHandler(notifyIcon1_MouseClick);
            notifyIcon1.Text = Version.product_name;

            UpdateMenu = new ToolStripMenuItem("Check for Updates...", null, Click_Update);

            contextMenuStrip1.Items.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Settings", null, Click_Setting),
                new ToolStripMenuItem("MGCert", null, Click_MGCert),
                new ToolStripMenuItem("Extensions", null, Click_Extensions),
                new ToolStripSeparator(), UpdateMenu,
                new ToolStripMenuItem("About", null, Click_info),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Exit", null, Click_Exit)
            });

            if (check_ready_to_update())
            {
                Run_setup(true);
            }
            else
            {
                msu_process.Exited += new EventHandler(ProcessExited_msu);
                msu_process.EnableRaisingEvents = true;
                notifyIcon1.Visible = true;
                notifyIcon1.Icon = Properties.Resources.mkbot_off;
            }

            bool auto_connect = false;
            if (configjson["connectOnStart"] != null)
            {
                auto_connect = (bool)configjson["connectOnStart"];
            }

            Infowin = new InfoForm();
            Shellwin = new ShellForm(MKBotCore, auto_connect);

            checkForTime.Elapsed += new System.Timers.ElapsedEventHandler(checkForTime_Elapsed_At_Startup);
            checkForTime.Enabled = true;
        }

        private void MKbotCore_Exit(object sender, MKBotCoreExitEventArgs e)
        {
            notifyIcon1.Icon = Properties.Resources.mkbot_off;

            if (can_update)
            {
                Run_setup(true);
            }
            if (e.ExitCode == 1)
            {
                ShowToast("An invalid token was provided.", "Review your settings for configured token.", "action=openConfig");
            }
        }

        private void MKBotCore_Started(object sender, EventArgs e)
        {
            notifyIcon1.Icon = Properties.Resources.mkbot_on;
        }

        //UI
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (Shellwin.Visible)
                {
                    //Shellwin.Activate();
                    Shellwin.Hide();
                }
                else
                {
                    Shellwin.Show();
                }
            }
        }

        private void Click_Setting(object source, EventArgs e)
        {
            Process.Start(UserDataPath + "\\config.json");
        }

        private void Click_MGCert(object source, EventArgs e)
        {
            Process.Start(UserDataPath + "\\mgcert.json");
        }

        private void Click_Extensions(object sender, EventArgs e)
        {
            Process.Start(UserDataPath + "\\extensions.json");
        }

        private void Click_Update(object sender, EventArgs e)
        {
            if (can_update)
            {
                if (MKBotCore.discord_online)
                {
                    MKBotCore.Kill();
                }
                else
                {
                    Run_setup(true);
                }
            }
            else
            {
                checking_update = true;
                UpdateMenu.Enabled = false;
                Run_msu("/c");
            }
        }

        private void Click_info(object sender, EventArgs e)
        {
            if (Infowin.Visible)
            {
                Infowin.Activate();
            }
            else
            {
                Infowin.ShowDialog();
            }
        }

        private void Click_Exit(object source, EventArgs e)
        {
            notifyIcon1.Visible = false;
            if (MKBotCore.discord_online)
            {
                MKBotCore.Kill();
            }
            if (can_update)
            {
                Run_setup(false);
            }
            Environment.Exit(0);
        }

        private void ShowToast(string title, string text, string type = "action=None")
        {
            ToastContent toastContent = new ToastContent()
            {
                // Arguments when the user taps body of toast
                Launch = type,

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title,
                                HintMaxLines = 1
                            },
                            new AdaptiveText()
                            {
                                Text = text
                            }
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = DirectoryPath + "\\resources\\app\\mkbot.png",
                            HintCrop = ToastGenericAppLogoCrop.None
                        }
                    }
                }
            };

            // And create the toast notification
            var toast = new ToastNotification(toastContent.GetXml());
            toast.Activated += Toast_Activated;
            // And then show it
            DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
        }

        private void ShowUpdateToast(string title = "Mulgyeol Software Update", string text = "Restart MK Bot to apply the latest update.", string type = "action=None")
        {
            ToastContent toastContent = new ToastContent()
            {
                // Arguments when the user taps body of toast
                Launch = type,

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title,
                                HintMaxLines = 1
                            },
                            new AdaptiveText()
                            {
                                Text = text
                            }
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = DirectoryPath + "\\resources\\app\\mkbot.png",
                            HintCrop = ToastGenericAppLogoCrop.None
                        }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("Update Now", "action=install")
                        {
                            ActivationType = ToastActivationType.Foreground
                        },

                        new ToastButton("Later", "action=later")
                        {
                            ActivationType = ToastActivationType.Foreground
                        }
                    }
                }
            };

            // And create the toast notification
            var toast = new ToastNotification(toastContent.GetXml());
            toast.Activated += Toast_Activated;
            // And then show it
            DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
        }

        //background
        private void Toast_Activated(ToastNotification sender, object arguments)
        {
            ToastActivatedEventArgs strArgs = arguments as ToastActivatedEventArgs;
            QueryString args = QueryString.Parse(strArgs.Arguments);

            switch (args["action"])
            {
                case "install":
                    Install_update();
                    break;
                case "openConfig":
                    Process.Start(UserDataPath + "\\config.json");
                    break;
                default:
                    Console.WriteLine("default");
                    break;
            }
        }

        private bool check_ready_to_update()
        {
            return File.Exists("./Update.flag");
        }

        private void checkForTime_Elapsed_At_Startup(object sender, System.Timers.ElapsedEventArgs e)
        {
            checkForTime.Enabled = false;
            checkForTime.Interval = 60 * 60 * 1000;

            CheckForUpdate();

            checkForTime.Elapsed -= new System.Timers.ElapsedEventHandler(checkForTime_Elapsed_At_Startup);
            checkForTime.Elapsed += new System.Timers.ElapsedEventHandler(checkForTime_Elapsed);
            checkForTime.Enabled = true;
        }

        private void checkForTime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (can_update && !MKBotCore.discord_online)
            {
                Run_setup(true, false);
            }
            else
            {
                CheckForUpdate();
            }
        }

        private void CheckForUpdate()
        {
            if (!can_update)
            {
                Run_msu("/c");
            }
        }

        private void Run_msu(string argv = "/c")
        {
            if (argv == "/c")
            {
                UpdateMenu.Enabled = false;
                UpdateMenu.Text = "Checking for Updates...";
            }
            psi2.Arguments = argv;
            try
            {
                if (msu_process.HasExited)
                {
                    msu_process.Start();
                }
            }
            catch (Exception)
            {
                msu_process.Start();
            }
        }

        private void Install_update()
        {
            if (can_update)
            {
                if (MKBotCore.discord_online)
                {
                    MKBotCore.Kill();
                }
                else
                {
                    Run_setup(true);
                }
            }
        }

        private void Run_setup(bool autorun, bool gui = true)
        {
            string param;

            param = "--start";

            if (gui)
            {
                param += " --gui";
            }

            if (autorun)
            {
                param += " --auto-run";
            }

            notifyIcon1.Visible = false;
            Process.Start("Update.exe", param);
            Environment.Exit(0);
        }

        private void ProcessExited_msu(object sender, EventArgs e)
        {
            if (msu_process.ExitCode == 0)
            {
                Console.WriteLine("> msu exit: 0");
                can_update = true;
                UpdateMenu.Enabled = true;
                UpdateMenu.Text = "Restart to Update";

                if (!(checking_update || MKBotCore.discord_online))
                {
                    Run_setup(true, false);
                }
                else
                {
                    ShowUpdateToast();
                }

                checking_update = false;
            }
            else if (msu_process.ExitCode == 1)
            {
                Console.WriteLine("> msu exit: 1");
                UpdateMenu.Enabled = true;
                UpdateMenu.Text = "Check for Updates...";

                if (checking_update)
                {
                    ShowToast("Mulgyeol Software Update", "There are currently no updates available.");
                    checking_update = false;
                }
            }
        }

        internal void Application_Exit(object sender, EventArgs e)
        {
            AsyncListener.TerminateListener();
        }
    }
}
