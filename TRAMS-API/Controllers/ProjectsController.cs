﻿using API.Mapping;
using API.Models.D365;
using API.Models.Downstream.D365;
using API.Models.Request;
using API.Models.Upstream.Response;
using API.Repositories;
using API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    /// <summary>
    /// API controller for retrieving Academy Transfers projects from TRAMS
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(string), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public class ProjectsController : Controller
    {
        private readonly IProjectsRepository _projectsRepository;
        private readonly IAcademiesRepository _academiesRepository;
        private readonly ITrustsRepository _trustsRepository;
        private readonly IMapper<PostProjectsRequestModel, PostAcademyTransfersProjectsD365Model> _postProjectsMapper;
        private readonly IMapper<GetProjectsD365Model, GetProjectsResponseModel> _getProjectsMapper;
        private readonly IMapper<AcademyTransfersProjectAcademy, 
                                 Models.Upstream.Response.GetProjectsAcademyResponseModel> _getProjectAcademyMapper;
        private readonly IRepositoryErrorResultHandler _repositoryErrorHandler;
        private readonly IConfiguration _config;

        public ProjectsController(IProjectsRepository projectsRepository,
                                  IAcademiesRepository academiesRepository,
                                  ITrustsRepository trustsRepository,
                                  IMapper<PostProjectsRequestModel, PostAcademyTransfersProjectsD365Model> postProjectsMapper,
                                  IMapper<GetProjectsD365Model, GetProjectsResponseModel> getProjectsMapper,
                                  IMapper<AcademyTransfersProjectAcademy, Models.Upstream.Response.GetProjectsAcademyResponseModel> getProjectAcademyMapper,
                                  IRepositoryErrorResultHandler repositoryErrorHandler,
                                  IConfiguration config)
        {
            _projectsRepository = projectsRepository;
            _academiesRepository = academiesRepository;
            _trustsRepository = trustsRepository;
            _postProjectsMapper = postProjectsMapper;
            _getProjectsMapper = getProjectsMapper;
            _getProjectAcademyMapper = getProjectAcademyMapper;
            _repositoryErrorHandler = repositoryErrorHandler;
            _config = config;
        }

        /// <summary>
        /// Gets an Academy Transfers project by its ID
        /// </summary>
        /// <param name="id">The ID of the project</param>
        /// <returns></returns>
        [HttpGet]
        [Route("/projects/{id}")]
        [ProducesResponseType(typeof(GetProjectsResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProjectById(Guid id)
        {
            var getProjectRepositoryResult = await _projectsRepository.GetProjectById(id);

            if (!getProjectRepositoryResult.IsValid)
            {
                return _repositoryErrorHandler.LogAndCreateResponse(getProjectRepositoryResult);
            }

            if (getProjectRepositoryResult.Result == null)
            {
                return NotFound($"Project with id '{id}' not found");
            }

            var externalModel = _getProjectsMapper.Map(getProjectRepositoryResult.Result);

            return Ok(externalModel);
        }

        /// <summary>
        /// Gets a Project Academy entity by its id
        /// </summary>
        /// <param name="projectId">The ID of the project</param>
        /// <param name="projectAcademyId">The ID of the ProjectAcademy entity - this is the id of the custom entity, not the ID of the academy in TRAMS</param>
        /// <returns></returns>
        [HttpGet]
        [Route("/projects/{projectId}/academies/{projectAcademyId}")]
        [ProducesResponseType(typeof(GetProjectsAcademyResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProjectAcademy(Guid projectId, Guid projectAcademyId)
        {
            var getProjectResult = await _projectsRepository.GetProjectById(projectId);

            if (!getProjectResult.IsValid)
            {
                return _repositoryErrorHandler.LogAndCreateResponse(getProjectResult);
            }

            if (getProjectResult.Result == null)
            {
                return NotFound($"Project with id '{projectId}' not found");
            }

            var getProjectRepositoryResult = await _projectsRepository.GetProjectAcademyById(projectAcademyId);

            if (!getProjectRepositoryResult.IsValid)
            {
                return _repositoryErrorHandler.LogAndCreateResponse(getProjectRepositoryResult);
            }

            if (getProjectRepositoryResult.Result == null)
            {
                return NotFound($"Project Academy with id '{projectAcademyId}' not found");
            }

            var externalModel = _getProjectAcademyMapper.Map(getProjectRepositoryResult.Result);

            return Ok(externalModel);
        }

        /// <summary>
        /// API endpoint for creating a new Academy Transfers Project
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/projects/")]
        [ProducesResponseType(typeof(GetProjectsResponseModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> InsertTrust([FromBody]PostProjectsRequestModel model)
        {
            var projectAcademiesIds = model.ProjectAcademies.Select(a => a.AcademyId).ToList();
            var unprocessableEntityErrors = new List<string>();

            #region Check Referenced Entities

            foreach (var academyId in projectAcademiesIds)
            {
                var academyRepoResult = await _academiesRepository.GetAcademyById(academyId);

                if (!academyRepoResult.IsValid)
                {
                    return _repositoryErrorHandler.LogAndCreateResponse(academyRepoResult);
                }

                if (academyRepoResult.Result == null)
                {
                    unprocessableEntityErrors.Add($"No academy found with the id of: {academyId}");
                }
            }

            var allTrustIds = new List<Guid>();

            if (model.ProjectAcademies != null && model.ProjectAcademies.Any())
            {
                allTrustIds.AddRange(model.ProjectAcademies.Where(a => a.Trusts != null && a.Trusts.Any())
                                                           .SelectMany(s => s.Trusts)
                                                           .Select(s => s.TrustId));
            }

            if (model.ProjectTrusts != null && model.ProjectTrusts.Any())
            {
                allTrustIds.AddRange(model.ProjectTrusts.Select(p => p.TrustId));
            }

            if (allTrustIds.Any())
            {
                foreach (var trustId in allTrustIds.Distinct())
                {
                    var trustsRepositoryResult = await _trustsRepository.GetTrustById(trustId);

                    if (!trustsRepositoryResult.IsValid)
                    {
                        return _repositoryErrorHandler.LogAndCreateResponse(trustsRepositoryResult);
                    }

                    if (trustsRepositoryResult.Result == null)
                    {
                        unprocessableEntityErrors.Add($"No trust found with the id of: {trustId}");
                    }
                }
            }

            #endregion

            //If errors are detected with entity referencing, return an UnprocessableEntity result
            if (unprocessableEntityErrors.Any())
            {
                var error = string.Join(". ", unprocessableEntityErrors);

                return UnprocessableEntity(error);
            }

            var internalModel = _postProjectsMapper.Map(model);

            var insertProjectRepositoryResult = await _projectsRepository.InsertProject(internalModel);

            if (!insertProjectRepositoryResult.IsValid)
            {
                return _repositoryErrorHandler.LogAndCreateResponse(insertProjectRepositoryResult);
            }

            var getProjectRepositoryResult = await _projectsRepository.GetProjectById(insertProjectRepositoryResult.Result.Value);

            if (!getProjectRepositoryResult.IsValid)
            {
                return _repositoryErrorHandler.LogAndCreateResponse(getProjectRepositoryResult);
            }

            var externalModel = _getProjectsMapper.Map(getProjectRepositoryResult.Result);

            var apiBaseUrl = _config["API:Url"];

            return Created($"{apiBaseUrl}/projects/{externalModel.ProjectId}", externalModel);
        }
        
        [HttpPatch]
        [Route("/projects/{projectId}/academies/{projectAcademyId}")]
        public async Task<IActionResult> UpdateProjectAcademy(Guid projectId, Guid projectAcademyId, [FromBody]PostProjectsAcademiesModel model)
        {
            var getProjectResult = await _projectsRepository.GetProjectById(projectId);

            if (!getProjectResult.IsValid)
            {
                return _repositoryErrorHandler.LogAndCreateResponse(getProjectResult);
            }

            if (getProjectResult.Result == null)
            {
                return NotFound($"Project with id '{projectId}' not found");
            }

            var getProjectRepositoryResult = await _projectsRepository.GetProjectAcademyById(projectAcademyId);

            if (!getProjectRepositoryResult.IsValid)
            {
                return _repositoryErrorHandler.LogAndCreateResponse(getProjectRepositoryResult);
            }

            if (getProjectRepositoryResult.Result == null)
            {
                return NotFound($"Project Academy with id '{projectAcademyId}' not found");
            }

            return null;
        }
    }
}