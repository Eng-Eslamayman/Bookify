namespace Application.Services;
public interface IGovernorateService
{
    IEnumerable<Governorate> GetActiveGovernorates();
}