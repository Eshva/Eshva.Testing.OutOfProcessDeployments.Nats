using System.Collections.Immutable;

namespace Eshva.Testing.OutOfProcessDeployments.Nats;

public partial class NatsServerDeployment {
  /// <summary>
  /// NATS server deployment configuration.
  /// </summary>
  /// <param name="Name">Deployment name.</param>
  /// <param name="ImageTag">Docker image tag used for deployment configuration.</param>
  /// <param name="ContainerName">Deployment configuration name.</param>
  /// <param name="HostNetworkClientPort">Host network NATS server client port number.</param>
  /// <param name="HostNetworkHttpManagementPort">Host network NATS server management port number.</param>
  /// <param name="ShouldEnableJetStream">Should JetStream be enabled.</param>
  /// <param name="ShouldEnableDebugOutput">Should server debug output be enabled.</param>
  /// <param name="ShouldEnableTraceOutput">Should server trace output be enabled.</param>
  /// <param name="Buckets">List of pre-created buckets.</param>
  public readonly record struct Configuration(
    string Name,
    string ImageTag,
    string ContainerName,
    ushort HostNetworkClientPort,
    ushort? HostNetworkHttpManagementPort,
    bool ShouldEnableJetStream,
    bool ShouldEnableDebugOutput,
    bool ShouldEnableTraceOutput,
    ImmutableArray<ObjectStoreBucket> Buckets) {
    /// <summary>
    /// Set docker image tag.
    /// </summary>
    /// <param name="imageTag">Docker image tag.</param>
    /// <returns>
    /// Deployment configuration.
    /// </returns>
    public Configuration FromImageTag(string imageTag) =>
      this with { ImageTag = imageTag };

    /// <summary>
    /// Set deployment docker container name.
    /// </summary>
    /// <param name="containerName">Docker container name.</param>
    /// <returns>
    /// Deployment configuration.
    /// </returns>
    public Configuration WithContainerName(string containerName) =>
      this with { ContainerName = containerName };

    /// <summary>
    /// Set host network NATS server client port number.
    /// </summary>
    /// <param name="hostNetworkClientPort">Host network NATS server client port number.</param>
    /// <returns>
    /// Deployment configuration.
    /// </returns>
    public Configuration WithHostNetworkClientPort(ushort hostNetworkClientPort) =>
      this with { HostNetworkClientPort = hostNetworkClientPort };

    /// <summary>
    /// Set host network NATS server management port number.
    /// </summary>
    /// <param name="hostNetworkHttpManagementPort">Host network NATS server management port number.</param>
    /// <returns>
    /// Deployment configuration.
    /// </returns>
    public Configuration WithHostNetworkHttpManagementPort(ushort hostNetworkHttpManagementPort) =>
      this with { HostNetworkHttpManagementPort = hostNetworkHttpManagementPort };

    /// <summary>
    /// Enable JetStream.
    /// </summary>
    /// <returns>
    /// Deployment configuration.
    /// </returns>
    public Configuration EnabledJetStream() =>
      this with { ShouldEnableJetStream = true };

    /// <summary>
    /// Enable server debug output.
    /// </summary>
    /// <returns>
    /// Deployment configuration.
    /// </returns>
    public Configuration EnableDebugOutput() =>
      this with { ShouldEnableDebugOutput = true };

    /// <summary>
    /// Enable server trace output.
    /// </summary>
    /// <returns>
    /// Deployment configuration.
    /// </returns>
    public Configuration EnableTraceOutput() =>
      this with { ShouldEnableTraceOutput = true };

    /// <summary>
    /// Add bucket configuration that should be pre-created after deployment starts.
    /// </summary>
    /// <param name="bucketSettings">Bucket configuration.</param>
    /// <returns>
    /// Deployment configuration.
    /// </returns>
    public Configuration CreateBucket(ObjectStoreBucket bucketSettings) =>
      this with {
        Buckets = !Buckets.IsDefault
          ? Buckets.Add(bucketSettings)
          : [bucketSettings]
      };
  }
}
