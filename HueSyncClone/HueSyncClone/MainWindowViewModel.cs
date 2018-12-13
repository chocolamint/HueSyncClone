using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using HueSyncClone.Core;
using HueSyncClone.Drawing;
using HueSyncClone.Hue;
using HueSyncClone.Models;

namespace HueSyncClone
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool _isAuthenticated = true;
        private bool _isConnecting = true;
        private string _imagePath;
        private IReadOnlyList<HueLight> _lights;
        private int _lightCount;

        public IHueUserNameStore HueUserNameStore { get; set; } = new FileHueUserNameStore();

        public ColorSelector ColorSelector { get; set; } = new ColorSelector();

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

        public ObservableCollection<Color> Colors { get; } = new ObservableCollection<Color>();

        public ICommand OnFileSelectedCommand { get; } 

        public MainWindowViewModel()
        {
            OnFileSelectedCommand = new DelegateCommand<string[]>(OnFileSelected);
            Initialize();
        }

        private void OnFileSelected(string[] filePaths)
        {
            ImagePath = filePaths.First();

            using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(ImagePath))
            {
                var colors = ColorSelector.SelectColor(bitmap, _lights.Count);
                foreach (var color in colors)
                {
                    Colors.Add(Color.FromRgb(color.R, color.G, color.B));
                }
            }
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
