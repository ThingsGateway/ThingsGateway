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
