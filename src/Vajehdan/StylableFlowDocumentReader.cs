using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Vajehdan
{
    public class StyleableFlowDocumentReader : FlowDocumentReader
    {
        protected override void OnFindCommand()
        {
            base.OnFindCommand();

            Action b = () => { GetFindTextBox(); };

            Dispatcher.BeginInvoke(b, DispatcherPriority.Render);
        }

        private void GetFindTextBox()
        {
            findTextBox = this.FindVisualChild("FindTextBox") as TextBox;
            if (findTextBox!=null)
                findTextBox.PreviewKeyDown += FindTextBox_PreviewKeyDown;
            ApplyFindTextBoxStyle();
        }

        private void FindTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(findTextBox.Text) && e.Key == Key.Space)
            {
                findTextBox.Clear();
                e.Handled = true;
            }

            //Allow typing half space
            var shift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
            var ctrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            var space = Keyboard.IsKeyDown(Key.Space);
            var two = Keyboard.IsKeyDown(Key.D2);
            // First condition:  Half space in standard persian keyboard
            // Second condition: Half space in non-standard persian keyboard
            if (shift && space || ctrl && shift && two)
            {
                int s = findTextBox.SelectionStart;
                findTextBox.Text = findTextBox.Text.Insert(s, '\u200c'.ToString());

                findTextBox.SelectionStart = s + 1;
                findTextBox.SelectionLength = findTextBox.Text.Length;
                e.Handled = true;
            }
        }

        private void ApplyFindTextBoxStyle()
        {
            if (findTextBox != null)
            {
                if (findTextBox.Style != null && FindTextBoxStyle != null)
                    findTextBox.Style = ExtensionMethods.MergeStyles(findTextBox.Style, FindTextBoxStyle);
                else
                    findTextBox.Style = FindTextBoxStyle ?? findTextBox.Style;
            }
        }


        private TextBox findTextBox;

        public Style FindTextBoxStyle
        {
            get
            {
                return GetValue(FindTextBoxStyleProperty) as Style;
            }
            set
            {
                SetValue(FindTextBoxStyleProperty, value);
            }
        }
        public static readonly DependencyProperty FindTextBoxStyleProperty = DependencyProperty.Register("FindTextBoxStyle", typeof(Style), typeof(StyleableFlowDocumentReader), new PropertyMetadata(null, (d, e) => ((StyleableFlowDocumentReader)d).ApplyFindTextBoxStyle()));



    }

    public static class ExtensionMethods
    {
        public static DependencyObject FindVisualChild(this DependencyObject obj, String Name)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject ChildObj = VisualTreeHelper.GetChild(obj, i);

                if ((ChildObj is FrameworkElement) && ((FrameworkElement)ChildObj).Name == Name)
                    return ChildObj;

                ChildObj = FindVisualChild(ChildObj, Name);

                if (ChildObj != null)
                    return ChildObj;
            }
            return null;
        }

        public static Style MergeStyles(Style style1, Style style2)
        {
            Style R = new Style();

            if (style1 == null)
                throw new ArgumentNullException("style1");
            if (style2 == null)
                throw new ArgumentNullException("style2");
            if (style2.BasedOn != null)
                style1 = MergeStyles(style1, style2.BasedOn);

            foreach (SetterBase currentSetter in style1.Setters)
                R.Setters.Add(currentSetter);

            foreach (TriggerBase currentTrigger in style1.Triggers)
                R.Triggers.Add(currentTrigger);

            foreach (object key in style1.Resources.Keys)
                R.Resources[key] = style1.Resources[key];

            foreach (SetterBase currentSetter in style2.Setters)
                R.Setters.Add(currentSetter);

            foreach (TriggerBase currentTrigger in style2.Triggers)
                R.Triggers.Add(currentTrigger);

            foreach (object key in style2.Resources.Keys)
                R.Resources[key] = style2.Resources[key];

            return R;
        }

    }
}
