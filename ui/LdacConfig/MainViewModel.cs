using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Threading;

namespace LdacConfig;

public enum Page { Device, About, Faq }

public sealed class MainViewModel : Observable
{
    private BtAudioDevice? _selected;
    private Page _page = Page.Device;
    private string _message = "";
    private bool _loading = true;
    private readonly DispatcherTimer _statusTimer;

    public ObservableCollection<BtAudioDevice> Devices { get; } = new();

    public MainViewModel()
    {
        SettingsStore.Load();
        // The encoder service rewrites the status file while streaming; poll it.
        _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _statusTimer.Tick += (_, _) => RaiseStatus();
        _statusTimer.Start();
    }

    public BtAudioDevice? Selected
    {
        get => _selected;
        set
        {
            if (!Set(ref _selected, value))
                return;
            Page = Page.Device;
            Message = "";
            Raise(nameof(HasDevice));
            RaiseStatus();
        }
    }

    public bool HasDevice => _selected is not null;
    public bool Loading { get => _loading; private set { Set(ref _loading, value); Raise(nameof(NoDevices)); } }
    public bool NoDevices => !Loading && Devices.Count == 0;

    public Page Page
    {
        get => _page;
        set
        {
            if (Set(ref _page, value))
            {
                Raise(nameof(ShowDevice));
                Raise(nameof(ShowAbout));
                Raise(nameof(ShowFaq));
            }
        }
    }

    public bool ShowDevice => Page == Page.Device;
    public bool ShowAbout => Page == Page.About;
    public bool ShowFaq => Page == Page.Faq;

    public string Message { get => _message; set => Set(ref _message, value); }

    public string SettingsPath => Path.Combine(SettingsStore.Root, "settings.json");

    // ---- current state ------------------------------------------------------

    private LinkStatus? _status;
    private string _serviceState = "";
    // a status file the service stopped updating (it writes one a second) is not current
    private LinkStatus? Status => _status is { } s && DateTime.UtcNow - s.Written < TimeSpan.FromSeconds(5) ? s : null;
    public bool HasStatus => Status is not null;
    public string ServiceState => _serviceState;
    public string StatusStreaming => Status is { } s ? (s.Streaming ? "Active" : "Paused (no audio)") : "—";
    public string StatusQuality => Status is { } s && s.Quality.Length > 0 ? s.Quality : "—";
    public string StatusVolume => Status is { } s
        ? (s.Muted ? "Muted" : $"{s.VolumeDb:0.0} dB") + (s.AbsoluteVolume ? ", headphones (AVRCP)" : ", encoder service")
        : "—";
    public string StatusAvrcp => Status?.Avrcp switch
    {
        null => "—",
        "absoluteVolume" => "Connected with absolute volume",
        "noAbsoluteVolume" => "Connected without absolute volume",
        "connected" => "Connected",
        "opening" => "Connecting",
        "closed" => "Disconnected",
        "none" => "No AVRCP device",
        var other => other,
    };
    public string StatusDropped => Status is { } s ? s.DroppedPackets.ToString(CultureInfo.CurrentCulture) : "—";
    public string StatusCodec => Status?.Codec ?? "—";
    public string StatusChannelMode => Status?.ChannelMode ?? "—";
    public string StatusSampleRate => Status is { } s ? FormatRate(s.SampleRate) : "—";
    public string StatusBitDepth => Status is { } s ? $"{s.BitDepth}-bit" : "—";
    public string StatusBitrate => Status is { } s ? $"{s.BitrateKbps} kbps" : "—";

    private static string FormatRate(int hz) =>
        (hz / 1000.0).ToString(hz % 1000 == 0 ? "0" : "0.0", CultureInfo.InvariantCulture) + " kHz";

    private void RaiseStatus()
    {
        _status = _selected is null ? null : SettingsStore.ReadStatus(_selected.AddressText);
        _serviceState = EncoderService.State();
        foreach (var p in new[] { nameof(HasStatus), nameof(StatusCodec), nameof(StatusChannelMode),
                                  nameof(StatusSampleRate), nameof(StatusBitDepth), nameof(StatusBitrate),
                                  nameof(ServiceState), nameof(StatusStreaming), nameof(StatusQuality),
                                  nameof(StatusVolume), nameof(StatusAvrcp), nameof(StatusDropped) })
            Raise(p);
    }

    // ---- actions ------------------------------------------------------------

    public async Task RefreshDevicesAsync()
    {
        Loading = true;
        var keep = _selected?.Address;
        try
        {
            var found = await BluetoothAudioDevices.FindPairedAsync();
            foreach (var d in Devices)
                d.Settings.PropertyChanged -= OnSettingsChanged;
            Devices.Clear();
            foreach (var (name, address, connected) in found)
            {
                var dev = new BtAudioDevice
                {
                    Name = name,
                    Address = address,
                    Connected = connected,
                    Settings = SettingsStore.For(BtAudioDevice.FormatAddress(address)),
                };
                dev.Settings.PropertyChanged += OnSettingsChanged;
                Devices.Add(dev);
            }
            Selected = Devices.FirstOrDefault(d => d.Address == keep) ?? Devices.FirstOrDefault(d =>
                SettingsStore.ReadStatus(d.AddressText) is { } status &&
                DateTime.UtcNow - status.Written < TimeSpan.FromSeconds(5))
                ?? Devices.FirstOrDefault(d => d.Connected) ?? Devices.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Message = "Could not list Bluetooth devices: " + ex.Message;
        }
        finally
        {
            Loading = false;
        }
    }

    private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            SettingsStore.Save();
            Message = $"Settings saved to {SettingsPath}";
        }
        catch (Exception ex)
        {
            Message = "Could not save settings: " + ex.Message;
        }
    }

    public void ApplyPreset()
    {
        if (_selected is null)
            return;
        _selected.Settings.ApplyPreset();
        OnSettingsChanged(null, new PropertyChangedEventArgs(null));
    }

    public void Reconnect()
    {
        if (_selected is null)
            return;
        if (!HasStatus)
        {
            Message = "No active LDAC A2DP session for " + _selected.Name +
                      ". Connect these headphones in Windows Bluetooth settings, then refresh the list.";
            return;
        }
        _selected.Settings.ReconnectRequest = Guid.NewGuid().ToString("N");
        try
        {
            SettingsStore.Save();
            Message = "Audio reconnection requested for " + _selected.Name + ". Check Live status below.";
        }
        catch (Exception ex)
        {
            Message = "Could not request reconnection: " + ex.Message;
        }
    }
}

/// <summary>Binds a RadioButton to one value of an enum property.</summary>
public sealed class EnumEqualsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value?.ToString() == parameter?.ToString();

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? Enum.Parse(targetType, (string)parameter) : Binding.DoNothing;
}

public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
