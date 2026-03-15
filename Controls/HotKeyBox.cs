using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CalabiyauQuotation.Controls
{
    public class HotKeyBox : Control
    {
        private TextBox? _textBox;
        private Button? _button;
        private bool _isRecording = false;
        private bool _cancelByButton = false;

        public static readonly DependencyProperty HotKeyProperty =
            DependencyProperty.Register(nameof(HotKey), typeof(string), typeof(HotKeyBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHotKeyChanged));

        public string HotKey
        {
            get => (string)GetValue(HotKeyProperty);
            set => SetValue(HotKeyProperty, value);
        }

        public event EventHandler? HotKeyChanged;

        private static void OnHotKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HotKeyBox box)
            {
                if (!box._isRecording)
                {
                    box.UpdateDisplay();
                }
                box.HotKeyChanged?.Invoke(box, EventArgs.Empty);
            }
        }

        static HotKeyBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HotKeyBox),
                new FrameworkPropertyMetadata(typeof(HotKeyBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textBox = GetTemplateChild("PART_TextBox") as TextBox;
            _button = GetTemplateChild("PART_Button") as Button;

            if (_button != null)
            {
                _button.PreviewMouseDown += Button_PreviewMouseDown;
                _button.Click += Button_Click;
            }

            UpdateDisplay();
        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _cancelByButton = _isRecording;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_isRecording)
            {
                StopRecording(true);
            }
            else
            {
                StartRecording();
            }
            _cancelByButton = false;
        }

        private void StartRecording()
        {
            _isRecording = true;
            _cancelByButton = false;
            
            if (_textBox != null)
            {
                _textBox.Text = "请按下快捷键...";
                _textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
                _textBox.LostFocus += TextBox_LostFocus;
                _textBox.Focus();
            }
            
            if (_button != null)
            {
                _button.Content = "取消";
            }
        }

        private void StopRecording(bool restoreHotKey)
        {
            _isRecording = false;
            
            if (_textBox != null)
            {
                _textBox.PreviewKeyDown -= TextBox_PreviewKeyDown;
                _textBox.LostFocus -= TextBox_LostFocus;
            }
            
            if (_button != null)
            {
                _button.Content = "录制";
            }

            if (restoreHotKey)
            {
                UpdateDisplay();
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            if (e.Key == Key.Escape)
            {
                StopRecording(true);
                return;
            }

            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
                e.Key == Key.LeftShift || e.Key == Key.RightShift ||
                e.Key == Key.LWin || e.Key == Key.RWin)
            {
                return;
            }

            string hotkey = string.Empty;

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                hotkey += "Ctrl+";
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                hotkey += "Alt+";
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                hotkey += "Shift+";
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Windows))
                hotkey += "Win+";

            hotkey += e.Key.ToString();
            HotKey = hotkey;
            StopRecording(false);
            UpdateDisplay();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_isRecording && !_cancelByButton)
            {
                StopRecording(true);
            }
        }

        private void UpdateDisplay()
        {
            if (_textBox != null && !_isRecording)
            {
                _textBox.Text = HotKey;
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            if (_textBox != null && !_isRecording)
            {
                _textBox.Focus();
            }
        }
    }
}
