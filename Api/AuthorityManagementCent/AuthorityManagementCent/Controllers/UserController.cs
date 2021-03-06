﻿using AuthorityManagementCent.Dto.Common;
using AuthorityManagementCent.Dto.Request;
using AuthorityManagementCent.Dto.Response;
using AuthorityManagementCent.Filters;
using AuthorityManagementCent.Managers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthorityManagementCent.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>    
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        public UserManager _UserManager;
        public ILogger<UserController> _Logger;

        public UserController(UserManager UserController, ILogger<UserController> Logger)
        {
            this._UserManager = UserController;
            this._Logger = Logger;
        }

        /// <summary>
        /// 获取所有用户信息
        /// </summary>
        /// <param name="conditionSearch"></param>
        /// <returns></returns>
        [HttpPost("getUsersMessages")]
        [JwtTokenAuthorize]
        public async Task<PagingResponseMessage<UsersResponse>> GetUsersMessageAsync(OranizationUserRequest conditionSearch)
        {
            var users = DataBaseUser.TokenModel;
            _Logger.LogInformation($"\r\n 用户{users?.UserName ?? ""},其ID:({users?.Id ?? ""}) 获取所有用户信息:\r\n" + (conditionSearch != null ? JsonHelpers.ToJSON(conditionSearch) : ""));

            var response = new PagingResponseMessage<UsersResponse>();
            if (conditionSearch == null)
            {
                _Logger.LogInformation($"{users.UserName}获取所有用户信息,请求的参数为空。");
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "请求参数为空";
            }
            try
            {
                return await _UserManager.GettingUsers(conditionSearch);
            }
            catch (Exception el)
            {
                _Logger.LogError($"用户{users?.UserName ?? ""}({users?.Id ?? ""})获取所有用户信息报错：\r\n{el.ToString()}");
                response.Code = ResponseCodeDefines.ModelStateInvalid;
                response.Message = $"获取所有用户信息报错：{ el.Message}";
            }
            return response;
        }


        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="userRequest">基本信息</param>
        /// <returns></returns>
        [HttpPost("addUser")]
        [JwtTokenAuthorize]
        public async Task<ResponseMessage> InsertUsers(UserRequest userRequest)
        {
            var users = DataBaseUser.TokenModel;
            _Logger.LogInformation($"\r\n 用户{users?.UserName ?? ""},其ID:({users?.Id ?? ""}) 获取所有用户信息:\r\n" + (userRequest != null ? JsonHelpers.ToJSON(userRequest) : "\r\n"));
            var response = new ResponseMessage();
            if (userRequest == null)
            {
                _Logger.LogInformation($"{users.UserName}添加用户,请求的参数为空。");
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "请求参数为空";
            }
            try
            {
                return await _UserManager.InsertUserInfo(userRequest);
            }
            catch (Exception el)
            {
                _Logger.LogError($"用户{users?.UserName ?? ""}({users?.Id ?? ""})获取所有用户信息报错：\r\n{el.ToString()}");
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = $"添加用户信息失败，请联系管理员.";
            }
            return response;
        }


        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        [HttpPost("delete")]
        [JwtTokenAuthorize]
        public async Task<ResponseMessage> DeleteUsers([FromBody] List<string> userId)
        {
            var users = DataBaseUser.TokenModel;
            _Logger.LogInformation($"\r\n 用户{users?.UserName ?? ""},其ID:({users?.Id ?? ""}) 删除其用户:\r\n" + (userId != null ? JsonHelpers.ToJSON(userId) : "\r\n"));
            var response = new ResponseMessage();
            if (userId == null)
            {
                _Logger.LogInformation($"{users.UserName}添加用户,请求的参数为空。");
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = "请求参数为空";
            }
            try
            {
                return await _UserManager.DeleteUser(userId);
            }
            catch (Exception el)
            {
                _Logger.LogError($"用户{users?.UserName ?? ""}({users?.Id ?? ""}),删除用户报错：\r\n{el.ToString()}");
                response.Code = ResponseCodeDefines.NotAllow;
                response.Message = $"删除用户失败，请联系管理员.";
            }
            return response;
        }
    }
}
