using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;

namespace CherylUI.Controls;

public partial class MobileNavigation : UserControl
{
    public Control? CurrentPage;
    public Stack<Control> Pages = new();

    public MobileNavigation()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public void PopPage()
    {
        if (Pages.Count == 0) return;
        var page = Pages.Pop();
        Content = page;
        CurrentPage = page;
    }

    public static void Pop()
    {
        var instance = GetMobileStackInstance();
        if (instance.Pages.Count == 0) return;

        var page = instance.Pages.Pop();
        instance.CurrentPage = page;
        var templateChildren = instance.GetTemplateChildren();
        var tcc = (TransitioningContentControl)templateChildren.First(f => f.Name == "base");
        tcc.Content = page;
    }

    public static void Push(UserControl content, bool DisableComeBack = false)
    {
        var instance = GetMobileStackInstance();
        instance.CurrentPage ??= (Control)instance.Content!;
        instance.Pages.Push(instance.CurrentPage);
        if (DisableComeBack) instance.Pages.Clear();
        instance.CurrentPage = content;
        var templateChildren = instance.GetTemplateChildren();
        var tcc = (TransitioningContentControl)templateChildren.First(f => f.Name == "base");
        tcc.Content = content;
    }

    public static MobileNavigation GetMobileStackInstance()
    {
        var lifetime = Application.Current?.ApplicationLifetime;
        if (lifetime != null)
        {
            var singleViewApp = lifetime as ISingleViewApplicationLifetime;
            var mainView = singleViewApp?.MainView;
            var mobileNavigation = mainView?.GetVisualDescendants().OfType<MobileNavigation>().FirstOrDefault();
            if (mobileNavigation != null) return mobileNavigation;

            var desktopApp = lifetime as IClassicDesktopStyleApplicationLifetime;
            var mainWindow = desktopApp?.MainWindow;
            mobileNavigation = mainWindow?.GetVisualDescendants().OfType<MobileNavigation>().FirstOrDefault();
            if (mobileNavigation != null) return mobileNavigation;
        }

        throw new Exception("This is no MobileNavigation control declared.");
    }
}