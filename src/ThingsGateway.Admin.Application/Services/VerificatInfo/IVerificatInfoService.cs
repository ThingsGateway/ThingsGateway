// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

namespace ThingsGateway.Admin.Application;

public interface IVerificatInfoService
{
    void Add(VerificatInfo verificatInfo);

    void Delete(long Id);

    List<VerificatInfo>? GetListByUserId(long userId);

    VerificatInfo GetOne(long id);

    void RemoveAllClientId();

    void Update(VerificatInfo verificatInfo);

    void Delete(List<long> ids);

    List<long>? GetIdListByUserId(long userId);

    List<VerificatInfo>? GetListByUserIds(List<long> userIds);

    List<long>? GetClientIdListByUserId(long userId);

    List<VerificatInfo>? GetListByIds(List<long> ids);
}
