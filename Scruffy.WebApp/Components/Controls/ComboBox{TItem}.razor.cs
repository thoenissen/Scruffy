using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Components;

using Scruffy.WebApp.Components.Controls.Abstraction;

namespace Scruffy.WebApp.Components.Controls;

/// <summary>
/// Combo box
/// </summary>
/// <typeparam name="TItem">Element type</typeparam>
public partial class ComboBox<TItem>
    where TItem : class, IComboBoxEntry
{
    #region Fields

    /// <summary>
    /// Selected index
    /// </summary>
    private int _selectedIndex;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public ComboBox()
    {
        _selectedIndex = -1;
    }

    #endregion // Constructor

    #region Properties

    #region Parameters

    /// <summary>
    /// Items
    /// </summary>
    [Parameter]
    public List<TItem> Items { get; set; }

    /// <summary>
    /// Selected item
    /// </summary>
    [Parameter]
    public TItem SelectedItem
    {
        get;
        set
        {
            field = value;

            SetSelectedIndex(value);
        }
    }

    /// <summary>
    /// Selected item changed
    /// </summary>
    [Parameter]
    public EventCallback<TItem> SelectedItemChanged { get; set; }

    /// <summary>
    /// Filter
    /// </summary>
    [Parameter]
    public Func<TItem, bool> Filter { get; set; } = _ => true;

    #endregion // Parameters

    /// <summary>
    /// Selected index
    /// </summary>
    private int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            _selectedIndex = value;

            if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
            {
                SelectedItem = Items[SelectedIndex];
            }
            else
            {
                SelectedItem = null;
            }

            SelectedItemChanged.InvokeAsync(SelectedItem);

            StateHasChanged();
        }
    }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Get items
    /// </summary>
    /// <returns>Items</returns>
    private IEnumerable<(string Name, List<(int Index, TItem Value)> Items)> GetItems()
    {
        (string Name, List<(int Index, TItem Value)> Items)? currentGroup = null;

        for (var index = 0; index < Items.Count; index++)
        {
            var item = Items[index];

            if (Filter != null && Filter(item) == false)
            {
                continue;
            }

            var groupKey = item is IComboBoxEntry groupable ? groupable.Group : null;

            if (currentGroup == null
                || currentGroup.Value.Name != groupKey)
            {
                if (currentGroup != null)
                {
                    yield return currentGroup.Value;
                }

                currentGroup = (groupKey, []);
            }

            currentGroup.Value.Items.Add((index, item));
        }

        if (currentGroup != null)
        {
            yield return currentGroup.Value;
        }
    }

    /// <summary>
    /// Set <see cref="SelectedIndex"/> based on the selected item"/>
    /// </summary>
    /// <param name="selectedItem">Selected item</param>
    private void SetSelectedIndex(TItem selectedItem)
    {
        if (selectedItem == null)
        {
            if (SelectedIndex != -1)
            {
                SelectedIndex = -1;
            }

            return;
        }

        for (var index = 0; index < Items.Count; index++)
        {
            if (selectedItem == Items[index])
            {
                if (SelectedIndex != index)
                {
                    SelectedIndex = index;
                }

                break;
            }
        }
    }

    #endregion // Methods

    #region ComponentBase

    /// <inheritdoc />
    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (firstRender && SelectedItem != null)
        {
            SetSelectedIndex(SelectedItem);
        }
    }

    #endregion // ComponentBase
}