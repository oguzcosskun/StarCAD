namespace StarCAD.Common.Constants;

public static class CacheKeys
{
    public const string CitiesCacheKey = "cities:list";
    public static string DistrictsByCity(int cityId) => $"cities:{cityId}:districts";
    
    public const string CodeGroupsList = "codegroups:list";
    public static string CodesByGroup(int groupId) => $"codes:group:{groupId}";
    public static string CodesByGroupName(string groupName) => $"codes:groupname:{groupName}";
    
    public const string GasDistributionGroupsList = "gasdistributiongroups:list";
}