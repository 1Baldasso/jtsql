using JsonToSql.CLI.Commands;
using JsonToSql.CLI.Commands.Settings;
using JsonToSql.CLI.Interceptors;
using JsonToSql.CLI.Providers;
using JsonToSql.CLI.Services;
using Spectre.Console.Cli;

var app = new CommandApp<ApplicationCommands>();
var conf = (IConfigurator builder) =>
{
    builder.Settings.Registrar.Register<IJsonTranspiler, JsonTranspiler>();
    builder.Settings.Registrar.Register<ISqlInitialTranspiler, SqlInitialTranspiler>();
    builder.Settings.Registrar.Register<ISqlIndexTanspiler, SqlIndexTranspiler>();
    builder.Settings.Registrar.Register<ISqlWriter, SqlWriter>();
    var instance = new SettingsProvider<CommonSettings>();
    builder.Settings.Registrar.RegisterInstance<ISettingsProvider<CommonSettings>, SettingsProvider<CommonSettings>>(instance);
    builder.Settings.Registrar.Register<ILogger, Logger>();
    builder.SetInterceptor(new SettingsCommandInterceptor(instance));
#if DEBUG
    builder.Settings.PropagateExceptions = true;
#endif
    builder.SetExceptionHandler((ex, tr) =>
    {
        var logger = (tr.Resolve(typeof(ILogger)) as ILogger);
        logger.LogError(ex);
    });
    //builder.AddCommand<ApplicationCommands>("tosql")
    //        .WithDescription("Translates a JSON value to a SQL DDL query.");
};
app.Configure(conf);
await app.RunAsync(args);