﻿using GemTracker.Shared.Dexchanges;
using GemTracker.Shared.Domain;
using GemTracker.Shared.Domain.Enums;
using GemTracker.Shared.Extensions;
using GemTracker.Shared.Services;
using NLog;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GemTracker.Agent.Jobs
{
    [DisallowConcurrentExecution]
    public class FetchDataFromUniswapJob : IJob
    {
        private static readonly Logger Logger = LogManager.GetLogger("GEM");
        private readonly IConfigurationService _configurationService;
        private readonly IUniswapService _uniswapService;
        private readonly IFileService _fileService;
        private readonly ITelegramService _telegramService;
        private readonly IEtherScanService _etherScanService;
        private readonly IEthPlorerService _ethPlorerService;
        private readonly string Dex = DexType.UNISWAP.GetDescription().ToUpperInvariant();
        public FetchDataFromUniswapJob(
            IConfigurationService configurationService,
            IUniswapService uniswapService,
            IFileService fileService,
            ITelegramService telegramService,
            IEtherScanService etherScanService,
            IEthPlorerService ethPlorerService)
        {
            _configurationService = configurationService;
            _uniswapService = uniswapService;
            _fileService = fileService;
            _telegramService = telegramService;
            _etherScanService = etherScanService;
            _ethPlorerService = ethPlorerService;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var jobConfigFileName = context.JobDetail.JobDataMap["FileName"] as string;
                var storagePath = context.JobDetail.JobDataMap["StoragePath"] as string;

                var cfg = await _configurationService.GetJobConfigAsync(jobConfigFileName);

                var uniswap = new UniDexchange(_uniswapService, _fileService, storagePath);

                var latestAll = await uniswap.FetchAllAsync();

                if (latestAll.Success)
                {
                    Logger.Info($"{Dex}|LATEST|{latestAll.ListResponse.Count()}");

                    var loadedAll = await uniswap.LoadAllAsync();

                    if (loadedAll.Success)
                    {
                        Logger.Info($"{Dex}|LOADED ALL|{loadedAll.OldList.Count()}");
                        Logger.Info($"{Dex}|LOADED ALL DELETED|{loadedAll.OldListDeleted.Count()}");
                        Logger.Info($"{Dex}|LOADED ALL ADDED|{loadedAll.OldListAdded.Count()}");

                        var recentlyDeletedAll = uniswap.CheckDeleted(loadedAll.OldList, latestAll.ListResponse, TokenActionType.DELETED);
                        var recentlyAddedAll = uniswap.CheckAdded(loadedAll.OldList, latestAll.ListResponse, TokenActionType.ADDED);

                        loadedAll.OldListDeleted.AddRange(recentlyDeletedAll);
                        loadedAll.OldListAdded.AddRange(recentlyAddedAll);

                        await _fileService.SetAsync(uniswap.StorageFilePathDeleted, loadedAll.OldListDeleted);
                        await _fileService.SetAsync(uniswap.StorageFilePathAdded, loadedAll.OldListAdded);

                        await _fileService.SetAsync(uniswap.StorageFilePath, latestAll.ListResponse);

                        if (cfg.JobConfig.Notify)
                        {
                            Logger.Info($"{Dex}|TELEGRAM|ON");

                            var telegramNotification = new UniNtf(
                                _telegramService,
                                _uniswapService,
                                _etherScanService,
                                _ethPlorerService);

                            var notifiedAboutDeleted = await telegramNotification.SendAsync(recentlyDeletedAll);

                            if (notifiedAboutDeleted.Success)
                                Logger.Info($"{Dex}|TELEGRAM|DELETED|SENT");
                            else
                                Logger.Warn($"{Dex}|TELEGRAM|DELETED|{notifiedAboutDeleted.Message}");

                            var notifiedAboutAdded = await telegramNotification.SendAsync(recentlyAddedAll);

                            if (notifiedAboutAdded.Success)
                                Logger.Info($"{Dex}|TELEGRAM|ADDED|SENT");
                            else
                                Logger.Warn($"{Dex}|TELEGRAM|ADDED|{notifiedAboutAdded.Message}");
                        }
                        else
                            Logger.Info($"{Dex}|TELEGRAM|OFF");
                    }
                    else
                        Logger.Error($"{Dex}|{loadedAll.Message}");
                }
                else
                    Logger.Error($"{Dex}|{latestAll.Message}");

                if (cfg.Success)
                    Logger.Info($"Job: {cfg.JobConfig.Label} - DONE");
            }
            catch (Exception e)
            {
                Logger.Fatal($"{e.GetFullMessage()}");
            }
        }
    }
}