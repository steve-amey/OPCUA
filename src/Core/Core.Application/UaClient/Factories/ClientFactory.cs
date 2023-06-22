using DataCollectors.OPCUA.Core.Application.Examples;
using DataCollectors.OPCUA.Core.Application.Shared.Enums;
using DataCollectors.OPCUA.Core.Application.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Configuration;

namespace DataCollectors.OPCUA.Core.Application.UaClient.Factories;

internal class ClientFactory : IClientFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public ClientFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public IUAClient? Client { get; private set; }

    public async Task CreateClient()
    {
        // The application name and config file names
        var applicationName = "ConsoleReferenceClient";
        var configSectionName = "DataCollectors.OPCUA";

        // command line options
        string username = null;
        string userpassword = null;
        bool renewCertificate = false;
        string password = null;

        // Define the Client application
        var passwordProvider = new CertificatePasswordProvider(password);
        var application = new ApplicationInstance
        {
            ApplicationName = applicationName,
            ApplicationType = ApplicationType.Client,
            ConfigSectionName = configSectionName,
            CertificatePasswordProvider = passwordProvider
        };

        // load the application configuration.
        var config = await application.LoadApplicationConfiguration(silent: false);

        // setup the logging
        var logger = _loggerFactory.CreateLogger("OPCUA.Client");
        Utils.SetLogger(logger);

        // delete old certificate
        if (renewCertificate)
        {
            await application.DeleteApplicationInstanceCertificate().ConfigureAwait(false);
        }

        // check the application certificate.
        bool haveAppCertificate = await application.CheckApplicationInstanceCertificate(false, minimumKeySize: 0).ConfigureAwait(false);
        if (!haveAppCertificate)
        {
            throw new ErrorExitException("Application instance certificate invalid!", ExitCode.ErrorCertificate);
        }

        var clientLogger = _loggerFactory.CreateLogger<UAClient>();
        var uaClient = new UAClient(application.ApplicationConfiguration, clientLogger, ClientBase.ValidateResponse)
        {
            AutoAccept = true
        };

        // set user identity
        if (!string.IsNullOrWhiteSpace(username))
        {
            uaClient.UserIdentity = new UserIdentity(username, userpassword ?? string.Empty);
        }

        Client = uaClient;
    }
}