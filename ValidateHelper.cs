using RCApp_Win.MyMessageBox;
using RCApp_Win.View.Custom_Controls;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace RCApp_Win.Logic.Utility1
{
    public class ValidateHelper
    {
        private List<ControlPair> ControlPairList = new List<ControlPair>();

        public void Add(Control control, string name, params IValidator[] validator)
        {
            ControlPairList.Add(new ControlPair(control, name, validator));
        }

        public bool Validate()
        {
            foreach (ControlPair controlPair in ControlPairList)
            {
                if (!controlPair.Validate())
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class ControlPair
    {
        public Control Control { set; get; }
        public string Name { set; get; }
        public Brush OriginalBrush { set; get; }
        public List<IValidator> ValidatorRules = new List<IValidator>();

        public ControlPair(Control control, string controlName, params IValidator[] rules)
        {
            this.Control = control;
            this.Name = controlName;
            this.OriginalBrush = control.BorderBrush;
            this.ValidatorRules.AddRange(rules);
        }

        public bool Validate()
        {
            if (Control == null)
                return false;

            foreach (IValidator validator in ValidatorRules)
            {
                bool result = validator.Validate(this);
                if (!result)
                {
                    ChangeColor(Control, Brushes.Red);
                    ErrorTip();
                    return false;
                }
                else
                {
                    ChangeColor(Control, OriginalBrush);
                }
            }
            return true;
        }

        public void ErrorTip()
        {
            YZWmessagebox.Show(Name + "为必填项");
        }

        public virtual void RemoveColor(object sender, EventArgs args)
        {
            Control control = sender as Control;
            ChangeColor(control, OriginalBrush);
        }

        private void ChangeColor(Control control, Brush brush)
        {
            Version currentVersion = new Version();
            currentVersion = Environment.OSVersion.Version;
            Version win7 = new Version("6.2");
            Version win10 = new Version("6.4");
            //坑爹的win8 combox的背景色和边框颜色无法正常显示
            if (control is ComboBox && currentVersion.CompareTo(win7) > 0 && currentVersion.CompareTo(win10) < 0)
            {
                var comboBoxTemplate = control.Template;
                var toggleButton = comboBoxTemplate.FindName("toggleButton", control) as ToggleButton;
                if (toggleButton != null)
                {
                    var toggleButtonTemplate = toggleButton.Template;
                    var border = toggleButtonTemplate.FindName("templateRoot", toggleButton) as Border;
                    border.BorderBrush = brush;
                }
            }
            else if (control is AutoCompeleteTextBox)
            {
                AutoCompeleteTextBox autoCompeleteTextBox = control as AutoCompeleteTextBox;
                autoCompeleteTextBox.textBox.BorderBrush = brush;
            }
            else
            {
                control.BorderBrush = brush;
            }
        }
    }

    public interface IValidator
    {
        bool Validate(ControlPair controlPair);
    }

    class RequiredValitator : IValidator
    {
        public override bool Validate(ControlPair controlPair)
        {
            Control control = controlPair.Control;
            if (control is ComboBox)
            {
                ComboBox comboBox = control as ComboBox;
                object selectedItem = comboBox.SelectedItem;
                if (selectedItem == null)
                {
                    return false;
                }
            }
            else if (control is TextBox)
            {
                TextBox textBox = control as TextBox;
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.TextChanged += controlPair.RemoveColor;
                    return false;
                }
            }
            else if (control is DatePicker)
            {
                DatePicker datePicker = control as DatePicker;
                if (datePicker.SelectedDate == null)
                {
                    datePicker.SelectedDateChanged += controlPair.RemoveColor;
                    return false;
                }
            }
            else if (control is PasswordBox)
            {
                PasswordBox password = control as PasswordBox;
                if (string.IsNullOrEmpty(password.Password))
                {
                    password.PasswordChanged += controlPair.RemoveColor;
                    return false;
                }
            }
            return true;
        }
    }
}
