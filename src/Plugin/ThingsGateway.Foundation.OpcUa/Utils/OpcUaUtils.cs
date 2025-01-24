//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text;

namespace ThingsGateway.Foundation.OpcUa;

/// <summary>
/// 辅助类
/// </summary>
public class OpcUaUtils
{

    /// <summary>
    /// Finds the endpoint that best matches the current settings.
    /// </summary>
    /// <param name="application">The application configuration.</param>
    /// <param name="discoveryUrl">The discovery URL.</param>
    /// <param name="useSecurity">if set to <c>true</c> select an endpoint that uses security.</param>
    /// <param name="discoverTimeout">The timeout for the discover operation.</param>
    /// <returns>The best available endpoint.</returns>
    public static async Task<EndpointDescription> SelectEndpointAsync(
        ApplicationConfiguration application,
        string discoveryUrl,
        bool useSecurity,
        int discoverTimeout
        )
    {
        var uri = CoreClientUtils.GetDiscoveryUrl(discoveryUrl);
        var endpointConfiguration = EndpointConfiguration.Create();
        endpointConfiguration.OperationTimeout = discoverTimeout;

        using (var client = DiscoveryClient.Create(application, uri, endpointConfiguration))
        {
            // Connect to the server's discovery endpoint and find the available configuration.
            Uri url = new Uri(client.Endpoint.EndpointUrl);
            var endpoints = await client.GetEndpointsAsync(null).ConfigureAwait(false);
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(url, endpoints, useSecurity);

            Uri endpointUrl = Utils.ParseUri(selectedEndpoint.EndpointUrl);
            if (endpointUrl != null && endpointUrl.Scheme == uri.Scheme)
            {
                UriBuilder builder = new UriBuilder(endpointUrl);
                builder.Host = uri.DnsSafeHost;
                builder.Port = uri.Port;
                selectedEndpoint.EndpointUrl = builder.ToString();
            }

            return selectedEndpoint;
        }
    }


    /// <summary>
    /// Browses the address space and returns the references found.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="nodesToBrowse">The set of browse operations to perform.</param>
    /// <param name="throwOnError">if set to <c>true</c> a exception will be thrown on an error.</param>
    /// <returns>
    /// The references found. Null if an error occurred.
    /// </returns>
    public static ReferenceDescriptionCollection Browse(ISession session, BrowseDescriptionCollection nodesToBrowse, bool throwOnError)
    {
        try
        {
            ReferenceDescriptionCollection references = new();
            BrowseDescriptionCollection unprocessedOperations = new();

            while (nodesToBrowse.Count > 0)
            {
                // start the browse operation.

                session.Browse(
                    null,
                    null,
                    0,
                    nodesToBrowse,
                    out BrowseResultCollection results,
                    out DiagnosticInfoCollection diagnosticInfos);

                ClientBase.ValidateResponse(results, nodesToBrowse);
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToBrowse);

                ByteStringCollection continuationPoints = new();

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
                ByteStringCollection revisedContiuationPoints = new();

                while (continuationPoints.Count > 0)
                {
                    // continue browse operation.
                    session.BrowseNext(
                        null,
                        true,
                        continuationPoints,
                        out results,
                        out diagnosticInfos);

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
                            revisedContiuationPoints.Add(results[ii].ContinuationPoint);
                        }
                    }

                    // check if browsing must continue;
                    revisedContiuationPoints = continuationPoints;
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
    /// 浏览地址空间
    /// </summary>
    /// <param name="session"></param>
    /// <param name="nodesToBrowse"></param>
    /// <param name="throwOnError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ServiceResultException"></exception>
    public static async Task<ReferenceDescriptionCollection> BrowseAsync(ISession session, BrowseDescriptionCollection nodesToBrowse, bool throwOnError, CancellationToken cancellationToken = default)
    {
        try
        {
            ReferenceDescriptionCollection references = new();
            BrowseDescriptionCollection unprocessedOperations = new();

            while (nodesToBrowse.Count > 0)
            {
                // start the browse operation.

                var result = await session.BrowseAsync(
                        null,
                        null,
                        0,
                        nodesToBrowse, cancellationToken).ConfigureAwait(false);
                var results = result.Results;
                var diagnosticInfos = result.DiagnosticInfos;
                ClientBase.ValidateResponse(results, nodesToBrowse);
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToBrowse);

                var continuationPoints = PrepareBrowseNext(result.Results);

                for (int ii = 0; ii < nodesToBrowse.Count; ii++)
                {
                    // check if all references have been fetched.
                    if (results[ii].References.Count == 0)
                    {
                        continue;
                    }

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

                    // save results.
                    references.AddRange(results[ii].References);
                }

                while (continuationPoints.Any())
                {
                    // continue browse operation.
                    var nextResult = await session.BrowseNextAsync(
                          null,
                          false,
                          continuationPoints
                          , cancellationToken).ConfigureAwait(false);
                    results = nextResult.Results;
                    diagnosticInfos = nextResult.DiagnosticInfos;
                    ClientBase.ValidateResponse(results, continuationPoints);
                    ClientBase.ValidateDiagnosticInfos(diagnosticInfos, continuationPoints);

                    for (int ii = 0; ii < continuationPoints.Count; ii++)
                    {
                        // check if all references have been fetched.
                        if (results[ii].References.Count == 0)
                        {
                            continue;
                        }

                        // check for error.
                        if (StatusCode.IsBad(results[ii].StatusCode))
                        {
                            continue;
                        }

                        // save results.
                        references.AddRange(results[ii].References);
                    }

                    // check if browsing must continue;
                    continuationPoints = PrepareBrowseNext(nextResult.Results);
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
    /// 浏览地址空间
    /// </summary>
    /// <param name="session"></param>
    /// <param name="nodeToBrowse"></param>
    /// <param name="throwOnError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ServiceResultException"></exception>
    public static async Task<ReferenceDescriptionCollection> BrowseAsync(ISession session, BrowseDescription nodeToBrowse, bool throwOnError, CancellationToken cancellationToken = default)
    {
        try
        {
            ReferenceDescriptionCollection references = new();

            // construct browse request.
            BrowseDescriptionCollection nodesToBrowse = new()
            {
                nodeToBrowse
            };

            // start the browse operation.

            var result = await session.BrowseAsync(
                  null,
                  null,
                  0,
                  nodesToBrowse, cancellationToken).ConfigureAwait(false);
            var results = result.Results;
            var diagnosticInfos = result.DiagnosticInfos;
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
                for (int ii = 0; ii < results[0].References.Count; ii++)
                {
                    references.Add(results[0].References[ii]);
                }

                // check if all references have been fetched.
                if (results[0].References.Count == 0 || results[0].ContinuationPoint == null)
                {
                    break;
                }

                // continue browse operation.
                ByteStringCollection continuationPoints = new()
                {
                    results[0].ContinuationPoint
                };

                var nextResult = await session.BrowseNextAsync(
                      null,
                      false,
                      continuationPoints, cancellationToken).ConfigureAwait(false);
                results = nextResult.Results;
                diagnosticInfos = nextResult.DiagnosticInfos;
                ClientBase.ValidateResponse(results, continuationPoints);
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, continuationPoints);
            }
            while (true);

            //return complete list.
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
    /// 浏览地址空间并返回指定类型的所有节点
    /// </summary>
    /// <param name="session"></param>
    /// <param name="typeId"></param>
    /// <param name="throwOnError"></param>
    /// <returns></returns>
    /// <exception cref="ServiceResultException"></exception>
    public static async Task<ReferenceDescriptionCollection> BrowseSuperTypesAsync(ISession session, NodeId typeId, bool throwOnError)
    {
        ReferenceDescriptionCollection supertypes = new();

        try
        {
            // find all of the children of the field.
            BrowseDescription nodeToBrowse = new()
            {
                NodeId = typeId,
                BrowseDirection = BrowseDirection.Inverse,
                ReferenceTypeId = ReferenceTypeIds.HasSubtype,
                IncludeSubtypes = false, // more efficient to use IncludeSubtypes=False when possible.
                NodeClassMask = 0, // the HasSubtype reference already restricts the targets to Types.
                ResultMask = (uint)BrowseResultMask.All
            };

            ReferenceDescriptionCollection references = await BrowseAsync(session, nodeToBrowse, throwOnError).ConfigureAwait(false);

            while (references != null && references.Count > 0)
            {
                // should never be more than one supertype.
                supertypes.Add(references[0]);

                // only follow references within this server.
                if (references[0].NodeId.IsAbsolute)
                {
                    break;
                }

                // get the references for the next level up.
                nodeToBrowse.NodeId = (NodeId)references[0].NodeId;
                references = await BrowseAsync(session, nodeToBrowse, throwOnError).ConfigureAwait(false);
            }

            // return complete list.
            return supertypes;
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
    /// Collects the fields for the instance.
    /// </summary>
    public static async Task CollectFieldsForInstanceAsync(ISession session, NodeId instanceId, SimpleAttributeOperandCollection fields, List<NodeId> fieldNodeIds)
    {
        Dictionary<NodeId, QualifiedNameCollection> foundNodes = new();
        QualifiedNameCollection parentPath = new();
        await CollectFieldsAsync(session, instanceId, parentPath, fields, fieldNodeIds, foundNodes).ConfigureAwait(false);
    }

    /// <summary>
    /// Collects the fields for the type.
    /// </summary>
    public static async Task CollectFieldsForType(ISession session, NodeId typeId, SimpleAttributeOperandCollection fields, List<NodeId> fieldNodeIds)
    {
        // get the supertypes.
        ReferenceDescriptionCollection supertypes = await OpcUaUtils.BrowseSuperTypesAsync(session, typeId, false).ConfigureAwait(false);

        if (supertypes == null)
        {
            return;
        }

        // process the types starting from the top of the tree.
        Dictionary<NodeId, QualifiedNameCollection> foundNodes = new();
        QualifiedNameCollection parentPath = new();

        for (int ii = supertypes.Count - 1; ii >= 0; ii--)
        {
            await CollectFieldsAsync(session, (NodeId)supertypes[ii].NodeId, parentPath, fields, fieldNodeIds, foundNodes).ConfigureAwait(false);
        }

        // collect the fields for the selected type.
        await CollectFieldsAsync(session, typeId, parentPath, fields, fieldNodeIds, foundNodes).ConfigureAwait(false);
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
    public static async Task<BaseEventState> ConstructEventAsync(
        ISession session,
        MonitoredItem monitoredItem,
        EventFieldList notification,
        Dictionary<NodeId, Type> knownEventTypes,
        Dictionary<NodeId, NodeId> eventTypeMappings)
    {
        // find the event type.
        NodeId eventTypeId = FindEventType(monitoredItem, notification);

        if (eventTypeId == null)
        {
            return null;
        }

        // look up the known event type.
        Type knownType = null;
        if (eventTypeMappings.TryGetValue(eventTypeId, out NodeId knownTypeId))
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
            // browse for the supertypes of the event type.
            ReferenceDescriptionCollection supertypes = await OpcUaUtils.BrowseSuperTypesAsync(session, eventTypeId, false).ConfigureAwait(false);

            // can't do anything with unknown types.
            if (supertypes == null)
            {
                return null;
            }

            // find the first supertype that matches a known event type.
            for (int ii = 0; ii < supertypes.Count; ii++)
            {
                NodeId superTypeId = (NodeId)supertypes[ii].NodeId;

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
        BaseEventState e = (BaseEventState)Activator.CreateInstance(knownType, [null]);

        // get the filter which defines the contents of the notification.
        EventFilter filter = monitoredItem.Status.Filter as EventFilter;

        // initialize the event with the values in the notification.
        e.Update(session.SystemContext, filter.SelectClauses, notification);

        // save the orginal notification.
        e.Handle = notification;

        return e;
    }

    /// <summary>
    /// Discovers the servers on the local machine.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <returns>A list of server urls.</returns>
    public static IList<string> DiscoverServers(ApplicationConfiguration configuration)
    {
        List<string> serverUrls = new();

        // set a short timeout because this is happening in the drop down event.
        EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(configuration);
        endpointConfiguration.OperationTimeout = 5000;

        // Connect to the local discovery server and find the available servers.
        using (DiscoveryClient client = DiscoveryClient.Create(new Uri("opc.tcp://localhost:4840"), endpointConfiguration))
        {
            ApplicationDescriptionCollection servers = client.FindServers(null);

            // populate the drop down list with the discovery URLs for the available servers.
            for (int ii = 0; ii < servers.Count; ii++)
            {
                if (servers[ii].ApplicationType == Opc.Ua.ApplicationType.DiscoveryServer)
                {
                    continue;
                }

                for (int jj = 0; jj < servers[ii].DiscoveryUrls.Count; jj++)
                {
                    string discoveryUrl = servers[ii].DiscoveryUrls[jj];

                    // Many servers will use the '/discovery' suffix for the discovery endpoint.
                    // The URL without this prefix should be the base URL for the server.
                    if (discoveryUrl.EndsWith("/discovery"))
                    {
                        discoveryUrl = discoveryUrl.Substring(0, discoveryUrl.Length - "/discovery".Length);
                    }

                    // ensure duplicates do not get added.
                    if (!serverUrls.Contains(discoveryUrl))
                    {
                        serverUrls.Add(discoveryUrl);
                    }
                }
            }
        }

        return serverUrls;
    }

    /// <summary>
    /// Finds the type of the event for the notification.
    /// </summary>
    /// <param name="monitoredItem">The monitored item.</param>
    /// <param name="notification">The notification.</param>
    /// <returns>The NodeId of the EventType.</returns>
    public static NodeId FindEventType(MonitoredItem monitoredItem, EventFieldList notification)
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
    /// 指定的属性的显示文本。
    /// </summary>
    public static string GetAttributeDisplayText(ISession session, uint attributeId, Variant value)
    {
        if (value == Variant.Null)
        {
            return String.Empty;
        }

        switch (attributeId)
        {
            case Attributes.AccessLevel:
            case Attributes.UserAccessLevel:
                {
                    byte? field = value.Value as byte?;

                    if (field != null)
                    {
                        return GetAccessLevelDisplayText(field.Value);
                    }

                    break;
                }

            case Attributes.EventNotifier:
                {
                    byte? field = value.Value as byte?;

                    if (field != null)
                    {
                        return GetEventNotifierDisplayText(field.Value);
                    }

                    break;
                }

            case Attributes.DataType:
                {
                    return session.NodeCache.GetDisplayText(value.Value as NodeId);
                }

            case Attributes.ValueRank:
                {
                    int? field = value.Value as int?;

                    if (field != null)
                    {
                        return GetValueRankDisplayText(field.Value);
                    }

                    break;
                }

            case Attributes.NodeClass:
                {
                    int? field = value.Value as int?;

                    if (field != null)
                    {
                        return ((NodeClass)field.Value).ToString();
                    }

                    break;
                }

            case Attributes.NodeId:
                {
                    NodeId field = value.Value as NodeId;

                    if (!NodeId.IsNull(field))
                    {
                        return field.ToString();
                    }

                    return "Null";
                }
        }

        // check for byte strings.
        if (value.Value is byte[])
        {
            return Utils.ToHexString(value.Value as byte[]);
        }

        // use default format.
        return value.ToString();
    }

    /// <summary>
    /// Finds the endpoint that best matches the current settings.
    /// </summary>
    /// <param name="discoveryUrl">The discovery URL.</param>
    /// <param name="useSecurity">if set to <c>true</c> select an endpoint that uses security.</param>
    /// <returns>The best available endpoint.</returns>
    public static EndpointDescription SelectEndpoint(string discoveryUrl, bool useSecurity)
    {
        // needs to add the '/discovery' back onto non-UA TCP URLs.
        if (!discoveryUrl.StartsWith(Utils.UriSchemeOpcTcp))
        {
            if (!discoveryUrl.EndsWith("/discovery"))
            {
                discoveryUrl += "/discovery";
            }
        }

        // parse the selected URL.
        Uri uri = new(discoveryUrl);

        // set a short timeout because this is happening in the drop down event.
        EndpointConfiguration configuration = EndpointConfiguration.Create();
        configuration.OperationTimeout = 5000;

        EndpointDescription selectedEndpoint = null;

        // Connect to the server's discovery endpoint and find the available configuration.
        using (DiscoveryClient client = DiscoveryClient.Create(uri, configuration))
        {
            EndpointDescriptionCollection endpoints = client.GetEndpoints(null);

            // select the best endpoint to use based on the selected URL and the UseSecurity checkbox.
            for (int ii = 0; ii < endpoints.Count; ii++)
            {
                EndpointDescription endpoint = endpoints[ii];

                // check for a match on the URL scheme.
                if (endpoint.EndpointUrl.StartsWith(uri.Scheme))
                {
                    // check if security was requested.
                    if (useSecurity)
                    {
                        if (endpoint.SecurityMode == MessageSecurityMode.None)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (endpoint.SecurityMode != MessageSecurityMode.None)
                        {
                            continue;
                        }
                    }

                    // pick the first available endpoint by default.
                    selectedEndpoint ??= endpoint;

                    // The security level is a relative measure assigned by the server to the
                    // endpoints that it returns. Clients should always pick the highest level
                    // unless they have a reason not too.
                    if (endpoint.SecurityLevel > selectedEndpoint.SecurityLevel)
                    {
                        selectedEndpoint = endpoint;
                    }
                }
            }

            // pick the first available endpoint by default.
            if (selectedEndpoint == null && endpoints.Count > 0)
            {
                selectedEndpoint = endpoints[0];
            }
        }

        // if a server is behind a firewall it may return URLs that are not accessible to the client.
        // This problem can be avoided by assuming that the domain in the URL used to call
        // GetEndpoints can be used to access any of the endpoints. This code makes that conversion.
        // Note that the conversion only makes sense if discovery uses the same protocol as the endpoint.

        Uri endpointUrl = Utils.ParseUri(selectedEndpoint.EndpointUrl);

        if (endpointUrl != null && endpointUrl.Scheme == uri.Scheme)
        {
            UriBuilder builder = new(endpointUrl)
            {
                Host = uri.DnsSafeHost,
                Port = uri.Port
            };
            selectedEndpoint.EndpointUrl = builder.ToString();
        }

        // return the selected endpoint.
        return selectedEndpoint;
    }

    /// <summary>
    /// 返回一组相对路径的节点id
    /// </summary>
    public static async Task<List<NodeId>> TranslateBrowsePaths(
        ISession session,
        NodeId startNodeId,
        NamespaceTable namespacesUris, CancellationToken cancellationToken,
        params string[] relativePaths)
    {
        // build the list of browse paths to follow by parsing the relative paths.
        BrowsePathCollection browsePaths = new();

        if (relativePaths != null)
        {
            for (int ii = 0; ii < relativePaths.Length; ii++)
            {
                BrowsePath browsePath = new()
                {
                    RelativePath = RelativePath.Parse(
                    relativePaths[ii],
                    session.TypeTree,
                    namespacesUris,
                    session.NamespaceUris),

                    StartingNode = startNodeId
                };

                browsePaths.Add(browsePath);
            }
        }

        // make the call to the server.

        var result = await session.TranslateBrowsePathsToNodeIdsAsync(
            null,
            browsePaths,
            cancellationToken).ConfigureAwait(false);
        BrowsePathResultCollection results = result.Results;
        DiagnosticInfoCollection diagnosticInfos = result.DiagnosticInfos;
        // ensure that the server returned valid results.
        ClientBase.ValidateResponse(results, browsePaths);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, browsePaths);

        // collect the list of node ids found.
        List<NodeId> nodes = new();

        for (int ii = 0; ii < results.Count; ii++)
        {
            // check if the start node actually exists.
            if (StatusCode.IsBad(results[ii].StatusCode))
            {
                nodes.Add(null);
                continue;
            }

            // an empty list is returned if no node was found.
            if (results[ii].Targets.Count == 0)
            {
                nodes.Add(null);
                continue;
            }

            // Multiple matches are possible, however, the node that matches the type model is the
            // one we are interested in here. The rest can be ignored.
            BrowsePathTarget target = results[ii].Targets[0];

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

    /// <summary>
    /// Collects the fields for the instance node.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="nodeId">The node id.</param>
    /// <param name="parentPath">The parent path.</param>
    /// <param name="fields">The event fields.</param>
    /// <param name="fieldNodeIds">The node id for the declaration of the field.</param>
    /// <param name="foundNodes">The table of found nodes.</param>
    private static async Task CollectFieldsAsync(
        ISession session,
        NodeId nodeId,
        QualifiedNameCollection parentPath,
        SimpleAttributeOperandCollection fields,
        List<NodeId> fieldNodeIds,
        Dictionary<NodeId, QualifiedNameCollection> foundNodes)
    {
        // find all of the children of the field.
        BrowseDescription nodeToBrowse = new()
        {
            NodeId = nodeId,
            BrowseDirection = BrowseDirection.Forward,
            ReferenceTypeId = ReferenceTypeIds.Aggregates,
            IncludeSubtypes = true,
            NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable),
            ResultMask = (uint)BrowseResultMask.All
        };

        ReferenceDescriptionCollection children = await OpcUaUtils.BrowseAsync(session, nodeToBrowse, false).ConfigureAwait(false);

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
            QualifiedNameCollection browsePath = new(parentPath)
            {
                child.BrowseName
            };

            // check if the browse path is already in the list.
            int index = ContainsPath(fields, browsePath);

            if (index < 0)
            {
                SimpleAttributeOperand field = new()
                {
                    TypeDefinitionId = ObjectTypeIds.BaseEventType,
                    BrowsePath = browsePath,
                    AttributeId = (child.NodeClass == NodeClass.Variable) ? Attributes.Value : Attributes.NodeId
                };

                fields.Add(field);
                fieldNodeIds.Add((NodeId)child.NodeId);
            }

            // recusively find all of the children.
            NodeId targetId = (NodeId)child.NodeId;

            // need to guard against loops.
            if (foundNodes.TryAdd(targetId, browsePath))
            {
                await CollectFieldsAsync(session, (NodeId)child.NodeId, browsePath, fields, fieldNodeIds, foundNodes).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// 判断指定的select子句包含的浏览路径。
    /// </summary>
    private static int ContainsPath(SimpleAttributeOperandCollection selectClause, QualifiedNameCollection browsePath)
    {
        for (int ii = 0; ii < selectClause.Count; ii++)
        {
            SimpleAttributeOperand field = selectClause[ii];

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

    /// <summary>
    ///访问级别属性的显示文本。
    /// </summary>
    private static string GetAccessLevelDisplayText(byte accessLevel)
    {
        StringBuilder buffer = new();

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
    /// 事件通知属性的显示文本
    /// </summary>
    private static string GetEventNotifierDisplayText(byte eventNotifier)
    {
        StringBuilder buffer = new();

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

    private static string GetValueRankDisplayText(int valueRank)
    {
        return valueRank switch
        {
            ValueRanks.Any => "Any",
            ValueRanks.Scalar => "Scalar",
            ValueRanks.ScalarOrOneDimension => "ScalarOrOneDimension",
            ValueRanks.OneOrMoreDimensions => "OneOrMoreDimensions",
            ValueRanks.OneDimension => "OneDimension",
            ValueRanks.TwoDimensions => "TwoDimensions",
            _ => valueRank.ToString(),
        };
    }

    /// <summary>
    /// Create the continuation point collection from the browse result
    /// collection for the BrowseNext service.
    /// </summary>
    /// <param name="browseResultCollection">The browse result collection to use.</param>
    /// <returns>The collection of continuation points for the BrowseNext service.</returns>
    private static ByteStringCollection PrepareBrowseNext(BrowseResultCollection browseResultCollection)
    {
        var continuationPoints = new ByteStringCollection();
        foreach (var browseResult in browseResultCollection)
        {
            if (browseResult.ContinuationPoint != null)
            {
                continuationPoints.Add(browseResult.ContinuationPoint);
            }
        }
        return continuationPoints;
    }
}
