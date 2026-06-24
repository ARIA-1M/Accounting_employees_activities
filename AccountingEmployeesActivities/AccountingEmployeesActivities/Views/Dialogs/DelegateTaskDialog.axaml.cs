using AccountingEmployeesActivities.ViewsModels.Dialog;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccountingEmployeesActivities.Views.Dialogs
{
    public partial class DelegateTaskDialog : Window
    {
        public DelegateTaskDialog() 
        {
            InitializeComponent();
        }
        public DelegateTaskDialog(int taskId, int currentEmployeeId)
        {
            InitializeComponent();
            DataContext = new DelegateTaskDialogViewModel(taskId, currentEmployeeId, this);
        }
    }
}
