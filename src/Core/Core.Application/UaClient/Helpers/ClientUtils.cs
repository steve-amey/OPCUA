using System.Text;
using Opc.Ua;
using Opc.Ua.Client;

namespace DataCollectors.OPCUA.Core.Application.UaClient.Helpers;

public class ClientUtils
{
    #region DisplayText Lookup

    /// <summary>
    /// Gets the display text for the access level attribute.
    /// </summary>
    /// <param name="accessLevel">The access level.</param>
    /// <returns>The access level formatted as a string.</returns>
    public static string GetAccessLevelDisplayText(byte accessLevel)
    {
        var buffer = new StringBuilder();

        if (accessLevel == AccessLevels.None)
        {
            buffer.Append("None");
        }

        if ((accessLevel & AccessLevels.CurrentRead) == AccessLevels.CurrentRead)
        {
            buffer.Append("Read");
        }

        if ((accessLevel & AccessLevels.CurrentWrite) == AccessLevels.CurrentWrite)
        {
            if (buffer.Length > 0)
            {
                buffer.Append(" | ");
            }

            buffer.Append("Write");
        }

        if ((accessLevel & AccessLevels.HistoryRead) == AccessLevels.HistoryRead)
        {
            if (buffer.Length > 0)
            {
                buffer.Append(" | ");
            }

            buffer.Append("HistoryRead");
        }

        if ((accessLevel & AccessLevels.HistoryWrite) == AccessLevels.HistoryWrite)
        {
            if (buffer.Length > 0)
            {
                buffer.Append(" | ");
            }

            buffer.Append("HistoryWrite");
        }

        if ((accessLevel & AccessLevels.SemanticChange) == AccessLevels.SemanticChange)
        {
            if (buffer.Length > 0)
            {
                buffer.Append(" | ");
            }

            buffer.Append("SemanticChange");
        }

        return buffer.ToString();
    }

    /// <summary>
    /// Gets the display text for the event notifier attribute.
    /// </summary>
    /// <param name="eventNotifier">The event notifier.</param>
    /// <returns>The event notifier formatted as a string.</returns>
    public static string GetEventNotifierDisplayText(byte eventNotifier)
    {
        var buffer = new StringBuilder();

        if (eventNotifier == EventNotifiers.None)
        {
            buffer.Append("None");
        }

        if ((eventNotifier & EventNotifiers.SubscribeToEvents) == EventNotifiers.SubscribeToEvents)
        {
            buffer.Append("Subscribe");
        }

        if ((eventNotifier & EventNotifiers.HistoryRead) == EventNotifiers.HistoryRead)
        {
            if (buffer.Length > 0)
            {
                buffer.Append(" | ");
            }

            buffer.Append("HistoryRead");
        }

        if ((eventNotifier & EventNotifiers.HistoryWrite) == EventNotifiers.HistoryWrite)
        {
            if (buffer.Length > 0)
            {
                buffer.Append(" | ");
            }

            buffer.Append("HistoryWrite");
        }

        return buffer.ToString();
    }

    /// <summary>
    /// Gets the display text for the value rank attribute.
    /// </summary>
    /// <param name="valueRank">The value rank.</param>
    /// <returns>The value rank formatted as a string.</returns>
    public static string GetValueRankDisplayText(int valueRank)
    {
        switch (valueRank)
        {
            case ValueRanks.Any: return "Any";
            case ValueRanks.Scalar: return "Scalar";
            case ValueRanks.ScalarOrOneDimension: return "ScalarOrOneDimension";
            case ValueRanks.OneOrMoreDimensions: return "OneOrMoreDimensions";
            case ValueRanks.OneDimension: return "OneDimension";
            case ValueRanks.TwoDimensions: return "TwoDimensions";
        }

        return valueRank.ToString();
    }

    /// <summary>
    /// Gets the display text for the specified attribute.
    /// </summary>
    /// <param name="session">The currently active session.</param>
    /// <param name="attributeId">The id of the attribute.</param>
    /// <param name="value">The value of the attribute.</param>
    /// <returns>The attribute formatted as a string.</returns>
    public static string GetAttributeDisplayText(Session session, uint attributeId, Variant value)
    {
        if (value == Variant.Null)
        {
            return string.Empty;
        }

        switch (attributeId)
        {
            case Attributes.AccessLevel:
            case Attributes.UserAccessLevel:
                {
                    if (value.Value is byte field)
                    {
                        return GetAccessLevelDisplayText(field);
                    }

                    break;
                }

            case Attributes.EventNotifier:
                {
                    if (value.Value is byte field)
                    {
                        return GetEventNotifierDisplayText(field);
                    }

                    break;
                }

            case Attributes.DataType:
                {
                    return session.NodeCache.GetDisplayText(value.Value as NodeId);
                }

            case Attributes.ValueRank:
                {
                    if (value.Value is int field)
                    {
                        return GetValueRankDisplayText(field);
                    }

                    break;
                }

            case Attributes.NodeClass:
                {
                    if (value.Value is int field)
                    {
                        return ((NodeClass)field).ToString();
                    }

                    break;
                }

            case Attributes.NodeId:
                {
                    var field = value.Value as NodeId;

                    if (!NodeId.IsNull(field))
                    {
                        return field!.ToString();
                    }

                    return "Null";
                }

            case Attributes.DataTypeDefinition:
                {
                    if (value.Value is ExtensionObject field)
                    {
                        return field.ToString();
                    }

                    break;
                }
        }

        // check for byte strings.
        if (value.Value is byte[] bytes)
        {
            return Utils.ToHexString(bytes);
        }

        // use default format.
        return value.ToString();
    }

    #endregion

    #region Browse

    /// <summary>
    /// Browses the address space and returns the references found.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="nodesToBrowse">The set of browse operations to perform.</param>
    /// <param name="throwOnError">if set to <c>true</c> a exception will be thrown on an error.</param>
    /// <returns>
    /// The references found. Null if an error occurred.
    /// </returns>
    public static ReferenceDescriptionCollection? Browse(Session session, BrowseDescriptionCollection nodesToBrowse, bool throwOnError)
    {
        return Browse(session, null, nodesToBrowse, throwOnError);
    }

    /// <summary>
    /// Browses the address space and returns the references found.
    /// </summary>
    public static ReferenceDescriptionCollection? Browse(Session session, ViewDescription? view, BrowseDescriptionCollection nodesToBrowse, bool throwOnError)
    {
        try
        {
            var references = new ReferenceDescriptionCollection();
            var unprocessedOperations = new BrowseDescriptionCollection();

            while (nodesToBrowse.Count > 0)
            {
                session
                    .Browse(null, view, 0, nodesToBrowse, out BrowseResultCollection results, out DiagnosticInfoCollection diagnosticInfos);

                ClientBase.ValidateResponse(results, nodesToBrowse);
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToBrowse);

                var continuationPoints = new ByteStringCollection();

                for (int ii = 0; ii < nodesToBrowse.Count; ii++)
                {
                    // check for error.
                    if (StatusCode.IsBad(results[ii].StatusCode))
                    {
                        // this error indicates that the server does not have enough simultaneously active
                        // continuation points. This request will need to be resent after the other operations
                        // have been completed and their continuation points released.
                        if (results[ii].StatusCode == StatusCodes.BadNoContinuationPoints)
                        {
                            unprocessedOperations.Add(nodesToBrowse[ii]);
                        }

                        continue;
                    }

                    // check if all references have been fetched.
                    if (results[ii].References.Count == 0)
                    {
                        continue;
                    }

                    // save results.
                    references.AddRange(results[ii].References);

                    // check for continuation point.
                    if (results[ii].ContinuationPoint != null)
                    {
                        continuationPoints.Add(results[ii].ContinuationPoint);
                    }
                }

                // process continuation points.
                var revisedContinuationPoints = new ByteStringCollection();

                while (continuationPoints.Count > 0)
                {
                    // continue browse operation.
                    session
                        .BrowseNext(null, false, continuationPoints, out results, out diagnosticInfos);

                    ClientBase.ValidateResponse(results, continuationPoints);
                    ClientBase.ValidateDiagnosticInfos(diagnosticInfos, continuationPoints);

                    for (int ii = 0; ii < continuationPoints.Count; ii++)
                    {
                        // check for error.
                        if (StatusCode.IsBad(results[ii].StatusCode))
                        {
                            continue;
                        }

                        // check if all references have been fetched.
                        if (results[ii].References.Count == 0)
                        {
                            continue;
                        }

                        // save results.
                        references.AddRange(results[ii].References);

                        // check for continuation point.
                        if (results[ii].ContinuationPoint != null)
                        {
                            revisedContinuationPoints.Add(results[ii].ContinuationPoint);
                        }
                    }

                    // check if browsing must continue;
                    revisedContinuationPoints = continuationPoints;
                }

                // check if unprocessed results exist.
                nodesToBrowse = unprocessedOperations;
            }

            // return complete list.
            return references;
        }
        catch (Exception exception)
        {
            if (throwOnError)
            {
                throw new ServiceResultException(exception, StatusCodes.BadUnexpectedError);
            }

            return null;
        }
    }

    /// <summary>
    /// Browses the address space and returns the references found.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="nodeToBrowse">The NodeId for the starting node.</param>
    /// <param name="throwOnError">if set to <c>true</c> a exception will be thrown on an error.</param>
    /// <returns>
    /// The references found. Null if an error occurred.
    /// </returns>
    public static ReferenceDescriptionCollection? Browse(Session session, BrowseDescription nodeToBrowse, bool throwOnError)
    {
        return Browse(session, null, nodeToBrowse, throwOnError);
    }

    /// <summary>
    /// Browses the address space and returns the references found.
    /// </summary>
    public static ReferenceDescriptionCollection? Browse(Session session, ViewDescription? view, BrowseDescription nodeToBrowse, bool throwOnError)
    {
        try
        {
            var references = new ReferenceDescriptionCollection();

            // construct browse request.
            var nodesToBrowse = new BrowseDescriptionCollection { nodeToBrowse };

            // start the browse operation.
            session
                .Browse(null, view, 0, nodesToBrowse, out BrowseResultCollection results, out DiagnosticInfoCollection diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToBrowse);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToBrowse);

            do
            {
                // check for error.
                if (StatusCode.IsBad(results[0].StatusCode))
                {
                    throw new ServiceResultException(results[0].StatusCode);
                }

                // process results.
                foreach (var referenceDescriptionCollection in results[0].References)
                {
                    references.Add(referenceDescriptionCollection);
                }

                // check if all references have been fetched.
                if (results[0].References.Count == 0 || results[0].ContinuationPoint == null)
                {
                    break;
                }

                // continue browse operation.
                var continuationPoints = new ByteStringCollection { results[0].ContinuationPoint };

                session
                    .BrowseNext(null, false, continuationPoints, out results, out diagnosticInfos);

                ClientBase.ValidateResponse(results, continuationPoints);
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, continuationPoints);
            }
            while (true);

            // return complete list.
            return references;
        }
        catch (Exception exception)
        {
            if (throwOnError)
            {
                throw new ServiceResultException(exception, StatusCodes.BadUnexpectedError);
            }

            return null;
        }
    }

    /// <summary>
    /// Browses the address space and returns all of the supertypes of the specified type node.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="typeId">The NodeId for a type node in the address space.</param>
    /// <param name="throwOnError">if set to <c>true</c> a exception will be thrown on an error.</param>
    /// <returns>
    /// The references found. Null if an error occurred.
    /// </returns>
    public static ReferenceDescriptionCollection? BrowseSuperTypes(Session session, NodeId typeId, bool throwOnError)
    {
        var superTypes = new ReferenceDescriptionCollection();

        try
        {
            // find all of the children of the field.
            var nodeToBrowse = new BrowseDescription
            {
                NodeId = typeId,
                BrowseDirection = BrowseDirection.Inverse,
                ReferenceTypeId = ReferenceTypeIds.HasSubtype,
                IncludeSubtypes = false, // more efficient to use IncludeSubtypes=False when possible.
                NodeClassMask = 0, // the HasSubtype reference already restricts the targets to Types.
                ResultMask = (uint)BrowseResultMask.All
            };

            var references = Browse(session, nodeToBrowse, throwOnError);

            while (references != null && references.Count > 0)
            {
                // should never be more than one super type.
                superTypes.Add(references[0]);

                // only follow references within this server.
                if (references[0].NodeId.IsAbsolute)
                {
                    break;
                }

                // get the references for the next level up.
                nodeToBrowse.NodeId = (NodeId)references[0].NodeId;
                references = Browse(session, nodeToBrowse, throwOnError);
            }

            // return complete list.
            return superTypes;
        }
        catch (Exception exception)
        {
            if (throwOnError)
            {
                throw new ServiceResultException(exception, StatusCodes.BadUnexpectedError);
            }

            return null;
        }
    }

    /// <summary>
    /// Returns the node ids for a set of relative paths.
    /// </summary>
    /// <param name="session">An open session with the server to use.</param>
    /// <param name="startNodeId">The starting node for the relative paths.</param>
    /// <param name="namespacesUris">The namespace URIs referenced by the relative paths.</param>
    /// <param name="relativePaths">The relative paths.</param>
    /// <returns>A collection of local nodes.</returns>
    public static IList<NodeId?> TranslateBrowsePaths(
        Session session,
        NodeId startNodeId,
        NamespaceTable namespacesUris,
        params string[]? relativePaths)
    {
        // build the list of browse paths to follow by parsing the relative paths.
        var browsePaths = new BrowsePathCollection();

        if (relativePaths != null)
        {
            foreach (var relativePath in relativePaths)
            {
                var browsePath = new BrowsePath
                {
                    // The relative paths used indexes in the namespacesUris table. These must be
                    // converted to indexes used by the server. An error occurs if the relative path
                    // refers to a namespaceUri that the server does not recognize.
                    // The relative paths may refer to ReferenceType by their BrowseName. The TypeTree object
                    // allows the parser to look up the server's NodeId for the ReferenceType.
                    RelativePath = RelativePath.Parse(relativePath, session.TypeTree, namespacesUris, session.NamespaceUris),
                    StartingNode = startNodeId
                };

                browsePaths.Add(browsePath);
            }
        }

        // make the call to the server.
        var responseHeader = session
            .TranslateBrowsePathsToNodeIds(null, browsePaths, out BrowsePathResultCollection results, out DiagnosticInfoCollection diagnosticInfos);

        // ensure that the server returned valid results.
        ClientBase.ValidateResponse(results, browsePaths);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, browsePaths);

        // collect the list of node ids found.
        var nodes = new List<NodeId?>();

        foreach (var browsePathResult in results)
        {
            // check if the start node actually exists.
            if (StatusCode.IsBad(browsePathResult.StatusCode))
            {
                nodes.Add(null);
                continue;
            }

            // an empty list is returned if no node was found.
            if (browsePathResult.Targets.Count == 0)
            {
                nodes.Add(null);
                continue;
            }

            // Multiple matches are possible, however, the node that matches the type model is the
            // one we are interested in here. The rest can be ignored.
            BrowsePathTarget target = browsePathResult.Targets[0];

            if (target.RemainingPathIndex != UInt32.MaxValue)
            {
                nodes.Add(null);
                continue;
            }

            // The targetId is an ExpandedNodeId because it could be node in another server.
            // The ToNodeId function is used to convert a local NodeId stored in a ExpandedNodeId to a NodeId.
            nodes.Add(ExpandedNodeId.ToNodeId(target.TargetId, session.NamespaceUris));
        }

        // return whatever was found.
        return nodes;
    }

    #endregion

    #region Events

    /// <summary>
    /// Finds the type of the event for the notification.
    /// </summary>
    /// <param name="monitoredItem">The monitored item.</param>
    /// <param name="notification">The notification.</param>
    /// <returns>The NodeId of the EventType.</returns>
    public static NodeId? FindEventType(MonitoredItem monitoredItem, EventFieldList notification)
    {
        if (monitoredItem.Status.Filter is EventFilter filter)
        {
            for (int ii = 0; ii < filter.SelectClauses.Count; ii++)
            {
                SimpleAttributeOperand clause = filter.SelectClauses[ii];

                if (clause.BrowsePath.Count == 1 && clause.BrowsePath[0] == BrowseNames.EventType)
                {
                    return notification.EventFields[ii].Value as NodeId;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Constructs an event object from a notification.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="monitoredItem">The monitored item that produced the notification.</param>
    /// <param name="notification">The notification.</param>
    /// <param name="knownEventTypes">The known event types.</param>
    /// <param name="eventTypeMappings">Mapping between event types and known event types.</param>
    /// <returns>
    /// The event object. Null if the notification is not a valid event type.
    /// </returns>
    public static BaseEventState? ConstructEvent(
        Session session,
        MonitoredItem monitoredItem,
        EventFieldList notification,
        Dictionary<NodeId, Type> knownEventTypes,
        Dictionary<NodeId, NodeId> eventTypeMappings)
    {
        // find the event type.
        var eventTypeId = FindEventType(monitoredItem, notification);

        if (eventTypeId == null)
        {
            return null;
        }

        // look up the known event type.
        Type? knownType = null;

        if (eventTypeMappings.TryGetValue(eventTypeId, out NodeId? knownTypeId))
        {
            knownType = knownEventTypes[knownTypeId];
        }

        // try again.
        if (knownType == null)
        {
            if (knownEventTypes.TryGetValue(eventTypeId, out knownType))
            {
                knownTypeId = eventTypeId;
                eventTypeMappings.Add(eventTypeId, eventTypeId);
            }
        }

        // try mapping it to a known type.
        if (knownType == null)
        {
            // browse for the super types of the event type.
            var superTypes = BrowseSuperTypes(session, eventTypeId, false);

            // can't do anything with unknown types.
            if (superTypes == null)
            {
                return null;
            }

            // find the first super type that matches a known event type.
            foreach (var referenceDescription in superTypes)
            {
                var superTypeId = (NodeId)referenceDescription.NodeId;

                if (knownEventTypes.TryGetValue(superTypeId, out knownType))
                {
                    knownTypeId = superTypeId;
                    eventTypeMappings.Add(eventTypeId, superTypeId);
                }

                if (knownTypeId != null)
                {
                    break;
                }
            }

            // can't do anything with unknown types.
            if (knownTypeId == null)
            {
                return null;
            }
        }

        // construct the event based on the known event type.
        var e = (BaseEventState)Activator.CreateInstance(knownType, new object[] { (NodeState)null });

        // get the filter which defines the contents of the notification.
        var filter = monitoredItem.Status.Filter as EventFilter;

        // initialize the event with the values in the notification.
        e!.Update(session.SystemContext, filter.SelectClauses, notification);

        // save the original notification.
        e.Handle = notification;

        return e;
    }

    #endregion

    #region Type Model Browsing

    /// <summary>
    /// Collects the instance declarations for a type.
    /// </summary>
    public static IList<InstanceDeclaration> CollectInstanceDeclarationsForType(Session session, NodeId typeId)
    {
        return CollectInstanceDeclarationsForType(session, typeId, true);
    }

    /// <summary>
    /// Collects the instance declarations for a type.
    /// </summary>
    public static IList<InstanceDeclaration> CollectInstanceDeclarationsForType(Session session, NodeId typeId, bool includeSuperTypes)
    {
        // process the types starting from the top of the tree.
        var instances = new List<InstanceDeclaration>();
        var map = new Dictionary<string, InstanceDeclaration>();

        // get the super types.
        if (includeSuperTypes)
        {
            var superTypes = BrowseSuperTypes(session, typeId, false);

            if (superTypes != null)
            {
                for (int ii = superTypes.Count - 1; ii >= 0; ii--)
                {
                    CollectInstanceDeclarations(session, (NodeId)superTypes[ii].NodeId, null, instances, map);
                }
            }
        }

        // collect the fields for the selected type.
        CollectInstanceDeclarations(session, typeId, null, instances, map);

        // return the complete list.
        return instances;
    }

    /// <summary>
    /// Collects the fields for the instance node.
    /// </summary>
    private static void CollectInstanceDeclarations(
        Session session,
        NodeId typeId,
        InstanceDeclaration? parent,
        IList<InstanceDeclaration> instances,
        IDictionary<string, InstanceDeclaration> map)
    {
        // find the children.
        var nodeToBrowse = new BrowseDescription
        {
            NodeId = parent == null ? typeId : parent.NodeId,
            BrowseDirection = BrowseDirection.Forward,
            ReferenceTypeId = ReferenceTypeIds.HasChild,
            IncludeSubtypes = true,
            NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method),
            ResultMask = (uint)BrowseResultMask.All
        };

        // ignore any browsing errors.
        var references = Browse(session, nodeToBrowse, false);

        if (references == null)
        {
            return;
        }

        // process the children.
        var nodeIds = new List<NodeId>();
        var children = new List<InstanceDeclaration>();

        for (int ii = 0; ii < references.Count; ii++)
        {
            ReferenceDescription reference = references[ii];

            if (reference.NodeId.IsAbsolute)
            {
                continue;
            }

            // create a new declaration.
            var child = new InstanceDeclaration
            {
                RootTypeId = typeId,
                NodeId = (NodeId)reference.NodeId,
                BrowseName = reference.BrowseName,
                NodeClass = reference.NodeClass,
                DisplayName = !LocalizedText.IsNullOrEmpty(reference.DisplayName) ? reference.DisplayName.Text : reference.BrowseName.Name
            };

            if (parent != null)
            {
                child.BrowsePath = new QualifiedNameCollection(parent.BrowsePath);
                child.BrowsePathDisplayText = $"{parent.BrowsePathDisplayText}/{reference.BrowseName}";
                child.DisplayPath = $"{parent.DisplayPath}/{reference.DisplayName}";
            }
            else
            {
                child.BrowsePath = new QualifiedNameCollection();
                child.BrowsePathDisplayText = $"{reference.BrowseName}";
                child.DisplayPath = $"{reference.DisplayName}";
            }

            child.BrowsePath.Add(reference.BrowseName);

            // check if reading an overridden declaration.
            if (map.TryGetValue(child.BrowsePathDisplayText, out InstanceDeclaration? overriden))
            {
                child.OverriddenDeclaration = overriden;
            }

            map[child.BrowsePathDisplayText] = child;

            // add to list.
            instances.Add(child);

            children.Add(child);
            nodeIds.Add(child.NodeId);
        }

        // check if nothing more to do.
        if (children.Count == 0)
        {
            return;
        }

        // find the modeling rules.
        var modelingRules = FindTargetOfReference(session, nodeIds, ReferenceTypeIds.HasModellingRule, false);

        if (modelingRules != null)
        {
            for (int ii = 0; ii < nodeIds.Count; ii++)
            {
                children[ii].ModellingRule = modelingRules[ii];

                // if the modeling rule is null then the instance is not part of the type declaration.
                if (NodeId.IsNull(modelingRules[ii]))
                {
                    map.Remove(children[ii].BrowsePathDisplayText);
                }
            }
        }

        // update the descriptions.
        UpdateInstanceDescriptions(session, children, false);

        // recursively collect instance declarations for the tree below.
        for (int ii = 0; ii < children.Count; ii++)
        {
            if (!NodeId.IsNull(children[ii].ModellingRule))
            {
                instances.Add(children[ii]);
                CollectInstanceDeclarations(session, typeId, children[ii], instances, map);
            }
        }
    }

    /// <summary>
    /// Finds the targets for the specified reference.
    /// </summary>
    private static IList<NodeId?>? FindTargetOfReference(Session session, IReadOnlyList<NodeId> nodeIds, NodeId referenceTypeId, bool throwOnError)
    {
        try
        {
            // construct browse request.
            var nodesToBrowse = new BrowseDescriptionCollection();

            foreach (var nodeId in nodeIds)
            {
                var nodeToBrowse = new BrowseDescription
                {
                    NodeId = nodeId,
                    BrowseDirection = BrowseDirection.Forward,
                    ReferenceTypeId = referenceTypeId,
                    IncludeSubtypes = false,
                    NodeClassMask = 0,
                    ResultMask = (uint)BrowseResultMask.None
                };

                nodesToBrowse.Add(nodeToBrowse);
            }

            // start the browse operation.
            session
                .Browse(null, null, 1, nodesToBrowse, out BrowseResultCollection results, out DiagnosticInfoCollection diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToBrowse);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToBrowse);

            var targetIds = new List<NodeId?>();
            var continuationPoints = new ByteStringCollection();

            for (int ii = 0; ii < nodeIds.Count; ii++)
            {
                targetIds.Add(null);

                // check for error.
                if (StatusCode.IsBad(results[ii].StatusCode))
                {
                    continue;
                }

                // check for continuation point.
                if (results[ii].ContinuationPoint != null && results[ii].ContinuationPoint.Length > 0)
                {
                    continuationPoints.Add(results[ii].ContinuationPoint);
                }

                // get the node id.
                if (results[ii].References.Count > 0)
                {
                    if (NodeId.IsNull(results[ii].References[0].NodeId) || results[ii].References[0].NodeId.IsAbsolute)
                    {
                        continue;
                    }

                    targetIds[ii] = (NodeId)results[ii].References[0].NodeId;
                }
            }

            // release continuation points.
            if (continuationPoints.Count > 0)
            {
                session
                    .BrowseNext(null, true, continuationPoints, out results, out diagnosticInfos);

                ClientBase.ValidateResponse(results, nodesToBrowse);
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToBrowse);
            }

            // return complete list.
            return targetIds;
        }
        catch (Exception exception)
        {
            if (throwOnError)
            {
                throw new ServiceResultException(exception, StatusCodes.BadUnexpectedError);
            }

            return null;
        }
    }

    /// <summary>
    /// Finds the targets for the specified reference.
    /// </summary>
    private static void UpdateInstanceDescriptions(Session session, List<InstanceDeclaration> instances, bool throwOnError)
    {
        try
        {
            var nodesToRead = new ReadValueIdCollection();

            foreach (var instanceDeclaration in instances)
            {
                var nodeToRead = new ReadValueId
                {
                    NodeId = instanceDeclaration.NodeId,
                    AttributeId = Attributes.Description
                };

                nodesToRead.Add(nodeToRead);

                nodeToRead = new ReadValueId
                {
                    NodeId = instanceDeclaration.NodeId,
                    AttributeId = Attributes.DataType
                };

                nodesToRead.Add(nodeToRead);

                nodeToRead = new ReadValueId
                {
                    NodeId = instanceDeclaration.NodeId,
                    AttributeId = Attributes.ValueRank
                };

                nodesToRead.Add(nodeToRead);
            }

            // start the browse operation.
            session
                .Read(null, 0, TimestampsToReturn.Neither, nodesToRead, out DataValueCollection results, out DiagnosticInfoCollection diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

            // update the instances.
            for (int ii = 0; ii < nodesToRead.Count; ii += 3)
            {
                var instance = instances[ii / 3];

                instance.Description = results[ii].GetValue(LocalizedText.Null).Text;
                instance.DataType = results[ii + 1].GetValue(NodeId.Null);
                instance.ValueRank = results[ii + 2].GetValue(ValueRanks.Any);

                if (!NodeId.IsNull(instance.DataType))
                {
                    instance.BuiltInType = DataTypes.GetBuiltInType(instance.DataType, session.TypeTree);
                    instance.DataTypeDisplayText = session.NodeCache.GetDisplayText(instance.DataType);

                    if (instance.ValueRank >= 0)
                    {
                        instance.DataTypeDisplayText += "[]";
                    }
                }
            }
        }
        catch (Exception exception)
        {
            if (throwOnError)
            {
                throw new ServiceResultException(exception, StatusCodes.BadUnexpectedError);
            }
        }
    }
    #endregion

    #region Private Methods

    /// <summary>
    /// Collects the fields for the type.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="typeId">The type id.</param>
    /// <param name="fields">The fields.</param>
    /// <param name="fieldNodeIds">The node id for the declaration of the field.</param>
    private static void CollectFieldsForType(Session session, NodeId typeId, SimpleAttributeOperandCollection fields, List<NodeId> fieldNodeIds)
    {
        // get the super types.
        var superTypes = BrowseSuperTypes(session, typeId, false);

        if (superTypes == null)
        {
            return;
        }

        // process the types starting from the top of the tree.
        var foundNodes = new Dictionary<NodeId, QualifiedNameCollection>();
        var parentPath = new QualifiedNameCollection();

        for (int ii = superTypes.Count - 1; ii >= 0; ii--)
        {
            CollectFields(session, (NodeId)superTypes[ii].NodeId, parentPath, fields, fieldNodeIds, foundNodes);
        }

        // collect the fields for the selected type.
        CollectFields(session, typeId, parentPath, fields, fieldNodeIds, foundNodes);
    }

    /// <summary>
    /// Collects the fields for the instance.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="instanceId">The instance id.</param>
    /// <param name="fields">The fields.</param>
    /// <param name="fieldNodeIds">The node id for the declaration of the field.</param>
    private static void CollectFieldsForInstance(Session session, NodeId instanceId, SimpleAttributeOperandCollection fields, List<NodeId> fieldNodeIds)
    {
        var foundNodes = new Dictionary<NodeId, QualifiedNameCollection>();
        var parentPath = new QualifiedNameCollection();
        CollectFields(session, instanceId, parentPath, fields, fieldNodeIds, foundNodes);
    }

    /// <summary>
    /// Collects the fields for the instance node.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="nodeId">The node id.</param>
    /// <param name="parentPath">The parent path.</param>
    /// <param name="fields">The event fields.</param>
    /// <param name="fieldNodeIds">The node id for the declaration of the field.</param>
    /// <param name="foundNodes">The table of found nodes.</param>
    private static void CollectFields(
        Session session,
        NodeId nodeId,
        QualifiedNameCollection parentPath,
        SimpleAttributeOperandCollection fields,
        IList<NodeId> fieldNodeIds,
        IDictionary<NodeId, QualifiedNameCollection> foundNodes)
    {
        // find all of the children of the field.
        var nodeToBrowse = new BrowseDescription
        {
            NodeId = nodeId,
            BrowseDirection = BrowseDirection.Forward,
            ReferenceTypeId = ReferenceTypeIds.Aggregates,
            IncludeSubtypes = true,
            NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable),
            ResultMask = (uint)BrowseResultMask.All
        };

        var children = Browse(session, nodeToBrowse, false);

        if (children == null)
        {
            return;
        }

        // process the children.
        for (int ii = 0; ii < children.Count; ii++)
        {
            ReferenceDescription child = children[ii];

            if (child.NodeId.IsAbsolute)
            {
                continue;
            }

            // construct browse path.
            var browsePath = new QualifiedNameCollection(parentPath) { child.BrowseName };

            // check if the browse path is already in the list.
            int index = ContainsPath(fields, browsePath);

            if (index < 0)
            {
                var field = new SimpleAttributeOperand
                {
                    TypeDefinitionId = ObjectTypeIds.BaseEventType,
                    BrowsePath = browsePath,
                    AttributeId = (child.NodeClass == NodeClass.Variable) ? Attributes.Value : Attributes.NodeId
                };

                fields.Add(field);
                fieldNodeIds.Add((NodeId)child.NodeId);
            }

            // recursively find all of the children.
            NodeId targetId = (NodeId)child.NodeId;

            // need to guard against loops.
            if (!foundNodes.ContainsKey(targetId))
            {
                foundNodes.Add(targetId, browsePath);
                CollectFields(session, (NodeId)child.NodeId, browsePath, fields, fieldNodeIds, foundNodes);
            }
        }
    }

    /// <summary>
    /// Determines whether the specified select clause contains the browse path.
    /// </summary>
    /// <param name="selectClause">The select clause.</param>
    /// <param name="browsePath">The browse path.</param>
    /// <returns>
    /// 	<c>true</c> if the specified select clause contains path; otherwise, <c>false</c>.
    /// </returns>
    private static int ContainsPath(SimpleAttributeOperandCollection selectClause, QualifiedNameCollection browsePath)
    {
        for (int ii = 0; ii < selectClause.Count; ii++)
        {
            var field = selectClause[ii];

            if (field.BrowsePath.Count != browsePath.Count)
            {
                continue;
            }

            bool match = true;

            for (int jj = 0; jj < field.BrowsePath.Count; jj++)
            {
                if (field.BrowsePath[jj] != browsePath[jj])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                return ii;
            }
        }

        return -1;
    }

    #endregion
}
