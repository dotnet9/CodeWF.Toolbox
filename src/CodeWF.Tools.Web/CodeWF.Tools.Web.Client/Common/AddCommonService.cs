namespace CodeWF.Tools.Web.Client.Common;

public static class AddCommonService
{
    public static IServiceCollection AddCodeWFService(this IServiceCollection services)
    {
        services.AddMasaBlazor(options =>
        {
            options.ConfigureSsr(ssr =>
            {
                ssr.Left = 256;
                ssr.Top = 64;
            });
            options.ConfigureTheme(theme =>
            {
                theme.Themes.Light.Primary = "#4318FF";
                theme.Themes.Light.Accent = "#4318FF";
            });
        });

        services.AddSingleton<ITranslationService, TranslationService>();
        return services;
    }
}