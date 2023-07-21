﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicDent2.Attached
{
    public class TextBoxProperties : DependencyObject
    {
        #region OnEnterKeyDown
        public static DependencyProperty OnEnterKeyDownProperty = DependencyProperty.RegisterAttached("OnEnterKeyDown", typeof(ICommand), typeof(TextBoxProperties), new PropertyMetadata(OnOnEnterKeyDownChanged));
        public static ICommand GetOnEnterKeyDown(DependencyObject dependencyObject)
        {
            return (ICommand)dependencyObject.GetValue(OnEnterKeyDownProperty);
        }
        public static void SetOnEnterKeyDown(DependencyObject dependencyObject,
        ICommand value)
        {
            dependencyObject.SetValue(OnEnterKeyDownProperty, value);
        }
        private static void OnOnEnterKeyDownChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = (TextBox)dependencyObject;
            if (e.OldValue == null && e.NewValue != null)
                textBox.PreviewKeyDown += TextBox_OnEnterKeyDown;
            else if (e.OldValue != null && e.NewValue == null)
                textBox.PreviewKeyDown -= TextBox_OnEnterKeyDown;
        }
        private static void TextBox_OnEnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                TextBox textBox = sender as TextBox;
                ICommand command = GetOnEnterKeyDown(textBox);
                if (command != null && command.CanExecute(textBox))
                    command.Execute(textBox);
            }
        }
        #endregion
    }
}
