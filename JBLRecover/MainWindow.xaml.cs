using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JBLRecover
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon notifyIcon = null;
        bool running = true;
        Thread timer;
        int playtime = 60 * 9;
        int defaultPlayTime = 60 * 9;

        public MainWindow()
        {
            InitializeComponent();
            LoadAudioDevices();
        }

        private void Play(string audioFile = "Sine_16196_96k_Float_LR.wav")
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (devices.SelectedItem == null) { return; }
            });

            Thread t = new Thread(() => {

                MMDevice device = null;
                WasapiOut outputDevice = null;
                AudioFileReader reader = null;
                try
                {
                    float masterpeak = 0;
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        device = ((CapsWrapper)devices.SelectedItem).Device;
                        outputDevice = new WasapiOut(device, NAudio.CoreAudioApi.AudioClientShareMode.Shared, true, 0);
                        masterpeak = device.AudioMeterInformation.MasterPeakValue;
                    });

                    if (device != null && outputDevice != null)
                    {
                        // do not play audio, because there is already audio running that should keep the headphone alive
                        if (masterpeak >= 0.0001200000)
                        {
                            return;
                        }
                        reader = new AudioFileReader(audioFile);
                        outputDevice.Init(reader);

                        int deviceIndex = 0;
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            deviceIndex = ((CapsWrapper)devices.SelectedValue).deviceIndex;
                        });

                        

                        outputDevice.Play();
                        while (outputDevice.PlaybackState == PlaybackState.Playing && running)
                        {
                            Thread.Sleep(100);

                            App.Current.Dispatcher.Invoke(() =>
                            {

                                float devicevol = outputDevice.Volume;
                                float filevol = outputDevice.AudioStreamVolume.GetChannelVolume(0);
                                float max = (device.AudioEndpointVolume.VolumeRange.MaxDecibels);


                                if (devicevol > 0.5)
                                {
                                    filevol = (devicevol) / 8;
                                }
                                else if (devicevol < 0.2)
                                {
                                    filevol = (devicevol);
                                }
                                else
                                {
                                    filevol = (devicevol) / 2;

                                }

                                int count = outputDevice.AudioStreamVolume.GetAllVolumes().Length;
                                for (int i = 0; i < count; i++)
                                {
                                    outputDevice.AudioStreamVolume.SetChannelVolume(i, filevol);
                                }

                                trackvolumebar.Value = GetDeviceVolumes(((CapsWrapper)devices.SelectedItem), outputDevice)[2];
                            });
                        }
                    }
                }
                catch (Exception) { }
                finally
                {
                    try { if (outputDevice != null) { outputDevice.Stop(); } } catch (Exception) { }
                    try { if (outputDevice != null) { outputDevice.Dispose(); } } catch (Exception) { }
                    try { if (reader != null) { reader.Dispose(); } }catch(Exception) { }
                }
            });
            t.Start();
        }

        private void LoadAudioDevices()
        {
            try
            {
                //Instantiate an Enumerator to find audio devices
                NAudio.CoreAudioApi.MMDeviceEnumerator MMDE = new NAudio.CoreAudioApi.MMDeviceEnumerator();
                //Get all the devices, no matter what condition or status
                NAudio.CoreAudioApi.MMDeviceCollection DevCol = MMDE.EnumerateAudioEndPoints(NAudio.CoreAudioApi.DataFlow.All, NAudio.CoreAudioApi.DeviceState.All);
                //Loop through all devices
                foreach (NAudio.CoreAudioApi.MMDevice dev in DevCol)
                {
                    try
                    {
                        if (dev.State != NAudio.CoreAudioApi.DeviceState.Active)
                        {
                            System.Diagnostics.Debug.Print("Cannot read volume for:" + dev.FriendlyName + " as it is not active.");
                            continue;
                        }
                        if (dev.DataFlow == NAudio.CoreAudioApi.DataFlow.Capture)
                        {
                            continue;
                        }

                        //Get its audio volume
                        if (dev != null && dev.AudioEndpointVolume != null)
                        {
                            System.Diagnostics.Debug.Print("Volume of " + dev.FriendlyName + " is " + dev.AudioEndpointVolume.MasterVolumeLevel.ToString());

                        }
                        devices.Items.Add(new CapsWrapper(dev));
                    }
                    catch (Exception ex)
                    {
                        //Do something with exception when an audio endpoint could not be muted
                        System.Diagnostics.Debug.Print(dev.FriendlyName + " could not be loaded");
                    }
                }


                foreach (object o in devices.Items)
                {
                    CapsWrapper c = (CapsWrapper)o;
                    if (c.ToString().ToLower().Contains("jbl")
                        &&
                        c.Device.AudioEndpointVolume.MasterVolumeLevel != 0.0f && c.Device.DataFlow==DataFlow.Render)
                    {
                        devices.SelectedItem = o;
                        Play();
                        break;
                    }
                }

                if (devices.SelectedItem==null)
                {
                    var enumerator = new MMDeviceEnumerator();
                    MMDevice mm =enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

                    bool deviceset = false;

                    if (mm != null)
                    {
                        foreach (object o in devices.Items)
                        {
                            CapsWrapper c = (CapsWrapper)o;
                            if (c.Device.AudioEndpointVolume.MasterVolumeLevel != 0.0f &&
                                c.Device.DataFlow == DataFlow.Render &&
                                c.Device.State == DeviceState.Active &&
                                c.Device.ID.Equals(mm.ID)

                                )
                            {
                                deviceset = true;
                                devices.SelectedItem = o;
                                Play();
                                break;
                            }
                        }
                    }


                    this.Show();
                    this.ShowInTaskbar = true;
                    this.WindowState = System.Windows.WindowState.Normal;
                    if (deviceset)
                    {
                        System.Windows.Forms.MessageBox.Show("A specific jbl device could not be found. The default active device was set instead, please check if everything is allright.");
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Could not find the correct speaker output, please select one manually.");
                    }
                }

                listener();
            }
            catch (Exception ex)
            {
                //When something happend that prevent us to iterate through the devices
                System.Diagnostics.Debug.Print("Could not enumerate devices due to an exception: " + ex.Message);
            }
        }




        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtwavetime.Text = (defaultPlayTime/60).ToString();

            System.Windows.Forms.Application.ApplicationExit += (o, e2) =>
            {
                running = false;
                Thread.Sleep(3000);
            };

            notifyIcon.Visible = true;
            this.ShowInTaskbar = false;
            this.Visibility = Visibility.Collapsed;

            timer = new Thread(() => {
                int count = 0;
                while (running)
                {
                    count = 0;
                    while (count < playtime)
                    {
                        Thread.Sleep(1000);
                        count ++;
                        if (!running) { break; }
                    }
                    try { Play(); }
                    catch(Exception) { }
                    if (!running) { break; }
                }
            });
            timer.IsBackground = true;
            timer.Start();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Click += new EventHandler(notifyIcon_Click);
            notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
            notifyIcon.Text = "Right click to close application";
            notifyIcon.Icon = new Icon("shower.ico");
        }

#region click and change events
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState==WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                this.Visibility = Visibility.Collapsed;
            }
        }

        private void txtwavetime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtwavetime.Text!=null && txtwavetime.Text!="")
            {
                int timeinminutes = defaultPlayTime;
                int.TryParse(txtwavetime.Text, out timeinminutes);
                this.playtime = 60 * timeinminutes;
            }
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.ShowInTaskbar = true;
            this.WindowState = System.Windows.WindowState.Normal;
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MouseEventArgs args = (System.Windows.Forms.MouseEventArgs)e;
            if (args.Button == MouseButtons.Right)
            {
                Environment.Exit(0);
            }
        }
#endregion

        private void listener()
        {
            bool minmaxset = false;
            Thread t = new Thread(() => {
                while (running)
                {
                    Thread.Sleep(100);
                    try
                    {
                        CapsWrapper wrapper = null;
                        App.Current.Dispatcher.Invoke(() => {
                            if (devices.SelectedItem != null)
                            {
                                wrapper = (CapsWrapper)devices.SelectedItem;
                            }
                        });

                        if (wrapper != null)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                double[] volumes = GetDeviceVolumes(wrapper);

                                if (!minmaxset)
                                {
                                    devicevolumebar.Minimum = volumes[0];
                                    devicevolumebar.Maximum = volumes[1];
                                    playbackvolumebar.Maximum = 1;
                                    playbackvolumebar.Minimum = 0;
                                    minmaxset = true;
                                }

                                devicevolumebar.Value = volumes[2];
                                playbackvolumebar.Value = wrapper.Device.AudioMeterInformation.MasterPeakValue;

                                trackvolumebar.Value = GetDeviceVolumes(wrapper)[2];
                            });
                        }
                    }
                    catch (Exception) { }
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        private double[] GetMinMxOffset(CapsWrapper device)
        {
            double offset = 0;
            if (device.Device.AudioEndpointVolume.VolumeRange.MinDecibels < 0)
            {
                offset = device.Device.AudioEndpointVolume.VolumeRange.MinDecibels * -1;
            }

            double min = device.Device.AudioEndpointVolume.VolumeRange.MinDecibels + offset;
            double max = device.Device.AudioEndpointVolume.VolumeRange.MaxDecibels + offset;

            return new double[] { min, max, offset };
        }

        double[] GetDeviceVolumes(CapsWrapper device, WasapiOut wasapi = null)
        {
            double[] vals = GetMinMxOffset(device);

            double offset = vals[2];
            double min = vals[0];
            double max = vals[1];

            double val = 0;

            double deviceval = 0;
            if (wasapi != null)
            {
                val = wasapi.AudioStreamVolume.GetChannelVolume(0) * max;
                deviceval = device.Device.AudioEndpointVolume.MasterVolumeLevelScalar * max;
            }
            else
            {
                val = device.Device.AudioEndpointVolume.MasterVolumeLevelScalar * max;
            }

            

            return new double[] { min, max, val, deviceval };
        }
    }
}
