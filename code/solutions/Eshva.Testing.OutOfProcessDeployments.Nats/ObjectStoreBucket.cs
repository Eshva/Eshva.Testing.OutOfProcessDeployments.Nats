using JetBrains.Annotations;

namespace Eshva.Testing.OutOfProcessDeployments.Nats;

/// <summary>
/// Object store bucket configuration.
/// </summary>
/// <param name="Name">Name of the bucket.</param>
/// <param name="BucketVolumeBytes">Bucket size in bytes.</param>
[PublicAPI]
public readonly record struct ObjectStoreBucket(string Name, long? BucketVolumeBytes) {
  /// <summary>
  /// Create a bucket configuration.
  /// </summary>
  /// <param name="bucketName">Name of the bucket.</param>
  /// <returns>
  /// Bucket configuration.
  /// </returns>
  public static ObjectStoreBucket Named(string bucketName) => new() { Name = bucketName };

  /// <summary>
  /// Set bucket size in bytes.
  /// </summary>
  /// <param name="bucketVolumeBytes">Bucket size in bytes.</param>
  /// <returns>Bucket configuration.</returns>
  public ObjectStoreBucket OfSize(long bucketVolumeBytes) => this with { BucketVolumeBytes = bucketVolumeBytes };
}
