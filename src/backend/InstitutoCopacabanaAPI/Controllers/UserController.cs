﻿using Firebase.Auth;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using InstitutoCopacabanaAPI.Data;
using InstitutoCopacabanaAPI.Models;
using InstitutoCopacabanaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InstitutoCopacabanaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly FirestoreDb _firebaseClient;
        private readonly IFirebaseAuthProvider _firebaseAuthProvider;
        private readonly IUserService _userService;
        private readonly IPasswordService _passwordService;
        private readonly ISessionService _sessionService;

        public UserController(ContextDb contextDb, IUserService userService, IPasswordService passwordService, ISessionService sessionService, AuthConnection connection)
        {
            _firebaseClient = contextDb.GetClient();
            _userService = userService;
            _passwordService = passwordService;
            _sessionService = sessionService;
            _firebaseAuthProvider = connection.GetAuth();
        }

        [HttpGet("GetUsers")]
        public async Task<ActionResult> GetUsers()
        {
            try
            {
                var token = HttpContext.Session.GetString("_userToken");

                if (token != null) 
                {
                    var session = await _sessionService.GetConnectedUser(token);

                    if (session.UserType == "Secretary")
                    {
                        CollectionReference usersRef = _firebaseClient.Collection("users");

                        QuerySnapshot snapshot = await usersRef.GetSnapshotAsync();

                        if (snapshot.Count() == 0)
                            return NotFound("Nenhum usuário foi encontrado.");

                        List<UserModel> usersList = new List<UserModel>();

                        foreach (DocumentSnapshot document in snapshot.Documents)
                        {
                            UserModel user = document.ConvertTo<UserModel>();
                            usersList.Add(user);
                        }

                        return StatusCode(200, usersList);
                    }
                    
                    return StatusCode(403, "Este usuário não pode acessar essa funcionalidade.");
                }

                return NotFound("Nenhum usuário conectado foi encontrado.");
                

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno do servidor: " + ex.Message);
            }
        }

        [HttpGet("GetUser")]
        public async Task<ActionResult> GetUserById(string id)
        {
            try
            {
                var token = HttpContext.Session.GetString("_userToken");

                if (token != null)
                {
                    var session = await _sessionService.GetConnectedUser(token);

                    if (session.UserType == "Secretary")
                    {
                        DocumentReference docRef = _firebaseClient.Collection("users").Document(id);
                        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                        if (!snapshot.Exists)
                        {
                            return NotFound("Nenhum usuário com esse Id foi encontrado.");
                        }

                        Dictionary<string, object> user = snapshot.ToDictionary();

                        return Ok(user);
                    }

                    return StatusCode(403, "Este usuário não pode acessar essa funcionalidade.");
                }

                return NotFound("Nenhum usuário conectado foi encontrado.");

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno do servidor: " + ex.Message);
            }

        }

        [HttpPost("CreateUser")]
        public async Task<ActionResult> PostUser(UserModel user)
        {
            try
            {
                var token = HttpContext.Session.GetString("_userToken");

                if (token != null)
                {
                    var session = await _sessionService.GetConnectedUser(token);

                    if (session.UserType == "Secretary")
                    {
                        if (!ModelState.IsValid)
                            return BadRequest("Todos os campos são obrigatórios.");

                        string IdGenerate = Guid.NewGuid().ToString("N");

                        user.Id = IdGenerate;

                        if (await _userService.VerifyPostEmail(user.Email))
                        {
                            if (!_passwordService.ValidatePassword(user.Password))
                                return BadRequest("A senha não atende os padrões.");                            

                            var finalUser = await _userService.PostUser(user);

                            return StatusCode(201, finalUser);
                        }

                        return Conflict("Este e-mail já está sendo utilizado.");
                    }

                    return StatusCode(403, "Este usuário não pode acessar essa funcionalidade.");
                }

                return NotFound("Nenhum usuário conectado foi encontrado.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno do servidor: " + ex.Message);
            }
        }

        [HttpPut("UpdateUser")]
        public async Task<ActionResult> UpdateUser(UserModel user)
        {
            try
            {
                var token = HttpContext.Session.GetString("_userToken");

                if (token != null)
                {
                    var session = await _sessionService.GetConnectedUser(token);

                    if (session.UserType == "Secretary")
                    {
                        if (!ModelState.IsValid)
                            return BadRequest("Todos os campos são obrigatórios.");


                        string userId = user.Id;

                        if (await _userService.VerifyPutEmail(user.Email, user.Id))
                        {

                            if (!_passwordService.ValidatePassword(user.Password))
                                return BadRequest("A senha não atende os padrões.");                            

                            var finalUser = await _userService.PutUser(user);

                            return Ok(finalUser);
                        }

                        return Conflict("Este e-mail já está sendo utilizado.");
                    }

                    return StatusCode(403, "Este usuário não pode acessar essa funcionalidade.");
                }

                return NotFound("Nenhum usuário conectado foi encontrado.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno do servidor: " + ex.Message);
            }
        }

        [HttpDelete("DeleteUser")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            try
            {
                var token = HttpContext.Session.GetString("_userToken");

                if (token != null)
                {
                    var session = await _sessionService.GetConnectedUser(token);

                    if (session.UserType == "Secretary")
                    {
                        DocumentReference docRef = _firebaseClient.Collection("users").Document(id);

                        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                        if (!snapshot.Exists)
                            return NotFound("Usuário não encontrado.");

                        //Excluir do firestore
                        await docRef.DeleteAsync();

                        //Excluir do Authentication
                        await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.DeleteUserAsync(id);

                        snapshot = await docRef.GetSnapshotAsync();
                        if (snapshot.Exists)
                            return StatusCode(500, "Falha ao deletar o usuário.");

                        return StatusCode(204, "Usuário deletado com sucesso.");
                    }

                    return StatusCode(403, "Este usuário não pode acessar essa funcionalidade.");
                }

                return NotFound("Nenhum usuário conectado foi encontrado.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno do servidor: " + ex.Message);
            }
        }
        
    }
}
