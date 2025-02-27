﻿using System.Net;
using Downloader;
using Serilog;

namespace RdtClient.Service.Services.Downloaders;

public class InternalDownloader : IDownloader
{
    public event EventHandler<DownloadCompleteEventArgs>? DownloadComplete;
    public event EventHandler<DownloadProgressEventArgs>? DownloadProgress;

    private readonly DownloadService _downloadService;
    private readonly DownloadConfiguration _downloadConfiguration;

    private readonly String _filePath;
    private readonly String _uri;

    private readonly ILogger _logger;

    private Boolean _finished;

    public InternalDownloader(String uri, String filePath)
    {
        _logger = Log.ForContext<InternalDownloader>();

        _uri = uri;
        _filePath = filePath;

        var settingProxyServer = Settings.Get.DownloadClient.ProxyServer;

        // For all options, see https://github.com/bezzad/Downloader
        _downloadConfiguration = new DownloadConfiguration
        {
            BufferBlockSize = 1024 * 8,
            MaxTryAgainOnFailover = 5,
            RangeDownload = false,
            ClearPackageOnCompletionWithFailure = false,
            MinimumSizeOfChunking = 1024,
            ReserveStorageSpaceBeforeStartingDownload = false,
            CheckDiskSizeBeforeDownload = true,
            RequestConfiguration =
            {
                Accept = "*/*",
                UserAgent = $"rdt-client",
                ProtocolVersion = HttpVersion.Version11,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                KeepAlive = true,
                UseDefaultCredentials = false
            }
        };

        SetSettings();

        if (!String.IsNullOrWhiteSpace(settingProxyServer))
        {
            _downloadConfiguration.RequestConfiguration.Proxy = new WebProxy(new Uri(settingProxyServer), false);
        }

        _downloadService = new DownloadService(_downloadConfiguration);

        _downloadService.DownloadProgressChanged += (_, args) =>
        {
            if (DownloadProgress == null)
            {
                return;
            }

            DownloadProgress.Invoke(this,
                                     new DownloadProgressEventArgs
                                     {
                                         Speed = (Int64)args.BytesPerSecondSpeed,
                                         BytesDone = args.ReceivedBytesSize,
                                         BytesTotal = args.TotalBytesToReceive
                                     });
        };

        _downloadService.DownloadFileCompleted += (_, args) =>
        {
            String? error = null;

            if (args.Cancelled)
            {
                error = $"The download was cancelled";
            }
            else if (args.Error != null)
            {
                error = args.Error.Message;
            }

            DownloadComplete?.Invoke(this,
                                     new DownloadCompleteEventArgs
                                     {
                                         Error = error
                                     });

            _finished = true;
        };
    }

    public Task<String?> Download()
    {
        _logger.Debug($"Starting download of {_uri}, writing to path: {_filePath}");

        Task.Run(async () =>
        {
            await _downloadService.DownloadFileTaskAsync(_uri, _filePath);
        });

        Task.Run(StartTimer);

        return Task.FromResult<String?>(null);
    }

    public Task Cancel()
    {
        _logger.Debug($"Cancelling download {_uri}");

        _downloadService.CancelAsync();

        return Task.CompletedTask;
    }

    public Task Pause()
    {
        return Task.CompletedTask;
    }

    public Task Resume()
    {
        return Task.CompletedTask;
    }

    private void SetSettings()
    {
        var settingDownloadParallelCount = Settings.Get.DownloadClient.ParallelCount;

        if (settingDownloadParallelCount <= 0)
        {
            settingDownloadParallelCount = 1;
        }

        var settingDownloadChunkCount = Settings.Get.DownloadClient.ChunkCount;

        if (settingDownloadChunkCount <= 0)
        {
            settingDownloadChunkCount = 1;
        }

        var settingDownloadMaxSpeed = Settings.Get.DownloadClient.MaxSpeed;

        if (settingDownloadMaxSpeed <= 0)
        {
            settingDownloadMaxSpeed = 0;
        }

        settingDownloadMaxSpeed = settingDownloadMaxSpeed * 1024 * 1024;

        var settingDownloadTimeout = Settings.Get.DownloadClient.Timeout;

        if (settingDownloadTimeout <= 0)
        {
            settingDownloadTimeout = 1000;
        }
        
        _downloadConfiguration.ChunkCount = settingDownloadChunkCount;
        _downloadConfiguration.MaximumBytesPerSecond = settingDownloadMaxSpeed;
        _downloadConfiguration.ParallelDownload = settingDownloadChunkCount > 1;
        _downloadConfiguration.ParallelCount = settingDownloadParallelCount;
        _downloadConfiguration.Timeout = settingDownloadTimeout;
    }

    private async Task StartTimer()
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (await timer.WaitForNextTickAsync())
        {
            if (_finished)
            {
                return;
            }

            SetSettings();
        }
    }
}