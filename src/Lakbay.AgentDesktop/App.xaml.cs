using System.Windows;
using Lakbay.AgentDesktop.Services;
using Lakbay.AgentDesktop.ViewModels;
using Microsoft.Extensions.Configuration;
using Unity;

namespace Lakbay.AgentDesktop;

/// <summary>
/// The Unity composition root (ADR-0022) - the one place in this app that builds the
/// container and registers every type. Nothing outside this file should call
/// <c>new</c> on a service or ViewModel that has dependencies of its own.
/// </summary>
public partial class App : Application
{
    private IUnityContainer? _container;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var options = new AgentDesktopOptions();
        configuration.GetSection(AgentDesktopOptions.SectionName).Bind(options);

        _container = new UnityContainer();
        _container.RegisterInstance(options);

        // Default to the fake client so the app is fully demoable standalone (ADR-0021).
        // Swap the line below to AgentOpsHttpClient to talk to a real running
        // Lakbay.AgentOps - see README "Switching to the real backend".
        _container.RegisterType<IAgentOpsClient, FakeAgentOpsClient>();
        // _container.RegisterType<IAgentOpsClient, AgentOpsHttpClient>();

        _container.RegisterType<ITelephonyBridgeClient, TelephonyBridgeClient>();

        _container.RegisterInstance<INavigationService>(new DialogNavigationService(CreateDialogWindow));

        _container.RegisterType<IncomingCallViewModel>();
        _container.RegisterType<AvailabilityViewModel>();
        _container.RegisterType<MainViewModel>();

        var mainViewModel = _container.Resolve<MainViewModel>();
        var mainWindow = new MainWindow { DataContext = mainViewModel };
        MainWindow = mainWindow;
        mainWindow.Show();

        _ = mainViewModel.ConnectTelephonyBridgeAsync();
    }

    /// <summary>
    /// The single ViewModel-type -> Window mapping this app needs. Grows into a small
    /// dictionary/factory registry if a second dialog type is ever added - not worth
    /// that abstraction for one dialog today.
    /// </summary>
    private Window CreateDialogWindow(Type viewModelType, object viewModel)
    {
        if (viewModelType == typeof(BookingConfirmationViewModel))
        {
            return new BookingConfirmationWindow();
        }

        throw new NotSupportedException($"No dialog window registered for view model type '{viewModelType.Name}'.");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _container?.Dispose();
        base.OnExit(e);
    }
}
