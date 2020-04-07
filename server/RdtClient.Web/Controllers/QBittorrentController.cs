﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RdtClient.Service.Models.QBittorrent;
using RdtClient.Service.Models.QBittorrent.QuickType;
using RdtClient.Service.Services;


namespace RdtClient.Web.Controllers
{
    /// <summary>
    /// This API behaves as a regular QBittorrent 4+ API
    /// Documentation is found here: https://github.com/qbittorrent/qBittorrent/wiki/Web-API-Documentation
    /// </summary>
    [ApiController]
    [Route("api/v2")]
    public class QBittorrentController : Controller
    {
        private readonly IQBittorrent _qBittorrent;

        public QBittorrentController(IQBittorrent qBittorrent)
        {
            _qBittorrent = qBittorrent;
        }

        [AllowAnonymous]
        [Route("auth/login")]
        [HttpGet]
        public async Task<ActionResult> AuthLogin([FromQuery] QBAuthLoginRequest request)
        {
            try
            {
                var result = await _qBittorrent.AuthLogin(request.UserName, request.Password);

                if (result)
                {
                    return Ok("Ok.");
                }

                return Ok("Fails.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [AllowAnonymous]
        [Route("auth/login")]
        [HttpPost]
        public async Task<ActionResult> AuthLoginPost([FromBody] QBAuthLoginRequest request)
        {
            return await AuthLogin(request);
        }

        [Authorize]
        [Route("auth/logout")]
        [HttpGet]
        [HttpPost]
        public async Task<ActionResult> AuthLogout()
        {
            try
            {
                await _qBittorrent.AuthLogout();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [Route("app/version")]
        [HttpGet]
        [HttpPost]
        public ActionResult AppVersion()
        {
            return Ok("v4.2.3");
        }

        [Authorize]
        [Route("app/webapiVersion")]
        [HttpGet]
        [HttpPost]
        public ActionResult AppWebVersion()
        {
            return Ok("2.4.1");
        }

        [Authorize]
        [Route("app/buildInfo")]
        [HttpGet]
        [HttpPost]
        public ActionResult AppBuildInfo()
        {
            var result = new AppBuildInfo
            {
                Bitness = 64,
                Boost = "1.72.0",
                Libtorrent = "1.2.5.0",
                Openssl = "1.1.1f",
                Qt = "5.13.2",
                Zlib = "1.2.11"
            };
            return Ok(result);
        }

        [Authorize]
        [Route("app/shutdown")]
        [HttpGet]
        [HttpPost]
        public ActionResult AppShutdown()
        {
            return Ok();
        }

        [Authorize]
        [Route("app/preferences")]
        [HttpGet]
        [HttpPost]
        public async Task<ActionResult<AppPreferences>> AppPreferences()
        {
            try
            {
                var result = await _qBittorrent.AppPreferences();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [Route("app/setPreferences")]
        [HttpGet]
        [HttpPost]
        public ActionResult AppSetPreferences()
        {
            return Ok();
        }

        [Authorize]
        [Route("app/defaultSavePath")]
        [HttpGet]
        [HttpPost]
        public async Task<ActionResult<AppPreferences>> AppDefaultSavePath()
        {
            try
            {
                var result = await _qBittorrent.AppDefaultSavePath();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [Authorize]
        [Route("torrents/info")]
        [HttpGet]
        [HttpPost]
        public async Task<ActionResult<IList<TorrentInfo>>> TorrentsInfo()
        {
            try
            {
                var result = await _qBittorrent.TorrentInfo();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [Authorize]
        [Route("torrents/properties")]
        [HttpGet]
        public async Task<ActionResult<IList<TorrentInfo>>> TorrentsProperties([FromQuery] QBTorrentsHashRequest request)
        {
            try
            {
                var result = await _qBittorrent.TorrentProperties(request.Hash);

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [Route("torrents/properties")]
        [HttpPost]
        public async Task<ActionResult<IList<TorrentInfo>>> TorrentsPropertiesPost([FromBody] QBTorrentsHashRequest request)
        {
            return await TorrentsProperties(request);
        }

        [Authorize]
        [Route("torrents/pause")]
        [HttpGet]
        [HttpPost]
        public ActionResult TorrentsPause()
        {
            return Ok();
        }

        [Authorize]
        [Route("torrents/resume")]
        [HttpGet]
        [HttpPost]
        public ActionResult TorrentsResume()
        {
            return Ok();
        }

        [Authorize]
        [Route("torrents/setShareLimits")]
        [HttpGet]
        [HttpPost]
        public ActionResult TorrentsSetShareLimits()
        {
            return Ok();
        }

        [Authorize]
        [Route("torrents/delete")]
        [HttpGet]
        public async Task<ActionResult> TorrentsDelete([FromQuery] QBTorrentsDeleteRequest request)
        {
            try
            {
                var hashes = request.Hashes.Split("|");

                foreach (var hash in hashes)
                {
                    await _qBittorrent.TorrentsDelete(hash, request.DeleteFiles);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [Route("torrents/delete")]
        [HttpPost]
        public async Task<ActionResult> TorrentsDeletePost([FromBody] QBTorrentsDeleteRequest request)
        {
            return await TorrentsDelete(request);
        }

        [Authorize]
        [Route("torrents/add")]
        [HttpGet]
        public async Task<ActionResult> TorrentsAdd([FromQuery] QBTorrentsAddRequest request)
        {
            try
            {
                var urls = request.Urls.Split("\n");

                foreach (var url in urls)
                {
                    await _qBittorrent.TorrentsAdd(url.Trim());
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [Route("torrents/add")]
        [HttpPost]
        public async Task<ActionResult> TorrentsAddPost([FromBody] QBTorrentsAddRequest request)
        {
            return await TorrentsAdd(request);
        }
        
        [Authorize]
        [Route("torrents/setCategory")]
        [HttpGet]
        public async Task<ActionResult> TorrentsSetCategory([FromQuery] QBTorrentsSetCategoryRequest request)
        {
            try
            {
                var hashes = request.Hashes.Split("|");

                foreach (var hash in hashes)
                {
                    await _qBittorrent.TorrentsSetCategory(hash, request.Category);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [Route("torrents/setCategory")]
        [HttpPost]
        public async Task<ActionResult> TorrentsSetCategoryPost([FromBody] QBTorrentsSetCategoryRequest request)
        {
            return await TorrentsSetCategory(request);
        }

        [Authorize]
        [Route("torrents/categories")]
        [HttpGet]
        [HttpPost]
        public async Task<ActionResult<IDictionary<String, TorrentCategory>>> TorrentsCategories()
        {
            try
            {
                var categories = await _qBittorrent.TorrentsCategories();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [Route("torrents/createCategory")]
        [HttpGet]
        [HttpPost]
        public ActionResult TorrentsCreateCategory()
        {
            return Ok();
        }

        [Authorize]
        [Route("torrents/editCategory")]
        [HttpGet]
        [HttpPost]
        public ActionResult TorrentsEditCategory()
        {
            return Ok();
        }
        
        [Authorize]
        [Route("torrents/removeCategories")]
        [HttpGet]
        [HttpPost]
        public ActionResult TorrentsRemoveCategories()
        {
            return Ok();
        }

        [Authorize]
        [Route("torrents/setForcestart")]
        [HttpGet]
        [HttpPost]
        public ActionResult TorrentsSetForceStart([FromQuery] QBTorrentsHashRequest request)
        {
            return Ok();
        }
    }

    public class QBAuthLoginRequest
    {
        public String UserName { get; set; }
        public String Password { get; set; }
    }
    
    public class QBTorrentsHashRequest
    {
        public String Hash { get; set; }
    }

    public class QBTorrentsDeleteRequest
    {
        public String Hashes { get; set; }
        public Boolean DeleteFiles { get; set; }
    }

    public class QBTorrentsAddRequest
    {
        public String Urls { get; set; }
    }

    public class QBTorrentsSetCategoryRequest
    {
        public String Hashes { get; set; }
        public String Category { get; set; }
    }

    public class QbTorrentsCreateCategoryRequest
    {
        public String Category { get; set; }
        public String SavePath { get; set; }
    }

    public class QbTorrentsRemoveCategoryRequest
    {
        public String Categories { get; set; }
    }
}