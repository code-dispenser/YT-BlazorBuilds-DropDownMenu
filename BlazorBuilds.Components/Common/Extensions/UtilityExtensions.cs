namespace BlazorBuilds.Components.Common.Extensions;

public static class UtilityExtensions
{

    public static void WhenTrue(this bool boolValue, Action act_whenTrue)
    {
        if (boolValue == true) act_whenTrue();
    }

    public static void WhenFalse(this bool boolValue, Action act_whenFalse)
    {
        if (boolValue == false) act_whenFalse();
    }

    public static async Task WhenTrue(this bool boolValue, Func<Task> act_whenTrue)
    {
        if (boolValue == true) await act_whenTrue();
    }

    public static async ValueTask WhenTrue(this bool boolValue, Func<ValueTask> act_whenTrue)
    {
        if (boolValue == true) await act_whenTrue();
    }
    public static async ValueTask WhenTrueElse(this bool boolValue, Func<ValueTask> act_whenTrue, Func<ValueTask> act_whenFalse)
    {
        if (boolValue == true) await act_whenTrue(); else await act_whenFalse();
    }

}
