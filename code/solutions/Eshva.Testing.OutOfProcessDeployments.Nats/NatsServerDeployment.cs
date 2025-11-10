using System.Text.RegularExpressions;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using JetBrains.Annotations;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.KeyValueStore;
using NATS.Client.ObjectStore;

namespace Eshva.Testing.OutOfProcessDeployments.Nats;

/// <summary>
/// NATS server deployment.
/// </summary>
[PublicAPI]
public partial class NatsServerDeployment {
  /// <summary>
  /// Initializes a new instance of NATS server deployment.
  /// </summary>
  /// <param name="configuration">Deployment configuration.</param>
  public NatsServerDeployment(Configuration configuration) {
    _configuration = configuration;
  }

  /// <summary>
  /// NATS-connection.
  /// </summary>
  public INatsConnection Connection { get; private set; } = null!;

  /// <summary>
  /// JetStream context.
  /// </summary>
  public INatsJSContext JetStreamContext { get; private set; } = null!;

  /// <summary>
  /// Object store context.
  /// </summary>
  public INatsObjContext ObjectStoreContext { get; private set; } = null!;

  /// <summary>
  /// Key-value store context.
  /// </summary>
  public INatsKVContext KeyValueContext { get; private set; } = null!;

  /// <summary>
  /// Create a new NATS server deployment configuration with name provided.
  /// </summary>
  /// <param name="name">Deployment name.</param>
  /// <returns>
  /// Deployment configuration.
  /// </returns>
  /// <exception cref="ArgumentException">
  /// Deployment name not specified.
  /// </exception>
  public static Configuration Named(string name) {
    if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Deployment name not specified.", nameof(name));

    return new Configuration { Name = name };
  }

  /// <summary>
  /// Create deployment from configuration.
  /// </summary>
  /// <param name="configuration">Deployment configuration.</param>
  /// <returns>
  /// NATS server deployment.
  /// </returns>
  public static implicit operator NatsServerDeployment(Configuration configuration) => new(configuration);

  /// <summary>
  /// Build docker container for deployment.
  /// </summary>
  /// <returns>
  /// A task that represents the completion of building of container.
  /// </returns>
  public virtual Task Build() {
    var builder = new ContainerBuilder()
      .WithImage(_configuration.ImageTag)
      .WithName(_configuration.ContainerName)
      .WithPortBinding(_configuration.HostNetworkClientPort, NatsClientPort)
      .WithCleanUp(cleanUp: true)
      .WithAutoRemove(autoRemove: true)
      .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Server is ready"));

    builder = EnableJetStreamIfRequired(builder);
    builder = MapHttpManagementPortToHostIfRequired(builder);
    builder = EnableDebugOutputIfRequired(builder);
    builder = EnableTraceOutputIfRequired(builder);

    return Task.FromResult(_container = builder.Build());
  }

  /// <summary>
  /// Start deployment container.
  /// </summary>
  /// <exception cref="InvalidOperationException">
  /// NATS server deployment is not initialized.
  /// </exception>
  public virtual async Task Start() {
    if (_container == null) throw new InvalidOperationException("NATS server deployment is not initialized.");

    await _container.StartAsync();
    await ConnectToNats();
    await CreateBuckets();
  }

  /// <inheritdoc cref="IAsyncDisposable.DisposeAsync"/>
  public async ValueTask DisposeAsync() {
    if (_container == null) return;

    await _container.DisposeAsync();
    _container = null;
  }

  private ContainerBuilder EnableDebugOutputIfRequired(ContainerBuilder builder) =>
    _configuration.ShouldEnableDebugOutput ? builder.WithCommand("--debug") : builder;

  private ContainerBuilder EnableTraceOutputIfRequired(ContainerBuilder builder) =>
    _configuration.ShouldEnableTraceOutput ? builder.WithCommand("--trace") : builder;

  private ContainerBuilder MapHttpManagementPortToHostIfRequired(ContainerBuilder builder) {
    if (_configuration.HostNetworkHttpManagementPort.HasValue) {
      builder = builder.WithPortBinding(_configuration.HostNetworkHttpManagementPort.Value, NatsHttpManagementPort)
        .WithCommand("--http_port", NatsHttpManagementPort.ToString());
    }

    return builder;
  }

  private ContainerBuilder EnableJetStreamIfRequired(ContainerBuilder builder) =>
    _configuration.ShouldEnableJetStream ? builder.WithCommand("--jetstream") : builder;

  private async Task ConnectToNats() {
    Connection = new NatsConnection(
      new NatsOpts { Url = $"nats://localhost:{_configuration.HostNetworkClientPort}" });
    await Connection.ConnectAsync();
    JetStreamContext = new NatsJSContext(Connection);
    ObjectStoreContext = new NatsObjContext(JetStreamContext);
    KeyValueContext = new NatsKVContext(JetStreamContext);
  }

  private async Task CreateBuckets() {
    if (_configuration.Buckets.IsDefaultOrEmpty) return;

    foreach (var bucket in _configuration.Buckets) {
      var bucketName = Regex.Replace(_configuration.Name, "[^a-zA-Z0-9]", "-");
      await ObjectStoreContext.CreateObjectStoreAsync(
        new NatsObjConfig(bucketName) { MaxBytes = bucket.BucketVolumeBytes });
    }
  }

  private readonly Configuration _configuration;

  private IContainer? _container; // TODO: Add NoneContainer.

  /// <summary>
  /// NATS server container network client port.
  /// </summary>
  public const ushort NatsClientPort = 4222;

  /// <summary>
  /// NATS server container network cluster routing port.
  /// </summary>
  public const ushort NatsClusterRoutingPort = 6222;

  /// <summary>
  /// NATS server container network management port.
  /// </summary>
  public const ushort NatsHttpManagementPort = 8222;
}
