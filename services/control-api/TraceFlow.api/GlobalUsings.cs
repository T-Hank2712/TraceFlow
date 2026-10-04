global using System.Net;
global using System.Net.Http.Headers;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.IdentityModel.Tokens.Jwt;
global using System.Text.RegularExpressions;
global using System.Threading.RateLimiting;

global using Asp.Versioning;

global using DotNetEnv;

global using FluentValidation;

global using MediatR;

global using Microsoft.AspNetCore.HttpOverrides;
global using Microsoft.AspNetCore.RateLimiting;
global using Microsoft.AspNetCore.Diagnostics;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.WebUtilities;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.OpenApi;
global using Microsoft.Extensions.Options;

global using TraceFlow.Api.Application.Common.Logs;
global using TraceFlow.Api.Application.Common.AccessControl;
global using TraceFlow.Api.Application.Common.Behaviors;
global using TraceFlow.Api.Application.Common.Exceptions;
global using TraceFlow.Api.Application.Common.Security;
global using TraceFlow.Api.Application.Common.Users;
global using TraceFlow.Api.Application.Common.Invitations;
global using TraceFlow.Api.Domain.Common.Extensions;
global using TraceFlow.Api.Domain.Constants;
global using TraceFlow.Api.Domain.Entities;
global using TraceFlow.Api.Infrastructure.Persistence;
global using TraceFlow.Api.Middleware;

global using TraceFlow.Api.Application.Auth.Commands.ChangePassword;
global using TraceFlow.Api.Application.Auth.Commands.Login;
global using TraceFlow.Api.Application.Auth.Commands.Logout;
global using TraceFlow.Api.Application.Auth.Commands.RefreshSession;
global using TraceFlow.Api.Application.Auth.Commands.Register;
global using TraceFlow.Api.Application.Auth.Queries.GetCurrentUser;

global using TraceFlow.Api.Application.Users.Commands.UpdateProfile;


global using TraceFlow.Api.Application.Workspaces.Commands.AcceptWorkspaceInvitation;
global using TraceFlow.Api.Application.Workspaces.Commands.CancelInvitation;
global using TraceFlow.Api.Application.Workspaces.Commands.ChangeMemberRole;
global using TraceFlow.Api.Application.Workspaces.Commands.CreateWorkspace;
global using TraceFlow.Api.Application.Workspaces.Commands.DeclineWorkspaceInvitation;
global using TraceFlow.Api.Application.Workspaces.Commands.DeleteWorkspace;
global using TraceFlow.Api.Application.Workspaces.Commands.InviteWorkspaceMember;
global using TraceFlow.Api.Application.Workspaces.Commands.LeaveWorkspace;
global using TraceFlow.Api.Application.Workspaces.Commands.RemoveMember;
global using TraceFlow.Api.Application.Workspaces.Commands.TransferOwnership;
global using TraceFlow.Api.Application.Workspaces.Commands.UpdateWorkspace;
global using TraceFlow.Api.Application.Workspaces.Queries.GetWorkspaceById;
global using TraceFlow.Api.Application.Workspaces.Queries.GetWorkspaces;
global using TraceFlow.Api.Application.Workspaces.Queries.InvitationInbox;
global using TraceFlow.Api.Application.Workspaces.Queries.InvitationSent;
global using TraceFlow.Api.Application.Workspaces.Queries.ListMembers;

global using TraceFlow.Api.Application.Projects.Commands.AcceptProjectInvitation;
global using TraceFlow.Api.Application.Projects.Commands.CancelProjectInvitation;
global using TraceFlow.Api.Application.Projects.Commands.ChangeProjectMemberRole;
global using TraceFlow.Api.Application.Projects.Commands.CreateProject;
global using TraceFlow.Api.Application.Projects.Commands.DeclineProjectInvitation;
global using TraceFlow.Api.Application.Projects.Commands.DeleteProject;
global using TraceFlow.Api.Application.Projects.Commands.InviteProjectMember;
global using TraceFlow.Api.Application.Projects.Commands.LeaveProject;
global using TraceFlow.Api.Application.Projects.Commands.RemoveProjectMember;
global using TraceFlow.Api.Application.Projects.Commands.UpdateProject;
global using TraceFlow.Api.Application.Projects.Queries.GetProjectById;
global using TraceFlow.Api.Application.Projects.Queries.ListProjectMembers;
global using TraceFlow.Api.Application.Projects.Queries.ListProjects;
global using TraceFlow.Api.Application.Projects.Queries.ProjectInvitationInbox;
global using TraceFlow.Api.Application.Projects.Queries.ProjectInvitationSent;

global using TraceFlow.Api.Application.TraceApplications.Commands.CreateTraceApplication;
global using TraceFlow.Api.Application.TraceApplications.Commands.DeleteTraceApplication;
global using TraceFlow.Api.Application.TraceApplications.Commands.UpdateApplication;
global using TraceFlow.Api.Application.TraceApplications.Queries.GetApplicationDetail;
global using TraceFlow.Api.Application.TraceApplications.Queries.ListTraceApplications;

global using TraceFlow.Api.Application.ApiKeys.Commands.CreateApiKey;
global using TraceFlow.Api.Application.ApiKeys.Commands.RevokeApiKey;
global using TraceFlow.Api.Application.ApiKeys.Commands.ValidateApiKey;
global using TraceFlow.Api.Application.ApiKeys.Queries.ListApiKeys;

global using TraceFlow.Api.Application.TraceLogs.Queries.SearchLogs;

global using TraceFlow.Api.Domain.Dtos.ApiKeys;
global using TraceFlow.Api.Domain.Dtos.Auth;
global using TraceFlow.Api.Domain.Dtos.Logs;
global using TraceFlow.Api.Domain.Dtos.Projects;
global using TraceFlow.Api.Domain.Dtos.TraceApplications;
global using TraceFlow.Api.Domain.Dtos.Users;
global using TraceFlow.Api.Domain.Dtos.Workspaces;

global using TraceFlow.Api.Domain.Common;

global using TraceFlow.Api.Infrastructure.Persistence.OpenSearch;
global using TraceFlow.Api.Infrastructure.Options;
global using TraceFlow.Api.Infrastructure.DependencyInjection;