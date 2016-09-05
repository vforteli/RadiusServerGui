using Flexinets.MobileData.SMS;
using FlexinetsDBEF;
using log4net;
using Microsoft.Azure;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Flexinets.Radius
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RadiusServer _rsIpass;
        private RadiusServer _rsMbb;
        private FlexinetsEntitiesFactory _contextFactory;
        private Boolean _autoScroll;
        private readonly ILog _log = LogManager.GetLogger(typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();

            _autoScroll = CheckBox_AutoScroll.IsChecked ?? true;
            WindowTraceListener listener = new WindowTraceListener(textBox1);
            Trace.Listeners.Add(listener);

            log4net.Config.XmlConfigurator.Configure();
            textBox_Port.Text = CloudConfigurationManager.GetSetting("Port");

            StartServer();
        }


        private void Button_Clear(object sender, RoutedEventArgs e)
        {
            textBox1.Clear();
        }

        private void Button_StartStop(object sender, RoutedEventArgs e)
        {
            if (_rsIpass != null && _rsIpass.Running)
            {
                StopServer();
            }
            else
            {
                StartServer();
            }
        }

        private void AutoScroll_Checked(object sender, RoutedEventArgs e)
        {

        }




        private void StartServer()
        {
            var port = 1645;
            Int32.TryParse(textBox_Port.Text, out port);

            //var clients = JsonConvert.DeserializeObject<RadiusServersModel>(File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\clients.json"));

            _log.Info("Reading configuration");
            _contextFactory = new FlexinetsEntitiesFactory(CloudConfigurationManager.GetSetting("SQLConnectionString"));
            var path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "\\dictionary";
            var dictionary = new RadiusDictionary(path);
            var ipassSecret = CloudConfigurationManager.GetSetting("ipasssecret");
            var mbbSecret = CloudConfigurationManager.GetSetting("mbbsecret");
            var disconnectSecret = CloudConfigurationManager.GetSetting("disconnectSecret");
            var apiUrl = CloudConfigurationManager.GetSetting("ApiUrl");
            _log.Info("Configuration read");

            _rsIpass = new RadiusServer(new IPEndPoint(IPAddress.Any, port), dictionary);
            _rsIpass.AddPacketHandler(IPAddress.Parse("127.0.0.1"), ipassSecret, new iPassPacketHandler(_contextFactory));
            _rsIpass.Start();


            var networkIdProvider = new NetworkIdProvider(_contextFactory, apiUrl);
            var smsgateway = new SMSGatewayTwilio(
                   CloudConfigurationManager.GetSetting("twilio.deliveryreporturl"),
                   CloudConfigurationManager.GetSetting("twilio.accountsid"),
                   CloudConfigurationManager.GetSetting("twilio.authtoken"));

            var welcomeSender = new WelcomeSender(_contextFactory, smsgateway);
            var disconnector = new RadiusDisconnector(_contextFactory, disconnectSecret);
            var mdPacketHandler = new MobileDataPacketHandler(_contextFactory, networkIdProvider, welcomeSender, disconnector);

            _rsMbb = new RadiusServer(new IPEndPoint(IPAddress.Any, port + 1), dictionary);   // daah...
            _rsMbb.AddPacketHandler(IPAddress.Parse("127.0.0.1"), mbbSecret, mdPacketHandler);
            _rsMbb.Start();

            button1.Content = "Stop";
        }

        private void StopServer()
        {
            _rsIpass.Stop();
            _rsIpass.Dispose();

            _rsMbb.Stop();
            _rsMbb.Dispose();
            button1.Content = "Start";
        }




        public class WindowTraceListener : TraceListener
        {
            private TextBox textbox;
            public Boolean autoscroll = true;

            public WindowTraceListener(TextBox textbox)
            {
                this.textbox = textbox;
            }


            public override void Write(string message)
            {
                Action append = delegate ()
                {
                    textbox.AppendText(message);
                    if (autoscroll)
                    {
                        textbox.ScrollToEnd();
                        textbox.CaretIndex = textbox.Text.Length;
                    }
                };
                if (textbox.Dispatcher.Thread != Thread.CurrentThread)
                {
                    textbox.Dispatcher.BeginInvoke(append);
                }
                else
                {
                    append();
                }
            }
            public override void Write(string message, string category)
            {
                Write(message);
            }


            public override void WriteLine(string message)
            {
                Write(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss fff") + " :: " + message + Environment.NewLine);
            }
            public override void WriteLine(string message, string category)
            {
                WriteLine(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss fff") + " :: " + category + " :: " + message);
            }
        }
    }
}