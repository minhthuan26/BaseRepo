using AutoMapper;
using CommonLibs.Options;
using CommonLibs.Repositories;
using CommonLibs.Services;
using CommonLibs.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Site.Domain.Entities;
using Site.Infrastructure.DBContext;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Site.Infrastructure.Repositories
{
    public class SiteRepository : BaseRepository<SiteRepository>
    {
        private readonly SiteDbContext _dbContext;
        private readonly string _errorPrefix = "Site_Repo_";

        public SiteRepository(IMapper mapper, ILogger<SiteRepository> logger, IConfiguration configuration, IHttpContextAccessor httpContext, SiteDbContext dbContext, IOptions<AppOptions> options, IAuditContext auditContext, IJwtTokenService jwtTokenService) : base(mapper, logger, configuration, httpContext, options, auditContext, jwtTokenService)
        {
            _dbContext = dbContext;
        }

        #region Stored Procedure
        #endregion

        #region Repository Methods

        #endregion

        #region Helpers
        
        #endregion
    }
}
