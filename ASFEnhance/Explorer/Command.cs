using ArchiSteamFarm.Core;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Steam;

namespace ASFEnhance.Explorer;

internal static class Command
{
    /// <summary>
    /// 浏览探索队列
    /// </summary>
    /// <param name="bot"></param>
    /// <returns></returns>
    internal static Task<string> ResponseExploreDiscoveryQueue(Bot bot)
    {
        if (!bot.IsConnectedAndLoggedOn)
        {
            return Task.FromResult(bot.FormatBotResponse(Strings.BotNotConnected));
        }

        var steamSaleEvent = Type.GetType("ArchiSteamFarm.Steam.Integration.SteamSaleEvent,ArchiSteamFarm");

        if (steamSaleEvent == null)
        {
            return Task.FromResult(bot.FormatBotResponse(Langs.SteamSaleEventIsNull));
        }

        var steamSaleEventCls = bot.GetPrivateField("SteamSaleEvent", steamSaleEvent);

        if (steamSaleEventCls == null)
        {
            return Task.FromResult(bot.FormatBotResponse(Langs.SteamSaleEventIsNull));
        }

        var saleEventTimer = steamSaleEventCls.GetPrivateField<Timer>("SaleEventTimer");

        saleEventTimer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromHours(8.1));

        return Task.FromResult(bot.FormatBotResponse(Langs.ExplorerStart));
    }

    /// <summary>
    /// 浏览探索队列
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="semaphore"></param>
    /// <returns></returns>
    internal static async Task<string?> ResponseExploreDiscoveryQueue(Bot bot, SemaphoreSlim semaphore)
    {
        if (!bot.IsConnectedAndLoggedOn)
        {
            return bot.FormatBotResponse(Strings.BotNotConnected);
        }

        await semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            var steamSaleEvent = Type.GetType("ArchiSteamFarm.Steam.Integration.SteamSaleEvent,ArchiSteamFarm");
            if (steamSaleEvent == null)
            {
                return bot.FormatBotResponse(Langs.SteamSaleEventIsNull);
            }
            var steamSaleEventCls = bot.GetPrivateField("SteamSaleEvent", steamSaleEvent);
            var saleEventTimer = steamSaleEventCls.GetPrivateField<Timer>("SaleEventTimer");
            saleEventTimer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromHours(8.1));
            return bot.FormatBotResponse(Langs.ExplorerStart);
        }
        finally
        {
            await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            semaphore.Release();
        }
    }

    /// <summary>
    /// 浏览探索队列 (多个Bot)
    /// </summary>
    /// <param name="botNames"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static async Task<string?> ResponseExploreDiscoveryQueue(string botNames)
    {
        if (string.IsNullOrEmpty(botNames))
        {
            throw new ArgumentNullException(nameof(botNames));
        }

        HashSet<Bot>? bots = Bot.GetBots(botNames);

        if ((bots == null) || (bots.Count == 0))
        {
            return FormatStaticResponse(string.Format(Strings.BotNotFound, botNames));
        }

        var semaphore = new SemaphoreSlim(1);

        IList<string?> results = await Utilities.InParallel(bots.Select(bot => ResponseExploreDiscoveryQueue(bot, semaphore))).ConfigureAwait(false);

        List<string> responses = new(results.Where(result => !string.IsNullOrEmpty(result))!);

        return responses.Count > 0 ? string.Join(Environment.NewLine, responses) : null;
    }
}
