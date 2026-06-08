using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;

namespace TeamTaskManager.Helpers
{
    public abstract class ExpandableTextItem : ObservableObject
    {
        private int _lineHeight;
        private int _maxLines;

        protected ExpandableTextItem(int lineHeight = 20, int maxLines = 3)
        {
            _lineHeight = lineHeight;
            _maxLines = maxLines;
            ToggleTextCommand = new RelayCommand(() => IsTextExpanded = !IsTextExpanded);
        }

        public ICommand ToggleTextCommand { get; protected set; } = null!;
        private bool _textOverflows;
        public bool TextOverflows
        {
            get => _textOverflows;
            set { if (_textOverflows != value) { _textOverflows = value; OnPropertyChanged(); } }
        }

        private bool _isTextExpanded;
        public bool IsTextExpanded
        {
            get => _isTextExpanded;
            set
            {
                if (_isTextExpanded != value)
                {
                    _isTextExpanded = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(MaxTextHeight));
                    OnPropertyChanged(nameof(ToggleTextLabel));
                }
            }
        }
        public double MaxTextHeight => IsTextExpanded ? double.PositiveInfinity : _lineHeight * _maxLines;
        public string ToggleTextLabel => IsTextExpanded ? "Zwiń" : "...Zobacz więcej";
    }

    public class FilterOption
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
    }

    // do focusowania bez code-behind
    public static class FocusHelper
    {
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached("IsFocused", typeof(bool), typeof(FocusHelper),
            new PropertyMetadata(false, OnIsFocusedPropertyChanged));

        public static void SetIsFocused(DependencyObject obj, bool value) => obj.SetValue(IsFocusedProperty, value);

        private static void OnIsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    textBox.Focus();
                    textBox.SelectionStart = textBox.Text.Length;
                    textBox.SelectionLength = 0;
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }
    }
}
