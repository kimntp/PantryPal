using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using PPApp.Model;

namespace PPApp.Controls
{
    public partial class ComboBox : ContentView
    {
        // Fired whenever the selection (chips) changes
        public event EventHandler? SelectedItemChanged;

        private List<string> _allItems = new();
        private List<string> _filteredItems = new();

        public ComboBox()
        {
            InitializeComponent();

            // Ensure SelectedIngredients is always non-null
            if (SelectedIngredients == null)
            {
                SelectedIngredients = new ObservableCollection<string>();
            }

            SelectedIngredients.CollectionChanged += OnSelectedIngredientsCollectionChanged;
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

        // Multi-select: currently selected ingredients (chips)
        public static readonly BindableProperty SelectedIngredientsProperty =
            BindableProperty.Create(
                nameof(SelectedIngredients),
                typeof(ObservableCollection<string>),
                typeof(ComboBox),
                defaultValueCreator: _ => new ObservableCollection<string>(),
                defaultBindingMode: BindingMode.TwoWay,
                propertyChanged: OnSelectedIngredientsPropertyChanged);

        public ObservableCollection<string> SelectedIngredients
        {
            get => (ObservableCollection<string>)GetValue(SelectedIngredientsProperty);
            set => SetValue(SelectedIngredientsProperty, value);
        }

        private static void OnSelectedIngredientsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ComboBox)bindable;

            if (oldValue is ObservableCollection<string> oldCollection)
            {
                oldCollection.CollectionChanged -= control.OnSelectedIngredientsCollectionChanged;
            }

            if (newValue is ObservableCollection<string> newCollection)
            {
                newCollection.CollectionChanged += control.OnSelectedIngredientsCollectionChanged;
            }

            control.BuildChips();
            control.RebuildFilterAndDropdown();
        }

        private void OnSelectedIngredientsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            BuildChips();
            RebuildFilterAndDropdown();
            SelectedItemChanged?.Invoke(this, EventArgs.Empty);
        }

        // -----------------------------
        // ItemsSource changed
        // -----------------------------

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ComboBox)bindable;

            control._allItems = (newValue as IEnumerable<string>)?.ToList() ?? new List<string>();
            control.RebuildFilterAndDropdown();
        }

        // -----------------------------
        // UI building: chips
        // -----------------------------

        private void BuildChips()
        {
            SelectedChipsLayout.Children.Clear();

            if (SelectedIngredients == null || SelectedIngredients.Count == 0)
                return;

            foreach (var ingredient in SelectedIngredients)
            {
                // Default pastel gray
                Color bg = Color.FromArgb("#E0E0E0");
                Color stroke = Color.FromArgb("#9E9E9E");

                if (_meta.TryGetValue(ingredient, out var info))
                {
                    if (info.IsMeat)
                    {
                        // pastel red
                        bg = Color.FromArgb("#FFCDD2");
                        stroke = Color.FromArgb("#E57373");
                    }
                    else if (info.HasGluten)
                    {
                        // pastel tan
                        bg = Color.FromArgb("#FFE0B2");
                        stroke = Color.FromArgb("#FFB74D");
                    }
                    else
                    {
                        // vegetarian-safe: pastel green
                        bg = Color.FromArgb("#C8E6C9");
                        stroke = Color.FromArgb("#81C784");
                    }
                }

                var chipBorder = new Border
                {
                    BackgroundColor = bg,
                    Stroke = stroke,
                    StrokeThickness = 1,
                    Padding = new Thickness(8, 4),
                    Margin = new Thickness(4, 2),
                    StrokeShape = new RoundRectangle { CornerRadius = 12 }
                };

                var chipLayout = new HorizontalStackLayout
                {
                    Spacing = 4
                };

                var label = new Label
                {
                    Text = ingredient,
                    FontSize = 14
                };

                var removeLabel = new Label
                {
                    Text = "✕",
                    FontSize = 14
                };

                var tapRemove = new TapGestureRecognizer();
                tapRemove.Tapped += (s, e) =>
                {
                    if (SelectedIngredients.Contains(ingredient))
                    {
                        SelectedIngredients.Remove(ingredient);
                    }
                };
                removeLabel.GestureRecognizers.Add(tapRemove);

                chipLayout.Children.Add(label);
                chipLayout.Children.Add(removeLabel);

                chipBorder.Content = chipLayout;
                SelectedChipsLayout.Children.Add(chipBorder);
            }
        }

        // -----------------------------
        // UI building: dropdown
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
                    // Add to multi-select set (chips)
                    if (!SelectedIngredients.Contains(item))
                    {
                        SelectedIngredients.Add(item);
                    }

                    InputEntry.Text = string.Empty;
                    Dropdown.IsVisible = false;
                };

                label.GestureRecognizers.Add(tap);
                ItemsContainer.Children.Add(label);
            }
        }

        // Note: currently unused, but kept in case you want a manual toggle button later.
        private void ToggleDropdown()
        {
            RebuildFilterAndDropdown();

            if (_filteredItems == null || _filteredItems.Count == 0)
            {
                Dropdown.IsVisible = false;
                return;
            }

            Dropdown.IsVisible = !Dropdown.IsVisible;
        }

        // -----------------------------
        // Filtering logic
        // -----------------------------

        private void OnInputTextChanged(object sender, TextChangedEventArgs e)
        {
            RebuildFilterAndDropdown();
            Dropdown.IsVisible = _filteredItems.Count > 0 && !string.IsNullOrWhiteSpace(e.NewTextValue);
        }

        private void RebuildFilterAndDropdown()
        {
            var text = InputEntry.Text ?? string.Empty;

            var baseList = _allItems
                .Where(i => SelectedIngredients == null || !SelectedIngredients.Contains(i))
                .ToList();

            if (string.IsNullOrWhiteSpace(text))
            {
                _filteredItems = baseList;
            }
            else
            {
                _filteredItems = baseList
                    .Where(i => i.Contains(text, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            BuildDropdown();
        }

        // -----------------------------
        // Ingredient metadata for colors
        // -----------------------------

        public class IngredientStyleInfo
        {
            public bool IsMeat { get; set; }
            public bool HasGluten { get; set; }
        }

        private Dictionary<string, IngredientStyleInfo> _meta =
            new(StringComparer.OrdinalIgnoreCase);

        public void SetIngredientMetadata(Dictionary<string, IngredientMeta> meta)
        {
            _meta = meta?.ToDictionary(
                kvp => kvp.Key,
                kvp => new IngredientStyleInfo
                {
                    IsMeat = kvp.Value.IsMeat,
                    HasGluten = kvp.Value.HasGluten
                },
                StringComparer.OrdinalIgnoreCase
            ) ?? new Dictionary<string, IngredientStyleInfo>(StringComparer.OrdinalIgnoreCase);

            BuildChips();
        }
    }
}
