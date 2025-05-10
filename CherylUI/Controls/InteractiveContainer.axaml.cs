using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace CherylUI.Controls;

public partial class InteractiveContainer : UserControl
{
    public InteractiveContainer()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (TopLevel.GetTopLevel(this) is { } toplevel)
            {
                toplevel.BackRequested += (_, e) =>
                {
                    MobileNavigation mobileNavigation = MobileNavigation.GetMobileStackInstance();
                    if (mobileNavigation.Pages.Count == 0) return;
                    MobileNavigation.Pop();
                    e.Handled = true;
                };
            }
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public static readonly StyledProperty<bool> ShowAtBottomProperty = 
        AvaloniaProperty.Register<InteractiveContainer, bool>(nameof(InteractiveContainer), defaultValue: false);

    public bool ShowAtBottom
    {
        get => GetValue(ShowAtBottomProperty);
        set => SetValue(ShowAtBottomProperty, value );
    }
    
    public static readonly StyledProperty<bool> IsDialogOpenProperty = 
        AvaloniaProperty.Register<InteractiveContainer, bool>(nameof(InteractiveContainer), defaultValue: false);

    public bool IsDialogOpen
    {
        get => GetValue(IsDialogOpenProperty);
        set => SetValue(IsDialogOpenProperty, value );
    }
    
    public static readonly StyledProperty<bool> IsToastOpenProperty = 
        AvaloniaProperty.Register<InteractiveContainer, bool>(nameof(InteractiveContainer), defaultValue: false);

    public bool IsToastOpen
    {
        get => GetValue(IsToastOpenProperty);
        set => SetValue(IsToastOpenProperty, value );
    }
    
    public static readonly StyledProperty<Control> DialogContentProperty =
        AvaloniaProperty.Register<InteractiveContainer, Control>(nameof(InteractiveContainer), defaultValue: new Grid());

    public Control DialogContent
    {
        get => GetValue(DialogContentProperty);
        set => SetValue(DialogContentProperty, value );
    }
    
    public static readonly StyledProperty<Control> ToastContentProperty = 
        AvaloniaProperty.Register<InteractiveContainer, Control>(nameof(InteractiveContainer), defaultValue: new Grid());

    public Control ToastContent
    {
        get => GetValue(ToastContentProperty);
        set => SetValue(ToastContentProperty, value );
    }

    private static InteractiveContainer? GetInteractiveContainerInstance()
    {
        var lifetime = Application.Current?.ApplicationLifetime;
        if (lifetime != null)
        {
            var singleViewApp = lifetime as ISingleViewApplicationLifetime;
            var mainView = singleViewApp?.MainView;
            var container = mainView?.GetVisualDescendants().OfType<InteractiveContainer>().FirstOrDefault();
            if (container != null) return container;

            var desktopApp = lifetime as IClassicDesktopStyleApplicationLifetime;
            var mainWindow = desktopApp?.MainWindow;
            container = mainWindow?.GetVisualDescendants().OfType<InteractiveContainer>().FirstOrDefault();
            if (container != null) return container;
        }

        return null;
    }

    public static void ShowToast(Control Message, int seconds)
    {
        var container = GetInteractiveContainerInstance();
        if (container == null) return;
        container.ToastContent = Message;
        container.IsToastOpen = true;
        Task.Run(() =>
        {
            Thread.Sleep(seconds * 1000);
            Dispatcher.UIThread.InvokeAsync(() => { container.IsToastOpen = false; });
        });
    }

    public static void CloseDialog()
    {
        var container = GetInteractiveContainerInstance();
        if (container == null) return;
        
        container.IsDialogOpen = false;
    }

    public static void WaitUntilDialogIsClosed()
    {
        InteractiveContainer? container = null;
        Dispatcher.UIThread.InvokeAsync(() => { container = GetInteractiveContainerInstance(); });
        var flag = true;
        do
        {
            Dispatcher.UIThread.InvokeAsync(() => { flag = container == null || container.IsDialogOpen; });
            Thread.Sleep(200);
        } while (flag);
    }
    
    public static void ShowDialog(Control content, bool showAtBottom = false)
    {
        var container = GetInteractiveContainerInstance();
        if(container == null) return;
        
        container.IsDialogOpen = true;
        container.DialogContent = content;
        container.ShowAtBottom = showAtBottom;
    }
}