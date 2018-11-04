using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DataImport.Interactive.Controls
{
    public class TextBoxExt : TextBox
    {
        private string _hint = "";
        public string Hint
        {
            get { return _hint; }
            set {
                this.FontStyle = FontStyles.Italic; 
                this.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6e6e6e"));
                this.Text = value; _hint = value; }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            this.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff"));
            this.FontStyle = FontStyles.Normal;

            if (this.Text == Hint)
            {
                this.Text = "";
            }
            base.OnGotFocus(e);
        }

        public string getText() {
            if (this.Text == Hint)
                return "";
            else
                return this.Text;
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.Text.Trim()))
            {
                this.FontStyle = FontStyles.Italic;
                this.Text = this.Hint;
                this.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6e6e6e"));
            }
            else
            {
                this.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff"));
            }

            base.OnLostFocus(e);
        }

    }
}
