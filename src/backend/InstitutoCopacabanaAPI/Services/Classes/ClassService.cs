﻿using Google.Cloud.Firestore;
using InstitutoCopacabanaAPI.Data;
using InstitutoCopacabanaAPI.Models;
using InstitutoCopacabanaAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InstitutoCopacabanaAPI.Services.Classes
{
    public class ClassService : IClassService
    {
        public readonly FirestoreDb _firebaseClient;

        public ClassService(ContextDb contextDb)
        {
            _firebaseClient = contextDb.GetClient();
        }

        public async Task<ClassModel?> GetClassByName(string className) 
        {
            CollectionReference classesRef = _firebaseClient.Collection("classes");

            Query query = classesRef.WhereEqualTo("Name", className);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            if (querySnapshot.Documents.Count > 0) 
            { 
                DocumentSnapshot document = querySnapshot.Documents[0];
                return document.ConvertTo<ClassModel>();
            }

            return null;
        }

        public async Task<UserModel?> GetStudentByName(string studentName)
        {
            CollectionReference classesRef = _firebaseClient.Collection("users");

            Query query = classesRef.WhereEqualTo("Name", studentName);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            if (querySnapshot.Documents.Count > 0)
            {
                DocumentSnapshot document = querySnapshot.Documents[0];
                return document.ConvertTo<UserModel>();
            }

            return null;
        }

        public async Task<List<StudentModel>> GetStudentsByClassName(string className)
        {
            ClassModel? classRef = await GetClassByName(className);

            List<StudentModel> studentsList = new List<StudentModel>();

            if (classRef == null)
                return studentsList;

            DocumentReference docRef = _firebaseClient.Collection("classes").Document(classRef.Id);

            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                if (snapshot.ContainsField("Students"))
                {
                    var studentsArray = snapshot.GetValue<IEnumerable<object>>("Students");

                    if (studentsArray != null)
                    {
                        foreach (var studentData in studentsArray)
                        {
                            if (studentData is Dictionary<string, object> studentDict)
                            {
                                StudentModel student = new StudentModel
                                {
                                    StudentId = studentDict.TryGetValue("StudentId", out var studentIdValue) ? studentIdValue?.ToString() ?? string.Empty : "Não foi possível localizar",
                                    Name = studentDict.TryGetValue("Name", out var nameValue) ? nameValue?.ToString() ?? string.Empty : "Não foi possível localizar", 
                                };

                                studentsList.Add(student);
                            }
                        }
                    }
                }
            }

            return studentsList;
        }

        public async Task<List<AttendanceModel>> GetAttendanceByStudentName(string studentName)
        {
            CollectionReference attendanceRef = _firebaseClient.Collection("attendance");

            Query query = attendanceRef.WhereEqualTo("StudentName", studentName);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            List<AttendanceModel> attendanceList = new List<AttendanceModel>();

            foreach (DocumentSnapshot document in querySnapshot.Documents)
            {
                AttendanceModel attendance = document.ConvertTo<AttendanceModel>();
                attendanceList.Add(attendance);
            }

            return attendanceList;
        }

        public async Task<ClassModel> CreateClass(ClassModel schoolClass)
        {
            DocumentReference docRef = _firebaseClient.Collection("classes").Document(schoolClass.Id);

            await docRef.SetAsync(schoolClass);

            return schoolClass;
        }
        public async Task UpdateStudentClassName(string className, UserModel user)
        {
            user.ClassName = className;

            DocumentReference studentRef = _firebaseClient.Collection("users").Document(user.Id);
            
            Dictionary<string, object> updateData = new Dictionary<string, object>
            {
                { "ClassName", className }
            };

            await studentRef.UpdateAsync(updateData);
        }


        public async Task<StudentModel> InsertStudent(string classId, UserModel user, string className)
        {
            await UpdateStudentClassName(className, user);

            StudentModel student = new StudentModel
            {
                StudentId = user.Id,
                Name = user.Name,
                Subjects = new List<SubjectModel> { new SubjectModel() }
            };

            DocumentReference docRef = _firebaseClient.Collection("classes").Document(classId);

            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists && !snapshot.ContainsField("Students"))
            {
                Dictionary<string, object> initData = new Dictionary<string, object>
                {
                    { "Students", new List<StudentModel>() }
                };

                await docRef.UpdateAsync(initData);
            }

            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "Students", FieldValue.ArrayUnion(student) }
            };

            await docRef.UpdateAsync(updates);

            return student;
        }
    }
}
