﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace CherylUI.Controls;

public partial class GroupBox : UserControl
{
    public GroupBox()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static readonly StyledProperty<string> HeaderProperty =
        AvaloniaProperty.Register<GroupBox, string>(nameof(Header), defaultValue: "Header");

    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly StyledProperty<string?> TextProperty =
        TextBlock.TextProperty.AddOwner<GroupBox>(new StyledPropertyMetadata<string?>(
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true));

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}