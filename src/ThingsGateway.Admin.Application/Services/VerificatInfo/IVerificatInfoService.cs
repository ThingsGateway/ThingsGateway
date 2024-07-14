namespace ThingsGateway.Admin.Application;

public interface IVerificatInfoService
{
    void Add(VerificatInfo verificatInfo);

    void Delete(long Id);

    void Delete(List<long> ids);

    List<long>? GetClientIdListByUserId(long userId);

    List<long>? GetIdListByUserId(long userId);

    List<VerificatInfo>? GetListByIds(List<long> ids);

    List<VerificatInfo>? GetListByUserId(long userId);

    List<VerificatInfo>? GetListByUserIds(List<long> userIds);

    VerificatInfo GetOne(long id);

    void RemoveAllClientId();

    void Update(VerificatInfo verificatInfo);
}
