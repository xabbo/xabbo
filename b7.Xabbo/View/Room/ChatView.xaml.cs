using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace b7.Xabbo.View
{
    public partial class ChatView : UserControl, INotifyPropertyChanged
    {
        #region - INotifyPropertyChanged -
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool _set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        private bool autoScroll = true;
        public bool AutoScroll
        {
            get => autoScroll;
            set => _set(ref autoScroll, value);
        }

        public ChatView()
        {
            InitializeComponent();

            textBoxLog.Loaded += TextBoxLog_Loaded;
            textBoxLog.TextChanged += TextBoxLog_TextChanged;
        }

        private void TextBoxLog_Loaded(object sender, RoutedEventArgs e)
        {
            if (AutoScroll)
                textBoxLog.ScrollToEnd();
        }

        private void TextBoxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AutoScroll)
                textBoxLog.ScrollToEnd();
        }
    }
}
