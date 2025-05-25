using ReactiveUI;
using System;
using System.Windows;

namespace DocCreator01.Views
{
    public partial class InputDialog : Window
    {
        private readonly InputDialogViewModel _viewModel;

        public InputDialog()
        {
            InitializeComponent();
            _viewModel = new InputDialogViewModel();
            DataContext = _viewModel;
        }

        public InputDialog(string title, string prompt, string initialValue, Func<string, (bool isValid, string errorMessage)> validationFunc) : this()
        {
            _viewModel.Title = title;
            _viewModel.Prompt = prompt;
            _viewModel.InputValue = initialValue;
            _viewModel.ValidationFunction = validationFunc;

            // Set focus to the text box when the dialog loads
            Loaded += (s, e) =>
            {
                InputTextBox.Focus();
                InputTextBox.SelectAll();
            };
        }

        public string InputValue => _viewModel.InputValue;
        public bool IsValid => _viewModel.IsValid;

        public new bool? ShowDialog()
        {
            // Set the owner to the application's main window
            this.Owner = Application.Current.MainWindow;

            // Show the dialog
            return base.ShowDialog();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class InputDialogViewModel : ReactiveObject
    {
        private string _title = "Input";
        private string _prompt = "Enter value:";
        private string _inputValue = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _hasError = false;
        private bool _isValid = true;

        public Func<string, (bool isValid, string errorMessage)> ValidationFunction { get; set; }

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public string Prompt
        {
            get => _prompt;
            set => this.RaiseAndSetIfChanged(ref _prompt, value);
        }

        public string InputValue
        {
            get => _inputValue;
            set
            {
                this.RaiseAndSetIfChanged(ref _inputValue, value);
                Validate();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public bool HasError
        {
            get => _hasError;
            set => this.RaiseAndSetIfChanged(ref _hasError, value);
        }

        public bool IsValid
        {
            get => _isValid;
            set => this.RaiseAndSetIfChanged(ref _isValid, value);
        }

        private void Validate()
        {
            // If no validation function is provided, all inputs are valid
            if (ValidationFunction == null)
            {
                HasError = false;
                ErrorMessage = string.Empty;
                IsValid = true;
                return;
            }

            // Use the provided validation function
            var (isValid, errorMessage) = ValidationFunction(InputValue);

            HasError = !isValid;
            ErrorMessage = errorMessage;
            IsValid = isValid;
        }
    }
}
