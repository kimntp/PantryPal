using Microsoft.Maui.Controls;

namespace PPApp.Controls
{
    public partial class ComboBox : ContentView
    {
        public event EventHandler? SelectedItemChanged;

        private List<string> _allItems = new();
        private List<string> _filteredItems = new();

        public ComboBox()
        {
            InitializeComponent();

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => ToggleDropdown();
            SelectedItemBorder.GestureRecognizers.Add(tapGesture);
        }

        // -----------------------------
        // Bindable Properties
        // -----------------------------

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(
                nameof(ItemsSource),
                typeof(IEnumerable<string>),
                typeof(ComboBox),
                propertyChanged: OnItemsSourceChanged);

        public IEnumerable<string> ItemsSource
        {
            get => (IEnumerable<string>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(
                nameof(SelectedItem),
                typeof(string),
                typeof(ComboBox),
                defaultBindingMode: BindingMode.TwoWay);

        public string? SelectedItem
        {
            get => (string?)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        // -----------------------------
        // ItemsSource changed
        // -----------------------------

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ComboBox)bindable;

            control._allItems = (newValue as IEnumerable<string>)?.ToList() ?? new List<string>();
            control._filteredItems = new List<string>(control._allItems);
            control.BuildDropdown();
        }

        // -----------------------------
        // Build dropdown from _filteredItems
        // -----------------------------

        private void BuildDropdown()
        {
            ItemsContainer.Children.Clear();

            if (_filteredItems == null || _filteredItems.Count == 0)
                return;

            foreach (var item in _filteredItems)
            {
                var label = new Label
                {
                    Text = item,
                    Padding = new Thickness(10),
                    FontSize = 16
                };

                var tap = new TapGestureRecognizer();
                tap.Tapped += (s, e) =>
                {
                    SelectedItem = item;
                    InputEntry.Text = item;       
                    SelectedItemChanged?.Invoke(this, EventArgs.Empty);
                    Dropdown.IsVisible = false;
                };

                label.GestureRecognizers.Add(tap);
                ItemsContainer.Children.Add(label);
            }
        }

        // -----------------------------
        // Toggle dropdown open/closed
        // -----------------------------

        private void ToggleDropdown()
        {
            if (_filteredItems == null || _filteredItems.Count == 0)
            {
                Dropdown.IsVisible = false;
                return;
            }

            Dropdown.IsVisible = !Dropdown.IsVisible;
        }

        // -----------------------------
        // Typing in the combo box
        // -----------------------------

        private void OnInputTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = e.NewTextValue ?? string.Empty;

            if (string.IsNullOrWhiteSpace(text))
            {
                _filteredItems = new List<string>(_allItems);
            }
            else
            {
                _filteredItems = _allItems
                    .Where(i => i.Contains(text, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            BuildDropdown();

            // Show dropdown when there are matches, hide if none
            Dropdown.IsVisible = _filteredItems.Count > 0;
        }
    }
}
