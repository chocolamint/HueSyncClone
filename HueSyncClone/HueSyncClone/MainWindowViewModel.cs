using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HueSyncClone.Hue;
using HueSyncClone.Models;

namespace HueSyncClone
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool _isAuthenticated = true;
        private bool _isConnecting = true;

        public IHueUserNameStore HueUserNameStore { get; set; } = new FileHueUserNameStore();

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

        public MainWindowViewModel()
        {
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
