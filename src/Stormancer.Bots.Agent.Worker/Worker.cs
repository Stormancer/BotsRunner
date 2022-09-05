using Newtonsoft.Json.Linq;
using Stormancer.Plugins;
using Stormancer.Plugins.RemoteControl;
using System.Diagnostics;
using System.IO.Compression;

namespace Stormancer.Bots.Agent.Worker
{
    public class StormancerConfigurationSection
    {
        public string Endpoint { get; set; }
        public string Account { get; set; }
        public string Application { get; set; }
    }

    public class ArgumentsFormatCtx
    {
        public string AgentName { get; internal set; } = default!;
        public int RunId { get; internal set; }
    }
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {

                var section = configuration.GetRequiredSection("Stormancer").Get<StormancerConfigurationSection>();

                ClientFactory.SetConfigFactory(() =>
                {
                    var configuration = ClientConfiguration.Create(section.Endpoint, section.Account, section.Application);

                    configuration.Plugins.Add(new RemoteControlAgentPlugin());
                    configuration.Plugins.Add(new AuthenticationPlugin());
                    configuration.Logger = ConsoleLogger.Instance;
                    return configuration;

                });

                var client = ClientFactory.GetClient(0);

                var rcConfig = client.DependencyResolver.Resolve<RemoteControlConfiguration>();
                rcConfig.Id = System.Environment.MachineName;

                var users = client.DependencyResolver.ResolveOptional<UserApi>();
                var api = client.DependencyResolver.Resolve<RemoteControlAgentApi>();
                api.AddCommandHandler("shutdown", ctx =>
                {
                    _ = users.Logout();
                    return Task.CompletedTask;
                });
                api.AddCommandHandler("echo", ctx =>
                {

                    ctx.SendResult("echo", JObject.FromObject(new { msg = string.Join(' ', ctx.CommandSegments), createdOn = DateTime.UtcNow }));
                    return Task.CompletedTask;
                });
                api.AddCommandHandler("run", async ctx =>
                {
                    if (ctx.CommandSegments.Length < 4)
                    {
                        ctx.SendResult("error", JObject.FromObject(new { msg = "missing arguments, expected 'run {zip url} {nb instances} {executable path in zip file} {args..}", createdOn = DateTime.UtcNow }));
                    }

                    var client = new HttpClient();

                    var compressedFilePath = Path.GetTempFileName();
                    var commandDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                    Directory.CreateDirectory(commandDirectory);

                    try
                    {
                        {
                            using var stream = await client.GetStreamAsync(ctx.CommandSegments[1]);
                            using var fileStream = File.OpenWrite(compressedFilePath);
                            await stream.CopyToAsync(fileStream);
                        }
                        ctx.SendResult("downloaded", JObject.FromObject(new { url = ctx.CommandSegments[1], createdOn = DateTime.UtcNow })) ;
                        {
                            using var stream = File.OpenRead(compressedFilePath);
                            using var archive = new ZipArchive(stream);
                            await Task.Run(() => archive.ExtractToDirectory(commandDirectory, true));

                        }
                        File.Delete(compressedFilePath);
                        var nbInstances = int.Parse(ctx.CommandSegments[2]);

                        var tasks = Enumerable.Range(0, nbInstances).Select(async i =>
                        {
                            var args = string.Join(' ', ctx.CommandSegments.Skip(4));

                            args = SmartFormat.Smart.Format(args, new ArgumentsFormatCtx { AgentName = rcConfig.Id, RunId = i });
                            var botId = $"{rcConfig.Id}-{i}";
                            try
                            {
                                var execPath = ctx.CommandSegments[3];


                                var startinfos = new ProcessStartInfo(Path.Combine(commandDirectory, execPath), args);
                                startinfos.CreateNoWindow = true;
                                startinfos.RedirectStandardOutput = true;
                                startinfos.RedirectStandardError = true;
                                var prc = Process.Start(startinfos);
                                using var registration = ctx.CancellationToken.Register(() => prc?.Kill(true));
                                //string accumulator = "";
                                prc.OutputDataReceived += (sender, args) =>
                                {
                                   
                                    ctx.SendResult("bot.output", JObject.FromObject(new { data = args.Data, botId = botId, createdOn = DateTime.UtcNow }));
                                    

                                };

                                prc.ErrorDataReceived += (sender, args) =>
                                {
                                   
                                    ctx.SendResult("bot.error", JObject.FromObject(new { data = args.Data, botId = botId, createdOn = DateTime.UtcNow }));
                                  

                                };

                                ctx.SendResult("bot.started", JObject.FromObject(new { filename = execPath, botId, arguments = args, createdOn = DateTime.UtcNow }));
                                prc.BeginOutputReadLine();
                                prc.BeginErrorReadLine();

                                await prc.WaitForExitAsync();
                               
                            }
                            catch (Exception ex)
                            {
                                ctx.SendResult("bot.error", JObject.FromObject(new { data= ex.Message, botId , createdOn = DateTime.UtcNow}));
                            }
                            finally
                            {
                                ctx.SendResult("bot.stopped", JObject.FromObject(new { botId, createdOn = DateTime.UtcNow }));
                            }
                        });

                        await Task.WhenAll(tasks);
                    }
                    finally
                    {
                        if (Directory.Exists(commandDirectory))
                        {
                            Directory.Delete(commandDirectory, true);
                        }
                    }
                });


                _logger.LogInformation("Starting agent...");
                await api.Run();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured");
                Environment.Exit(1);
            }
        }
    }
}