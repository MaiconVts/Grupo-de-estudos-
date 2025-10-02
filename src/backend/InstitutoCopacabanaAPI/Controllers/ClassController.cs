﻿using Firebase.Auth;
using Google.Cloud.Firestore;
using InstitutoCopacabanaAPI.Data;
using InstitutoCopacabanaAPI.Models;
using InstitutoCopacabanaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstitutoCopacabanaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassController : ControllerBase
    {
        private readonly FirestoreDb _firebaseClient;
        private readonly ISessionService _sessionService;
        private readonly IClassService _classService;

        public ClassController(ContextDb contextDb, ISessionService sessionService, IClassService classService) 
        {
            _firebaseClient = contextDb.GetClient();
            _sessionService = sessionService;
            _classService = classService;
        }

        [HttpGet("GetClasses")]
        public async Task<ActionResult> GetClasses()
        {
            try
            {
                if (HttpContext.Session.GetString("_userToken") != null)
                {
                    CollectionReference classesRef = _firebaseClient.Collection("classes");

                    QuerySnapshot snapshot = await classesRef.GetSnapshotAsync();

                    if (snapshot.Count == 0)
                        return NotFound("Nenhuma classe foi encontrada.");

                    List<ClassModel> classesList = new List<ClassModel>();

                    foreach (DocumentSnapshot document in snapshot.Documents)
                    {
                        ClassModel schoolClass = document.ConvertTo<ClassModel>();
                        classesList.Add(schoolClass);
                    }

                    return StatusCode(200, classesList);
                }

                return StatusCode(403, "Nenhum usuário conectado foi encontrado.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno do servidor: " + ex.Message);
            }
        }

        [HttpGet("GetStudents")]
        public async Task<ActionResult> GetStudents(string className)
        {
            try
            {
                var token = HttpContext.Session.GetString("_userToken");

                if (token != null)
                {
                    var session = await _sessionService.GetConnectedUser(token);

                    if (session.UserType != "Student")
                    {

                        List<StudentModel> studentsList = await _classService.GetStudentsByClassName(className);

                        if (studentsList.Count == 0)
                            return NotFound("Não foi possível encontrar nenhum aluno nessa classe.");

                        return Ok(studentsList);
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

        [HttpPost("CreateClass")]
        public async Task<ActionResult> CreateClass(ClassModel schoolClass)
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

                        var classDocument = await _classService.GetClassByName(schoolClass.Name);

                        if (classDocument != null)
                            return BadRequest("Essa turma já existe.");

                        string IdGenerate = Guid.NewGuid().ToString("N");

                        schoolClass.Id = IdGenerate;

                        var finalClass = await _classService.CreateClass(schoolClass);

                        return StatusCode(201, finalClass);
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


        [HttpPut("InsertStudent")]
        public async Task<ActionResult> InsertStudentToClass(string className, string studentName)
        {
            try
            {
                var token = HttpContext.Session.GetString("_userToken");

                if (token != null)
                {
                    var session = await _sessionService.GetConnectedUser(token);

                    if (session.UserType == "Secretary")
                    {
                        var classDocument = await _classService.GetClassByName(className);

                        if (classDocument == null)
                            return NotFound("Essa turma não foi registrada.");

                        UserModel? student = await _classService.GetStudentByName(studentName);

                        if (student == null) 
                            return NotFound("Esse aluno não existe.");

                        if (student.UserType != "Student")
                            return StatusCode(403, "Apenas alunos podem ser inseridos nas turmas.");

                        var insertedStudent = await _classService.InsertStudent(classDocument.Id, student, className);

                        return StatusCode(201, "Aluno inserido com sucesso.");
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

        [HttpPut("UpdateClass/{id}")]
        public async Task<ActionResult> UpdateClass(string id, ClassModel updatedClass)
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

                        var documentRef = _firebaseClient.Collection("classes").Document(id);
                        var document = await documentRef.GetSnapshotAsync();

                        if (!document.Exists)
                            return NotFound($"Classe com ID '{id}' não foi encontrada.");

                        await documentRef.SetAsync(updatedClass);

                        return Ok("Classe atualizada com sucesso.");
                    }

                    return Unauthorized("Este usuário não pode acessar essa funcionalidade.");
                }

                return NotFound("Nenhum usuário conectado foi encontrado.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno do servidor: " + ex.Message);
            }
        }

        [HttpDelete("DeleteClass/{id}")]
        public async Task<ActionResult> DeleteClass(string id)
        {
            try
            {
                var token = HttpContext.Session.GetString("_userToken");

                if (token != null)
                {
                    var session = await _sessionService.GetConnectedUser(token);

                    if (session.UserType == "Secretary")
                    {
                        var documentRef = _firebaseClient.Collection("classes").Document(id);
                        var document = await documentRef.GetSnapshotAsync();

                        if (!document.Exists)
                            return NotFound($"Classe com ID '{id}' não foi encontrada.");

                        await documentRef.DeleteAsync();

                        return Ok("Classe excluída com sucesso.");
                    }

                    return Unauthorized("Este usuário não pode acessar essa funcionalidade.");
                }

                return NotFound("Nenhum usuário conectado foi encontrado.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro interno do servidor: " + ex.Message);
            }
        }

        [HttpGet("GetClass/{id}")]
        public async Task<ActionResult> GetClass(string id)
        {
            try
            {
                var token = HttpContext.Session.GetString("_userToken");

                if (token != null)
                {
                    var session = await _sessionService.GetConnectedUser(token);

                    if (session.UserType == "Secretary")
                    {
                        var documentRef = _firebaseClient.Collection("classes").Document(id);
                        var document = await documentRef.GetSnapshotAsync();

                        if (!document.Exists)
                            return NotFound($"Classe com ID '{id}' não foi encontrada.");

                        var schoolClass = document.ConvertTo<ClassModel>();

                        return Ok(schoolClass);
                    }

                    return Unauthorized("Este usuário não pode acessar essa funcionalidade.");
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
