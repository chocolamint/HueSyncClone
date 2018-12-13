using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using HueSyncClone.Core;
using HueSyncClone.Drawing;
using HueSyncClone.Hue;
using HueSyncClone.Models;
using Color = System.Windows.Media.Color;

namespace HueSyncClone
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool _isAuthenticated = true;
        private bool _isConnecting = true;
        private string _imagePath;
        private IReadOnlyList<HueLight> _lights;
        private int _lightCount;
        private DispatcherTimer _timer;
        private bool _isFileDroppable;
        private bool _synchronizeScreen;

        public IHueUserNameStore HueUserNameStore { get; set; } = new FileHueUserNameStore();

        public ColorPicker ColorPicker { get; set; } = new ColorPicker(new Random().Next());

        public bool IsConnecting
        {
            get => _isConnecting;
            set => SetField(ref _isConnecting, value);
        }

        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set => SetField(ref _isAuthenticated, value);
        }

        public string ImagePath
        {
            get => _imagePath;
            set => SetField(ref _imagePath, value);
        }

        public int LightCount
        {
            get => _lightCount;
            set => SetField(ref _lightCount, value);
        }

        public bool IsFileDroppable
        {
            get => _isFileDroppable;
            set => SetField(ref _isFileDroppable, value);
        }

        public bool SynchronizeScreen
        {
            get => _synchronizeScreen;
            set => SetField(ref _synchronizeScreen, value);
        }

        public ObservableCollection<Color> Colors { get; } = new ObservableCollection<Color>();

        public ICommand OnFileSelectedCommand { get; }

        public MainWindowViewModel()
        {
            OnFileSelectedCommand = new DelegateCommand<string[]>(OnFileSelected);
            Initialize();
        }

        private async void Initialize()
        {
            var userName = HueUserNameStore.Load();
            var bridge = (await HueBridge.GetBridgesAsync()).First();

            IsConnecting = false;

            if (userName == null)
            {
                IsAuthenticated = false;

                await bridge.AuthorizeAsync("HueSyncClone", CancellationToken.None);
                HueUserNameStore.Save(bridge.UserName);
            }
            else
            {
                bridge.UserName = userName;
            }

            IsConnecting = true;

            _lights = await bridge.GetLightsAsync();

            LightCount = _lights.Count;
            IsConnecting = false;
            IsAuthenticated = true;

            _timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, OnTick, Dispatcher.CurrentDispatcher);

            IsFileDroppable = true;
        }

        private async void OnFileSelected(string[] filePaths)
        {
            if (IsFileDroppable)
            {
                ImagePath = filePaths.First();

                using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(ImagePath))
                {
                    await ChangeColorsAsync(bitmap);
                }
            }
        }

        private async void OnTick(object sender, EventArgs e)
        {
            if (SynchronizeScreen)
            {
                using (var bitmap = ScreenShot.CaptureScreen())
                {
                    await ChangeColorsAsync(bitmap);
                }
            }
        }

        private async Task ChangeColorsAsync(System.Drawing.Bitmap bitmap)
        {
            var tasks = new List<Task>();

            Colors.Clear();

            var colors = ColorPicker.PickColors(bitmap, _lights.Count);
            foreach (var (color, index) in colors.Select((x, i) => (x, i)))
            {
                Colors.Add(Color.FromRgb(color.R, color.G, color.B));

                var xy = XyColor.FromRgb(color.R, color.G, color.B);
                var brightness = new[] { color.R, color.G, color.B }.Max();

                var task = _lights[index].SetColorAsync(xy, brightness);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        #region INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);
        #endregion
    }
}
