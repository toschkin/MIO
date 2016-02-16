using System.Windows.Controls;
using System.Windows.Input;

namespace MIOConfigurator.Utilites
{
    public class EditableDataGrid : DataGrid
    {
        protected override void OnCanExecuteBeginEdit(CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
    }
}
