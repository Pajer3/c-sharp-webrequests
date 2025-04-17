using System.Windows;
using System.Windows.Controls;

namespace SimpleWebRequest // Consider renaming the namespace if this becomes a standalone project
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string taskText = TaskInputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(taskText))
            {
                MessageBox.Show("Please enter a task.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Stop execution if the input is empty
            }

            // Add the task to the ListBox
            TasksListBox.Items.Add(taskText);

            // Clear the TextBox
            TaskInputTextBox.Clear();
        }

        private void RemoveTaskButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected item
            var selectedItem = TasksListBox.SelectedItem;

            if (selectedItem != null)
            {
                // Remove the selected item from the ListBox
                TasksListBox.Items.Remove(selectedItem);
            }
            else
            {
                MessageBox.Show("Please select a task to remove.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
