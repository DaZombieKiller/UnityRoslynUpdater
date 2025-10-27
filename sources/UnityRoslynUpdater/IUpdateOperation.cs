namespace UnityRoslynUpdater;

internal interface IUpdateOperation
{
    Task ExecuteAsync(UpdateContext context);
}
