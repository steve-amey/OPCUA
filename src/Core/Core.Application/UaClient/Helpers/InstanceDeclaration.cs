using Opc.Ua;

namespace DataCollectors.OPCUA.Core.Application.UaClient.Helpers;

/// <summary>
/// Stores an instance declaration fetched from the server.
/// </summary>
public class InstanceDeclaration
{
    /// <summary>
    /// The type that the declaration belongs to.
    /// </summary>
    public NodeId RootTypeId;

    /// <summary>
    /// The browse path to the instance declaration.
    /// </summary>
    public QualifiedNameCollection BrowsePath;

    /// <summary>
    /// The browse path to the instance declaration.
    /// </summary>
    public string BrowsePathDisplayText;

    /// <summary>
    /// A localized path to the instance declaration.
    /// </summary>
    public string DisplayPath;

    /// <summary>
    /// The node id for the instance declaration.
    /// </summary>
    public NodeId NodeId;

    /// <summary>
    /// The node class of the instance declaration.
    /// </summary>
    public NodeClass NodeClass;

    /// <summary>
    /// The browse name for the instance declaration.
    /// </summary>
    public QualifiedName BrowseName;

    /// <summary>
    /// The display name for the instance declaration.
    /// </summary>
    public string DisplayName;

    /// <summary>
    /// The description for the instance declaration.
    /// </summary>
    public string Description;

    /// <summary>
    /// The modelling rule for the instance declaration (i.e. Mandatory or Optional).
    /// </summary>
    public NodeId ModellingRule;

    /// <summary>
    /// The data type for the instance declaration.
    /// </summary>
    public NodeId DataType;

    /// <summary>
    /// The value rank for the instance declaration.
    /// </summary>
    public int ValueRank;

    /// <summary>
    /// The built-in type parent for the data type.
    /// </summary>
    public BuiltInType BuiltInType;

    /// <summary>
    /// A localized name for the data type.
    /// </summary>
    public string DataTypeDisplayText;

    /// <summary>
    /// An instance declaration that has been overridden by the current instance.
    /// </summary>
    public InstanceDeclaration OverriddenDeclaration;
}
