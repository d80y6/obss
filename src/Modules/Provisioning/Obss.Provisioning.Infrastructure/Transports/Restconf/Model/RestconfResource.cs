namespace Obss.Provisioning.Infrastructure.Transports.Restconf.Model;

public enum RestconfResourceType { Datastore = 1, Operation = 2, Stream = 3 }

public sealed record RestconfQueryParams(int? Depth = null, string? Fields = null, string? WithDefaults = null);