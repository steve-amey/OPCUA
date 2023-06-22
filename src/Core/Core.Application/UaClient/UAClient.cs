using System.Collections;
using System.Text.Json;
using DataCollectors.OPCUA.Core.Application.Examples;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;

#pragma warning disable SA1201
#pragma warning disable SA1611

namespace DataCollectors.OPCUA.Core.Application.UaClient;

public class UAClient : IUAClient, IDisposable
{
    private readonly object _lock = new object();
    private readonly ApplicationConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly Action<IList, IList> _validateResponse;

    private SessionReconnectHandler? _reconnectHandler;
    private Session? _session;

    public UAClient(ApplicationConfiguration configuration, ILogger logger, Action<IList, IList> validateResponse)
    {
        _validateResponse = validateResponse;
        _logger = logger;
        _configuration = configuration;
        _configuration.CertificateValidator.CertificateValidation += CertificateValidation;
    }

    /// <summary>
    /// Dispose objects.
    /// </summary>
    public void Dispose()
    {
        Utils.SilentDispose(_session);
        _configuration.CertificateValidator.CertificateValidation -= CertificateValidation;
    }

    public Action<IList, IList> ValidateResponse => _validateResponse;

    /// <summary>
    /// Gets the client session.
    /// </summary>
    public ISession? Session => _session;

    /// <summary>
    /// The session keepalive interval to be used in ms.
    /// </summary>
    public int KeepAliveInterval { get; set; } = 5000;

    /// <summary>
    /// The reconnect period to be used in ms.
    /// </summary>
    public int ReconnectPeriod { get; set; } = 10000;

    /// <summary>
    /// The session lifetime.
    /// </summary>
    public uint SessionLifeTime { get; set; } = 30 * 1000;

    /// <summary>
    /// The user identity to use to connect to the server.
    /// </summary>
    public IUserIdentity UserIdentity { get; set; } = new UserIdentity();

    /// <summary>
    /// Auto accept untrusted certificates.
    /// </summary>
    public bool AutoAccept { get; set; } = false;

    /// <summary>
    /// Creates a session with the UA server
    /// </summary>
    public async Task<bool> ConnectAsync(string serverUrl, bool useSecurity = true)
    {
        if (serverUrl == null)
        {
            throw new ArgumentNullException(nameof(serverUrl));
        }

        try
        {
            if (_session is { Connected: true })
            {
                _logger.LogWarning("Session already connected!");
            }
            else
            {
                _logger.LogInformation("Connecting to... {server}", serverUrl);

                // Get the endpoint by connecting to server's discovery endpoint.
                // Try to find the first endpoint with security.
                var endpointDescription = CoreClientUtils.SelectEndpoint(_configuration, serverUrl, useSecurity);
                var endpointConfiguration = EndpointConfiguration.Create(_configuration);
                var endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                // Create the session
                var session = await Opc.Ua.Client.Session
                    .Create(_configuration, endpoint, false, false, _configuration.ApplicationName, SessionLifeTime, UserIdentity, null)
                    .ConfigureAwait(false);

                // Assign the created session
                if (session is { Connected: true })
                {
                    _session = session;

                    // override keep alive interval
                    _session.KeepAliveInterval = KeepAliveInterval;

                    // set up keep alive callback.
                    _session.KeepAlive += Session_KeepAlive;
                }

                // Session created successfully.
                _logger.LogInformation("New Session Created with SessionName = {name}", _session!.SessionName);
            }

            return true;
        }
        catch (Exception ex)
        {
            // Log Error
            _logger.LogError("Create Session Error : {error}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Disconnects the session.
    /// </summary>
    public void Disconnect()
    {
        try
        {
            if (_session != null)
            {
                _logger.LogInformation("Disconnecting...");

                lock (_lock)
                {
                    _session.KeepAlive -= Session_KeepAlive;
                    _reconnectHandler?.Dispose();
                    _reconnectHandler = null;
                }

                _session.Close();
                _session.Dispose();
                _session = null;

                _logger.LogInformation("Session Disconnected.");
            }
            else
            {
                _logger.LogWarning("Session not created!");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Disconnect Error : {error}", ex.Message);
        }
    }

    /// <summary>
    /// Handles a keep alive event from a session and triggers a reconnect if necessary.
    /// </summary>
    private void Session_KeepAlive(ISession session, KeepAliveEventArgs e)
    {
        try
        {
            // check for events from discarded sessions.
            if (!ReferenceEquals(session, _session))
            {
                return;
            }

            // start reconnect sequence on communication error.
            if (ServiceResult.IsBad(e.Status))
            {
                if (ReconnectPeriod <= 0)
                {
                    Utils.LogWarning("KeepAlive status {0}, but reconnect is disabled.", e.Status);
                    return;
                }

                lock (_lock)
                {
                    if (_reconnectHandler == null)
                    {
                        Utils.LogInfo("KeepAlive status {0}, reconnecting in {1}ms.", e.Status, ReconnectPeriod);
                        _logger.LogInformation("--- RECONNECTING {0} ---", e.Status);
                        _reconnectHandler = new SessionReconnectHandler(true);
                        _reconnectHandler.BeginReconnect(_session, ReconnectPeriod, Client_ReconnectComplete);
                    }
                    else
                    {
                        _logger.LogInformation("KeepAlive status {0}, reconnect in progress.", e.Status);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error in OnKeepAlive.");
        }
    }

    /// <summary>
    /// Called when the reconnect attempt was successful.
    /// </summary>
    private void Client_ReconnectComplete(object sender, EventArgs e)
    {
        // ignore callbacks from discarded objects.
        if (!ReferenceEquals(sender, _reconnectHandler))
        {
            return;
        }

        lock (_lock)
        {
            // if session recovered, Session property is null
            if (_reconnectHandler.Session != null)
            {
                _session = _reconnectHandler.Session as Session;
            }

            _reconnectHandler.Dispose();
            _reconnectHandler = null;
        }

        _logger.LogInformation("--- RECONNECTED ---");
    }

    /// <summary>
    /// Handles the certificate validation event.
    /// This event is triggered every time an untrusted certificate is received from the server.
    /// </summary>
    protected virtual void CertificateValidation(CertificateValidator sender, CertificateValidationEventArgs e)
    {
        bool certificateAccepted = false;

        // ****
        // Implement a custom logic to decide if the certificate should be
        // accepted or not and set certificateAccepted flag accordingly.
        // The certificate can be retrieved from the e.Certificate field
        // ***
        ServiceResult error = e.Error;

        _logger.LogError(JsonSerializer.Serialize(error));

        if (error.StatusCode == StatusCodes.BadCertificateUntrusted && AutoAccept)
        {
            certificateAccepted = true;
        }

        if (certificateAccepted)
        {
            _logger.LogInformation("Untrusted Certificate accepted. Subject = {subject}", e.Certificate.Subject);
            e.Accept = true;
        }
        else
        {
            _logger.LogInformation("Untrusted Certificate rejected. Subject = {subject}", e.Certificate.Subject);
        }
    }
}