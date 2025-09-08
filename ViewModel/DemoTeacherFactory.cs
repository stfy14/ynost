//using System;
//using Ynost.Models;

//namespace Ynost.ViewModels
//{
//    internal static class DemoTeacherFactory
//    {
//        // -------------------- Иванов ----------------------------------------
//        public static TeacherViewModel BuildIvanov()
//        {
//            var t = new Teacher
//            {
//                Id = Guid.NewGuid(),
//                FullName = "Иванов Иван Иванович",
//                IsLecturer = true
//            };

//            // 1. Академ. результаты
//            t.AcademicResults.AddRange(new[]
//            {
//                new AcademicYearResult
//                {
//                    Group            = "МИ-8",
//                    AcademicPeriod   = "2023-2024",
//                    Subject          = "Математика",
//                    AvgSem1          = "4.2",
//                    AvgSem2          = "4.4",
//                    DynamicsSem         = "+0.2",
//                    AvgSuccessRate   = "73.5",
//                    AvgQualityRate   = "95.0",
//                    SouRate          = "0.95"
//                },
//                new AcademicYearResult
//                {
//                    Group          = "ПИ-3",
//                    AcademicPeriod = "2022-2023",
//                    Subject        = "Математика",
//                    AvgSem1        = "4.0",
//                    AvgSem2        = "4.2",
//                    DynamicsSem       = "+0.2",
//                    AvgSuccessRate = "71.0",
//                    AvgQualityRate = "94.0",
//                    SouRate        = "0.94"
//                },
//                new AcademicYearResult
//                {
//                    Group          = "СУ-100",
//                    AcademicPeriod = "2021-2022",
//                    Subject        = "Математика",
//                    AvgSem1        = "3.8",
//                    AvgSem2        = "4.0",
//                    DynamicsSem       = "+0.2",
//                    AvgSuccessRate = "68.0",
//                    AvgQualityRate = "92.0",
//                    SouRate        = "0.92"
//                }
//            });

//            // 2. ГИА
//            t.GiaResults.Add(new GiaResult
//            {
//                Subject = "Математика",
//                Group = "11Б",
//                TotalParticipants = "22",
//                Count5 = "10.0",
//                Count4 = "25.0",
//                Count3 = "55.0",
//                PctFail = "5.0",
//                AvgScore = "75.5"
//            });
//            t.GiaResults.Add(new GiaResult
//            {
//                Subject = "Математика",
//                Group = "9А",
//                TotalParticipants = "28",
//                Count5 = "15.0",
//                Count4 = "30.0",
//                Count3 = "45.0",
//                PctFail = "10.0",
//                AvgScore = "71.0"
//            });

//            // 3. ДЭ
//            t.DemoExamResults.Add(new DemoExamResult
//            {
//                Subject = "Сетевое и системное администрирование",
//                Group = "ИС-41",
//                TotalParticipants = "20",
//                Count5 = "25.0",
//                Count4 = "50.0",
//                Count3 = "20.0",
//                Count2 = "5.0",
//                AvgScore = "4.5"
//            });

//            // 4. НОКО
//            t.IndependentAssessments.Add(new IndependentAssessment
//            {
//                AssessmentName = "ВПР",
//                AssessmentDate = "10.04.2024",
//                ClassSubject = "7А/Математика",
//                StudentsTotal = "27",
//                StudentsParticipated = "27",
//                StudentsPassed = "24",
//                Count5 = "88.8",
//                Count4 = "90.0",
//                Count3 = "0.9"
//            });

//            // 5–14. Прочие разделы (объект-инициализаторы)
//            t.SelfDeterminations.Add(new SelfDeterminationActivity
//            {
//                Level = "муниципальный",
//                Name = "День профессий",
//                Role = "координатор"
//            });

//            t.StudentOlympiads.Add(new StudentOlympiad
//            {
//                Level = "региональный",
//                Name = "Кенгуру",
//                Form = "очно",
//                Cadet = "Петрова А.А.",
//                Result = "призёр"
//            });

//            t.JuryActivities.Add(new JuryActivity
//            {
//                Level = "региональный",
//                Name = "Олимпиада школьников",
//                EventDate = "01.12.2023"
//            });

//            t.MasterClasses.Add(new MasterClass
//            {
//                Level = "школьный",
//                Name = "GeoGebra на уроке",
//                EventDate = "18.02.2024"
//            });

//            t.Speeches.Add(new Speech
//            {
//                Level = "вуз",
//                Name = "Методика решения задач",
//                EventDate = "05.10.2023"
//            });

//            t.Publications.Add(new Publication
//            {
//                Level = "региональный",
//                Title = "Игровые технологии",
//                Date = "01.06.2022"
//            });

//            t.ExperimentalProjects.Add(new ExperimentalProject
//            {
//                Name = "Гибридное обучение",
//                Date = "20.01.2024"
//            });

//            t.Mentorships.Add(new Mentorship
//            {
//                Trainee = "Сидоров С.С.",
//                OrderNo = "№15-н",
//                OrderDate = "10.09.2023"
//            });

//            t.ProgramSupports.Add(new ProgramMethodSupport
//            {
//                ProgramName = "Алгебра 7–9",
//                HasControlMaterials = true
//            });

//            t.ProfessionalCompetitions.Add(new ProfessionalCompetition
//            {
//                Level = "федеральный",
//                Name = "Учитель года",
//                Achievement = "участник",
//                EventDate = "15.05.2023"
//            });

//            return new TeacherViewModel(t);
//        }

//        // -------------------- Петров ----------------------------------------
//        public static TeacherViewModel BuildPetrov()
//        {
//            var p = new Teacher
//            {
//                Id = Guid.NewGuid(),
//                FullName = "Петров Пётр Петрович",
//                IsLecturer = false
//            };

//            p.AcademicResults.Add(new AcademicYearResult
//            {
//                Group = "ДИ-20",
//                AcademicPeriod = "2023-2024",
//                Subject = "Физика",
//                AvgSem1 = "4.0",
//                AvgSem2 = "4.5",
//                DynamicsSem = "+0.5",
//                AvgSuccessRate = "78.0",
//                AvgQualityRate = "97.0",
//                SouRate = "0.97"
//            });

//            p.GiaResults.Add(new GiaResult
//            {
//                Subject = "Физика",
//                Group = "11А",
//                TotalParticipants = "18",
//                Count5 = "12.5",
//                Count4 = "50.0",
//                Count3 = "35.0",
//                PctFail = "2.5",
//                AvgScore = "70.0"
//            });
//            p.GiaResults.Add(new GiaResult
//            {
//                Subject = "Физика",
//                Group = "9Б",
//                TotalParticipants = "25",
//                Count5 = "20.0",
//                Count4 = "40.0",
//                Count3 = "30.0",
//                PctFail = "10.0",
//                AvgScore = "68.0"
//            });

//            p.DemoExamResults.Add(new DemoExamResult
//            {
//                Subject = "Ремонт и обслуживание легковых автомобилей",
//                Group = "ТО-31",
//                TotalParticipants = "15",
//                Count5 = "20.0",
//                Count4 = "60.0",
//                Count3 = "15.0",
//                Count2 = "5.0",
//                AvgScore = "4.2"
//            });

//            p.IndependentAssessments.Add(new IndependentAssessment
//            {
//                AssessmentName = "НИКО",
//                AssessmentDate = "15.04.2024",
//                ClassSubject = "8Б/Физика",
//                StudentsTotal = "26",
//                StudentsParticipated = "26",
//                StudentsPassed = "22",
//                Count5 = "84.6",
//                Count4 = "85.0",
//                Count3 = "0.85"
//            });

//            p.SelfDeterminations.Add(new SelfDeterminationActivity
//            {
//                Level = "региональный",
//                Name = "Форум профессий",
//                Role = "организатор"
//            });

//            p.StudentOlympiads.Add(new StudentOlympiad
//            {
//                Level = "всероссийский",
//                Name = "Шаг в науку",
//                Form = "дист.",
//                Cadet = "Сидоров С.С.",
//                Result = "лауреат"
//            });

//            p.JuryActivities.Add(new JuryActivity
//            {
//                Level = "муниципальный",
//                Name = "Конкурс роботов",
//                EventDate = "10.11.2023"
//            });

//            p.MasterClasses.Add(new MasterClass
//            {
//                Level = "муниципальный",
//                Name = "STEM-урок",
//                EventDate = "20.02.2024"
//            });

//            p.Speeches.Add(new Speech
//            {
//                Level = "региональный",
//                Name = "STEAM-круглый стол",
//                EventDate = "05.03.2024"
//            });

//            p.Publications.Add(new Publication
//            {
//                Level = "региональный",
//                Title = "Arduino на уроках",
//                Date = "01.10.2022"
//            });

//            p.ExperimentalProjects.Add(new ExperimentalProject
//            {
//                Name = "VR-лаборатория",
//                Date = "15.01.2024"
//            });

//            p.Mentorships.Add(new Mentorship
//            {
//                Trainee = "Кузнецов К.К.",
//                OrderNo = "№12-к",
//                OrderDate = "12.09.2023"
//            });

//            p.ProgramSupports.Add(new ProgramMethodSupport
//            {
//                ProgramName = "Физика 10–11",
//                HasControlMaterials = true
//            });

//            p.ProfessionalCompetitions.Add(new ProfessionalCompetition
//            {
//                Level = "региональный",
//                Name = "Учитель года",
//                Achievement = "финалист",
//                EventDate = "20.05.2022"
//            });

//            return new TeacherViewModel(p);
//        }
//    }
//}
